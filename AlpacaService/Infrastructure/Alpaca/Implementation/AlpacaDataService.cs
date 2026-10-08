namespace BN.PROJECT.AlpacaService;

public class AlpacaDataService : IAlpacaDataService
{
    private readonly ILogger<AlpacaDataService> _logger;
    private readonly IAlpacaClient _alpacaClient;
    private readonly IRedisService _redisService;
    private readonly SemaphoreSlim _streamConnectLock = new(1, 1);
    private readonly ConcurrentDictionary<string, IAlpacaDataSubscription<ITrade>> _tradeSubscriptions = new();
    private bool _streamConnected;

    public AlpacaDataService(IAlpacaClient alpacaClient, ILogger<AlpacaDataService> logger, IRedisService redisService)
    {
        _alpacaClient = alpacaClient;
        _logger = logger;
        _redisService = redisService;
    }

    // Check if markets are open
    public async Task<IClock> GetClockAsync()
    {
        var tradingClient = _alpacaClient.GetCommonTradingClient();
        var clock = await tradingClient.GetClockAsync();
        return clock;
    }

    public async Task<List<AlpacaCalendar>?> ListIntervalCalendarAsync(DateOnly startDate, DateOnly endDate = default)
    {
        var alpacaCalendar = new List<AlpacaCalendar>();
        var tradingClient = _alpacaClient.GetCommonTradingClient();

        CalendarRequest req = new(
            startDate,
            endDate == default ? DateOnly.FromDateTime(DateTime.UtcNow) : endDate);

        var calendarList = await tradingClient.ListIntervalCalendarAsync(req);
        foreach (var calendar in calendarList)
        {
            alpacaCalendar.Add(calendar.ToAlpacaCalendar());
        }

        return alpacaCalendar;
    }
    public async Task<List<AlpacaAsset>> GetAssetsAsync()
    {
        var tradingClient = _alpacaClient.GetCommonTradingClient();
        var req = new AssetsRequest();
        req.AssetClass = AssetClass.UsEquity;
        var assets = await tradingClient.ListAssetsAsync(req);
        return assets.Select(a => a.ToAlpacaAsset()).ToList();
    }
    public async Task<IAsset> GetAssetBySymbolAsync(string symbol)
    {
        var tradingClient = _alpacaClient.GetCommonTradingClient();
        return await tradingClient.GetAssetAsync(symbol);
    }
    // Bars   

    public async Task<List<AlpacaBar>> GetHistoricalBarsBySymbol(string symbol, DateTime startDate, DateTime endDate, BarTimeFrame timeFrame)
    {
        var result = new List<AlpacaBar>();
        var client = _alpacaClient.GetAlpacaDataClient();

        var req = new HistoricalBarsRequest(symbol, startDate, endDate, timeFrame)
        {
            Feed = MarketDataFeed.Iex
        };
        var barSet = await client.ListHistoricalBarsAsync(req);

        foreach (var bar in barSet.Items)
        {
            result.Add(bar.ToAlpacaBar(symbol));
        }

        return result;
    }

    // get latest bar for symbol
    public async Task<AlpacaBar> GetLatestBarBySymbol(string symbol)
    {
        var client = _alpacaClient.GetAlpacaDataClient();

        var req = new LatestMarketDataRequest(symbol)
        {
            Feed = MarketDataFeed.Iex,
        };
        var latestBar = await client.GetLatestBarAsync(req);

        return latestBar.ToAlpacaBar(symbol);
    }

    // Trades

    public async Task<List<AlpacaTrade>> GetTradesBySymbol(string symbol, DateTime startDate, DateTime endDate)
    {
        var client = _alpacaClient.GetAlpacaDataClient();

        var tradesRequest = new HistoricalTradesRequest(symbol, startDate, endDate)
        {
            Feed = MarketDataFeed.Iex
        };
        var tradeSet = await client.GetHistoricalTradesAsync(tradesRequest);
        var trades = tradeSet.Items[symbol].Select(t => t.ToAlpacaTrade()).ToList();
        return trades;
    }

    public async Task<AlpacaTrade> GetLatestTradeBySymbol(string symbol)
    {
        var client = _alpacaClient.GetAlpacaDataClient();
        var req = new LatestMarketDataRequest(symbol)
        {
            Feed = MarketDataFeed.Iex,
        };
        var trade = await client.GetLatestTradeAsync(req);
        return trade.ToAlpacaTrade();
    }

    // Quotes
    public async Task<AlpacaQuote> GetLatestQuoteBySymbol(string symbol)
    {
        var client = _alpacaClient.GetAlpacaDataClient();

        var quote = await client.GetLatestQuoteAsync(new LatestMarketDataRequest(symbol)
        {
            Feed = MarketDataFeed.Iex
        });
        return quote.ToAlpacaQuote();
    }

    public async Task<List<AlpacaQuote>> GetQuotesBySymbol(string symbol, DateTime startDate, DateTime endDate)
    {
        var result = new List<AlpacaQuote>();
        var client = _alpacaClient.GetAlpacaDataClient();

        var quotesRequest = new HistoricalQuotesRequest(symbol, startDate, endDate)
        {
            Feed = MarketDataFeed.Iex,
        };
        var quoteSet = await client.GetHistoricalQuotesAsync(quotesRequest);
        var quotes = quoteSet.Items;

        foreach (var quote in quotes[symbol].ToList())
        {
            result.Add(quote.ToAlpacaQuote());
        }

        return result;
    }

    public IAlpacaDataStreamingClient GetStreamingClient()
    {
        return _alpacaClient.GetStreamingClient();
    }

    private async Task<IAlpacaDataStreamingClient> GetConnectedStreamingClientAsync()
    {
        var client = GetStreamingClient();
        if (_streamConnected)
        {
            return client;
        }

        await _streamConnectLock.WaitAsync();
        try
        {
            if (!_streamConnected)
            {
                // Alpaca allows only one live streaming connection per API key; a second
                // process/environment connecting with the same key will force this one shut.
                client.SocketClosed += () =>
                {
                    _streamConnected = false;
                    _logger.LogWarning("Alpaca streaming socket closed unexpectedly (another connection with the same API key may have taken over)");
                };
                client.OnError += ex => _logger.LogError(ex, "Alpaca streaming connection error");
                client.OnWarning += message => _logger.LogWarning("Alpaca streaming warning: {Message}", message);

                await client.ConnectAndAuthenticateAsync();
                _streamConnected = true;
            }
        }
        finally
        {
            _streamConnectLock.Release();
        }

        return client;
    }

    public async Task SubscribeToTradeUpdates(string symbol)
    {
        var client = await GetConnectedStreamingClientAsync();

        // Reuse the existing subscription object per symbol so unsubscribe targets the same instance.
        var tradeSubscription = _tradeSubscriptions.GetOrAdd(symbol, s =>
        {
            var subscription = client.GetTradeSubscription(s);
            subscription.Received += trade =>
            {
                var alpacaTrade = trade.ToAlpacaTrade();
                _logger.LogInformation("Received trade update: {@AlpacaTrade}", alpacaTrade);

                _ = _redisService.PublishTradesToStream(s, new List<AlpacaTrade> { alpacaTrade }, 100000);
            };
            return subscription;
        });

        await client.SubscribeAsync(tradeSubscription);
    }

    public async Task UnSubscribeFromTradeUpdates(string symbol)
    {
        if (!_tradeSubscriptions.TryRemove(symbol, out var tradeSubscription))
        {
            _logger.LogWarning("No active trade subscription found for {Symbol}", symbol);
            return;
        }

        var client = GetStreamingClient();
        await client.UnsubscribeAsync(tradeSubscription);

        // Keep the shared connection alive while other symbols are still subscribed.
        if (_tradeSubscriptions.IsEmpty)
        {
            await _streamConnectLock.WaitAsync();
            try
            {
                if (_tradeSubscriptions.IsEmpty && _streamConnected)
                {
                    await client.DisconnectAsync();
                    _streamConnected = false;
                }
            }
            finally
            {
                _streamConnectLock.Release();
            }
        }
    }
}
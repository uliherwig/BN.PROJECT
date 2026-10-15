namespace BN.PROJECT.AlpacaService;

public class AlpacaDataService : IAlpacaDataService
{
    private readonly ILogger<AlpacaDataService> _logger;
    private readonly IAlpacaClient _alpacaClient;
    private readonly IRedisService _redisService;
    private readonly SemaphoreSlim _streamConnectLock = new(1, 1);
    private readonly ConcurrentDictionary<string, IAlpacaDataSubscription<ITrade>> _tradeSubscriptions = new();
    private readonly IAlpacaDataClient _dataClient;
    private readonly IAlpacaTradingClient _tradingClient;
    private bool _streamConnected;

    public AlpacaDataService(IAlpacaClient alpacaClient, ILogger<AlpacaDataService> logger, IRedisService redisService)
    {
        _alpacaClient = alpacaClient;
        _logger = logger;
        _redisService = redisService;
        _dataClient = alpacaClient.GetAlpacaDataClient();
        _tradingClient = alpacaClient.GetCommonTradingClient();
    }

    // Check if markets are open
    public async Task<IClock> GetClockAsync()
    {
        var clock = await _tradingClient.GetClockAsync();
        return clock;
    }

    public async Task<List<AlpacaCalendar>?> ListIntervalCalendarAsync(DateOnly startDate, DateOnly endDate = default)
    {
        var alpacaCalendar = new List<AlpacaCalendar>();

        CalendarRequest req = new(
            startDate,
            endDate == default ? DateOnly.FromDateTime(DateTime.UtcNow) : endDate);

        var calendarList = await _tradingClient.ListIntervalCalendarAsync(req);
        foreach (var calendar in calendarList)
        {
            alpacaCalendar.Add(calendar.ToAlpacaCalendar());
        }

        return alpacaCalendar;
    }
    public async Task<List<AlpacaAsset>> GetAssetsAsync()
    {
        var req = new AssetsRequest();
        req.AssetClass = AssetClass.UsEquity;
        var assets = await _tradingClient.ListAssetsAsync(req);
        return assets.Select(a => a.ToAlpacaAsset()).ToList();
    }
    public async Task<IAsset> GetAssetBySymbolAsync(string symbol)
    {
        return await _tradingClient.GetAssetAsync(symbol);
    }
    // Bars   

    public async Task<List<AlpacaBar>> GetHistoricalBarsBySymbol(string symbol, DateTime startDate, DateTime endDate, BarTimeFrame timeFrame)
    {
        var result = new List<AlpacaBar>();

        var req = new HistoricalBarsRequest(symbol, startDate, endDate, timeFrame)
        {
            Feed = MarketDataFeed.Iex
        };
        var barSet = await _dataClient.ListHistoricalBarsAsync(req);

        foreach (var bar in barSet.Items)
        {
            result.Add(bar.ToAlpacaBar(symbol));
        }

        return result;
    }

    // get latest bar for symbol
    public async Task<AlpacaBar> GetLatestBarBySymbol(string symbol)
    {
        var req = new LatestMarketDataRequest(symbol)
        {
            Feed = MarketDataFeed.Iex,
        };
        var latestBar = await _dataClient.GetLatestBarAsync(req);

        return latestBar.ToAlpacaBar(symbol);
    }

    // Trades

    public async Task<List<AlpacaTrade>> GetTradesBySymbol(string symbol, DateTime startDate, DateTime endDate)
    {
        var tradesRequest = new HistoricalTradesRequest(symbol, startDate, endDate)
        {
            Feed = MarketDataFeed.Iex
        };
        var tradeSet = await _dataClient.GetHistoricalTradesAsync(tradesRequest);
        var trades = tradeSet.Items[symbol].Select(t => t.ToAlpacaTrade()).ToList();
        return trades;
    }

    public async Task<AlpacaTrade> GetLatestTradeBySymbol(string symbol)
    {
        var req = new LatestMarketDataRequest(symbol)
        {
            Feed = MarketDataFeed.Iex,
        };
        var trade = await _dataClient.GetLatestTradeAsync(req);
        return trade.ToAlpacaTrade();
    }

    // Quotes
    public async Task<AlpacaQuote> GetLatestQuoteBySymbol(string symbol)
    {
        var quote = await _dataClient.GetLatestQuoteAsync(new LatestMarketDataRequest(symbol)
        {
            Feed = MarketDataFeed.Iex
        });
        return quote.ToAlpacaQuote();
    }

    public async Task<List<AlpacaQuote>> GetQuotesBySymbol(string symbol, DateTime startDate, DateTime endDate)
    {
        var result = new List<AlpacaQuote>();
        var quotesRequest = new HistoricalQuotesRequest(symbol, startDate, endDate)
        {
            Feed = MarketDataFeed.Iex,
        };
        var quoteSet = await _dataClient.GetHistoricalQuotesAsync(quotesRequest);
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
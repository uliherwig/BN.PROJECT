namespace BN.PROJECT.AlpacaService;

public class AlpacaDataService : IAlpacaDataService
{
    private readonly ILogger<AlpacaDataService> _logger;
    private readonly IAlpacaClient _alpacaClient;
    private readonly IRedisService _redisService;


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

    public async Task SubscribeToTradeUpdates(string symbol)
    {
        var streamKey = RedisUtilities.GetTradesStreamKey(symbol);

        var client = GetStreamingClient();
        await client.ConnectAndAuthenticateAsync();
        var tradeSubscription = client.GetTradeSubscription(symbol);

        tradeSubscription.Received += (trade) =>
        {
            var alpacaTrade = trade.ToAlpacaTrade();
            _logger.LogInformation("Received trade update: {@AlpacaTrade}", alpacaTrade);

            _redisService.PublishTradesToStream(streamKey, new List<AlpacaTrade> { alpacaTrade }, 100000);
        };

        await client.SubscribeAsync(tradeSubscription);
    }

    public async Task UnSubscribeFromTradeUpdates(string symbol)
    {
        var client = GetStreamingClient();
        var tradeSubscription = client.GetTradeSubscription(symbol);
        await client.UnsubscribeAsync(tradeSubscription);
        await client.DisconnectAsync();
    }
}
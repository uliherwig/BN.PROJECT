namespace BN.PROJECT.AlpacaService;

public class AlpacaDataService : IAlpacaDataService
{
    private readonly ILogger<AlpacaDataService> _logger;
    private readonly IAlpacaClient _alpacaClient;

    public AlpacaDataService(IAlpacaClient alpacaClient, ILogger<AlpacaDataService> logger)
    {
        _alpacaClient = alpacaClient;
        _logger = logger;
    }

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

    public async Task<List<ITrade>> GetTradesBySymbol(string symbol, DateTime startDate, DateTime endDate)
    {
        var client = _alpacaClient.GetAlpacaDataClient();

        var into = DateTime.Now;
        var from = into.Subtract(TimeSpan.FromMinutes(25));

        var tradesRequest = new HistoricalTradesRequest(symbol, from, into)
        {
            Feed = MarketDataFeed.Iex
        };
        var tradeSet = await client.GetHistoricalTradesAsync(tradesRequest);
        var trades = tradeSet.Items;
        return trades[symbol].ToList();
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
}
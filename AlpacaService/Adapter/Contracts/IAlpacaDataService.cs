namespace BN.PROJECT.AlpacaService;

public interface IAlpacaDataService
{
    Task<List<AlpacaBar>> GetHistoricalBarsBySymbol(string symbol, DateTime startDate, DateTime endDate, BarTimeFrame timeFrame);
    Task<AlpacaBar> GetLatestBarBySymbol(string symbol);
    Task<List<AlpacaQuote>> GetQuotesBySymbol(string symbol, DateTime startDate, DateTime endDate);
    Task<AlpacaQuote> GetLatestQuoteBySymbol(string symbol);
    Task<List<ITrade>> GetTradesBySymbol(string symbol, DateTime startDate, DateTime endDate);
}
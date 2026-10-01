namespace BN.PROJECT.AlpacaService;

public interface IAlpacaDataService
{
    Task<IClock> GetClockAsync();
    Task<List<AlpacaCalendar>?> ListIntervalCalendarAsync(DateOnly startDate, DateOnly endDate = default);
    Task<List<AlpacaAsset>> GetAssetsAsync();
    Task<IAsset> GetAssetBySymbolAsync(string symbol);
    Task<List<AlpacaBar>> GetHistoricalBarsBySymbol(string symbol, DateTime startDate, DateTime endDate, BarTimeFrame timeFrame);
    Task<AlpacaBar> GetLatestBarBySymbol(string symbol);
    Task<List<AlpacaQuote>> GetQuotesBySymbol(string symbol, DateTime startDate, DateTime endDate);
    Task<AlpacaQuote> GetLatestQuoteBySymbol(string symbol);
    Task<List<AlpacaTrade>> GetTradesBySymbol(string symbol, DateTime startDate, DateTime endDate);
    Task<AlpacaTrade> GetLatestTradeBySymbol(string symbol);
    Task SubscribeToTradeUpdates(string symbol);
    Task UnSubscribeFromTradeUpdates(string symbol);
}
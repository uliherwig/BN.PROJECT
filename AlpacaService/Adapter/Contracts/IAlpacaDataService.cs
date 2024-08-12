namespace BN.TRADER.AlpacaService
{
    public interface IAlpacaDataService
    {
        Task<List<AlpacaBar>> GetHistoricalBarsBySymbol(string symbol, DateTime startDate, DateTime endDate, BarTimeFrame timeFrame);

        Task<AlpacaBar> GetLatestBarBySymbol(string symbol);

        Task<List<IQuote>> GetQuotesBySymbol(string symbol, DateTime startDate, DateTime endDate);

        Task<List<ITrade>> GetTradesBySymbol(string symbol, DateTime startDate, DateTime endDate);
    }
}
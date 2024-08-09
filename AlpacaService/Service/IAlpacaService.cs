namespace BN.TRADER.AlpacaService
{
    public interface IAlpacaService
    {
        Task CreateOrderAsync();

        Task DeleteAllOrdersAsync();

        Task DeleteOrderByIdAsync(string orderId);

        Task<List<BnOhlc>> GetHistoricalBarsBySymbol(string symbol, string timeFrame, DateTime startDate, DateTime endDate);

        Task<string> GetHistoricalQuotesBySymbol(string symbol);
    }
}
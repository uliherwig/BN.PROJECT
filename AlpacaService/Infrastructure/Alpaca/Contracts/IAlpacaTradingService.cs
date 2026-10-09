namespace BN.PROJECT.AlpacaService
{
    public interface IAlpacaTradingService
    {

        Task<IAccount?> GetAccountAsync(UserSettingsModel userSettings);

        Task<List<AlpacaOrder>> GetAllOrdersAsync(OrderStatusFilter orderStatusFilter);

        Task<AlpacaOrder> GetOrderByIdAsync(string orderId);

        Task<bool> CancelOrderByIdAsync(Guid orderId);

        Task<bool> CreateOrderAsync(OrderRequest orderRequest);

        Task<List<AlpacaPosition>> GetAllOpenPositions();

        Task<AlpacaPosition> GetPositionsBySymbol(string symbol);

        Task<AlpacaOrder> ClosePositionOrder(string symbol);
        
        Task<bool> CloseAllPositions();
    }
}
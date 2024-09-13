namespace BN.PROJECT.AlpacaService
{
    public interface IAlpacaTradingService
    {
        Task<IAccount> GetAccountAsync();

        Task<List<AlpacaAsset>> GetAssetsAsync();

        Task<IAsset> GetAssetBySymbolAsync(string symbol);

        Task<List<AlpacaOrder>> GetAllOrdersAsync(OrderStatusFilter orderStatusFilter);

        Task<AlpacaOrder> GetOrderByIdAsync(string orderId);

        Task<bool> CancelOrderByIdAsync(Guid orderId);

        Task<AlpacaOrder> CreateOrderAsync(string symbol, OrderQuantity qty, OrderSide side, OrderType orderType, TimeInForce timeInForce);

        Task<List<AlpacaPosition>> GetAllOpenPositions();

        Task<AlpacaPosition> GetPositionsBySymbol(string symbol);

        Task<AlpacaOrder> ClosePositionOrder(string symbol);
    }
}
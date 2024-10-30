namespace BN.PROJECT.AlpacaService
{
    public interface IAlpacaTradingService
    {
        Task<IAccount?> GetAccountAsync(UserSettings userSettings);

        Task<List<AlpacaAsset>> GetAssetsAsync();

        Task<IAsset> GetAssetBySymbolAsync(string symbol);

        Task<List<AlpacaOrder>> GetAllOrdersAsync(string userId, OrderStatusFilter orderStatusFilter);

        Task<AlpacaOrder> GetOrderByIdAsync(string userId, string orderId);

        Task<bool> CancelOrderByIdAsync(string userId, Guid orderId);

        Task<AlpacaOrder> CreateOrderAsync(string userId, string symbol, OrderQuantity qty, OrderSide side, OrderType orderType, TimeInForce timeInForce);

        Task<List<AlpacaPosition>> GetAllOpenPositions(string userId);

        Task<AlpacaPosition> GetPositionsBySymbol(string userId, string symbol);

        Task<AlpacaOrder> ClosePositionOrder(string userId, string symbol);
    }
}
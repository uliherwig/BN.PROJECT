namespace BN.PROJECT.AlpacaService
{
    public interface IAlpacaTradingService
    {
        Task<IAccount?> GetAccountAsync(UserSettings userSettings);

        Task<List<AlpacaAsset>> GetAssetsAsync();

        Task<IAsset> GetAssetBySymbolAsync(string symbol);

        Task<List<AlpacaOrder>> GetAllOrdersAsync(UserSettings userSettings, OrderStatusFilter orderStatusFilter);

        Task<AlpacaOrder> GetOrderByIdAsync(UserSettings userSettings, string orderId);

        Task<bool> CancelOrderByIdAsync(UserSettings userSettings, Guid orderId);

        Task<AlpacaOrder> CreateOrderAsync(UserSettings userSettings, string symbol, OrderQuantity qty, OrderSide side, OrderType orderType, TimeInForce timeInForce);

        Task<List<AlpacaPosition>> GetAllOpenPositions(UserSettings userSettings);

        Task<AlpacaPosition> GetPositionsBySymbol(UserSettings userSettings, string symbol);

        Task<AlpacaOrder> ClosePositionOrder(UserSettings userSettings, string symbol);
    }
}
namespace BN.PROJECT.AlpacaService
{
    public interface IAlpacaTradingService
    {
        Task<IAccount?> GetAccountAsync(UserSettingsModel userSettings);

        Task<List<AlpacaAsset>> GetAssetsAsync();

        Task<IAsset> GetAssetBySymbolAsync(string symbol);

        Task<List<AlpacaOrder>> GetAllOrdersAsync(UserSettingsModel userSettings, OrderStatusFilter orderStatusFilter);

        Task<AlpacaOrder> GetOrderByIdAsync(UserSettingsModel userSettings, string orderId);

        Task<bool> CancelOrderByIdAsync(UserSettingsModel userSettings, Guid orderId);

        Task<AlpacaOrder> CreateOrderAsync(UserSettingsModel userSettings, string symbol, OrderQuantity qty, OrderSide side, OrderType orderType, TimeInForce timeInForce);

        Task<List<AlpacaPosition>> GetAllOpenPositions(UserSettingsModel userSettings);

        Task<AlpacaPosition> GetPositionsBySymbol(UserSettingsModel userSettings, string symbol);

        Task<AlpacaOrder> ClosePositionOrder(UserSettingsModel userSettings, string symbol);
    }
}
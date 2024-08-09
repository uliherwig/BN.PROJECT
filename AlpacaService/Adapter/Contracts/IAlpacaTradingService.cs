namespace BN.TRADER.AlpacaService
{
    public interface IAlpacaTradingService
    {
        Task<IAccount> GetAccountAsync();

        Task<List<IAsset>> GetAssetsAsync();

        Task<IAsset> GetAssetBySymbolAsync(string symbol);

        Task<List<IOrder>> GetAllOrdersAsync();

        Task<IOrder> GetOrderByIdAsync(string orderId);

        Task<bool> CancelOrderByIdAsync(Guid orderId);

        Task<IOrder> CreateOrderAsync(string symbol, OrderQuantity qty, OrderSide side, OrderType orderType, TimeInForce timeInForce);

        Task<List<IPosition>> GetPositions();

        Task<IPosition> GetPositionsBySymbol(string symbol);

        Task<IOrder> ClosePosition(string symbol);
    }
}
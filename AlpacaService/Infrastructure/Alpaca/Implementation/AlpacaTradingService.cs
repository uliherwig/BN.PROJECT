

namespace BN.PROJECT.AlpacaService
{
    public class AlpacaTradingService : IAlpacaTradingService
    {
        private readonly IAlpacaClient _alpacaClient;
        private readonly ILogger<AlpacaTradingService> _logger;

        public AlpacaTradingService(IAlpacaClient alpacaClient,
            ILogger<AlpacaTradingService> logger)
        {
            _alpacaClient = alpacaClient;
            _logger = logger;
        }

        public async Task<IAccount?> GetAccountAsync(UserSettingsModel userSettings)
        {
            try
            {
                var tradingClient = _alpacaClient.GetPrivateTradingClient(userSettings);
                return tradingClient != null ? await tradingClient.GetAccountAsync() : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting account for user id {userSettings.UserId}");
            }
            return null;
        }

        public async Task<List<AlpacaAsset>> GetAssetsAsync()
        {
            var tradingClient = _alpacaClient.GetCommonTradingClient();
            var req = new AssetsRequest();
            req.AssetClass = AssetClass.UsEquity;
            var assets = await tradingClient.ListAssetsAsync(req);
            return assets.Select(a => a.ToAlpacaAsset()).ToList();
        }

        public async Task<IAsset> GetAssetBySymbolAsync(string symbol)
        {
            var tradingClient = _alpacaClient.GetCommonTradingClient();
            return await tradingClient.GetAssetAsync(symbol);
        }

        public async Task<List<AlpacaOrder>> GetAllOrdersAsync(UserSettingsModel userSettings, OrderStatusFilter orderStatusFilter)
        {
            var tradingClient = _alpacaClient.GetPrivateTradingClient(userSettings);
            var req = new ListOrdersRequest
            {
                OrderStatusFilter = orderStatusFilter
            };

            var orders = await tradingClient.ListOrdersAsync(req);
            return orders.Select(order => order.ToAlpacaOrder()).ToList();
        }

        public async Task<AlpacaOrder> GetOrderByIdAsync(UserSettingsModel userSettings, string orderId)
        {
            var tradingClient = _alpacaClient.GetPrivateTradingClient(userSettings);
            var order = await tradingClient.GetOrderAsync(orderId);
            return order.ToAlpacaOrder();
        }

        public async Task<bool> CancelOrderByIdAsync(UserSettingsModel userSettings, Guid orderId)
        {
            var tradingClient = _alpacaClient.GetPrivateTradingClient(userSettings);
            return await tradingClient.CancelOrderAsync(orderId);
        }

        public async Task<AlpacaOrder> CreateOrderAsync(UserSettingsModel userSettings, string symbol, OrderQuantity qty, OrderSide side, OrderType orderType, TimeInForce timeInForce)
        {
            var tradingClient = _alpacaClient.GetPrivateTradingClient(userSettings);
            var req = new NewOrderRequest(symbol, qty, side, orderType, timeInForce);
            var order = await tradingClient.PostOrderAsync(req);
            var alpacaOrder = order.ToAlpacaOrder();
            return alpacaOrder;
        }

        public async Task<List<AlpacaPosition>> GetAllOpenPositions(UserSettingsModel userSettings)
        {
            var tradingClient = _alpacaClient.GetPrivateTradingClient(userSettings);
            var positions = await tradingClient.ListPositionsAsync();
            return positions.Select(p => p.ToAlpacaPosition()).ToList();
        }

        public async Task<AlpacaPosition> GetPositionsBySymbol(UserSettingsModel userSettings, string symbol)
        {
            var tradingClient = _alpacaClient.GetPrivateTradingClient(userSettings);
            var pos = await tradingClient.GetPositionAsync(symbol);
            return pos.ToAlpacaPosition();
        }

        public async Task<AlpacaOrder> ClosePositionOrder(UserSettingsModel userSettings, string symbol)
        {
            var tradingClient = _alpacaClient.GetPrivateTradingClient(userSettings);
            var deletePositionRequest = new DeletePositionRequest(symbol);
            var order = await tradingClient.DeletePositionAsync(deletePositionRequest);
            return order.ToAlpacaOrder();
        }
    }
}
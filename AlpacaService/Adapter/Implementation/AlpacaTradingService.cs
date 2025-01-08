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

        public async Task<IAccount?> GetAccountAsync(UserSettings userSettings)
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
            var assets = await tradingClient.ListAssetsAsync(new AssetsRequest());
            return assets.Select(a => a.ToAlpacaAsset()).ToList();
        }

        public async Task<IAsset> GetAssetBySymbolAsync(string symbol)
        {
            var tradingClient = _alpacaClient.GetCommonTradingClient();
            return await tradingClient.GetAssetAsync(symbol);
        }

        public async Task<List<AlpacaOrder>> GetAllOrdersAsync(string userId, OrderStatusFilter orderStatusFilter)
        {
            var tradingClient = _alpacaClient.GetCommonTradingClient();
            var req = new ListOrdersRequest
            {
                OrderStatusFilter = orderStatusFilter
            };

            var orders = await tradingClient.ListOrdersAsync(req);
            return orders.Select(order => order.ToAlpacaOrder()).ToList();
        }

        public async Task<AlpacaOrder> GetOrderByIdAsync(string userId, string orderId)
        {
            var tradingClient = _alpacaClient.GetCommonTradingClient();
            var order = await tradingClient.GetOrderAsync(orderId);
            return order.ToAlpacaOrder();
        }

        public async Task<bool> CancelOrderByIdAsync(string userId, Guid orderId)
        {
            var tradingClient = _alpacaClient.GetCommonTradingClient();
            return await tradingClient.CancelOrderAsync(orderId);
        }

        public async Task<AlpacaOrder> CreateOrderAsync(string userId, string symbol, OrderQuantity qty, OrderSide side, OrderType orderType, TimeInForce timeInForce)
        {
            var tradingClient = _alpacaClient.GetCommonTradingClient();
            var req = new NewOrderRequest(symbol, qty, side, orderType, timeInForce);
            var order = await tradingClient.PostOrderAsync(req);
            var alpacaOrder = order.ToAlpacaOrder();
            return alpacaOrder;
        }

        public async Task<List<AlpacaPosition>> GetAllOpenPositions(string userId)
        {
            var tradingClient = _alpacaClient.GetCommonTradingClient();
            var positions = await tradingClient.ListPositionsAsync();
            return positions.Select(p => p.ToAlpacaPosition()).ToList();
        }

        public async Task<AlpacaPosition> GetPositionsBySymbol(string userId, string symbol)
        {
            var tradingClient = _alpacaClient.GetCommonTradingClient();
            var pos = await tradingClient.GetPositionAsync(symbol);
            return pos.ToAlpacaPosition();
        }

        public async Task<AlpacaOrder> ClosePositionOrder(string userId, string symbol)
        {
            var tradingClient = _alpacaClient.GetCommonTradingClient();
            var deletePositionRequest = new DeletePositionRequest(symbol);
            var order = await tradingClient.DeletePositionAsync(deletePositionRequest);
            return order.ToAlpacaOrder();
        }
    }
}
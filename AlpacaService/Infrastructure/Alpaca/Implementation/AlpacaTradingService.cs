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

        public async Task<List<AlpacaOrder>> GetAllOrdersAsync(OrderStatusFilter orderStatusFilter)
        {
            var tradingClient = _alpacaClient.GetCommonTradingClient();
            var req = new ListOrdersRequest
            {
                OrderStatusFilter = orderStatusFilter
            };

            var orders = await tradingClient.ListOrdersAsync(req);
            return orders.Select(order => order.ToAlpacaOrder()).ToList();
        }

        public async Task<AlpacaOrder> GetOrderByIdAsync(string orderId)
        {
            var tradingClient = _alpacaClient.GetCommonTradingClient();
            var order = await tradingClient.GetOrderAsync(orderId);
            return order.ToAlpacaOrder();
        }

        public async Task<bool> CancelOrderByIdAsync(Guid orderId)
        {
            var tradingClient = _alpacaClient.GetCommonTradingClient();
            return await tradingClient.CancelOrderAsync(orderId);
        }

        public async Task<AlpacaOrder> CreateOrderAsync(string symbol, OrderQuantity qty, OrderSide side, OrderType orderType, TimeInForce timeInForce)
        {
            var tradingClient = _alpacaClient.GetCommonTradingClient();
            var req = new NewOrderRequest(symbol, qty, side, orderType, timeInForce);
            var order = await tradingClient.PostOrderAsync(req);
            var alpacaOrder = order.ToAlpacaOrder();
            return alpacaOrder;
        }

        public async Task<List<AlpacaPosition>> GetAllOpenPositions()
        {
            var tradingClient = _alpacaClient.GetCommonTradingClient();
            var positions = await tradingClient.ListPositionsAsync();
            return positions.Select(p => p.ToAlpacaPosition()).ToList();
        }

        public async Task<AlpacaPosition> GetPositionsBySymbol(string symbol)
        {
            var tradingClient = _alpacaClient.GetCommonTradingClient();
            var pos = await tradingClient.GetPositionAsync(symbol);
            return pos.ToAlpacaPosition();
        }

        public async Task<AlpacaOrder> ClosePositionOrder(string symbol)
        {
            var tradingClient = _alpacaClient.GetCommonTradingClient();
            var deletePositionRequest = new DeletePositionRequest(symbol);
            var order = await tradingClient.DeletePositionAsync(deletePositionRequest);
            return order.ToAlpacaOrder();
        }
        public async Task<bool> CloseAllPositions()
        {
            var tradingClient = _alpacaClient.GetCommonTradingClient();
            var closed = await tradingClient.DeleteAllPositionsAsync();
            return true;
        }
    }
}
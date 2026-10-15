namespace BN.PROJECT.AlpacaService
{
    public class AlpacaTradingService(IAlpacaClient alpacaClient,
        ILogger<AlpacaTradingService> logger) : IAlpacaTradingService
    {
        private readonly ILogger<AlpacaTradingService> _logger = logger;
        private readonly IAlpacaTradingClient _tradingClient = alpacaClient.GetCommonTradingClient();

        public async Task<IAccount?> GetAccountAsync(UserSettingsModel userSettings)
        {
            try
            {
                return await _tradingClient.GetAccountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting account for user id {userSettings.UserId}");
            }
            return null;
        }

        public async Task<List<AlpacaOrder>> GetAllOrdersAsync(OrderStatusFilter orderStatusFilter)
        {
            var req = new ListOrdersRequest
            {
                OrderStatusFilter = orderStatusFilter
            };

            var orders = await _tradingClient.ListOrdersAsync(req);
            return orders.Select(order => order.ToAlpacaOrder()).ToList();
        }

        public async Task<AlpacaOrder> GetOrderByIdAsync(string orderId)
        {
            var order = await _tradingClient.GetOrderAsync(orderId);
            return order.ToAlpacaOrder();
        }

        public async Task<bool> CancelOrderByIdAsync(Guid orderId)
        {
            return await _tradingClient.CancelOrderAsync(orderId);
        }

        public async Task<IOrder?> CreateOrderAsync(OrderRequest orderRequest)
        {
            // check if market is open
            var clock = await _tradingClient.GetClockAsync();
            if (clock == null || !clock.IsOpen)
            {
                _logger.LogWarning($"Market is closed");
                return null;
            }

            var req = new NewOrderRequest(orderRequest.Asset, (int)orderRequest.Quantity, orderRequest.Side == SideEnum.Buy ? OrderSide.Buy : OrderSide.Sell, OrderType.Market, TimeInForce.Gtc);
            var order = await _tradingClient.PostOrderAsync(req);
            return order;
        }

        public async Task<List<AlpacaPosition>> GetAllOpenPositions()
        {
            var positions = await _tradingClient.ListPositionsAsync();
            return positions.Select(p => p.ToAlpacaPosition()).ToList();
        }

        public async Task<AlpacaPosition> GetPositionsBySymbol(string symbol)
        {
            var pos = await _tradingClient.GetPositionAsync(symbol);
            return pos.ToAlpacaPosition();
        }

        public async Task<AlpacaOrder> ClosePositionOrder(string symbol)
        {
            var deletePositionRequest = new DeletePositionRequest(symbol);
            var order = await _tradingClient.DeletePositionAsync(deletePositionRequest);
            return order.ToAlpacaOrder();
        }
        public async Task<bool> CloseAllPositions()
        {
            var closed = await _tradingClient.DeleteAllPositionsAsync();
            return true;
        }
    }
}
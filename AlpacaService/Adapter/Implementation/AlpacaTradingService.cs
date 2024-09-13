namespace BN.PROJECT.AlpacaService
{
    public class AlpacaTradingService : IAlpacaTradingService
    {
        private readonly IConfiguration _configuration;
        private readonly IAlpacaTradingClient _tradingClient;
        private readonly ILogger<AlpacaTradingService> _logger;

        public AlpacaTradingService(IConfiguration iConfig, ILogger<AlpacaTradingService> logger)
        {
            _configuration = iConfig;
            _logger = logger;
            _tradingClient = GetAlpacaTradingClient();
        }

        private IAlpacaTradingClient GetAlpacaTradingClient()
        {
            var alpacaId = _configuration.GetValue<string>("Alpaca:KEY_ID") ?? string.Empty;
            var alpacaSecret = _configuration.GetValue<string>("Alpaca:SECRET_KEY") ?? string.Empty;
            return Alpaca.Markets.Environments.Paper.GetAlpacaTradingClient(new SecretKey(alpacaId, alpacaSecret));
        }

        public async Task<IAccount> GetAccountAsync()
        {
            return await _tradingClient.GetAccountAsync();
        }

        public async Task<List<AlpacaAsset>> GetAssetsAsync()
        {
            var assets = await _tradingClient.ListAssetsAsync(new AssetsRequest());
            return assets.Select(a => a.ToAlpacaAsset()).ToList();
        }

        public async Task<IAsset> GetAssetBySymbolAsync(string symbol)
        {
            return await _tradingClient.GetAssetAsync(symbol);
        }

        public async Task<List<AlpacaOrder>> GetAllOrdersAsync(OrderStatusFilter orderStatusFilter)
        {
            var req = new ListOrdersRequest
            {
                OrderStatusFilter = orderStatusFilter
            };

            var orders = await _tradingClient.ListOrdersAsync(req);
            return orders.Select(order => order.ToAlpacaOrder()).ToList(); ;
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

        public async Task<AlpacaOrder> CreateOrderAsync(string symbol, OrderQuantity qty, OrderSide side, OrderType orderType, TimeInForce timeInForce)
        {
            var req = new NewOrderRequest(symbol, qty, side, orderType, timeInForce);
            var order = await _tradingClient.PostOrderAsync(req);
            var alpacaOrder = order.ToAlpacaOrder();
            return alpacaOrder;
        }

        public async Task<List<AlpacaPosition>> GetAllOpenPositions()
        {
            var positions = await _tradingClient.ListPositionsAsync();
            return positions.Select(p => p.ToAlpacaPosition()).ToList(); ;
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
    }
}
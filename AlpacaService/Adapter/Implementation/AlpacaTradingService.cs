namespace BN.PROJECT.AlpacaService
{
    public class AlpacaTradingService : IAlpacaTradingService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AlpacaTradingService> _logger;
        private readonly IAlpacaRepository _alpacaRepository;

        public AlpacaTradingService(IConfiguration iConfig,
            ILogger<AlpacaTradingService> logger,
            IAlpacaRepository alpacaRepository)
        {
            _configuration = iConfig;
            _logger = logger;
            _alpacaRepository = alpacaRepository;
        }

        private IAlpacaTradingClient GetCommonTradingClient()
        {
            var alpacaId = _configuration.GetValue<string>("Alpaca:KEY_ID") ?? string.Empty;
            var alpacaSecret = _configuration.GetValue<string>("Alpaca:SECRET_KEY") ?? string.Empty;
            return Alpaca.Markets.Environments.Paper.GetAlpacaTradingClient(new SecretKey(alpacaId, alpacaSecret));
        }
        private IAlpacaTradingClient? GetPrivateTradingClient(UserSettings userSettings)
        {
            var alpacaId = userSettings.AlpacaKey;
            var alpacaSecret = userSettings.AlpacaSecret;
            return Alpaca.Markets.Environments.Paper.GetAlpacaTradingClient(new SecretKey(alpacaId, alpacaSecret));
        }

        public async Task<IAccount?> GetAccountAsync(UserSettings userSettings)
        {
            try
            {
                var tradingClient = GetPrivateTradingClient(userSettings);
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
            var tradingClient = GetCommonTradingClient();
            var assets = await tradingClient.ListAssetsAsync(new AssetsRequest());
            return assets.Select(a => a.ToAlpacaAsset()).ToList();
        }

        public async Task<IAsset> GetAssetBySymbolAsync(string symbol)
        {
            var tradingClient = GetCommonTradingClient();
            return await tradingClient.GetAssetAsync(symbol);
        }

        public async Task<List<AlpacaOrder>> GetAllOrdersAsync(string userId, OrderStatusFilter orderStatusFilter)
        {
            var tradingClient = GetCommonTradingClient();
            var req = new ListOrdersRequest
            {
                OrderStatusFilter = orderStatusFilter
            };

            var orders = await tradingClient.ListOrdersAsync(req);
            return orders.Select(order => order.ToAlpacaOrder()).ToList();
        }

        public async Task<AlpacaOrder> GetOrderByIdAsync(string userId, string orderId)
        {
            var tradingClient = GetCommonTradingClient();
            var order = await tradingClient.GetOrderAsync(orderId);
            return order.ToAlpacaOrder();
        }

        public async Task<bool> CancelOrderByIdAsync(string userId, Guid orderId)
        {
            var tradingClient = GetCommonTradingClient();
            return await tradingClient.CancelOrderAsync(orderId);
        }

        public async Task<AlpacaOrder> CreateOrderAsync(string userId, string symbol, OrderQuantity qty, OrderSide side, OrderType orderType, TimeInForce timeInForce)
        {
            var tradingClient = GetCommonTradingClient();
            var req = new NewOrderRequest(symbol, qty, side, orderType, timeInForce);
            var order = await tradingClient.PostOrderAsync(req);
            var alpacaOrder = order.ToAlpacaOrder();
            return alpacaOrder;
        }

        public async Task<List<AlpacaPosition>> GetAllOpenPositions(string userId)
        {
            var tradingClient = GetCommonTradingClient();
            var positions = await tradingClient.ListPositionsAsync();
            return positions.Select(p => p.ToAlpacaPosition()).ToList();
        }

        public async Task<AlpacaPosition> GetPositionsBySymbol(string userId, string symbol)
        {
            var tradingClient = GetCommonTradingClient();
            var pos = await tradingClient.GetPositionAsync(symbol);
            return pos.ToAlpacaPosition();
        }

        public async Task<AlpacaOrder> ClosePositionOrder(string userId, string symbol)
        {
            var tradingClient = GetCommonTradingClient();
            var deletePositionRequest = new DeletePositionRequest(symbol);
            var order = await tradingClient.DeletePositionAsync(deletePositionRequest);
            return order.ToAlpacaOrder();
        }
    }
}

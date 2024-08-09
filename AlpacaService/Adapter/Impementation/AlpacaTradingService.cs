namespace BN.TRADER.AlpacaService
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

        public async Task<List<IAsset>> GetAssetsAsync()
        {
            var assets = await _tradingClient.ListAssetsAsync(new AssetsRequest());
            return assets.ToList();
        }

        public async Task<IAsset> GetAssetBySymbolAsync(string symbol)
        {
            return await _tradingClient.GetAssetAsync(symbol);
        }

        public async Task<List<IOrder>> GetAllOrdersAsync()
        {
            return (List<IOrder>)await _tradingClient.ListOrdersAsync(new ListOrdersRequest());
        }

        public async Task<IOrder> GetOrderByIdAsync(string orderId)
        {
            return await _tradingClient.GetOrderAsync(orderId);
        }

        public async Task<bool> CancelOrderByIdAsync(Guid orderId)
        {
            return await _tradingClient.CancelOrderAsync(orderId);
        }

        public async Task<IOrder> CreateOrderAsync(string symbol, OrderQuantity qty, OrderSide side, OrderType orderType, TimeInForce timeInForce)
        {
            var order = await _tradingClient.PostOrderAsync(new NewOrderRequest(symbol, qty, side, orderType, timeInForce));
            return order;
        }

        public async Task<List<IPosition>> GetPositions()
        {
            var positions = await _tradingClient.ListPositionsAsync();
            return positions.ToList();
        }

        public async Task<IPosition> GetPositionsBySymbol(string symbol)
        {
            return await _tradingClient.GetPositionAsync(symbol);
        }

        public async Task<IOrder> ClosePosition(string symbol)
        {
            var deletePositionRequest = new DeletePositionRequest(symbol);

            return await _tradingClient.DeletePositionAsync(deletePositionRequest);
        }
    }
}
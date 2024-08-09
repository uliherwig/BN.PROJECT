namespace BN.TRADER.AlpacaService
{
    public class AlpacaMarketsService : IAlpacaMarketsService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AlpacaMarketsService> _logger;

        public AlpacaMarketsService(IConfiguration iConfig, ILogger<AlpacaMarketsService> logger)
        {
            _configuration = iConfig;
            _logger = logger;
        }

        private IAlpacaTradingClient GetAlpacaTradingClient()
        {
            var alpacaId = _configuration.GetValue<string>("Alpaca:KEY_ID") ?? string.Empty;
            var alpacaSecret = _configuration.GetValue<string>("Alpaca:SECRET_KEY") ?? string.Empty;
            return Alpaca.Markets.Environments.Paper.GetAlpacaTradingClient(new SecretKey(alpacaId, alpacaSecret));
        }

        private IAlpacaDataClient GetAlpacaDataClient()
        {
            var alpacaId = _configuration.GetValue<string>("Alpaca:KEY_ID") ?? string.Empty;
            var alpacaSecret = _configuration.GetValue<string>("Alpaca:SECRET_KEY") ?? string.Empty;
            return Alpaca.Markets.Environments.Paper.GetAlpacaDataClient(new SecretKey(alpacaId, alpacaSecret));
        }

        public async Task<List<BnAsset>?> GetAssetsAsync()
        {
            var client = GetAlpacaTradingClient();
            var assets = await client.ListAssetsAsync(new AssetsRequest());
            return assets?.Where(a => a.Shortable).Select(a => new BnAsset { Symbol = a.Symbol, Name = a.Name }).ToList();
        }

        public async Task<IAsset> GetAssetBySymbolAsync(string symbol)
        {
            var client = GetAlpacaTradingClient();
            return await client.GetAssetAsync(symbol);
        }

        public async Task<List<BnOhlc>> GetHistoricalBarsBySymbol(string symbol, DateTime startDate, DateTime endDate)
        {
            var client = GetAlpacaDataClient();

            var into = DateTime.Now;
            var from = into.Subtract(TimeSpan.FromMinutes(25));

            var test = new HistoricalBarsRequest(symbol, from, into, BarTimeFrame.Minute);
            test.Feed = MarketDataFeed.Iex;
            var barSet = await client.ListHistoricalBarsAsync(test);
            var bars = barSet.Items;

            // Print the historical data
            foreach (var bar in bars)
            {
                _logger.LogInformation($"Time: {bar.TimeUtc}, Open: {bar.Open}, High: {bar.High}, Low: {bar.Low}, Close: {bar.Close}, Volume: {bar.Volume}");
            }

            return bars.Select(a => new BnOhlc { Time = a.TimeUtc, Open = a.Open, High = a.High, Low = a.Low, Close = a.Close, Volume = a.Volume }).ToList();
        }

        public async Task CreateOrderAsync()
        {
            var client = GetAlpacaTradingClient();
            var order = await client.PostOrderAsync(new NewOrderRequest("SPY", 1, OrderSide.Buy, OrderType.Market, TimeInForce.Day));
        }

        public async Task<IAccount> GetAccountAsync()
        {
            var client = GetAlpacaTradingClient();
            return await client.GetAccountAsync();
        }
    }
}
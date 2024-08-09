using RestSharp;
using System.Net;

namespace BN.TRADER.AlpacaService
{
    public class AlpacaService : IAlpacaService
    {
        private IConfiguration _configuration;
        private readonly ILogger<AlpacaService> _logger;

        public AlpacaService(IConfiguration iConfig, ILogger<AlpacaService> logger)
        {
            _configuration = iConfig;
            _logger = logger;
        }

        // get all orders
        public async Task<List<IOrder>> GetAllOrdersAsync()
        {
            var request = GetRequest();
            var client = GetRestClient("orders");
            var response = await client.GetAsync(request);
            _logger.LogInformation(response.Content);
            return JsonConvert.DeserializeObject<List<IOrder>>(response.Content);
        }

        // get order by id
        public async Task<IOrder> GetOrderByIdAsync(string orderId)
        {
            var request = GetRequest();
            var client = GetRestClient($"orders/{orderId}");
            var response = await client.GetAsync(request);
            _logger.LogInformation(response.Content);
            return JsonConvert.DeserializeObject<IOrder>(response.Content);
        }

        // delete order by id
        public async Task DeleteOrderByIdAsync(string orderId)
        {
            var request = GetRequest();
            var client = GetRestClient($"orders/{orderId}");
            var response = await client.DeleteAsync(request);
            _logger.LogInformation(response.Content);
        }

        // replace order by id
        public async Task ReplaceOrderByIdAsync(string orderId)
        {
            var request = GetRequest();
            var client = GetRestClient($"orders/{orderId}");
            request.AddJsonBody("{\"qty\":\"2\"}", false);
            var response = await client.PatchAsync(request);
            _logger.LogInformation(response.Content);
        }

        // create alpaca order
        public async Task CreateOrderAsync()
        {
            var request = GetRequest();
            var client = GetRestClient("orders");

            request.AddJsonBody("{\"symbol\":\"SPY\",\"qty\":\"1\",\"side\":\"buy\",\"type\":\"market\",\"time_in_force\":\"day\"}", false);
            var response = await client.PostAsync(request);
            _logger.LogInformation(response.Content);
        }

        private RestClient GetRestClient(string parameters)
        {
            var alpacaSecret = _configuration.GetValue<string>("Alpaca:SECRET_KEY") ?? string.Empty;

            var url = $"{_configuration.GetValue<string>("Alpaca:BASE_URL")}/{parameters}";
            var options = new RestClientOptions(url);
            var client = new RestClient(options);

            return client;
        }

        private RestRequest GetRequest()
        {
            var alpacaId = _configuration.GetValue<string>("Alpaca:KEY_ID") ?? string.Empty;
            var alpacaSecret = _configuration.GetValue<string>("Alpaca:SECRET_KEY") ?? string.Empty;

            var request = new RestRequest("");
            request.AddHeader("accept", "application/json");
            request.AddHeader("APCA-API-KEY-ID", alpacaId);
            request.AddHeader("APCA-API-SECRET-KEY", alpacaSecret);

            var client = Alpaca.Markets.Environments.Paper
             .GetAlpacaDataStreamingClient(new SecretKey(alpacaId, alpacaSecret));

            return request;
        }

        public Task DeleteAllOrdersAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<List<BnOhlc>> GetHistoricalBarsBySymbol(string symbol, string timeFrame, DateTime startDate, DateTime endDate)
        {
            string start = WebUtility.UrlEncode(startDate.ToString("yyyy-MM-ddTHH:mm:ssZ"));
            string end = WebUtility.UrlEncode(endDate.ToString("yyyy-MM-ddTHH:mm:ssZ"));
            string url = $"https://data.alpaca.markets/v2/stocks/bars?symbols={symbol}&timeframe={timeFrame}&start={start}&end={end}&limit=1000&adjustment=raw&feed=iex&sort=asc";

            var options = new RestClientOptions(url);
            var client = new RestClient(options);
            var request = GetRequest();
            var response = await client.GetAsync(request);

            _logger.LogInformation(response.Content);

            var data = JsonConvert.DeserializeObject<BarsModel>(response.Content);

            return data.Bars[symbol].Select(a => new BnOhlc { Time = a.T, Open = a.O, High = a.H, Low = a.L, Close = a.C, Volume = a.V }).ToList();
        }

        public async Task<string> GetHistoricalQuotesBySymbol(string symbol)
        {
            string url = "https://data.alpaca.markets/v2/stocks/SPY/quotes?start=2022-01-03T00%3A00%3A00Z&end=2022-01-04T00%3A00%3A00Z&feed=sip&sort=asc";

            var options = new RestClientOptions(url);
            var client = new RestClient(options);
            var request = GetRequest();
            var response = await client.GetAsync(request);

            _logger.LogInformation(response.Content);

            var data = JsonConvert.DeserializeObject<BarsModel>(response.Content);

            return response.Content.ToString();
        }
    }
}
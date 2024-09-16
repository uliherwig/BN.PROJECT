namespace BN.PROJECT.AlpacaService
{

    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution]
    public class AlpacaHistoryJob : IJob
    {
        private readonly ILogger<AlpacaHistoryJob> _logger;
        private readonly IConfiguration _configuration;
        private readonly IAlpacaDataService _alpacaDataService;
        private readonly DataServiceClient _dataServiceClient;

        public AlpacaHistoryJob(
            ILogger<AlpacaHistoryJob> logger,
            IConfiguration configuration,
            IAlpacaDataService alpacaDataService,
            DataServiceClient dataServiceClient)
        {
            _logger = logger;
            _configuration = configuration;
            _alpacaDataService = alpacaDataService;
            _dataServiceClient = dataServiceClient;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            JobKey key = context.JobDetail.Key;

            _logger.LogInformation("Instance " + key + " History Job start");

            var assetsAsString = _configuration.GetValue<string>("Alpaca:TRADED_ASSETS") ?? string.Empty;
            if (string.IsNullOrEmpty(assetsAsString))
                throw new Exception("No assets defined in configuration");

            var assetsSelection = assetsAsString.Split(",").ToList();

            foreach (var symbol in assetsSelection)
            {
                _logger.LogInformation("Asset: " + symbol);

                var latestBarFromDb = await _dataServiceClient.GetLatestBar(symbol);
                var latestBarFromAlpaca = await _alpacaDataService.GetLatestBarBySymbol(symbol);

                var startDate = latestBarFromDb == null ? new DateTime(2024, 1, 1) : latestBarFromDb.T;

                while (startDate < latestBarFromAlpaca.T)
                {
                    var endDate = startDate.AddDays(1);
                    var bars = await _alpacaDataService.GetHistoricalBarsBySymbol(symbol, startDate, endDate, BarTimeFrame.Minute);
                    _logger.LogInformation("Start: " + startDate.ToString() + "  Bars: " + bars.Count);

                    if (bars.Count > 0)
                    {
                        await _dataServiceClient.AddBarsAsync(bars);
                    }

                    startDate = startDate.AddDays(1);
                }
            }

            _logger.LogInformation("Instance " + key + " History Job end");
        }
    }
}
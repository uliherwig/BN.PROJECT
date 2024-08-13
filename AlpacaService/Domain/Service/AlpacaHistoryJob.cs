using static System.Runtime.InteropServices.JavaScript.JSType;

[PersistJobDataAfterExecution]
[DisallowConcurrentExecution]
public class AlpacaHistoryJob : IJob
{
    private readonly ILogger<AlpacaHistoryJob> _logger;
    private readonly IConfiguration _configuration;
    private readonly IAlpacaDataService _alpacaDataService;
    private readonly IDbRepository _dbRepository;


    public AlpacaHistoryJob(
        ILogger<AlpacaHistoryJob> logger,
        IConfiguration configuration,
        IAlpacaDataService alpacaDataService,
        IDbRepository dbRepository)
    {
        _logger = logger;
        _configuration = configuration;
        _alpacaDataService = alpacaDataService;
        _dbRepository = dbRepository;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        JobKey key = context.JobDetail.Key;

        var assetsAsString = _configuration.GetValue<string>("Alpaca:TRADED_ASSETS") ?? string.Empty;
        if (string.IsNullOrEmpty(assetsAsString))
            throw new Exception("No assets defined in configuration");


        var assets = assetsAsString.Split(",").ToList();

        foreach (var symbol in assets)
        {
            _logger.LogInformation("Asset: " + symbol);

            var latestBarFromDb = await _dbRepository.GetLatestBar(symbol);
            var latestBarFromAlpaca = await _alpacaDataService.GetLatestBarBySymbol(symbol);

            var startDate = latestBarFromDb == null ? new DateTime(2024, 1, 1) : latestBarFromDb.T;

            while (startDate < latestBarFromAlpaca.T)
            {
                var endDate = startDate.AddDays(1);
                var bars = await _alpacaDataService.GetHistoricalBarsBySymbol(symbol, startDate, endDate, BarTimeFrame.Minute);
                _logger.LogInformation("Start: " + startDate.ToString() + "  Bars: " + bars.Count);
                
                await _dbRepository.AddBarsAsync(bars);

                startDate = startDate.AddDays(1);
            }
        }

        _logger.LogInformation("Instance " + key + " History Job end");


    }
}

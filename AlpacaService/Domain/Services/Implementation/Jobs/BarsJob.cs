namespace BN.PROJECT.AlpacaService;

[PersistJobDataAfterExecution]
[DisallowConcurrentExecution]
public class BarsJob : IJob
{
    private readonly ILogger<BarsJob> _logger;
    private readonly IConfiguration _configuration;
    private readonly IAlpacaDataService _alpacaDataService;
    private readonly IAlpacaRepository _alpacaRepository;

    public BarsJob(
        ILogger<BarsJob> logger,
        IConfiguration configuration,
        IAlpacaDataService alpacaDataService,
        IAlpacaRepository alpacaRepository)
    {
        _logger = logger;
        _configuration = configuration;
        _alpacaDataService = alpacaDataService;
        _alpacaRepository = alpacaRepository;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        JobKey key = context.JobDetail.Key;       

        var assetsAsString = _configuration.GetValue<string>("Alpaca:TRADED_ASSETS") ?? string.Empty;
        var assetsSelection = assetsAsString.Split(",").ToList();

        await UpdateHistoricalBars(assetsSelection);
        _logger.LogInformation("Instance " + key + " History Job end");
    }
    private async Task UpdateHistoricalBars(List<string> assetsSelection)
    {
        foreach (var symbol in assetsSelection)
        {
            _logger.LogInformation("UpdateHistoricalBars Asset: " + symbol);

            var latestBarFromDb = await _alpacaRepository.GetLatestBar(symbol);
            if (latestBarFromDb == null)
            {
                _logger.LogInformation("UpdateHistoricalBars Asset: " + symbol + " no latestBar ");
            }

            var startDate = latestBarFromDb == null ? new DateTime(2024, 1, 1) : latestBarFromDb.T;

            while (startDate < DateTime.UtcNow)
            {
                var endDate = startDate.AddDays(1);
                var bars = await _alpacaDataService.GetHistoricalBarsBySymbol(symbol, startDate, endDate, BarTimeFrame.Minute);
                _logger.LogInformation("UpdateHistoricalBars: " + startDate.ToString() + "  Bars: " + bars.Count);

                if (bars.Count > 0)
                {
                    await _alpacaRepository.AddBarsAsync(bars);
                }
                startDate = startDate.AddDays(1);
            }
        }
        _logger.LogInformation("UpdateHistoricalBars: DONE ");
    }

}
namespace BN.PROJECT.AlpacaService;

[PersistJobDataAfterExecution]
[DisallowConcurrentExecution]
public class CalendarJob : IJob
{
    private readonly ILogger<CalendarJob> _logger;
    private readonly IConfiguration _configuration;
    private readonly IAlpacaDataService _alpacaDataService;
    private readonly IAlpacaTradingService _alpacaTradingService;
    private readonly IAlpacaRepository _alpacaRepository;

    public CalendarJob(
        ILogger<CalendarJob> logger,
        IConfiguration configuration,
        IAlpacaDataService alpacaDataService,
        IAlpacaTradingService alpacaTradingService,
        IAlpacaRepository alpacaRepository)
    {
        _logger = logger;
        _configuration = configuration;
        _alpacaDataService = alpacaDataService;
        _alpacaTradingService = alpacaTradingService;
        _alpacaRepository = alpacaRepository;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        JobKey key = context.JobDetail.Key;

        var executionEnabled = _configuration.GetValue<bool>("CalendarEnabled:Enabled");
        if (!executionEnabled)
        {
            return;
        }

        var assetsAsString = _configuration.GetValue<string>("Alpaca:TRADED_ASSETS") ?? string.Empty;
        if (string.IsNullOrEmpty(assetsAsString))
            throw new Exception("No assets defined in configuration");



        var assetsSelection = assetsAsString.Split(",").ToList();
        _logger.LogInformation("Instance " + key + " Calendar Job start");

        await UpdateAssets(assetsSelection);

        await UpdateCalendar();

        _logger.LogInformation("Instance " + key + " Calendar Job end");
    }

    private async Task UpdateCalendar()
    {
        var startDate = new DateOnly(2024, 1, 1);
        // get the date of the end of the current month
        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        DateOnly endOfMonth = new DateOnly(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));

        var lastCalendarFromDb = await _alpacaRepository.GetLatestCalendar();
        if (lastCalendarFromDb != null)
        {
            startDate = lastCalendarFromDb.TradingDate.AddDays(1);
        }
        while (startDate < endOfMonth)
        {
            var endDate = startDate.AddMonths(1);

            var calendarList = await _alpacaTradingService.ListIntervalCalendarAsync(startDate, endDate);

            if (calendarList != null && calendarList.Count > 0)
            {
                await _alpacaRepository.AddCalendarAsync(calendarList);
            }
            startDate = endDate;
        }
    }

    private async Task UpdateAssets(List<string> assetsSelection)
    {
        var assets = await _alpacaTradingService.GetAssetsAsync();
        var assetsDb = await _alpacaRepository.GetAssets();
        var alpacaAssets = new List<AlpacaAsset>();
        foreach (var symbol in assetsSelection)
        {
            if (assetsDb.Any(a => a.Symbol == symbol))
            {                
                continue;
            }
            var asset = assets.FirstOrDefault(a => a.Symbol == symbol);
            if (asset == null)
            {
                continue;
            }
            var alpacaAsset = new AlpacaAsset
            {
                Name = asset.Name,
                Symbol = symbol
            };
            alpacaAssets.Add(alpacaAsset);            
        }
        if (alpacaAssets.Count > 0)
        {
            await _alpacaRepository.AddAssetsAsync(alpacaAssets);
        }
    }

}
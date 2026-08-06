namespace BN.PROJECT.AlpacaService;

[PersistJobDataAfterExecution]
[DisallowConcurrentExecution]
public class TradesJob : IJob
{
    private readonly ILogger<TradesJob> _logger;
    private readonly IConfiguration _configuration;
    private readonly IAlpacaDataService _alpacaDataService;
    private readonly IAlpacaTradingService _alpacaTradingService;
    private readonly IAlpacaRepository _alpacaRepository;

    public TradesJob(
        ILogger<TradesJob> logger,
        IConfiguration configuration,
        IAlpacaDataService alpacaDataService,
        IAlpacaTradingService alpacaTradingService,
        IAlpacaRepository alpacaRepository)
    {
        _logger = logger;
        _configuration = configuration;
        _alpacaDataService = alpacaDataService;
        _alpacaRepository = alpacaRepository;
        _alpacaTradingService = alpacaTradingService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        JobKey key = context.JobDetail.Key;
        var assetsAsString = _configuration.GetValue<string>("Alpaca:TRADED_ASSETS") ?? string.Empty;
        var assetsSelection = assetsAsString.Split(",").ToList();
        await UpdateHistoricalTrades(assetsSelection);
    }
    private async Task UpdateHistoricalTrades(List<string> assetsSelection)
    {
        var calendar = await _alpacaRepository.GetCalendarAsync();
        if (calendar == null || calendar.Count == 0)
        {
            _logger.LogError("No calendar data found in the database.");
            return;
        }
        foreach (var symbol in assetsSelection)
        {
            _logger.LogInformation("UpdateHistoricalTrades Asset: " + symbol);

            var latestTradeFromDb = await _alpacaRepository.GetLatestTrade(symbol);


            var startDate = latestTradeFromDb == null ? new DateTime(2024, 1, 1) : latestTradeFromDb.TimestampUtc;
            var endDate = DateTime.UtcNow;

            while (startDate < endDate)
            {
                DateOnly currentDate = DateOnly.FromDateTime(startDate);
                TimeSpan currentTime = startDate.TimeOfDay;
                var calendarEntry = calendar.FirstOrDefault(c => c.TradingDate == currentDate);
                if (calendarEntry == null)
                {
                    // Skip to the next day if the market is closed
                    startDate = startDate.AddDays(1).Date;
                    continue;
                }
                var intervalDate = startDate.AddMinutes(10);
                if (currentTime < calendarEntry.SessionOpen || currentTime >= calendarEntry.SessionClose)
                {
                    startDate = intervalDate;
                    continue;
                }

                
                var trades = await _alpacaDataService.GetTradesBySymbol(symbol, startDate, intervalDate);

                if (trades.Count > 0)
                {
                    await _alpacaRepository.AddTradesAsync(trades);
                }
                startDate = intervalDate;

            }

        }
    }
}
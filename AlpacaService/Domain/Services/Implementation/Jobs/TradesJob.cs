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
        DateOnly startDate = DateOnly.Parse("2026-09-04");
        var calendar = await _alpacaRepository.GetCalendarAsync(startDate);
        if (calendar == null || calendar.Count == 0)
        {
            _logger.LogError("No calendar data found in the database.");
            return;
        }
        // TradingOpen/TradingClose are stored in Eastern Time; convert to UTC to compare against stamp (UTC).
        var easternZone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
        var tradingDate = calendar.First().TradingDate;
        var tradingOpen = TimeZoneInfo.ConvertTimeToUtc(
            tradingDate.ToDateTime(TimeOnly.FromTimeSpan(calendar.First().TradingOpen), DateTimeKind.Unspecified),
            easternZone).TimeOfDay;
        var tradingClose = TimeZoneInfo.ConvertTimeToUtc(
            tradingDate.ToDateTime(TimeOnly.FromTimeSpan(calendar.First().TradingClose), DateTimeKind.Unspecified),
            easternZone).TimeOfDay;
        foreach (var symbol in new[] { "SPY" })
        {
            _logger.LogInformation("UpdateHistoricalTrades Asset: " + symbol);

            var latestTradeFromDb = await _alpacaRepository.GetLatestTrade(symbol);
      
            var stamp = latestTradeFromDb == null ? startDate.ToDateTime(TimeOnly.MinValue) : latestTradeFromDb.TimestampUtc;
            var endDate = DateTime.UtcNow;

            while (stamp < endDate)
            {
                var intervalStamp = stamp.AddSeconds(5);

                if (stamp.TimeOfDay < tradingOpen || stamp.TimeOfDay >= tradingClose)
                {
                    stamp = intervalStamp;
                    continue;
                }
                var trades = await _alpacaDataService.GetTradesBySymbol(symbol, stamp, intervalStamp);

                if (trades.Count > 0)
                {
                    await _alpacaRepository.AddTradesAsync(trades);
                }
                stamp = intervalStamp;
            }
        }
    }
}
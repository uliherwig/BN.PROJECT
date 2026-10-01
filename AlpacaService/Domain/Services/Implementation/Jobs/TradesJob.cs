using System.Globalization;

namespace BN.PROJECT.AlpacaService;

[PersistJobDataAfterExecution]
[DisallowConcurrentExecution]
public class TradesJob : IJob
{
    private readonly ILogger<TradesJob> _logger;
    private readonly IConfiguration _configuration;
    private readonly IAlpacaDataService _alpacaDataService;
    private readonly IAlpacaRepository _alpacaRepository;
    private readonly IRedisStreamPublisher _redisStreamPublisher;
    private readonly IRedisService _redisService;

    public TradesJob(
        ILogger<TradesJob> logger,
        IConfiguration configuration,
        IAlpacaDataService alpacaDataService,
        IAlpacaRepository alpacaRepository,
        IRedisStreamPublisher redisStreamPublisher,
        IRedisService redisService)
    {
        _logger = logger;
        _configuration = configuration;
        _alpacaDataService = alpacaDataService;
        _alpacaRepository = alpacaRepository;
        _redisStreamPublisher = redisStreamPublisher;
        _redisService = redisService;
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
        assetsSelection = new[] { "SPY" }.ToList();
        // get the last 3 trading days from calendar
        var testDate = DateTime.UtcNow.AddDays(-10);
        var yesterday = DateTime.UtcNow.AddDays(-1);
        var calendar = await _alpacaRepository.GetCalendarAsync(DateOnly.FromDateTime(testDate), DateOnly.FromDateTime(yesterday));
        if (calendar == null || calendar.Count == 0)
        {
            return;
        }
        // find the last trading day from the calendar
        calendar = calendar.OrderByDescending(c => c.TradingDate).Take(1).ToList();

        var lastTrade = await _alpacaRepository.GetLatestTrade("SPY");

        if (lastTrade != null && DateOnly.FromDateTime(lastTrade.TimestampUtc) == calendar.First().TradingDate)
        {
            return;
        }

        foreach (var day in calendar)
        {
            _logger.LogInformation("Trading day: {TradingDate}", day.TradingDate);

            // TradingOpen/TradingClose are stored in Eastern Time; convert to UTC to compare against stamp (UTC).
            var easternZone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
            var tradingDate = day.TradingDate;
            var tradingOpen = TimeZoneInfo.ConvertTimeToUtc(
                tradingDate.ToDateTime(TimeOnly.FromTimeSpan(day.SessionOpen), DateTimeKind.Unspecified),
                easternZone);
            var tradingClose = TimeZoneInfo.ConvertTimeToUtc(
                tradingDate.ToDateTime(TimeOnly.FromTimeSpan(day.SessionClose), DateTimeKind.Unspecified),
                easternZone);


            foreach (var symbol in assetsSelection)
            {

               
                var stamp = tradingOpen;

                // iterate through each minute of the trading day from tradingOpen to tradingClose

                while (stamp < tradingClose)
                {
                    var intervalStamp = stamp.AddMinutes(5);

                    if (stamp < tradingOpen || stamp > tradingClose)
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


                // var stamp = calendar.First().TradingDate.ToDateTime(TimeOnly.FromTimeSpan(day.TradingOpen), DateTimeKind.Unspecified);
                // stamp = TimeZoneInfo.ConvertTimeToUtc(stamp, easternZone);

                // var intervalStamp = stamp.AddMinutes(1);

                // if (stamp.TimeOfDay < tradingOpen || stamp.TimeOfDay > tradingClose)
                // {
                //     continue;
                // }
                // var trades = await _alpacaDataService.GetTradesBySymbol(symbol, stamp, intervalStamp);

                // if (trades.Count > 0)
                // {
                //     await _alpacaRepository.AddTradesAsync(trades);

                // }
            }








        }






    }

}
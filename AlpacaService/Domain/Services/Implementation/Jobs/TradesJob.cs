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
        assetsSelection = new[] { "SPY" }.ToList();
        await UpdateHistoricalTrades(assetsSelection);
    }
    private async Task UpdateHistoricalTrades(List<string> assetsSelection)
    {
        var pushTradesToStream = await GetPushTradesToStreamFlagAsync();

        var now = DateTime.UtcNow;
        var interval = TimeSpan.FromSeconds(6);

        var calendar = await _alpacaRepository.GetCalendarAsync(DateOnly.FromDateTime(now));
        if (calendar == null || calendar.Count == 0)
        {
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

        foreach (var symbol in assetsSelection)
        {
            var stamp = now.Add(interval.Negate());

            var intervalStamp = stamp.Add(interval);

            if (stamp.TimeOfDay < tradingOpen || stamp.TimeOfDay > tradingClose)
            {        
                continue;
            }
            var trades = await _alpacaDataService.GetTradesBySymbol(symbol, stamp, intervalStamp);

            if (trades.Count > 0)
            {
                await _alpacaRepository.AddTradesAsync(trades);

                if (pushTradesToStream)
                {
                    await PublishTradesToStream(symbol, trades, 100000);
                }
            } 
        }
    }

    private async Task<bool> GetPushTradesToStreamFlagAsync()
    {
        // Redis flag takes priority so ops can toggle streaming without a redeploy; appsettings is the fallback default.
        var flagKey = RedisUtilities.GetFeatureFlagKey("push-trades-to-stream");
        var flagValue = await _redisService.GetStringAsync(flagKey);
        return bool.TryParse(flagValue, out var parsed) && parsed;
    }

    private async Task PublishTradesToStream(string symbol, List<AlpacaTrade> trades, int? maxLength)
    {
        var streamKey = RedisUtilities.GetTradesStreamKey(symbol);
        foreach (var trade in trades)
        {
            var fields = new[]
            {
                new NameValueEntry("symbol", trade.Symbol),
                new NameValueEntry("timestampUtc", trade.TimestampUtc.ToString("O", CultureInfo.InvariantCulture)),
                new NameValueEntry("price", trade.Price.ToString(CultureInfo.InvariantCulture)),
                new NameValueEntry("size", trade.Size.ToString(CultureInfo.InvariantCulture)),
                new NameValueEntry("exchange", trade.Exchange),
                new NameValueEntry("tape", trade.Tape),
                new NameValueEntry("tradeId", trade.TradeId.ToString(CultureInfo.InvariantCulture)),
            };

            try
            {
                await _redisStreamPublisher.AddAsync(streamKey, fields, maxLength);
            }
            catch (Exception e)
            {
                // Stream push is a best-effort side channel; DB persistence above must not be affected by Redis failures.
                _logger.LogError(e, "Failed to push trade {TradeId} for {Symbol} to Redis stream", trade.TradeId, symbol);
            }
        }
    }
}
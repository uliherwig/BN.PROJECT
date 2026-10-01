namespace BN.PROJECT.AlpacaService;

[PersistJobDataAfterExecution]
[DisallowConcurrentExecution]
public class EndOfDayLiquidationJob : IJob
{
    private readonly ILogger<EndOfDayLiquidationJob> _logger;
    private readonly IConfiguration _configuration;
    private readonly IAlpacaDataService _alpacaDataService;
    private readonly IPositionLifecycleService _positionLifecycleService;
    private readonly IRedisService _redisService;

    public EndOfDayLiquidationJob(
        ILogger<EndOfDayLiquidationJob> logger,
        IConfiguration configuration,
        IAlpacaDataService alpacaDataService,
        IPositionLifecycleService positionLifecycleService,
        IRedisService redisService)
    {
        _logger = logger;
        _configuration = configuration;
        _alpacaDataService = alpacaDataService;
        _positionLifecycleService = positionLifecycleService;
        _redisService = redisService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var bufferMinutes = _configuration.GetValue<int>("PositionManagement:EodBufferMinutes");

        var clock = await _alpacaDataService.GetClockAsync();
        if (!clock.IsOpen)
        {
            return;
        }

        var minutesToClose = (clock.NextCloseUtc - DateTime.UtcNow).TotalMinutes;
        if (minutesToClose > bufferMinutes)
        {
            return;
        }

        // Guard against re-running within the same close window if the job fires multiple times.
        var flagKey = RedisUtilities.GetFeatureFlagKey($"eod-liquidation-{DateOnly.FromDateTime(DateTime.UtcNow)}");
        var alreadyRun = await _redisService.GetStringAsync(flagKey);
        if (alreadyRun == "true")
        {
            return;
        }

        _logger.LogInformation("Market closes in {Minutes} min, running end-of-day liquidation", minutesToClose);
        await _positionLifecycleService.CloseAllPositionsEndOfDayAsync();
        await _redisService.SetStringAsync(flagKey, "true");
    }
}

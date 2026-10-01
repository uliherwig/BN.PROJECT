namespace BN.PROJECT.AlpacaService;

[PersistJobDataAfterExecution]
[DisallowConcurrentExecution]
public class PositionTimeoutJob : IJob
{
    private readonly ILogger<PositionTimeoutJob> _logger;
    private readonly IConfiguration _configuration;
    private readonly IPositionLifecycleService _positionLifecycleService;
    private readonly IAlpacaDataService _alpacaDataService;


    public PositionTimeoutJob(
        ILogger<PositionTimeoutJob> logger,
        IConfiguration configuration,
        IPositionLifecycleService positionLifecycleService,
        IAlpacaDataService alpacaDataService)
    {
        _logger = logger;
        _configuration = configuration;
        _positionLifecycleService = positionLifecycleService;
        _alpacaDataService = alpacaDataService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var clock = await _alpacaDataService.GetClockAsync();
        if (!clock.IsOpen)
        {
            return;
        }
        var maxHoldingMinutes = _configuration.GetValue<int>("PositionManagement:MaxHoldingMinutes");
        if (maxHoldingMinutes <= 0)
        {
            return;
        }

        await _positionLifecycleService.CloseExpiredPositionsAsync(TimeSpan.FromMinutes(maxHoldingMinutes));
    }
}

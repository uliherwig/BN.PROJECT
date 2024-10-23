namespace BN.PROJECT.StrategyService;


[PersistJobDataAfterExecution]
[DisallowConcurrentExecution]
public class CleanUpJob : IJob
{
    private readonly ILogger<CleanUpJob> _logger;
    private readonly IConfiguration _configuration;
    private readonly IStrategyRepository _strategyRepository;


    public CleanUpJob(
        ILogger<CleanUpJob> logger,
        IConfiguration configuration,
        IStrategyRepository strategyRepository)
    {
        _logger = logger;
        _configuration = configuration;
        _strategyRepository = strategyRepository;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        JobKey key = context.JobDetail.Key;

        _logger.LogInformation("Instance " + key + " CleanUp Job start");

        await _strategyRepository.CleanupBacktests();

        _logger.LogInformation("Instance " + key + " CleanUp Job end");
    }

}
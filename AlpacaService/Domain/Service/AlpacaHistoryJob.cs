[PersistJobDataAfterExecution]
[DisallowConcurrentExecution]
public class AlpacaHistoryJob : IJob
{
    private readonly ILogger<AlpacaHistoryJob> _logger;

    // Konstruktor für Dependency Injection
    public AlpacaHistoryJob(ILogger<AlpacaHistoryJob> logger)
    {
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context)
    {
        JobKey key = context.JobDetail.Key;   

        _logger.LogInformation("Instance " + key + " History Job started");


        return Task.CompletedTask;
    }
}

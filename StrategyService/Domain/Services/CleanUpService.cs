namespace BN.PROJECT.StrategyService;

public class CleanUpService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public CleanUpService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var schedulerFactory = scope.ServiceProvider.GetRequiredService<ISchedulerFactory>();
            var scheduler = await schedulerFactory.GetScheduler();

            var cleanUpJob = JobBuilder.Create<CleanUpJob>()
                .WithIdentity("cleanUpJob", "strategyGroup")
                .SetJobData(new JobDataMap { { "key", "TestCleanUpJob" } })
                .Build();

            var triggerCleanUpJob = TriggerBuilder.Create()
               .WithIdentity("cleanUpTrigger", "strategyGroup")
               .WithDailyTimeIntervalSchedule(x => x.WithIntervalInHours(24).OnEveryDay())
               .Build();

            await scheduler.ScheduleJob(cleanUpJob, triggerCleanUpJob);       

        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
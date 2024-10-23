namespace BN.PROJECT.StrategyService;

public class BacktestCleanUpService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public BacktestCleanUpService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var schedulerFactory = scope.ServiceProvider.GetRequiredService<ISchedulerFactory>();
            var scheduler = await schedulerFactory.GetScheduler();

            var job = JobBuilder.Create<CleanUpJob>()
                .WithIdentity("cleanUpJob", "strategyGroup")
                .SetJobData(new JobDataMap { { "key", "TestCleanUpJob" } })
                .Build();

            var trigger = TriggerBuilder.Create()
               .WithIdentity("strategyTrigger", "strategyGroup")
               .WithDailyTimeIntervalSchedule(x => x.WithIntervalInHours(24).OnEveryDay())
               .Build();

            await scheduler.ScheduleJob(job, trigger);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
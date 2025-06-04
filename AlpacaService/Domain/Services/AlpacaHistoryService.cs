namespace BN.PROJECT.AlpacaService;

public class AlpacaHistoryService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public AlpacaHistoryService(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var schedulerFactory = scope.ServiceProvider.GetRequiredService<ISchedulerFactory>();
            var scheduler = await schedulerFactory.GetScheduler();
            var interval = _configuration.GetValue<int>("UpdateAlpacaDb:IntervalInMinutes");
            var executionEnabled = _configuration.GetValue<bool>("UpdateAlpacaDb:Enabled");
            if (!executionEnabled)
            {
                return;
            }
            var job = JobBuilder.Create<AlpacaHistoryJob>()
                .WithIdentity("historyJob", "alpacaGroup")
                .SetJobData(new JobDataMap { { "key", "AlpacaHistoryJob" } })
                .Build();

            var trigger = TriggerBuilder.Create()
               .WithIdentity("alpacaTrigger", "alpacaGroup")
               .StartNow()
               .WithSimpleSchedule(x => x
                   .WithIntervalInMinutes(interval)
                   .RepeatForever())
               .Build();

            await scheduler.ScheduleJob(job, trigger);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
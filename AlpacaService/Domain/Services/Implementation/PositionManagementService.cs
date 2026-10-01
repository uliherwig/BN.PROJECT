namespace BN.PROJECT.AlpacaService;

public class PositionManagementService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public PositionManagementService(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var positionManagementSection = _configuration.GetSection("PositionManagement");
        if (!positionManagementSection.Exists())
        {
            throw new InvalidOperationException("Missing configuration section: PositionManagement");
        }

        if (!positionManagementSection.GetValue<bool>("Enabled"))
        {
            return;
        }

        using (var scope = _serviceProvider.CreateScope())
        {
            var timeoutCheckIntervalMinutes = positionManagementSection.GetValue<int>("TimeoutCheckIntervalMinutes");

            var schedulerFactory = scope.ServiceProvider.GetRequiredService<ISchedulerFactory>();
            var scheduler = await schedulerFactory.GetScheduler();

            var job = JobBuilder.Create<PositionTimeoutJob>()
                .WithIdentity("positionTimeoutJob", "alpacaGroup")
                .SetJobData(new JobDataMap { { "key", "PositionTimeoutJob" } })
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity("positionTimeoutTrigger", "alpacaGroup")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(timeoutCheckIntervalMinutes)
                    .RepeatForever())
                .Build();

            await scheduler.ScheduleJob(job, trigger);
        }

        using (var scope = _serviceProvider.CreateScope())
        {
            var eodCheckIntervalMinutes = positionManagementSection.GetValue<int>("EodCheckIntervalMinutes");

            var schedulerFactory = scope.ServiceProvider.GetRequiredService<ISchedulerFactory>();
            var scheduler = await schedulerFactory.GetScheduler();

            var job = JobBuilder.Create<EndOfDayLiquidationJob>()
                .WithIdentity("eodLiquidationJob", "alpacaGroup")
                .SetJobData(new JobDataMap { { "key", "EndOfDayLiquidationJob" } })
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity("eodLiquidationTrigger", "alpacaGroup")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(eodCheckIntervalMinutes)
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

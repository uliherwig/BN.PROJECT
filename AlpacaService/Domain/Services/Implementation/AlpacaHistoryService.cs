namespace BN.PROJECT.AlpacaService;

public class AlpacaHistoryService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public AlpacaHistoryService(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var historyJobSection = _configuration.GetSection("HistoryJob");
        if (!historyJobSection.Exists())
        {
            throw new InvalidOperationException("Missing configuration section: HistoryJob");
        }

        using (var scope = _serviceProvider.CreateScope())
        {
            var executionEnabled = historyJobSection.GetValue<bool>("CalendarEnabled");
            if (executionEnabled)
            {
                var calendarInterval = historyJobSection.GetValue<int>("CalendarIntervalDays");         

                var schedulerFactory = scope.ServiceProvider.GetRequiredService<ISchedulerFactory>();
                var scheduler = await schedulerFactory.GetScheduler();

                var job = JobBuilder.Create<CalendarJob>()
                    .WithIdentity("calendarJob", "alpacaGroup")
                    .SetJobData(new JobDataMap { { "key", "CalendarJob" } })
                    .Build();

                var trigger = TriggerBuilder.Create()
                    .WithIdentity("calendarTrigger", "alpacaGroup")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInMinutes(calendarInterval * 24 * 60) // Convert days to minutes
                        .RepeatForever())
                    .Build();

                await scheduler.ScheduleJob(job, trigger);
            }
        }

        using (var scope = _serviceProvider.CreateScope())
        {
            var executionEnabled = historyJobSection.GetValue<bool>("BarsEnabled");
            if (executionEnabled)
            {
                var barsInterval = historyJobSection.GetValue<int>("BarsIntervalMinutes");  

                var schedulerFactory = scope.ServiceProvider.GetRequiredService<ISchedulerFactory>();
                var scheduler = await schedulerFactory.GetScheduler();
                var job = JobBuilder.Create<BarsJob>()
                    .WithIdentity("historyBarsJob", "alpacaGroup")
                    .SetJobData(new JobDataMap { { "key", "BarsJob" } })
                    .Build();

                var trigger = TriggerBuilder.Create()
                   .WithIdentity("historyBarsTrigger", "alpacaGroup")
                   .StartNow()
                   .WithSimpleSchedule(x => x
                       .WithIntervalInMinutes(barsInterval)
                       .RepeatForever())
                   .Build();

                await scheduler.ScheduleJob(job, trigger);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

}
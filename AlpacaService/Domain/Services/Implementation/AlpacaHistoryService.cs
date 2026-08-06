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
            var executionEnabled = _configuration.GetValue<bool>("HistoryJob:CalendarEnabled");
            if (!executionEnabled)
            {
                return;
            }
            var interval = _configuration.GetValue<int>("HistoryJob:CalendarIntervalDays");
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
                    .WithIntervalInMinutes(interval * 24 * 60) // Convert days to minutes
                    .RepeatForever())
                .Build();

            await scheduler.ScheduleJob(job, trigger);
        }

        using (var scope = _serviceProvider.CreateScope())
        {
            var executionEnabled = _configuration.GetValue<bool>("HistoryJob:BarsEnabled");
            if (!executionEnabled)
            {
                return;
            }
            var interval = _configuration.GetValue<int>("HistoryJob:BarsIntervalMinutes");
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
                   .WithIntervalInMinutes(interval)
                   .RepeatForever())
               .Build();

            await scheduler.ScheduleJob(job, trigger);
        }
        using (var scope = _serviceProvider.CreateScope())
        {   
            var executionEnabled = _configuration.GetValue<bool>("HistoryJob:TradesEnabled");
            if (!executionEnabled)
            {
                return;
            }
            var interval = _configuration.GetValue<int>("HistoryJob:TradesIntervalMinutes");
            var schedulerFactory = scope.ServiceProvider.GetRequiredService<ISchedulerFactory>();
            var scheduler = await schedulerFactory.GetScheduler();
            var job = JobBuilder.Create<TradesJob>()
                .WithIdentity("historyTradesJob", "alpacaGroup")
                .SetJobData(new JobDataMap { { "key", "TradesJob" } })
                .Build();

            var trigger = TriggerBuilder.Create()
               .WithIdentity("historyTradesTrigger", "alpacaGroup")
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
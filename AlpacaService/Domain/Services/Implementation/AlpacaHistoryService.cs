namespace BN.PROJECT.AlpacaService;

public class AlpacaHistoryService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AlpacaHistoryService> _logger;

    public AlpacaHistoryService(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<AlpacaHistoryService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing AlpacaHistoryService...");

        foreach (var kvp in _configuration.AsEnumerable())
        {
            _logger.LogInformation($"###################################   {kvp.Key}: {kvp.Value}");
        }
        
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await InitializeAsync();
        var assetsAsString = _configuration.GetValue<string>("Alpaca:TRADED_ASSETS") ?? string.Empty;
        var assetsSelection = assetsAsString.Split(",").ToList();

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
                if (calendarInterval <= 0)
                {
                    _logger.LogWarning("CalendarIntervalDays must be greater than 0. Using default value of 1 day.");
                    calendarInterval = 1; // Set a default value if the interval is less than or equal to 0
                }

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
                if (barsInterval <= 0)
                {
                    _logger.LogWarning("BarsIntervalMinutes must be greater than 0. Using default value of 1 minute.");
                    barsInterval = 10; // Set a default value if the interval is less than or equal to 0
                }

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

        using (var scope = _serviceProvider.CreateScope())
        {
            var executionEnabled = historyJobSection.GetValue<bool>("TradesEnabled");
            if (executionEnabled)
            {
                var tradesInterval = historyJobSection.GetValue<int>("TradesIntervalDays");
                if (tradesInterval <= 0)
                {
                    _logger.LogWarning("TradesIntervalDays must be greater than 0. Using default value of 1 day.");
                    tradesInterval = 1; // Set a default value if the interval is less than or equal to 0
                }

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
                       .WithIntervalInMinutes(tradesInterval * 24 * 60) // Convert days to minutes
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
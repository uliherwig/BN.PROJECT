namespace BN.PROJECT.AlpacaService;


public class SendQuoteTaskService : IHostedService, IDisposable
{
    private readonly IServiceProvider _serviceProvider;

    private readonly ILogger<SendQuoteTaskService> _logger;
    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _tasks;

    public SendQuoteTaskService(ILogger<SendQuoteTaskService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _tasks = new ConcurrentDictionary<Guid, CancellationTokenSource>();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var schedulerFactory = scope.ServiceProvider.GetRequiredService<ISchedulerFactory>();
            var scheduler = await schedulerFactory.GetScheduler();

            var job = JobBuilder.Create<SendQuoteJob>()
                .WithIdentity("QuoteJob", "alpacaGroup")
                .SetJobData(new JobDataMap { { "key", "SendQuoteJob" } })
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity("quoteTrigger", "alpacaGroup")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(1)
                    .RepeatForever())
                .Build();

            await scheduler.ScheduleJob(job, trigger);
        }
    }

    public Task<Guid> StartTaskAsync(Func<CancellationToken, Task> taskFunc)
    {
        var taskId = Guid.NewGuid();
        var cts = new CancellationTokenSource();
        _tasks[taskId] = cts;

        Task.Run(() => taskFunc(cts.Token), cts.Token)
            .ContinueWith(t =>
            {
                _tasks.TryRemove(taskId, out _);
                if (t.IsFaulted)
                {
                    _logger.LogError(t.Exception, "Task {TaskId} failed.", taskId);
                }
                else
                {
                    _logger.LogInformation("Task {TaskId} completed.", taskId);
                }
            }, cts.Token);

        return Task.FromResult(taskId);
    }

    public Task StopTaskAsync(Guid taskId)
    {
        if (_tasks.TryRemove(taskId, out var cts))
        {
            cts.Cancel();
            _logger.LogInformation("Task {TaskId} is stopping.", taskId);
        }
        else
        {
            _logger.LogWarning("Task {TaskId} not found.", taskId);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Periodic Task Service is stopping.");


        foreach (var cts in _tasks.Values)
        {
            cts.Cancel();
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        foreach (var cts in _tasks.Values)
        {
            cts.Dispose();
        }
    }
}

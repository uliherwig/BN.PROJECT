namespace BN.PROJECT.StrategyService;

public class MessageConsumerService : IHostedService
{
    private readonly ILogger<MessageConsumerService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public MessageConsumerService(
        ILogger<MessageConsumerService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var redisSubscriber = scope.ServiceProvider.GetRequiredService<IRedisSubscriber>();

                var channelName = RedisUtilities.GetChannelName(RedisChannelEnum.Strategy);
                if (string.IsNullOrEmpty(channelName))
                {
                    return Task.CompletedTask;
                }

                redisSubscriber.Subscribe(channelName, (channel, msg) =>
                {
                    ConsumeMessage(msg);
                });
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in MessageConsumerService");
        }
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async void ConsumeMessage(string messageJson)
    {
        var message = JsonConvert.DeserializeObject<StrategyMessage>(messageJson); 

        if (message != null && message.StrategyTask == StrategyTaskEnum.Backtest || message.StrategyTask == StrategyTaskEnum.Optimize)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var schedulerFactory = scope.ServiceProvider.GetRequiredService<ISchedulerFactory>();
                var scheduler = await schedulerFactory.GetScheduler();
                var jobKey = new JobKey($"{message.StrategyTask.ToString().ToLower()}Job_{message.StrategyId}", "strategyGroup");
                Type jobType = message.StrategyTask switch
                {
                    StrategyTaskEnum.Backtest => typeof(BacktestJob),
                    StrategyTaskEnum.Optimize => typeof(OptimizeJob),
                    _ => throw new ArgumentException("Unknown strategy task", nameof(message.StrategyTask))
                };
                var strategyJob = JobBuilder.Create(jobType)
                    .WithIdentity(jobKey)
                    .SetJobData(new JobDataMap
                        {
                            { "key", jobKey },
                            { "strategyId", message.StrategyId }
                        })
                    .Build();

                var trigger = TriggerBuilder.Create()
                   .StartNow()
                   .Build();

                await scheduler.ScheduleJob(strategyJob, trigger);
            }
        }
        return;


    }
}
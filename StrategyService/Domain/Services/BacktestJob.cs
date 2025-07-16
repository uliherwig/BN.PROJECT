using StackExchange.Redis;

namespace BN.PROJECT.StrategyService;

[PersistJobDataAfterExecution]
[DisallowConcurrentExecution]
public class BacktestJob : IJob
{
    private readonly ILogger<BacktestJob> _logger;
    private readonly IConfiguration _configuration;
    private readonly IStrategyRepository _strategyRepository;
    private readonly IServiceProvider _serviceProvider;

    private readonly IStrategyServiceStore _serviceStore;
    private readonly IDatabase _redisDatabase;



    public BacktestJob(
        ILogger<BacktestJob> logger,
        IConfiguration configuration,
        IStrategyRepository strategyRepository,
        IServiceProvider serviceProvider,
        IStrategyServiceStore strategyServiceStore,
        IConnectionMultiplexer redis
        )
    {
        _logger = logger;
        _configuration = configuration;
        _strategyRepository = strategyRepository;
        _serviceProvider = serviceProvider;
        _serviceStore = strategyServiceStore;
        _redisDatabase = redis.GetDatabase();

    }

    public async Task Execute(IJobExecutionContext context)
    {
        // get strategy settings
        // get quotes
        // create a strategy message
        // create the strategy service
        // start the backtest
        // evaluate quotes
        // get positions
        // get test result
        // to db: update strategy settings with end time
        // to db: save positions

        var dataMap = context.JobDetail.JobDataMap;
        var strategyId = (Guid)dataMap.WrappedMap["strategyId"];

        var strategySettings = await _strategyRepository.GetStrategyByIdAsync(strategyId);
        if (strategySettings == null)
        {
            _logger.LogError($"Strategy with ID {strategyId} not found.");
            return;
        }

        // Initialize Kafka producer for notifications
        var notificationTopic = KafkaUtilities.GetTopicName(KafkaTopicEnum.Notification);
        var notificationProducer = _serviceStore.GetOrCreateKafkaProducer(strategyId);

        var notificationMessage = NotificationMessageFactory.CreateNotificationMessage(
            strategySettings.UserId,
            NotificationEnum.BacktestStart
        );
        await notificationProducer.SendMessageAsync(notificationTopic, notificationMessage.ToJson());

        var quotes = new List<Quote>();
        var symbol = strategySettings.Asset;
        var startDate = strategySettings.StartDate.ToUniversalTime();
        var endDate = strategySettings.EndDate.ToUniversalTime();
        TimeSpan timeFrame = TimeSpan.FromDays(1);

        var stamp = startDate.ToUniversalTime();

        while (stamp < endDate)
        {
            var key = $"quotes:{symbol}:{stamp:yyyy-MM-dd}";
            if (_redisDatabase.KeyExists(key))
            {
                var quotesDay = _redisDatabase.StringGet(key);
                quotes.AddRange(quotesDay.ToString().FromJson<List<Quote>>());
            }
            stamp = stamp.Add(timeFrame).ToUniversalTime();
        }

        var message = new StrategyMessage
        {
            StrategyId = strategySettings.Id,
            IsBacktest = true,
            UserId = strategySettings.UserId,
            Settings = strategySettings,
            Strategy = strategySettings.StrategyType,
            MessageType = MessageTypeEnum.Start
        };

        var strategyService = _serviceStore.GetOrCreateStrategyService(strategyId, strategySettings.StrategyType);

        await strategyService.StartTest(message);

        foreach (var q in quotes)
        {
            await strategyService.EvaluateQuote(strategySettings.Id, message.UserId, q);
        }

        // Store results
        var positions = strategyService.GetPositions();

        strategySettings.StampEnd = DateTime.UtcNow.ToUniversalTime();
        await _strategyRepository.UpdateStrategyAsync(strategySettings);
        await _strategyRepository.AddPositionsAsync(positions);

        // Notify user
        notificationMessage.NotificationType = NotificationEnum.BacktestStop;
        await notificationProducer.SendMessageAsync(notificationTopic, notificationMessage.ToJson());

        // Clean up the service store
        _serviceStore.RemoveStrategyService(strategyId);
        _serviceStore.RemoveKafkaProducer(strategyId);
    }
}
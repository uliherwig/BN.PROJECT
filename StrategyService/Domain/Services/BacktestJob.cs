namespace BN.PROJECT.StrategyService;

public class BacktestJob : IJob
{
    private readonly ILogger<BacktestJob> _logger;
    private readonly IConfiguration _configuration;
    private readonly IStrategyServiceStore _serviceStore;
    private readonly IStrategyRepository _strategyRepository;
    private readonly IRedisPublisher _publisher;
    private readonly IDatabase _redisDatabase;

    public BacktestJob(
        ILogger<BacktestJob> logger,
        IConfiguration configuration,
        IStrategyServiceStore serviceStore,
        IStrategyRepository strategyRepository,
        IConnectionMultiplexer redis,
        IRedisPublisher publisher
        )
    {
        _logger = logger;
        _configuration = configuration;
        _serviceStore = serviceStore;
        _strategyRepository = strategyRepository;
        _redisDatabase = redis.GetDatabase();
        _publisher = publisher;

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
        if (context.CancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation($"Backtest job for {strategyId} was cancelled before start.");
            return;
        }
        if (strategySettings == null)
        {
            _logger.LogError($"Strategy with ID {strategyId} not found.");
            return;
        }


        var notificationTopic = RedisUtilities.GetChannelName(RedisChannelEnum.Notification);
        var notificationMessage = NotificationMessageFactory.CreateNotificationMessage(
            strategySettings.UserId,
            NotificationEnum.BacktestStart
        );
        await _publisher.PublishAsync(notificationTopic, notificationMessage.ToJson());


        var bars = new List<BarModel>();
        var symbol = strategySettings.Asset;
        var startDate = strategySettings.StartDate.ToUniversalTime();
        var endDate = strategySettings.EndDate.ToUniversalTime();
        TimeSpan timeFrame = TimeSpan.FromDays(1);

        var stamp = startDate.ToUniversalTime();

        while (stamp < endDate)
        {
            if (context.CancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Backtest job for {strategyId} was cancelled during quote loading.");
                return;
            }

            var barsKey = RedisUtilities.GetBarsKey(symbol, stamp);
            var barsDay = _redisDatabase.StringGet(barsKey);
            if (!barsDay.IsNullOrEmpty)
            {
                var deserializedBars = JsonConvert.DeserializeObject<List<BarModel>>(barsDay!);
                if (deserializedBars != null)
                    bars.AddRange(deserializedBars);
            }

            stamp = stamp.Add(timeFrame).ToUniversalTime();
        }
        var quotes = bars.Select(b => new Quote
        {
            Date = b.TimestampUtc,
            Open = b.Open,
            High = b.High,
            Low = b.Low,
            Close = b.Close,
            Volume = b.Volume
        }).ToList();


        var indicatorService = _serviceStore.GetOrCreateIndicatorService(strategyId, strategySettings.IndicatorType);

        var signals = await indicatorService.StartTest(strategySettings, quotes); // List<BarSignalModel> 


        // Store results
        var positions = StrategyOperations.RunBacktest(strategySettings, signals).ToList();

        strategySettings.StampEnd = DateTime.UtcNow.ToUniversalTime();
        await _strategyRepository.UpdateStrategyAsync(strategySettings);
        await _strategyRepository.AddPositionsAsync(positions);

        // Notify user
        notificationMessage.NotificationType = NotificationEnum.BacktestStop;
        await _publisher.PublishAsync(notificationTopic, notificationMessage.ToJson());



    }
}
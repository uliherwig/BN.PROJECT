using Newtonsoft.Json.Serialization;

namespace BN.PROJECT.StrategyService;

[PersistJobDataAfterExecution]
[DisallowConcurrentExecution]
public class OptimizeJob : IJob
{
    private readonly ILogger<OptimizeJob> _logger;
    private readonly IConfiguration _configuration;
    private readonly IStrategyRepository _strategyRepository;
    private readonly IStrategyServiceStore _serviceStore;
    private readonly IDatabase _redisDatabase;

    public OptimizeJob(
        ILogger<OptimizeJob> logger,
        IConfiguration configuration,
        IStrategyRepository strategyRepository,
        IStrategyServiceStore strategyServiceStore,
        IConnectionMultiplexer redis
        )
    {
        _logger = logger;
        _configuration = configuration;
        _strategyRepository = strategyRepository;
        _serviceStore = strategyServiceStore;
        _redisDatabase = redis.GetDatabase();
    }

    public async Task Execute(IJobExecutionContext context)
    {    
        var dataMap = context.JobDetail.JobDataMap;
        var strategyId = (Guid)dataMap.WrappedMap["strategyId"];

   
        var strategySettings = await _strategyRepository.GetStrategyByIdAsync(strategyId);
        if (strategySettings == null)
        {
            _logger.LogError($"Strategy with ID {strategyId} not found.");
            return;
        }
        var optimizationResult = new OptimizationResultModel
        {
            StrategyId = strategySettings.Id,
            Settings = strategySettings
        };

        // Initialize Kafka producer for notifications
        var notificationTopic = RedisUtilities.GetChannelName(RedisChannelEnum.Notification);

        var notificationMessage = NotificationMessageFactory.CreateNotificationMessage(
            strategySettings.UserId,
            NotificationEnum.OptimizeStart
        );


        // create quotes
        var quotes = new List<PriceQuote>();
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
                quotes.AddRange(quotesDay.ToString().FromJson<List<PriceQuote>>());
            }
            stamp = stamp.Add(timeFrame).ToUniversalTime();
        }


        // create combinations for optimization
        var combinations = Enumerable.Empty<dynamic>();
        switch (strategySettings.IndicatorType)
        {
            case IndicatorEnum.BREAKOUT:
                var strategyParams = JsonConvert.DeserializeObject<BreakoutModel>(strategySettings.StrategyParams);
                if (strategyParams == null)
                {
                    return;
                }

                switch (strategyParams.StopLossType)
                {
                    case StopLossTypeEnum.None:

                        var tp_sl_Grid = new Dictionary<string, IEnumerable<decimal>>
                        {
                            { "sl", Enumerable.Range(0, 2).Select(i => 0.5m + i * 0.2m) },
                            { "tp", Enumerable.Range(0, 2).Select(i => 0.5m + i * 0.2m) }
                        };

                        combinations =
                           from sl in tp_sl_Grid["sl"]
                           from tp in tp_sl_Grid["tp"]

                           select new { testSl = sl, testTp = tp };
                        break;

                    case StopLossTypeEnum.Breakout:

                        var paramGrid = new Dictionary<string, IEnumerable<TimeFrameEnum>>
                            {
                                { "period", Enum.GetValues(typeof(TimeFrameEnum)).Cast<TimeFrameEnum>() }
                            };

                        combinations =
                            from period in paramGrid["period"]
                            select new { period };
                        break;
                    default:
                        _logger.LogError($"Unknown StopLossType for strategy {strategySettings.Name}.");
                        return;
                }

                foreach (var combo in combinations)
                {
                    var strategyService = _serviceStore.GetOrCreateStrategyService(strategyId, strategySettings.IndicatorType);

                    switch (strategyParams.StopLossType)
                    {
                        case StopLossTypeEnum.None:
                            strategySettings.TakeProfitPercent = combo.testTp;
                            strategySettings.StopLossPercent = combo.testSl;
                            break;
                        case StopLossTypeEnum.Breakout:

                            strategyParams.BreakoutPeriod = combo.period;

                            break;
                        default:
                            _logger.LogError($"Unknown StopLossType for strategy {strategySettings.Name}.");
                            return;
                    }


                    strategySettings.StrategyParams = JsonConvert.SerializeObject(
                        strategyParams,
                        new JsonSerializerSettings
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        }
                    );

                    var strategyMessage = new StrategyMessage
                    {
                        StrategyId = strategySettings.Id,
                        IsBacktest = true,
                        UserId = strategySettings.UserId,
                        Settings = strategySettings,
                        Strategy = strategySettings.StrategyType,
                        MessageType = MessageTypeEnum.Start
                    };
                 
                    await strategyService.StartTest(strategyMessage);

                    foreach (var q in quotes)
                    {
                        await strategyService.EvaluateQuote(strategySettings.Id, strategyMessage.UserId, q);
                    }

                    var positions = strategyService.GetPositions();
                    var testResult = await strategyService.GetTestResult();
                    if (positions == null || testResult == null)
                    {
                        continue;
                    }

                    if (positions.Count > 10 && testResult.TotalProfitLoss > optimizationResult.Profit)
                    {
                        optimizationResult = new OptimizationResultModel
                        {
                            StrategyId = strategySettings.Id,
                            Settings = strategySettings,
                            Positions = positions,
                            Profit = testResult.TotalProfitLoss,
                            Result = testResult
                        };
                    }
                  _serviceStore.RemoveStrategyService(strategyId);
                }
                break;

      
            default:
                _logger.LogWarning($"Unknown strategy type for {strategySettings.Name}.");
                return;
        }

        // Store results
        optimizationResult.Settings.StampEnd = DateTime.UtcNow.ToUniversalTime();
        await _strategyRepository.UpdateStrategyAsync(optimizationResult.Settings);
        await _strategyRepository.AddPositionsAsync(optimizationResult.Positions);

        // Notify user
        notificationMessage.NotificationType = NotificationEnum.OptimizeStop;

        // Clean up the service store
        _serviceStore.RemoveStrategyService(strategyId);
    }
}
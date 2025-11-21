using Newtonsoft.Json.Serialization;
using StackExchange.Redis;

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

        var optimizationResult = new OptimizationResultModel();
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
            NotificationEnum.OptimizeStart
        );
        await notificationProducer.SendMessageAsync(notificationTopic, notificationMessage.ToJson());


        // create quotes
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


        // create combinations for optimization
        var combinations = Enumerable.Empty<dynamic>();
        switch (strategySettings.StrategyType)
        {
            case StrategyEnum.Breakout:
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
                    var strategyService = _serviceStore.GetOrCreateStrategyService(strategyId, strategySettings.StrategyType);

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

            case StrategyEnum.SMA:
                var smaSettings = JsonConvert.DeserializeObject<SmaModel>(strategySettings.StrategyParams);
                if (smaSettings == null)
                {
                    _logger.LogError($"Failed to deserialize SMA settings for strategy {strategySettings.Name}.");
                    return;
                }

                switch (smaSettings.StopLossType)
                {
                    case StopLossTypeEnum.None:

                        var tp_sl_Grid = new Dictionary<string, IEnumerable<decimal>>
                        {
                            { "sl", Enumerable.Range(0, 2).Select(i => 0.5m + i * 0.2m) },  // 10 to 95
                            { "tp", Enumerable.Range(0, 2).Select(i => 0.5m + i * 0.2m) }   // 50 to 290
                        };

                        combinations =
                           from sl in tp_sl_Grid["sl"]
                           from tp in tp_sl_Grid["tp"]

                           select new { testSl = sl, testTp = tp };
                        break;

                    case StopLossTypeEnum.SMAIntersection:
                        int smaShortPeriod = smaSettings.ShortPeriod;
                        int smaLongPeriod = smaSettings.LongPeriod;
                        var paramGrid = new Dictionary<string, IEnumerable<int>>
                        {
                            { "ma_short", Enumerable.Range(0, 2).Select(i => smaShortPeriod + i * 10) },  // 10 to 95
                            { "ma_long", Enumerable.Range(0, 2).Select(i => smaLongPeriod + i * 10) }   // 50 to 290
                        };

                        combinations =
                           from maShort in paramGrid["ma_short"]
                           from maLong in paramGrid["ma_long"]
                           where maShort < maLong  // Optional: skip invalid combos
                           select new { ma_short = maShort, ma_long = maLong };
                        break;
                    default:
                        _logger.LogError($"Unknown StopLossType for strategy {strategySettings.Name}.");
                        return;
                }

                foreach (var combo in combinations)
                {
                    var strategyService = _serviceStore.GetOrCreateStrategyService(strategyId, strategySettings.StrategyType);

                    switch (smaSettings.StopLossType)
                    {
                        case StopLossTypeEnum.None:
                            strategySettings.TakeProfitPercent = combo.testTp;
                            strategySettings.StopLossPercent = combo.testSl;
                            break;
                        case StopLossTypeEnum.SMAIntersection:
                            smaSettings.ShortPeriod = combo.ma_short;
                            smaSettings.LongPeriod = combo.ma_long;
                            break;
                        default:
                            _logger.LogError($"Unknown StopLossType for strategy {strategySettings.Name}.");
                            return;
                    }


                    strategySettings.StrategyParams = JsonConvert.SerializeObject(
                        smaSettings,
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
        await notificationProducer.SendMessageAsync(notificationTopic, notificationMessage.ToJson());

        // Clean up the service store
        _serviceStore.RemoveStrategyService(strategyId);
        _serviceStore.RemoveKafkaProducer(strategyId);
    }
}
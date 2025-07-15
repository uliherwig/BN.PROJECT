using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Newtonsoft.Json.Serialization;
using NuGet.Configuration;
using StackExchange.Redis;
using System.Diagnostics;


namespace BN.PROJECT.StrategyService;

public class OptimizerService : IOptimizerService
{
    private readonly ILogger<OptimizerService> _logger;
    private readonly IKafkaProducerService _kafkaProducer;
    private readonly IEnumerable<IStrategyService> _strategyServices;

    private readonly ConcurrentDictionary<Guid, List<TestResult>> _testResults = new();
    private readonly ConcurrentDictionary<Guid, List<Quote>> _quotes = new();
    private readonly ConcurrentDictionary<Guid, OptimizationResultModel> _optimizationResult = new();

    private readonly string _notificationTopic = KafkaUtilities.GetTopicName(KafkaTopicEnum.Notification);
    private readonly IDatabase _redisDatabase;


    public OptimizerService(
        ILogger<OptimizerService> logger,
        IEnumerable<IStrategyService> strategyServices,
        IKafkaProducerService kafkaProducer, IConnectionMultiplexer redis)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logger.LogInformation("OptimizerService initialized.");

        _strategyServices = strategyServices;
        _kafkaProducer = kafkaProducer;
        _redisDatabase = redis.GetDatabase();
    }

    public async Task Initialize(Guid strategyId)
    {
        _ = _quotes.TryAdd(strategyId, []);
        _ = _testResults.TryAdd(strategyId, []);
        _ = _optimizationResult.TryAdd(strategyId, new OptimizationResultModel
        {
            StrategyId = strategyId,
            Profit = -10000m
        });


        await _kafkaProducer.SendMessageAsync(_notificationTopic, "");
    }

    public async Task<bool> Run(StrategySettingsModel strategySettings)
    {
        var strategyService = _strategyServices.Single(s => s.CanHandle(strategySettings.StrategyType));

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
                _quotes[strategySettings.Id].AddRange(quotesDay.ToString().FromJson<List<Quote>>());
            }
            stamp = stamp.Add(timeFrame).ToUniversalTime();
        }

        // create dummy combinations for optimization
        var combinations = Enumerable.Empty<dynamic>();
        switch (strategySettings.StrategyType)
        {
            case StrategyEnum.Breakout:
                var strategyParams = JsonConvert.DeserializeObject<BreakoutModel>(strategySettings.StrategyParams);
                if (strategyParams == null)
                {
                    return false;
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

                        var paramGrid = new Dictionary<string, IEnumerable<BreakoutPeriodEnum>>
                            {
                                { "period", Enum.GetValues(typeof(BreakoutPeriodEnum)).Cast<BreakoutPeriodEnum>() }
                            };

                        combinations =
                            from period in paramGrid["period"]
                            select new { period };
                        break;
                    default:
                        _logger.LogError($"Unknown StopLossType for strategy {strategySettings.Name}.");
                        return false;
                }

                foreach (var combo in combinations)
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();

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
                            return false;
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

                    foreach (var q in _quotes[strategySettings.Id])
                    {
                        await strategyService.EvaluateQuote(strategySettings.Id, strategyMessage.UserId, q);
                    }

                    var positions = strategyService.GetPositions();
                    var testResult = await strategyService.GetTestResult();
                    if (positions == null || testResult == null)
                    {
                        continue;
                    }

                    if (positions.Count > 10 && testResult.TotalProfitLoss > _optimizationResult[strategySettings.Id].Profit)
                    {
                        _optimizationResult[strategySettings.Id] = new OptimizationResultModel
                        {
                            StrategyId = strategySettings.Id,
                            Settings = strategySettings,
                            Positions = positions,
                            Profit = testResult.TotalProfitLoss,
                            Result = testResult
                        };
                    }
                    stopwatch.Stop();
                    _logger.LogInformation($"Strategy completed in {stopwatch.ElapsedMilliseconds} ms. Profit: {testResult.TotalProfitLoss} Positions count: {positions.Count}");

                }
                break;

            case StrategyEnum.SMA:
                var smaSettings = JsonConvert.DeserializeObject<SmaModel>(strategySettings.StrategyParams);
                if (smaSettings == null)
                {
                    _logger.LogError($"Failed to deserialize SMA settings for strategy {strategySettings.Name}.");
                    return false;
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
                        return false;
                }

                foreach (var combo in combinations)
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();

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
                            return false;
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




                    foreach (var q in _quotes[strategySettings.Id])
                    {
                        await strategyService.EvaluateQuote(strategySettings.Id, strategyMessage.UserId, q);
                    }

                    var positions = strategyService.GetPositions();
                    var testResult = await strategyService.GetTestResult();
                    if (positions == null || testResult == null)
                    {
                        continue;
                    }

                    if (positions.Count > 10 && testResult.TotalProfitLoss > _optimizationResult[strategySettings.Id].Profit)
                    {
                        _optimizationResult[strategySettings.Id] = new OptimizationResultModel
                        {
                            StrategyId = strategySettings.Id,
                            Settings = strategySettings,
                            Positions = positions,
                            Profit = testResult.TotalProfitLoss,
                            Result = testResult
                        };
                    }
                    stopwatch.Stop();
                    _logger.LogInformation($"Strategy completed in {stopwatch.ElapsedMilliseconds} ms. Profit: {testResult.TotalProfitLoss} Positions count: {positions.Count}");

                }
                break;

            default:
                _logger.LogWarning($"Unknown strategy type for {strategySettings.Name}.");
                return false;
        }

        return true;
    }

    public async Task<OptimizationResultModel> Finalize(Guid strategyId)
    {
        // Implement logic to stop optimization if needed
        _logger.LogInformation("Optimization process stopped.");

        return _optimizationResult[strategyId];

    }


}

using System.Diagnostics;

namespace BN.PROJECT.AlpacaService;

public class StrategyTestService : IStrategyTestService
{
    private readonly IAlpacaRepository _alpacaRepository;
    private readonly IKafkaProducerService _kafkaProducer;
    private readonly IAlpacaTradingService _alpacaTradingService;
    private readonly IStrategyServiceClient _strategyServiceClient;
    private readonly ILogger<StrategyTestService> _logger;
    private readonly IHubContext<AlpacaHub> _hubContext;
    private readonly IDatabase _redisDatabase;

    public StrategyTestService(IAlpacaRepository alpacaRepository,
        ILogger<StrategyTestService> logger,
        IKafkaProducerService kafkaProducer,
        IStrategyServiceClient strategyServiceClient,
        IAlpacaTradingService alpacaTradingService,
        IHubContext<AlpacaHub> hubContext,
        IConnectionMultiplexer redis)
    {
        _alpacaRepository = alpacaRepository;
        _logger = logger;
        _kafkaProducer = kafkaProducer;
        _alpacaTradingService = alpacaTradingService;
        _strategyServiceClient = strategyServiceClient;
        _hubContext = hubContext;
        _redisDatabase = redis.GetDatabase();
    }

    public async Task RunBacktest(StrategySettingsModel testSettings)
    {
        var strategyTopic = KafkaUtilities.GetTopicName(KafkaTopicEnum.Strategy);
        var notificationTopic = KafkaUtilities.GetTopicName(KafkaTopicEnum.Notification);
        await StoreQuotesToRedis(testSettings);

        var message = new StrategyMessage
        {
            IsBacktest = true,
            StrategyTask = StrategyTaskEnum.Backtest,
            UserId = testSettings.UserId,
            StrategyId = testSettings.Id
        }; 

        await _kafkaProducer.SendMessageAsync(strategyTopic, message.ToJson());
    }
    public async Task OptimizeStrategy(StrategySettingsModel testSettings)
    {
        var optimizeTopic = KafkaUtilities.GetTopicName(KafkaTopicEnum.Strategy);
        await StoreQuotesToRedis(testSettings);

        var message = new StrategyMessage
        {
            IsBacktest = true,
            StrategyTask = StrategyTaskEnum.Optimize,
            UserId = testSettings.UserId,
            StrategyId = testSettings.Id
        };       
        await _kafkaProducer.SendMessageAsync(optimizeTopic, message.ToJson());
    }
    public async Task StartExecution(Guid userId, Guid strategyId)
    {
        var testSettings = await _strategyServiceClient.GetStrategyAsync(strategyId.ToString());
        if (testSettings == null)
        {
            _logger.LogError($"RunExecution: Strategy {strategyId} not found");
            return;
        }
        var message = new StrategyMessage
        {
            IsBacktest = false,
            UserId = userId,
            StrategyTask = StrategyTaskEnum.PaperTrade,
            StrategyId = testSettings.Id,
            Strategy = testSettings.StrategyType,
            MessageType = MessageTypeEnum.Start,
            Settings = testSettings
        };

        await _kafkaProducer.SendMessageAsync("strategy", message.ToJson());
    }
    public async Task StopExecution(Guid userId, Guid strategyId)
    {
        var message = new StrategyMessage
        {
            MessageType = MessageTypeEnum.Stop,
            IsBacktest = false,
            UserId = userId,
            StrategyId = strategyId
        };
        await _kafkaProducer.SendMessageAsync("strategy", message.ToJson());
    }
    public async Task CreateAlpacaOrder(OrderMessage orderMessage)
    {
        _logger.LogInformation($"Run Alpaca Execution");

        var userId = orderMessage.UserId;
        var symbol = orderMessage.Position.Symbol;
        var qty = (int)orderMessage.Position.Quantity;
        var side = orderMessage.Position.Side == SideEnum.Buy ? OrderSide.Buy : OrderSide.Sell;
        if (orderMessage.Position.PriceClose > 0)
        {
            side = orderMessage.Position.Side == SideEnum.Sell ? OrderSide.Buy : OrderSide.Sell;
        }

        var orderType = OrderType.Market;
        var timeInForce = TimeInForce.Day;

        var userSettings = await _alpacaRepository.GetUserSettingsAsync(userId.ToString());
        if (userSettings == null)
        {
            _logger.LogError($"User settings not found for userId: {userId}");
            return;
        }

        await _alpacaTradingService.CreateOrderAsync(userSettings, symbol, qty, side, orderType, timeInForce);

    }
    private async Task StoreQuotesToRedis(StrategySettingsModel testSettings)
    {
        var symbol = testSettings.Asset;
        var startDate = testSettings.StartDate.ToUniversalTime();
        var endDate = testSettings.EndDate.ToUniversalTime();

        var bars = await _alpacaRepository.GetHistoricalBars(symbol, startDate, endDate);

        // send quotes per day
        TimeSpan timeFrame = TimeSpan.FromDays(1);
        var stamp = startDate.ToUniversalTime();

        while (stamp < endDate)
        {
            var key = $"quotes:{symbol}:{stamp:yyyy-MM-dd}";
            if (!_redisDatabase.KeyExists(key))
            {
                var stampEnd = stamp.Add(timeFrame).ToUniversalTime();
                if (bars.Count == 0)
                {
                    stamp = stamp.Add(timeFrame).ToUniversalTime();
                    continue;
                }
                var quotesDay = new List<Quote>();
                foreach (var bar in bars.Where(b => b.T > stamp && b.T < stampEnd))
                {
                    var q = new Quote
                    {
                        Symbol = symbol,
                        AskPrice = bar.C + 0.1m,
                        BidPrice = bar.C - 0.1m,
                        TimestampUtc = bar.T.ToUniversalTime()
                    };
                    quotesDay.Add(q);
                }

                _redisDatabase.StringSet($"quotes:{symbol}:{stamp:yyyy-MM-dd}", quotesDay.ToJson(), TimeSpan.FromDays(1));
            }

            stamp = stamp.Add(timeFrame).ToUniversalTime();
        }
    }


}
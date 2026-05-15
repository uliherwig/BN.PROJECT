using System.Diagnostics;

namespace BN.PROJECT.AlpacaService;

public class StrategyTestService : IStrategyTestService
{
    private readonly IAlpacaRepository _alpacaRepository;
    private readonly IAlpacaTradingService _alpacaTradingService;
    private readonly IStrategyServiceClient _strategyServiceClient;
    private readonly ILogger<StrategyTestService> _logger;
    private readonly IHubContext<AlpacaHub> _hubContext;
    private readonly IDatabase _redisDatabase;
    private readonly IRedisPublisher _publisher;

    public StrategyTestService(IAlpacaRepository alpacaRepository,
        ILogger<StrategyTestService> logger,
        IStrategyServiceClient strategyServiceClient,
        IAlpacaTradingService alpacaTradingService,
        IHubContext<AlpacaHub> hubContext,
        IConnectionMultiplexer redis,
        IRedisPublisher publisher)
    {
        _alpacaRepository = alpacaRepository;
        _logger = logger;
        _alpacaTradingService = alpacaTradingService;
        _strategyServiceClient = strategyServiceClient;
        _hubContext = hubContext;
        _redisDatabase = redis.GetDatabase();
        _publisher = publisher;
    }

    public async Task RunBacktest(StrategySettingsModel testSettings)
    {
        var strategyTopic = RedisUtilities.GetChannelName(RedisChannelEnum.Strategy);
        var notificationTopic = RedisUtilities.GetChannelName(RedisChannelEnum.Notification);
        //await StoreQuotesToRedis(testSettings);

        var message = new StrategyMessage
        {
            IsBacktest = true,
            StrategyTask = StrategyTaskEnum.Backtest,
            UserId = testSettings.UserId,
            StrategyId = testSettings.Id
        }; 

        await _publisher.PublishAsync(strategyTopic, message.ToJson());

    }
    public async Task OptimizeStrategy(StrategySettingsModel testSettings)
    {
        var optimizeTopic = RedisUtilities.GetChannelName(RedisChannelEnum.Strategy);
        await StoreBarsToRedis(testSettings.Asset);

        var message = new StrategyMessage
        {
            IsBacktest = true,
            StrategyTask = StrategyTaskEnum.Optimize,
            UserId = testSettings.UserId,
            StrategyId = testSettings.Id
        };       
        //await _kafkaProducer.SendMessageAsync(optimizeTopic, message.ToJson());
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

        //await _kafkaProducer.SendMessageAsync("strategy", message.ToJson());
        await _publisher.PublishAsync("strategy", message.ToJson());

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
        //await _kafkaProducer.SendMessageAsync("strategy", message.ToJson());
        await _publisher.PublishAsync("strategy", message.ToJson());

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
    public async Task StoreBarsToRedis(string asset)
    {      
        var stamp = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);       
        var alpacaBars = await _alpacaRepository.GetHistoricalBars(asset, stamp, DateTime.UtcNow); 

        // convert AlpacaBars to BarModel
        var barModels = alpacaBars.Select(b => new BarModel
        {
            Asset = asset,
            TimestampUtc = b.T,
            Open = b.O,
            High = b.H,
            Low = b.L,
            Close = b.C,
            Volume = b.V
        }).ToList();

        // group bars by day and store in Redis with key pattern "bars:{symbol}:{date}"
        var barsByDay = barModels
            .GroupBy(b => b.TimestampUtc.Date)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var kvp in barsByDay)
        {
            var date = kvp.Key;
            var barsForDay = kvp.Value;
            var barsKey = RedisUtilities.GetBarsKey(asset, date);
            if (!_redisDatabase.KeyExists(barsKey))
            {
                _redisDatabase.StringSet(barsKey, barsForDay.ToJson(), TimeSpan.FromDays(100));
            }
        }   
    }
}
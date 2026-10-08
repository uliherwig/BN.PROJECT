namespace BN.PROJECT.AlpacaService;

[Route("[controller]")]
[ApiController]
//[AuthorizeUser(["user", "admin"])]
public class AlpacaTestController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly IAlpacaRepository _alpacaRepository;
    private readonly IStrategyTestService _strategyTestService;
    private readonly IStrategyServiceClient _strategyServiceClient;
    private readonly IFinAIServiceClient _finAIServiceClient;
    private readonly ILogger<AlpacaTestController> _logger;
    private readonly IRedisPublisher _publisher;
    private readonly IRedisService _redisService;
    private readonly IAlpacaDataService _alpacaDataService;




    public AlpacaTestController(
        IWebHostEnvironment env,
        IAlpacaRepository alpacaRepository,
        IStrategyTestService backtestService,
        IStrategyServiceClient strategyServiceClient,
        IFinAIServiceClient finAIServiceClient,
        ILogger<AlpacaTestController> logger,
        IRedisPublisher redisPublisher,
        IRedisService redisService,
        IAlpacaDataService alpacaDataService)
    {
        _env = env;
        _alpacaRepository = alpacaRepository;
        _strategyTestService = backtestService;
        _strategyServiceClient = strategyServiceClient;
        _finAIServiceClient = finAIServiceClient;
        _logger = logger;
        _publisher = redisPublisher;     
        _redisService = redisService;
        _alpacaDataService = alpacaDataService;
    }


    [HttpGet("ai-strategies")]
    public async Task<IActionResult> GetAiStrategies()
    {
        var models = await _finAIServiceClient.GetAiStrategies();    
        return Ok(models);
    }

    [HttpGet("running-execution")]
    public async Task<IActionResult> GetAlpacaExecution()
    {
        var strategyTrackings = await _alpacaRepository.GetAllActiveStrategyTrackings();  
        return Ok(strategyTrackings);
    }


    [HttpPost("test-execution")]
    public async Task<IActionResult> TestAlpacaExecution([FromBody] StrategySettingsDTO strategySettings)
    {

        var flagKey = RedisUtilities.GetFeatureFlagKey("ai-strategy");
    
        var flagValue = await _redisService.GetStringAsync(flagKey);
        if (flagValue == "true")
        {
            return Conflict("AI test stream is already running.");
        }
        await _redisService.SetStringAsync(flagKey, "true");

        var tenDaysAgo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-20));
        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));

        var calendars = await _alpacaRepository.GetCalendarAsync(tenDaysAgo, yesterday);

        var lastTradingDay = calendars.LastOrDefault();
        if (lastTradingDay == null)
        {
            return Conflict("No trading day found in the last 20 days.");
        }

        var startDate = DateTime.SpecifyKind(tenDaysAgo.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);;
        var endDate = DateTime.SpecifyKind(lastTradingDay.TradingDate.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);

        var result = await _finAIServiceClient.StartAlpacaPaperTradingAsync(strategySettings);
        var trades = await _alpacaRepository.GetHistoricalTrades(strategySettings.Asset, startDate, endDate);

        // Fire-and-forget: errors are logged inside PublishTradesToStream, client doesn't need to wait for it.
        _ = _redisService.PublishTradesToStream(strategySettings.Asset, trades, 300000);

        return Ok();
    }

    [HttpPost("start-execution")]
    public async Task<IActionResult> StartAlpacaExecution([FromBody] StrategySettingsDTO strategySettings)
    {

        var strategyTracking = await _alpacaRepository.GetLatestActiveStrategyTracking(strategySettings.Name);  
        if (strategyTracking != null)
        {
            return Conflict("Strategy is already being tracked.");
        }
        await _alpacaRepository.AddStrategyTrackingAsync(new AlpacaStrategyTracking
        {
            Name = strategySettings.Name,
            StartedAtUtc = DateTime.UtcNow,
            StrategyType = strategySettings.StrategyType,
            Asset = strategySettings.Asset
        });

        var tenDaysAgo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10));
        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
   
        var calendars = await _alpacaRepository.GetCalendarAsync(tenDaysAgo, yesterday);

        var lastTradingDay = calendars.LastOrDefault();
        if(lastTradingDay == null)
        {
            return Conflict("No trading day found in the last 10 days.");
        }

        var startDate = DateTime.SpecifyKind(lastTradingDay.TradingDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var endDate = DateTime.SpecifyKind(lastTradingDay.TradingDate.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);

        var trades = await _alpacaRepository.GetHistoricalTrades(strategySettings.Asset, startDate, endDate);
        var result = await _finAIServiceClient.StartAlpacaPaperTradingAsync(strategySettings);

        _ = _redisService.PublishTradesToStream(strategySettings.Asset, trades, 100000);
        await _alpacaDataService.SubscribeToTradeUpdates(strategySettings.Asset);

        return Ok();
    }

    [HttpPut("stop-execution")]
    public async Task<IActionResult> StopAlpacaExecution([FromBody] StrategySettingsDTO strategySettings)
    {

        var strategyTracking = await _alpacaRepository.GetLatestActiveStrategyTracking(strategySettings.Name);
        if (strategyTracking == null)
        {
            return Conflict("No active strategy tracking found.");
        }

        await _alpacaRepository.StopStrategyTrackingAsync(strategySettings.Name);

        await _finAIServiceClient.StopAlpacaPaperTradingAsync(strategySettings);

        if (strategySettings.StrategyType == StrategyEnum.PaperTrading)
        {
            await _alpacaDataService.UnSubscribeFromTradeUpdates(strategySettings.Asset);    
        }
        
        return Ok();
    }

    [HttpPut("subscribe-trade-updates/{symbol}")]
    public async Task<IActionResult> SubscribeToTradeUpdates(string symbol)
    {
        await _alpacaDataService.SubscribeToTradeUpdates(symbol);
        return Ok();
    }

    [HttpPut("unsubscribe-trade-updates/{symbol}")]
    public async Task<IActionResult> UnSubscribeFromTradeUpdates(string symbol)
    {
        await _alpacaDataService.UnSubscribeFromTradeUpdates(symbol);
        return Ok();
    }

    [HttpPost("run-test")]
    public async Task<ActionResult> RunBacktest([FromBody] StrategySettingsModel settings)
    {
        if (settings == null)
        {
            return BadRequest("StrategySettingsModel cannot be null");
        }

        var userId = HttpContext.Items["UserId"]?.ToString();
        settings.UserId = new Guid(userId!);
        settings.Id = Guid.NewGuid();
        settings.StartDate = settings.StartDate.ToUniversalTime();
        settings.EndDate = settings.EndDate.ToUniversalTime();
        settings.StampStart = DateTime.UtcNow.ToUniversalTime();
        settings.StampEnd = DateTimeExtension.PostgresMinValue().ToUniversalTime();
        await _strategyTestService.StoreBarsToRedis(settings.Asset);

        var startResponse = await _strategyServiceClient.StartStrategyAsync(settings);
        if (startResponse == "true")
        {
            var notificationTopic = RedisUtilities.GetChannelName(RedisChannelEnum.Notification);
            var notificationMessage = NotificationMessageFactory.CreateNotificationMessage(
                settings.UserId,
                NotificationEnum.BacktestStart
            );
            await _publisher.PublishAsync(notificationTopic, notificationMessage.ToJson());

            var msg = new BacktestMessage
            {
                StrategyId = settings.Id
            };
            await _publisher.PublishAsync(RedisUtilities.GetChannelName(RedisChannelEnum.Strategy), msg.ToJson());

        }
        return Ok(startResponse);
    }

    [HttpGet("test-optimization-service")]
    public async Task<IActionResult> TestOptimizationAsync()
    {
        var test = await _finAIServiceClient.TestOptimizationAsync();
        return Ok(test);
    }

    [HttpPost("optimize")]
    public async Task<IActionResult> RunOptimization([FromBody] StrategySettingsModel settings)
    {
        if (settings == null)
        {
            return BadRequest("StrategySettingsModel cannot be null");
        }
        var userId = HttpContext.Items["UserId"]?.ToString();
        settings.UserId = new Guid(userId!);
        settings.Id = Guid.NewGuid();
        settings.StartDate = settings.StartDate.ToUniversalTime();
        settings.EndDate = settings.EndDate.ToUniversalTime();
        settings.StampStart = DateTime.UtcNow.ToUniversalTime();
        settings.StampEnd = DateTimeExtension.PostgresMinValue().ToUniversalTime();
        var result = await _strategyServiceClient.StartStrategyAsync(settings);
        if (result == "true")
        {
            await _strategyTestService.OptimizeStrategy(settings);
        }
        return Ok(result);
    }
}
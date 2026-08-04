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
 

    public AlpacaTestController(
        IWebHostEnvironment env,
        IAlpacaRepository alpacaRepository,
        IStrategyTestService backtestService,
        IStrategyServiceClient strategyServiceClient,
        IFinAIServiceClient finAIServiceClient,
        ILogger<AlpacaTestController> logger,
        IRedisPublisher redisPublisher)
    {
        _env = env;
        _alpacaRepository = alpacaRepository;
        _strategyTestService = backtestService;
        _strategyServiceClient = strategyServiceClient;
        _finAIServiceClient = finAIServiceClient;
        _logger = logger;
        _publisher = redisPublisher;     
    }

    [HttpGet("historical-bars/{symbol}")]
    public async Task<IActionResult> GetHistoricalBarsBySymbol(string symbol, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var bars = await _alpacaRepository.GetHistoricalBars(symbol, startDate.ToUniversalTime(), endDate.ToUniversalTime());
        return Ok(bars);
    }

    [HttpGet("historical-quotes/{symbol}")]
    public async Task<ActionResult<IEnumerable<PriceQuote>>> GetHistoricalQuotesBySymbol(string symbol, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var bars = await _alpacaRepository.GetHistoricalBars(symbol, startDate.ToUniversalTime(), endDate.ToUniversalTime());

        var quotes = new List<PriceQuote>();
        foreach (var bar in bars)
        {
            var q = new PriceQuote
            {
                Symbol = symbol,
                AskPrice = bar.C + 0.1m,
                BidPrice = bar.C - 0.1m,
                TimestampUtc = bar.T.ToUniversalTime(),
                Volume = bar.V
            };
            quotes.Add(q);
        }
        return quotes;
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

    [HttpPost("store-to-redis")]
    public async Task<IActionResult> StoreToRedis([FromBody] string asset)
    {
        await _strategyTestService.StoreBarsToRedis(asset);
        return Ok();
    }

    [HttpGet("save-assets")]
    public async Task<IActionResult> SaveAssets()
    {
        string path = Path.Combine(_env.ContentRootPath, "Assets", "alpaca-assets.json");

        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                var jsonString = await reader.ReadToEndAsync();

                var assets = JsonConvert.DeserializeObject<List<AlpacaAsset>>(jsonString);
                if (assets != null)
                {
                    await _alpacaRepository.AddAssetsAsync(assets);
                }
                return Ok();
            }
        }
    }
}
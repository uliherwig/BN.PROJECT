namespace BN.PROJECT.AlpacaService;

[ApiController]
[Route("[controller]")]
// [AuthorizeUser(["user", "admin"])]
public class AlpacaTradingController : ControllerBase
{
    private readonly IAlpacaTradingService _alpacaTradingService;
    private readonly IAlpacaRepository _alpacaRepository;
    private readonly IStrategyTestService _strategyTestService;
    private readonly IStrategyServiceClient _strategyServiceClient;
    private readonly IFinAIServiceClient _finAIServiceClient;
    private readonly IRedisService _redisService;


    public AlpacaTradingController(
        IAlpacaTradingService alpacaTradingService, 
        IAlpacaRepository alpacaRepository, 
        IStrategyTestService strategyTestService, 
        IStrategyServiceClient strategyServiceClient,
        IFinAIServiceClient finAIServiceClient,
        IRedisService redisService)
    {
        _alpacaTradingService = alpacaTradingService;
        _alpacaRepository = alpacaRepository;
        _strategyTestService = strategyTestService;
        _strategyServiceClient = strategyServiceClient;
        _finAIServiceClient = finAIServiceClient;
        _redisService = redisService;
    }

    [HttpPost("start-execution/{strategyName}")]
    public async Task<IActionResult> StartAlpacaExecution(string strategyName)
    {
        // var result = await _finAIServiceClient.StartAlpacaPaperTradingAsync(strategyName);
        var flagKey = RedisUtilities.GetFeatureFlagKey("push-trades-to-stream");
        await _redisService.SetStringAsync(flagKey, "true");


        var trades = await _alpacaRepository.GetHistoricalTrades("SPY", DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);

        await _redisService.PublishTradesToStream("SPY", trades, 100000);

        return Ok();
    }

    [HttpPut("stop-execution")]
    public async Task<IActionResult> StopAlpacaExecution()
    {
        // var result = await _finAIServiceClient.StopAlpacaPaperTradingAsync();
        var flagKey = RedisUtilities.GetFeatureFlagKey("push-trades-to-stream");
        await _redisService.SetStringAsync(flagKey, "false");
        return Ok();
    }

    [HttpGet("clock")]
    public async Task<IActionResult> GetClockAsync()
    {
        var result = await _alpacaTradingService.GetClockAsync();
        return Ok(result);
    }

    [HttpGet("interval-calendar")]
    public async Task<IActionResult> ListIntervalCalendarAsync(DateOnly startDate, DateOnly endDate = default)
    {
        var result = await _alpacaTradingService.ListIntervalCalendarAsync(startDate, endDate == default ? DateOnly.FromDateTime(DateTime.UtcNow) : endDate);
        return Ok(result);
    }

    [HttpGet("assets")]
    public async Task<IActionResult> GetAssets()
    {
        var assets = await _alpacaRepository.GetAssets();
        return Ok(assets);
    }

    [HttpGet("asset/{symbol}")]
    public async Task<IActionResult> GetAssetBySymbol(string symbol)
    {
        var asset = await _alpacaTradingService.GetAssetBySymbolAsync(symbol);
        return Ok(asset);
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetAllOrders(OrderStatusFilter orderStatusFilter)
    {
        var orders = await _alpacaTradingService.GetAllOrdersAsync(orderStatusFilter);
        return Ok(orders);
    }

    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetOrderById(string orderId)
    {
        var order = await _alpacaTradingService.GetOrderByIdAsync(orderId);
        return Ok(order);
    }

    [HttpDelete("order/{orderId}")]
    public async Task<IActionResult> CancelOrderById(Guid orderId)
    {
        var result = await _alpacaTradingService.CancelOrderByIdAsync(orderId);
        return Ok(result);
    }

    [HttpGet("positions")]
    public async Task<IActionResult> GetPositions()
    {
        var positions = await _alpacaTradingService.GetAllOpenPositions();
        foreach (var position in positions)
        {
            position.Symbol = position.Symbol.ToUpper();
        }

        return Ok(positions);
    }

    [HttpGet("position/{symbol}")]
    public async Task<IActionResult> GetPositionsBySymbol(string symbol)
    {  
        var position = await _alpacaTradingService.GetPositionsBySymbol(symbol);
        return Ok(position);
    }

    [HttpDelete("position/{symbol}")]
    public async Task<IActionResult> ClosePosition(string symbol)
    {
        var result = await _alpacaTradingService.ClosePositionOrder(symbol);
        return Ok(result);
    }
}

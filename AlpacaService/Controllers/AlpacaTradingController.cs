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
    private readonly IPositionLifecycleService _positionLifecycleService;


    public AlpacaTradingController(
        IAlpacaTradingService alpacaTradingService,
        IAlpacaRepository alpacaRepository,
        IStrategyTestService strategyTestService,
        IStrategyServiceClient strategyServiceClient,
        IFinAIServiceClient finAIServiceClient,
        IRedisService redisService,
        IPositionLifecycleService positionLifecycleService)
    {
        _alpacaTradingService = alpacaTradingService;
        _alpacaRepository = alpacaRepository;
        _strategyTestService = strategyTestService;
        _strategyServiceClient = strategyServiceClient;
        _finAIServiceClient = finAIServiceClient;
        _redisService = redisService;
        _positionLifecycleService = positionLifecycleService;
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

    [HttpPost("order")]
    public async Task<IActionResult> CreateMarketOrder(OrderRequest orderRequest)
    {

        var result = await _alpacaTradingService.CreateOrderAsync(orderRequest);
        return Ok(result);
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
        return Ok(positions);
    }

    [HttpGet("position/{symbol}")]
    public async Task<IActionResult> GetPositionsBySymbol(string symbol)
    {
        var position = await _alpacaTradingService.GetPositionsBySymbol(symbol);
        return Ok(position);
    }

    [HttpDelete("position/{symbol}")]
    public async Task<IActionResult> ClosePositionBySymbol(string symbol)
    {
        var result = await _alpacaTradingService.ClosePositionOrder(symbol);
        return Ok(result);
    }

    [HttpDelete("positions")]
    public async Task<IActionResult> CloseAllPositions()
    {
        var result = await _alpacaTradingService.CloseAllPositions();
        return Ok(result);
    }

    [HttpPost("positions/close-expired")]
    public async Task<IActionResult> CloseExpiredPositions([FromQuery] int maxHoldingMinutes = 60)
    {
        await _positionLifecycleService.CloseExpiredPositionsAsync(TimeSpan.FromMinutes(maxHoldingMinutes));
        return Ok();
    }

    [HttpPost("positions/close-eod")]
    public async Task<IActionResult> CloseEndOfDayPositions()
    {
        await _positionLifecycleService.CloseAllPositionsEndOfDayAsync();
        return Ok();
    }

    [HttpPost("ai-market-test-order")]
    public async Task<IActionResult> CreateAIMarketTestOrder(TestOrderRequest orderRequest)
    {
        var strategy = (await _alpacaRepository.GetStrategyTrackings())
            .FirstOrDefault(s => s.Asset == orderRequest.Asset && s.StoppedAtUtc != null);

        if (strategy == null)
        {
            return BadRequest("No active strategy found for this asset");
        }

        // Check if AI market order for this symbol has already been executed   - only one AI market order per symbol is allowed
        var position = await _alpacaRepository.GetLatestOpenPositionTracking(orderRequest.Asset);

        if (position != null)
        {

            if (position.Side ==orderRequest.Side)
            {

                // Not an error: skipping a duplicate AI order for an already-open symbol is expected behavior.
                return Ok(new
                {
                    skipped = true,
                    message = "AI market order for this symbol has already been executed.",
                    symbol = position.Asset,
                    openedAtUtc = position.OpenedAtUtc
                });
            }
            else
            {
                position.ClosedAtUtc = orderRequest.StampUtc;
                position.ClosePrice = orderRequest.Price;
                await _alpacaRepository.UpdatePositionTrackingAsync(position);
                return Ok(new
                {
                    skipped = true,
                    message = "Position closed",
                    symbol = position.Asset,
                    openedAtUtc = position.ClosedAtUtc
                });
            }

        }
        // Value is already UTC; only the Kind needs fixing (Npgsql rejects Unspecified for timestamptz).
        orderRequest.StampUtc = DateTime.SpecifyKind(orderRequest.StampUtc, DateTimeKind.Utc);
        var positionTracking = new AlpacaPositionTracking
        {
            StrategyId = strategy.Id,
            Asset = orderRequest.Asset,
            Quantity = (int)orderRequest.Quantity,
            Side = orderRequest.Side,
            OpenPrice = orderRequest.Price,
            OpenedAtUtc = orderRequest.StampUtc,
            ClosedAtUtc = null
        };
        await _alpacaRepository.AddPositionTrackingAsync(positionTracking);
        return Ok(positionTracking);
    }

    [HttpPost("ai-market-order")]
    public async Task<IActionResult> CreateAIMarketOrder(OrderRequest orderRequest)
    {
        // Check if AI market order for this symbol has already been executed   - only one AI market order per symbol is allowed
        var isPositionAlreadyExecuted = await _alpacaRepository.GetLatestOpenPositionTracking(orderRequest.Asset);

        if (isPositionAlreadyExecuted != null)
        {
            // Not an error: skipping a duplicate AI order for an already-open symbol is expected behavior.
            return Ok(new
            {
                skipped = true,
                message = "AI market order for this symbol has already been executed.",
                symbol = isPositionAlreadyExecuted.Asset,
                openedAtUtc = isPositionAlreadyExecuted.OpenedAtUtc
            });
        }

        var strategy = (await _alpacaRepository.GetAllActiveStrategyTrackings())
            .FirstOrDefault(s => s.Asset == orderRequest.Asset);

        if (strategy == null)
        {
            return BadRequest("No active strategy found for this asset");
        }

        var result = await _alpacaTradingService.CreateOrderAsync(orderRequest);
        if (result == false)
        {
            return BadRequest("Failed to create AI market order.");
        }
        var positionTracking = new AlpacaPositionTracking
        {
            StrategyId = strategy.Id,
            Asset = orderRequest.Asset,
            Quantity = (int)orderRequest.Quantity,
            Side = orderRequest.Side,
            OpenPrice = 0,
            OpenedAtUtc = DateTime.UtcNow,
            ClosedAtUtc = null
        };
        await _alpacaRepository.AddPositionTrackingAsync(positionTracking);
        return Ok(positionTracking);
    }

}

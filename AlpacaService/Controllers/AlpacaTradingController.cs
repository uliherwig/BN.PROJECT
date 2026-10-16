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

    [HttpPost("ai-market-order")]
    public async Task<IActionResult> CreateAIMarketOrder(OrderRequest orderRequest)
    {
        var strategy = (await _alpacaRepository.GetStrategyTrackings())
            .FirstOrDefault(s => s.Asset == orderRequest.Asset && s.StoppedAtUtc == null);

        if (strategy == null)
        {
            return BadRequest("No active strategy found for this asset");
        }

        // Check if AI market order for this symbol has already been executed   - only one AI market order per symbol is allowed
        var position = await _alpacaRepository.GetLatestOpenPositionTracking(orderRequest.Asset, strategy.Id);

        if (position != null)
        {
            if (position.Side == orderRequest.Side)
            {
                return Ok(new
                {
                    executed = false,
                    message = $"Alpaca position with symbol {position.Asset} and Side {position.Side} already running .",
                });
            }
            if (strategy.StrategyType == StrategyEnum.PaperTrading)
            {
                var closeOrder = await _alpacaTradingService.CreateOrderAsync(orderRequest);
                if (closeOrder == null)
                {
                    return Conflict("Failed to create AI market order.");
                }
            }

            position.ClosedAtUtc = (DateTime)orderRequest.StampUtc;
            position.ClosePrice = (decimal)orderRequest.Price;
            await _alpacaRepository.UpdatePositionTrackingAsync(position);
            return Ok(new
            {
                executed = true,
                message = $"Alpaca reverse order with symbol {position.Asset} and Side {position.Side} executed .",
            });
        }

        if (strategy.StrategyType == StrategyEnum.PaperTrading)
        {
            var openOrder = await _alpacaTradingService.CreateOrderAsync(orderRequest);
            if (openOrder == null)
            {
                return Conflict("Failed to create AI market order.");
            }
        }

        var positionTracking = new AlpacaPositionTracking
        {
            StrategyId = strategy.Id,
            Asset = orderRequest.Asset,
            Quantity = (int)orderRequest.Quantity,
            Side = orderRequest.Side,
            OpenPrice = (decimal)orderRequest.Price,
            OpenedAtUtc = (DateTime)orderRequest.StampUtc,
            ClosedAtUtc = null
        };
        await _alpacaRepository.AddPositionTrackingAsync(positionTracking);
        return Ok(new
        {
            executed = true,
            message = $"Alpaca new order with symbol {orderRequest.Asset} and Side {orderRequest.Side} executed .",
        });
    }
}

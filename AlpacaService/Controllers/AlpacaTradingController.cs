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

        var symbol = orderRequest.Symbol;
        var qty = (int)orderRequest.Quantity;
        var side = orderRequest.Side == "Buy" ? OrderSide.Buy : OrderSide.Sell;
        //  if (orderRequest.PriceClose > 0)
        //  {
        //      side = orderRequest.Side == "Sell" ? OrderSide.Buy : OrderSide.Sell;
        //  }

        var orderType = OrderType.Market;
        var timeInForce = TimeInForce.Day;

        var order = await _alpacaTradingService.CreateOrderAsync(symbol, qty, side, orderType, timeInForce);
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
        // Check if AI market order for this symbol has already been executed   - only one AI market order per symbol is allowed
        var isPositionAlreadyExecuted = await _alpacaRepository.GetLatestOpenPositionTracking(orderRequest.Symbol);

        if (isPositionAlreadyExecuted != null)
        {
            // Not an error: skipping a duplicate AI order for an already-open symbol is expected behavior.
            return Ok(new
            {
                skipped = true,
                message = "AI market order for this symbol has already been executed.",
                symbol = isPositionAlreadyExecuted.Symbol,
                openedAtUtc = isPositionAlreadyExecuted.OpenedAtUtc
            });
        }

        orderRequest.Symbol = orderRequest.Symbol.ToUpper();
        orderRequest.Side = orderRequest.Side.ToUpper();
        // Value is already UTC; only the Kind needs fixing (Npgsql rejects Unspecified for timestamptz).
        orderRequest.OpenedAtUtc = DateTime.SpecifyKind(orderRequest.OpenedAtUtc, DateTimeKind.Utc);
        orderRequest.OrderType = "Market";
        orderRequest.TimeInForce = "Day";

 
        var positionTracking = new AlpacaPositionTracking
        {
            Symbol = orderRequest.Symbol,
            OpenedAtUtc = orderRequest.OpenedAtUtc,
            ClosedAtUtc = null

        };
        await _alpacaRepository.AddPositionTrackingAsync(positionTracking);
        return Ok(positionTracking);
    }

    [HttpPost("ai-market-order")]
    public async Task<IActionResult> CreateAIMarketOrder(OrderRequest orderRequest)
    {
        // Check if AI market order for this symbol has already been executed   - only one AI market order per symbol is allowed
        var isPositionAlreadyExecuted = await _alpacaRepository.GetLatestOpenPositionTracking(orderRequest.Symbol);

        if (isPositionAlreadyExecuted != null)
        {
            // Not an error: skipping a duplicate AI order for an already-open symbol is expected behavior.
            return Ok(new
            {
                skipped = true,
                message = "AI market order for this symbol has already been executed.",
                symbol = isPositionAlreadyExecuted.Symbol,
                openedAtUtc = isPositionAlreadyExecuted.OpenedAtUtc
            });
        }

        orderRequest.Symbol = orderRequest.Symbol.ToUpper();
        orderRequest.Side = orderRequest.Side.ToUpper();
        orderRequest.OrderType = "Market";
        orderRequest.TimeInForce = "Day";

        var alpacaOrder = await _alpacaTradingService.CreateOrderAsync(orderRequest.Symbol, (int)orderRequest.Quantity, orderRequest.Side == "Buy" ? OrderSide.Buy : OrderSide.Sell, OrderType.Market, TimeInForce.Day);
        if(alpacaOrder == null || alpacaOrder.CreatedAtUtc == null)
        {
            return BadRequest("Failed to create AI market order.");
        }
        var positionTracking = new AlpacaPositionTracking
        {
            Symbol = orderRequest.Symbol,
            OpenedAtUtc = (DateTime)alpacaOrder.CreatedAtUtc,
            ClosedAtUtc = null

        };
        await _alpacaRepository.AddPositionTrackingAsync(positionTracking);
        return Ok(alpacaOrder);
    }

}

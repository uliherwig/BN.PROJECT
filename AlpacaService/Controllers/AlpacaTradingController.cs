namespace BN.PROJECT.AlpacaService;

[ApiController]
[Route("[controller]")]
public class AlpacaTradingController : ControllerBase
{
    private readonly IAlpacaTradingService _alpacaTradingService;
    private readonly IAlpacaRepository _alpacaRepository;

    public AlpacaTradingController(
        IAlpacaTradingService alpacaTradingService, IAlpacaRepository alpacaRepository)
    {
        _alpacaTradingService = alpacaTradingService;
        _alpacaRepository = alpacaRepository;
    }

    [HttpGet("account/{userId}")]
    public async Task<IActionResult> GetAccount(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(new { Message = "UserId cannot be null" });
        }
        var userSettings = await _alpacaRepository.GetUserSettingsAsync(userId);
        if (userSettings == null)
        {
            return NotFound(new { Message = "NoCredentials" });
        }

        var account = await _alpacaTradingService.GetAccountAsync(userSettings);
        if (account == null)
        {
            return NotFound(new { Message = "WrongCredentials" });
        }

        return Ok(account);
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

    [HttpPost("market-order")]
    public async Task<IActionResult> CreateMarketOrder(string userId, string symbol, int quantity, bool isBuy)
    {
        OrderQuantity qty = quantity;
        OrderSide side = isBuy ? OrderSide.Buy : OrderSide.Sell;
        var order = await _alpacaTradingService.CreateOrderAsync(userId, symbol, qty, side, OrderType.Market, TimeInForce.Day);
        if (order != null)
        {
            await _alpacaRepository.AddOrderAsync(order);
        }
        return Ok(order);
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetAllOrders(string userId, OrderStatusFilter orderStatusFilter)
    {
        var orders = await _alpacaTradingService.GetAllOrdersAsync(userId, orderStatusFilter);
        return Ok(orders);
    }

    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetOrderById(string userId, string orderId)
    {
        var order = await _alpacaTradingService.GetOrderByIdAsync(userId, orderId);
        return Ok(order);
    }

    [HttpDelete("order/{orderId}")]
    public async Task<IActionResult> CancelOrderById(string userId, Guid orderId)
    {
        var result = await _alpacaTradingService.CancelOrderByIdAsync(userId, orderId);
        return Ok(result);
    }

    [HttpGet("positions")]
    public async Task<IActionResult> GetPositions(string userId)
    {
        var positions = await _alpacaTradingService.GetAllOpenPositions(userId);
        return Ok(positions);
    }

    [HttpGet("position/{symbol}")]
    public async Task<IActionResult> GetPositionsBySymbol(string userId, string symbol)
    {
        var position = await _alpacaTradingService.GetPositionsBySymbol(userId, symbol);
        return Ok(position);
    }

    [HttpDelete("position/{symbol}")]
    public async Task<IActionResult> ClosePosition(string userId, string symbol)
    {
        var result = await _alpacaTradingService.ClosePositionOrder(userId, symbol);
        return Ok(result);
    }
}
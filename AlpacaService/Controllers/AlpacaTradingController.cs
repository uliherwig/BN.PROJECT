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
        var result = new BrokerAccount();
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("UserId cannot be null or empty");
        }
        var userSettings = await _alpacaRepository.GetUserSettingsAsync(userId);
        if (userSettings == null)
        {
            result.AccountStatus = AccountStatusEnum.NoCredentials;
        }
        else
        {
            var account = await _alpacaTradingService.GetAccountAsync(userSettings);
            if (account != null)
            {
                result = account.ToBrokerAccount(AccountStatusEnum.AccountLoaded, new Guid(userId));
            }
            else
            {
                result.AccountStatus = AccountStatusEnum.WrongCredentials;
                result.UserId = new Guid(userId);
            }
        }
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
    public async Task<IActionResult> GetAllOrders(string userId, OrderStatusFilter orderStatusFilter)
    {
        var userSettings = await _alpacaRepository.GetUserSettingsAsync(userId);
        if (userSettings == null)
        {
            return NotFound("User settings not found");
        }

        var orders = await _alpacaTradingService.GetAllOrdersAsync(userSettings, orderStatusFilter);
        return Ok(orders);
    }

    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetOrderById(string userId, string orderId)
    {
        var userSettings = await _alpacaRepository.GetUserSettingsAsync(userId);
        if (userSettings == null)
        {
            return NotFound("User settings not found");
        }

        var order = await _alpacaTradingService.GetOrderByIdAsync(userSettings, orderId);
        return Ok(order);
    }

    [HttpDelete("order/{orderId}")]
    public async Task<IActionResult> CancelOrderById(string userId, Guid orderId)
    {
        var userSettings = await _alpacaRepository.GetUserSettingsAsync(userId);
        if (userSettings == null)
        {
            return NotFound("User settings not found");
        }

        var result = await _alpacaTradingService.CancelOrderByIdAsync(userSettings, orderId);
        return Ok(result);
    }

    [HttpGet("positions")]
    public async Task<IActionResult> GetPositions(string userId)
    {
        var userSettings = await _alpacaRepository.GetUserSettingsAsync(userId);
        if (userSettings == null)
        {
            return NotFound("User settings not found");
        }

        var positions = await _alpacaTradingService.GetAllOpenPositions(userSettings);
        return Ok(positions);
    }

    [HttpGet("position/{symbol}")]
    public async Task<IActionResult> GetPositionsBySymbol(string userId, string symbol)
    {
        var userSettings = await _alpacaRepository.GetUserSettingsAsync(userId);
        if (userSettings == null)
        {
            return NotFound("User settings not found");
        }

        var position = await _alpacaTradingService.GetPositionsBySymbol(userSettings, symbol);
        return Ok(position);
    }

    [HttpDelete("position/{symbol}")]
    public async Task<IActionResult> ClosePosition(string userId, string symbol)
    {
        var userSettings = await _alpacaRepository.GetUserSettingsAsync(userId);
        if (userSettings == null)
        {
            return NotFound("User settings not found");
        }

        var result = await _alpacaTradingService.ClosePositionOrder(userSettings, symbol);
        return Ok(result);
    }
}

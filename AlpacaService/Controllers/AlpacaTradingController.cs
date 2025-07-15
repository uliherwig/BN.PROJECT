namespace BN.PROJECT.AlpacaService;

[ApiController]
[Route("[controller]")]
[AuthorizeUser(["user", "admin"])]
public class AlpacaTradingController : ControllerBase
{
    private readonly IAlpacaTradingService _alpacaTradingService;
    private readonly IAlpacaRepository _alpacaRepository;
    private readonly IStrategyTestService _strategyTestService;
    private readonly IStrategyServiceClient _strategyServiceClient;

    public AlpacaTradingController(
        IAlpacaTradingService alpacaTradingService, IAlpacaRepository alpacaRepository, IStrategyTestService strategyTestService, IStrategyServiceClient strategyServiceClient)
    {
        _alpacaTradingService = alpacaTradingService;
        _alpacaRepository = alpacaRepository;
        _strategyTestService = strategyTestService;
        _strategyServiceClient = strategyServiceClient;
    }

    [HttpGet("account")]
    public async Task<IActionResult> GetAccount()
    {
        var userId = HttpContext.Items["UserId"]?.ToString();
        var result = new BrokerAccount();
        var userSettings = await _alpacaRepository.GetUserSettingsAsync(userId!);
        if (userSettings == null)
        {
            result.AccountStatus = AccountStatusEnum.NoCredentials;
        }
        else
        {
            var account = await _alpacaTradingService.GetAccountAsync(userSettings);
            if (account != null)
            {
                result = account.ToBrokerAccount(AccountStatusEnum.AccountLoaded, new Guid(userId!));
            }
            else
            {
                result.AccountStatus = AccountStatusEnum.WrongCredentials;
                result.UserId = new Guid(userId!);
            }
        }
        return Ok(result);
    }

    [HttpGet("get-execution")]
    public async Task<IActionResult> GetActiveExecutionByUserId()
    {
        var userId = HttpContext.Items["UserId"]?.ToString();
        var exec = await _alpacaRepository.GetActiveAlpacaExecutionByUserIdAsync(Guid.Parse(userId!));
        if (exec == null)
        {
            exec = new AlpacaExecutionModel();
        }
        return Ok(exec);
    }

    [HttpPost("start-execution/{strategyId}")]
    public async Task<IActionResult> StartAlpacaExecution(Guid strategyId)
    {
        var userId = HttpContext.Items["UserId"]?.ToString();
        var strategy = await _strategyServiceClient.GetStrategyAsync(strategyId.ToString());
        if (strategy == null)
        {
            return BadRequest("Strategy not found");
        }
        if (strategy.UserId != Guid.Parse(userId!))
        {
            return BadRequest("User is not the owner of the strategy");
        }
        var alpacaExecution = new AlpacaExecutionModel
        {
            Id = Guid.NewGuid(),
            UserId = strategy.UserId,
            StrategyId = strategy.Id,
            Assets = strategy.Asset,
            StrategyType = strategy.StrategyType,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.MinValue
        };

        await _alpacaRepository.AddAlpacaExecutionAsync(alpacaExecution);

        await _strategyTestService.StartExecution(strategy.UserId, strategyId);

        return Ok(alpacaExecution);
    }

    [HttpPut("stop-execution")]
    public async Task<IActionResult> StopAlpacaExecution()
    {
        var userId = HttpContext.Items["UserId"]?.ToString();
        var execution = await _alpacaRepository.GetActiveAlpacaExecutionByUserIdAsync(Guid.Parse(userId!));
        if (execution == null)
        {
            return BadRequest("No active execution found");
        }
        execution.EndDate = DateTime.UtcNow.ToUniversalTime();
        await _alpacaRepository.UpdateAlpacaExecutionAsync(execution);
        return Ok();
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
        var userId = HttpContext.Items["UserId"]?.ToString();

        var userSettings = await _alpacaRepository.GetUserSettingsAsync(userId!);
        if (userSettings == null)
        {
            return NotFound("User settings not found");
        }

        var orders = await _alpacaTradingService.GetAllOrdersAsync(userSettings, orderStatusFilter);
        return Ok(orders);
    }

    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetOrderById(string orderId)
    {
        var userId = HttpContext.Items["UserId"]?.ToString();

        var userSettings = await _alpacaRepository.GetUserSettingsAsync(userId!);
        if (userSettings == null)
        {
            return NotFound("User settings not found");
        }

        var order = await _alpacaTradingService.GetOrderByIdAsync(userSettings, orderId);
        return Ok(order);
    }

    [HttpDelete("order/{orderId}")]
    public async Task<IActionResult> CancelOrderById(Guid orderId)
    {
        var userId = HttpContext.Items["UserId"]?.ToString();

        var userSettings = await _alpacaRepository.GetUserSettingsAsync(userId!);
        if (userSettings == null)
        {
            return NotFound("User settings not found");
        }

        var result = await _alpacaTradingService.CancelOrderByIdAsync(userSettings, orderId);
        return Ok(result);
    }

    [HttpGet("positions")]
    public async Task<IActionResult> GetPositions()
    {
        var userId = HttpContext.Items["UserId"]?.ToString();
        var userSettings = await _alpacaRepository.GetUserSettingsAsync(userId!);
        if (userSettings == null)
        {
            return NotFound("User settings not found");
        }

        var positions = await _alpacaTradingService.GetAllOpenPositions(userSettings);
        foreach (var position in positions)
        {
            position.Symbol = position.Symbol.ToUpper();
        }

        return Ok(positions);
    }

    [HttpGet("position/{symbol}")]
    public async Task<IActionResult> GetPositionsBySymbol(string symbol)
    {
        var userId = HttpContext.Items["UserId"]?.ToString();

        var userSettings = await _alpacaRepository.GetUserSettingsAsync(userId!);
        if (userSettings == null)
        {
            return NotFound("User settings not found");
        }

        var position = await _alpacaTradingService.GetPositionsBySymbol(userSettings, symbol);
        return Ok(position);
    }

    [HttpDelete("position/{symbol}")]
    public async Task<IActionResult> ClosePosition(string symbol)
    {
        var userId = HttpContext.Items["UserId"]?.ToString();

        var userSettings = await _alpacaRepository.GetUserSettingsAsync(userId!);
        if (userSettings == null)
        {
            return NotFound("User settings not found");
        }

        var result = await _alpacaTradingService.ClosePositionOrder(userSettings, symbol);
        return Ok(result);
    }

    [HttpDelete("delete-executions")]
    public async Task<IActionResult> DeleteExecutionsByUserId()
    {
        var userId = HttpContext.Items["UserId"]?.ToString();
        await _alpacaRepository.DeleteAlpacaExecutionsAsync(new Guid(userId!));
        return Ok();
    }
}

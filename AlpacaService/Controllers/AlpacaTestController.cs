namespace BN.PROJECT.AlpacaService;

[Route("[controller]")]
[ApiController]
[AuthorizeUser(["user","admin"])]
public class AlpacaTestController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly IAlpacaRepository _alpacaRepository;
    private readonly IStrategyTestService _strategyTestService;
    private readonly IStrategyServiceClient _strategyServiceClient;
    private readonly ILogger<AlpacaTestController> _logger;

    public AlpacaTestController(
        IWebHostEnvironment env,
        IAlpacaRepository alpacaRepository,
        IStrategyTestService backtestService,
        IStrategyServiceClient strategyServiceClient,
        ILogger<AlpacaTestController> logger)
    {
        _env = env;
        _alpacaRepository = alpacaRepository;
        _strategyTestService = backtestService;
        _strategyServiceClient = strategyServiceClient;
        _logger = logger;
    }

    [HttpGet("historical-bars/{symbol}")]
    public async Task<IActionResult> GetHistoricalBarsBySymbol(string symbol, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var bars = await _alpacaRepository.GetHistoricalBars(symbol, startDate.ToUniversalTime(), endDate.ToUniversalTime());
        return Ok(bars);
    }

    [HttpGet("get-execution/{userId}")]
    public async Task<IActionResult> GetActiveExecutionByUserId(string userId)
    {
        var exec = await _alpacaRepository.GetActiveAlpacaExecutionByUserIdAsync(Guid.Parse(userId));
        if (exec == null)
        {
            exec = new AlpacaExecutionModel();
        }
        return Ok(exec);
    }

    [HttpPost("run-test")]
    public async Task<IActionResult> RunBacktest([FromBody] StrategySettingsModel settings)
    {
        settings.Id = Guid.NewGuid();
        settings.StartDate = settings.StartDate.ToUniversalTime();
        settings.EndDate = settings.EndDate.ToUniversalTime();
        settings.TestStamp = DateTime.UtcNow;

        if (settings == null)
        {
            return BadRequest("BacktestSettings cannot be null");
        }

        var result = await _strategyServiceClient.StartStrategyAsync(settings);
        if (Enum.TryParse(result, out ErrorCode errorCode))
        {
            return BadRequest(errorCode);
        }

        if (result == "true")
        {
            await _strategyTestService.RunBacktest(settings);
        }

        return Ok(result);
    }

    //[KeycloakAuthorize("admin")]
    [HttpPost("start-execution/{userId}/{strategyId}")]
    public async Task<IActionResult> StartAlpacaExecution(Guid userId, Guid strategyId)
    {
        var strategy = await _strategyServiceClient.GetStrategyAsync(strategyId.ToString());
        if(strategy == null)
        {
            return BadRequest("Strategy not found");
        }
        if(strategy.UserId != userId)
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

    [HttpPost("stop-execution/{userId}")]
    public async Task<IActionResult> StopAlpacaExecution(Guid userId)
    {
        var execution = await _alpacaRepository.GetActiveAlpacaExecutionByUserIdAsync(userId);
        if (execution == null)
        {
            return BadRequest("No active execution found");
        }
        execution.EndDate = DateTime.UtcNow.ToUniversalTime();
        await _alpacaRepository.UpdateAlpacaExecutionAsync(execution);
        return Ok();
    }

    [HttpDelete("delete-executions")]
    public async Task<IActionResult> DeleteExecutionsByUserId()
    {
        var userId = HttpContext.Items["UserId"]?.ToString();
        await _alpacaRepository.DeleteAlpacaExecutionsAsync(new Guid(userId!));
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
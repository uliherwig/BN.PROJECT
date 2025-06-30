namespace BN.PROJECT.AlpacaService;

[Route("[controller]")]
[ApiController]
[AuthorizeUser(["user", "admin"])]
public class AlpacaTestController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly IAlpacaRepository _alpacaRepository;
    private readonly IStrategyTestService _strategyTestService;
    private readonly IStrategyServiceClient _strategyServiceClient;
    private readonly IOptimizerServiceClient _optimizerServiceClient;
    private readonly ILogger<AlpacaTestController> _logger;

    public AlpacaTestController(
        IWebHostEnvironment env,
        IAlpacaRepository alpacaRepository,
        IStrategyTestService backtestService,
        IStrategyServiceClient strategyServiceClient,
        IOptimizerServiceClient optimizerServiceClient,
        ILogger<AlpacaTestController> logger)
    {
        _env = env;
        _alpacaRepository = alpacaRepository;
        _strategyTestService = backtestService;
        _strategyServiceClient = strategyServiceClient;
        _optimizerServiceClient = optimizerServiceClient;
        _logger = logger;
    }

    [HttpGet("historical-bars/{symbol}")]
    public async Task<IActionResult> GetHistoricalBarsBySymbol(string symbol, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var bars = await _alpacaRepository.GetHistoricalBars(symbol, startDate.ToUniversalTime(), endDate.ToUniversalTime());
        return Ok(bars);
    }

    [HttpPost("run-test")]
    public async Task<IActionResult> RunBacktest([FromBody] StrategySettingsModel settings)
    {
        var userId = HttpContext.Items["UserId"]?.ToString();
        settings.UserId = new Guid(userId!);
        settings.Id = Guid.NewGuid();
        settings.StartDate = settings.StartDate.ToUniversalTime();
        settings.EndDate = settings.EndDate.ToUniversalTime();
        settings.TestStamp = DateTime.UtcNow;

        if (settings == null)
        {
            return BadRequest("StrategySettingsModel cannot be null");
        }

        var result = await _strategyServiceClient.StartStrategyAsync(settings);
        if (Enum.TryParse(result, out Core.BnErrorCode errorCode))
        {
            return base.BadRequest(errorCode);
        }

        if (result == "true")
        {
            await _strategyTestService.RunBacktest(settings);
        }

        return Ok(result);
    }

    [HttpGet("test-optimization-service")]
    public async Task<IActionResult> TestOptimizationAsync()
    {

        var test = await _optimizerServiceClient.TestOptimizationAsync();
     
        return Ok(test);
    }

    [HttpPost("optimize")]
    public async Task<IActionResult> RunOptimization([FromBody] StrategySettingsModel settings)
    {
        var userId = HttpContext.Items["UserId"]?.ToString();
        settings.UserId = new Guid(userId!);
        settings.Id = Guid.NewGuid();
        settings.StartDate = settings.StartDate.ToUniversalTime();
        settings.EndDate = settings.EndDate.ToUniversalTime();
        settings.TestStamp = DateTime.UtcNow;

        if (settings == null)
        {
            return BadRequest("StrategySettingsModel cannot be null");
        }

        var result = await _optimizerServiceClient.StartOptimizerAsync(settings);
        if (Enum.TryParse(result, out Core.BnErrorCode errorCode))
        {
            return base.BadRequest(errorCode);
        }

        if (result == "true")
        {
            await _strategyTestService.OptimizeStratgy(settings);
        }

        return Ok(result);
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
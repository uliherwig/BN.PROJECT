namespace BN.PROJECT.AlpacaService;

[Route("[controller]")]
[ApiController]
public class AlpacaTestController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly IAlpacaRepository _alpacaRepository;
    private readonly IBacktestService _backtestService;
    private readonly IStrategyServiceClient _strategyServiceClient;
    private readonly ILogger<AlpacaTestController> _logger;

    public AlpacaTestController(
        IWebHostEnvironment env,
        IAlpacaRepository alpacaRepository,
        IBacktestService backtestService,
        IStrategyServiceClient strategyServiceClient,
        ILogger<AlpacaTestController> logger)
    {
        _env = env;
        _alpacaRepository = alpacaRepository;
        _backtestService = backtestService;
        _strategyServiceClient = strategyServiceClient;
        _logger = logger;
    }

    [HttpGet("historical-bars/{symbol}")]
    public async Task<IActionResult> GetHistoricalBarsBySymbol(string symbol, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var bars = await _alpacaRepository.GetHistoricalBars(symbol, startDate.ToUniversalTime(), endDate.ToUniversalTime());
            return Ok(bars);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> RunBacktest([FromBody] StrategySettingsModel settings)
    {
        settings.Id = Guid.NewGuid();
        settings.StartDate = settings.StartDate.ToUniversalTime();
        settings.EndDate = settings.EndDate.ToUniversalTime();
        settings.TestStamp = DateTime.UtcNow;

        try
        {
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
                await _backtestService.RunBacktest(settings);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user settings");
        }
        return Ok(false);
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
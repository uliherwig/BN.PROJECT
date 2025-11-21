using Alpaca.Markets;

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

    [HttpGet("historical-quotes/{symbol}")]
    public async Task<IActionResult> GetHistoricalQuotesBySymbol(string symbol, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var bars = await _alpacaRepository.GetHistoricalBars(symbol, startDate.ToUniversalTime(), endDate.ToUniversalTime());

        var quotes = new List<Quote>();
        foreach (var bar in bars)
        {
            var q = new Quote
            {
                Symbol = symbol,
                AskPrice = bar.C + 0.1m,
                BidPrice = bar.C - 0.1m,
                TimestampUtc = bar.T.ToUniversalTime()
            };
            quotes.Add(q);
        }
        return Ok(quotes);
    }

    [HttpPost("run-test")]
    public async Task<IActionResult> RunBacktest([FromBody] StrategySettingsModel settings)
    {
        if (settings == null)
        {
            return BadRequest("StrategySettingsModel cannot be null");
        } 
        var userId = HttpContext.Items["UserId"]?.ToString();
        settings.UserId = new Guid(userId!);
        settings.Id = Guid.NewGuid();
        settings.StartDate = settings.StartDate.ToUniversalTime();
        settings.EndDate = settings.EndDate.ToUniversalTime();
        settings.StampStart = DateTime.UtcNow.ToUniversalTime();
        settings.StampEnd = DateTimeExtension.PostgresMinValue().ToUniversalTime();

        var result = await _strategyServiceClient.StartStrategyAsync(settings);
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
        if (settings == null)
        {
            return BadRequest("StrategySettingsModel cannot be null");
        }
        var userId = HttpContext.Items["UserId"]?.ToString();
        settings.UserId = new Guid(userId!);
        settings.Id = Guid.NewGuid();
        settings.StartDate = settings.StartDate.ToUniversalTime();
        settings.EndDate = settings.EndDate.ToUniversalTime();
        settings.StampStart = DateTime.UtcNow.ToUniversalTime();
        settings.StampEnd = DateTimeExtension.PostgresMinValue().ToUniversalTime();      
        var result = await _strategyServiceClient.StartStrategyAsync(settings);
        if (result == "true")
        {
            await _strategyTestService.OptimizeStrategy(settings);
        }
        return Ok(result);
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
namespace BN.PROJECT.AlpacaService;

[Route("[controller]")]
[ApiController]
public class BacktestController : ControllerBase
{
    private readonly IAlpacaRepository _alpacaRepository;
    private readonly ILogger<UserSettingsController> _logger;
    private readonly BacktestService _backtestService;
    private readonly IStrategyServiceClient _strategyServiceClient;
    public BacktestController(IAlpacaRepository alpacaRepository,
        ILogger<UserSettingsController> logger,
        BacktestService backtestService,
        IStrategyServiceClient strategyServiceClient)
    {
        _alpacaRepository = alpacaRepository;
        _logger = logger;
        _backtestService = backtestService;
        _strategyServiceClient = strategyServiceClient;
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
}

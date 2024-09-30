namespace BN.PROJECT.AlpacaService;

[Route("[controller]")]
[ApiController]
public class BacktestController : ControllerBase
{
    private readonly IAlpacaRepository _alpacaRepository;
    private readonly ILogger<UserSettingsController> _logger;
    private readonly BacktestService _backtestService;
    public BacktestController(IAlpacaRepository alpacaRepository, ILogger<UserSettingsController> logger, BacktestService backtestService)
    {
        _alpacaRepository = alpacaRepository;
        _logger = logger;
        _backtestService = backtestService;
    }

    [HttpPost]
    public async Task<IActionResult> RunBacktest([FromBody] BacktestSettings backtestSettings)
    {
        backtestSettings = new BacktestSettings
        {
            Symbol = "SPY",
            TakeProfitFactor = 0.01,
            StopLossFactor = 0.01,
            StartDate = DateTime.Now.AddYears(-1),
            EndDate = DateTime.Now,
            Strategy = Strategy.Breakout,
            TimeFrame = Core.TimeFrame.Day,
            UserEmail = "johndoe@test.de"
        };
        try
        {
            if (backtestSettings == null)
            {
                return BadRequest("BacktestSettings cannot be null");
            }

            if (string.IsNullOrEmpty(backtestSettings.Symbol))
            {
                return BadRequest("Symbol cannot be null or empty");
            }
            // input validation can happen in UI 




           var result =  await _backtestService.RunBacktest(backtestSettings);
            return Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user settings");
        }
        return Ok(false);

    }


}

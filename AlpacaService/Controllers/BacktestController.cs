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
    public async Task<IActionResult> RunBacktest([FromBody] BacktestSettings backtestSettings)
    {

        backtestSettings.Id = Guid.NewGuid();
        backtestSettings.StartDate = backtestSettings.StartDate.ToUniversalTime();
        backtestSettings.EndDate = backtestSettings.EndDate.ToUniversalTime();
        backtestSettings.TestStamp = DateTime.UtcNow;

        //var  backtestSettings = new BacktestSettings
        //{
        //    Symbol = "SPY",
        //    Name = "Test",
        //    TakeProfitFactor = 0.01,
        //    StopLossFactor = 0.01,
        //    StartDate = new DateTime(2024, 9, 2).ToUniversalTime(),
        //    EndDate = new DateTime(2024, 9, 7).ToUniversalTime(),
        //    Strategy = Strategy.Breakout,
        //    TimeFrame = Core.TimeFrame.Day,
        //    UserEmail = "johndoe@test.de"
        //};

        try
        {
            if (backtestSettings == null)
            {
                return BadRequest("BacktestSettings cannot be null");
            }


            var result = await _strategyServiceClient.StartStrategyAsync(backtestSettings);
            if (Enum.TryParse(result, out ErrorCode errorCode))
            {
                return BadRequest(errorCode);
            }

            if (result == "true")
            {
                await _backtestService.RunBacktest(backtestSettings);
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

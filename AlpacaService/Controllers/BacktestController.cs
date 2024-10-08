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
        backtestSettings = new BacktestSettings
        {
            Symbol = "SPY",
            TakeProfitFactor = 0.01,
            StopLossFactor = 0.01,
            StartDate = new DateTime(2024, 9, 2).ToUniversalTime(),
            EndDate = new DateTime(2024, 9, 7).ToUniversalTime(),
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


            var result = await _strategyServiceClient.StartStrategyAsync(backtestSettings);   
            
            if(result == "true")
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

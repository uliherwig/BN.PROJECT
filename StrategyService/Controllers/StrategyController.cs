
namespace BN.PROJECT.StrategyService;

[Route("[controller]")]
[ApiController]
public class StrategyController : ControllerBase
{
    private readonly ILogger<StrategyController> _logger;
    private readonly IStrategyRepository _strategyRepository;

    public StrategyController(ILogger<StrategyController> logger, 
        IStrategyRepository strategyRepository)
    {
        _logger = logger;
        _strategyRepository = strategyRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetStrategy()
    {
        try
        {
            return Ok("true");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting strategy");
        }
        return Ok(false);
    }

    [HttpPost]
    public async Task<IActionResult> StartStrategy([FromBody] BacktestSettings testSettings)
    {
        try
        {
            await _strategyRepository.AddBacktestAsync(testSettings);
            return Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting strategy");
        }
        return Ok(false);
    }

    [HttpGet("settings/{email}")]
    public async Task<IActionResult> GetTestSettingsByEmail(string email)
    {
        try
        {
            var settings = await _strategyRepository.GetBacktestsByEmailAsync(email);
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error GetTestSettingsByEmail");
        }
        return Ok(false);
    }

    [HttpGet("results/{testId}")]
    public async Task<IActionResult> GetTestResultsByTestId(Guid testId)
    {
        try
        {
            var positions = await _strategyRepository.GetPositionsByTestId(testId);
            return Ok(positions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error GetTestSettingsByEmail");
        }
        return Ok(false);
    }
}

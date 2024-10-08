
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
            //_testQueueService.Add(testSettings.Id, testSettings);
            //_strategyService.InitializeStrategyTest(testSettings);

            return Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting strategy");
        }
        return Ok(false);
    }
    [HttpPost("delete")]
    public async Task<IActionResult> DeleteMessages()
    {
        try
        {
            var email = "johndoe@test.de";
            var topic = $"test_{email.Replace('@', '-').Replace('.', '-')}";

            return Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting strategy");
        }
        return Ok(false);
    }

    [HttpPost("results/{email}")]
    public async Task<IActionResult> GetTestResults(string email)
    {
        try
        {
            //await _strategyService.CreateTestResult(email);

            return Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting strategy");
        }
        return Ok(false);
    }
}


namespace BN.PROJECT.StrategyService;

[Route("[controller]")]
[ApiController]
public class StrategyController : ControllerBase
{

    private readonly ILogger<StrategyController> _logger;
    private readonly IStrategyService _strategyService;


    public StrategyController(ILogger<StrategyController> logger, IStrategyService strategyService)
    {
        _logger = logger;
        _strategyService = strategyService;
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
    public async Task<IActionResult> StartStrategy([FromBody] BacktestSettings backtestSettings)
    {
        try
        {
            await _strategyService.RunStrategyTest(backtestSettings);

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
            var topic = $"backtest-{email.Replace('@', '-').Replace('.', '-')}";

            var kafkaDeleteMessagesService = new KafkaDeleteMessagesService();
            await kafkaDeleteMessagesService.DeleteMessagesAsync(topic);
            return Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting strategy");
        }
        return Ok(false);
    }
}

namespace BN.PROJECT.StrategyService;

[Route("[controller]")]
[ApiController]
public class InternalStrategyController : ControllerBase
{
    private readonly ILogger<InternalStrategyController> _logger;
    private readonly IStrategyRepository _strategyRepository;

    // GlobalExceptionMiddleware for logging exceptions

    public InternalStrategyController(ILogger<InternalStrategyController> logger,
        IStrategyRepository strategyRepository)
    {
        _logger = logger;
        _strategyRepository = strategyRepository;
    }

    [HttpGet("{strategyId}")]
    public async Task<IActionResult> GetStrategyById(Guid strategyId)
    {
        var test = await _strategyRepository.GetStrategyByIdAsync(strategyId);
        if (test == null)
        {
            return NotFound();
        }
        return Ok(test);
    }

    [HttpPost]
    public async Task<IActionResult> AddStrategy([FromBody] StrategySettingsModel testSettings)
    {
        await _strategyRepository.AddStrategyAsync(testSettings);
        return Ok(true);
    }

}
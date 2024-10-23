
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

    [HttpGet("{testId}")]
    public async Task<IActionResult> GetStrategy(Guid testId)
    {
        try
        {
            var test = await _strategyRepository.GetBacktestByIdAsync(testId);
            return Ok(test);
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
            return BadRequest(ex);
        }
    }

    [HttpDelete("{testId}")]
    public async Task<IActionResult> DeleteTestAndPositions(Guid testId)
    {
        try
        {
            var test = await _strategyRepository.GetBacktestByIdAsync(testId);
            if (test == null)
            {
                return NotFound();
            }

            var positions = await _strategyRepository.GetPositionsByTestId(testId);
            await _strategyRepository.DeleteBacktest(test);
            await _strategyRepository.DeletePositions(positions);
            return Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting strategy");
            return BadRequest(ex);
        }
    }

    [HttpPut("{testId}")]
    public async Task<IActionResult> UpdateTestSettings(Guid testId)
    {
        try
        {
            var test = await _strategyRepository.GetBacktestByIdAsync(testId);
            if (test == null)
            {
                return NotFound();
            }

            await _strategyRepository.UpdateBacktestAsync(test);
            return Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating strategy");
            return BadRequest(ex);
        }
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

    [HttpGet("positions/{testId}")]
    public async Task<IActionResult> GetTestPositionsByTestId(Guid testId)
    {
        try
        {
            var positions = await _strategyRepository.GetPositionsByTestId(testId);

            return Ok(positions.Where(p => p.PriceClose > 0));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error GetTestPositionsByTestId");
        }
        return Ok(false);
    }

    [HttpGet("results/{testId}")]
    public async Task<IActionResult> GetTestResultsByTestId(Guid testId)
    {
        try
        {
            var testSettings = await _strategyRepository.GetBacktestByIdAsync(testId);
            var positions = await _strategyRepository.GetPositionsByTestId(testId);
            var testResult = new TestResult
            {
                Id = testId,
                Symbol = testSettings.Symbol,
                StartDate = testSettings.StartDate,
                EndDate = testSettings.EndDate,
                TimeFrame = testSettings.BreakoutPeriod,
                NumberOfPositions = positions.Count,
                NumberOfBuyPositions = positions.Count(p => p.Side == Side.Buy),
                NumberOfSellPositions = positions.Count(p => p.Side == Side.Sell),
                TotalProfitLoss = positions.Sum(p => p.ProfitLoss),
                BuyProfitLoss = positions.Where(p => p.Side == Side.Buy).Sum(p => p.ProfitLoss),
                SellProfitLoss = positions.Where(p => p.Side == Side.Sell).Sum(p => p.ProfitLoss)
            };

            return Ok(testResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error GetTestResultsByTestId");
        }
        return Ok(false);
    }
}

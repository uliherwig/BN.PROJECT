using NuGet.Configuration;

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
    public async Task<IActionResult> GetStrategyById(Guid testId)
    {
        try
        {
            var test = await _strategyRepository.GetStrategyByIdAsync(testId);
            return Ok(test);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting strategy");
        }
        return Ok(false);
    }

    [HttpGet("{name}/{userId}")]
    public async Task<IActionResult> GetStrategyNameExists(string name, Guid userId)
    {
        try
        {
            var strategies = await _strategyRepository.GetStrategiesByUserIdAsync(userId, false);
            var result = strategies.Any(s => s.Name == name);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting strategy");
        }
        return Ok(false);
    }

    [HttpPost]
    public async Task<IActionResult> AddStrategy([FromBody] StrategySettingsModel testSettings)
    {
        try
        {
            await _strategyRepository.AddStrategyAsync(testSettings);
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
            var test = await _strategyRepository.GetStrategyByIdAsync(testId);
            if (test == null)
            {
                return NotFound();
            }

            var positions = await _strategyRepository.GetPositionsByStrategyIdAsync(testId);
            await _strategyRepository.DeleteStrategyAsync(test);
            await _strategyRepository.DeletePositionsAsync(positions);
            return Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting strategy");
            return BadRequest(ex);
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateStrategy([FromBody] StrategySettingsModel strategy)
    {
        try
        {
            var result = await _strategyRepository.UpdateStrategyAsync(strategy);
            return Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating strategy");
            return BadRequest(ex);
        }
    }

    [HttpGet("settings/{userId}")]
    public async Task<IActionResult> GetStrategiesByUserId(Guid userId, bool bookmarked = false)
    {
        try
        {
            var settings = await _strategyRepository.GetStrategiesByUserIdAsync(userId, bookmarked);
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error GetTestSettingsByEmail");
        }
        return Ok(false);
    }

    [HttpGet("positions/{testId}")]
    public async Task<IActionResult> GetTestPositionsByStrategyId(Guid testId)
    {
        try
        {
            var positions = await _strategyRepository.GetPositionsByStrategyIdAsync(testId);

            return Ok(positions.Where(p => p.PriceClose > 0));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error GetTestPositionsByStrategyId");
        }
        return Ok(false);
    }

    [HttpGet("results/{testId}")]
    public async Task<IActionResult> GetTestResultsByStrategyId(Guid testId)
    {
        try
        {
            var strategySettings = await _strategyRepository.GetStrategyByIdAsync(testId);
            if (strategySettings == null) return Ok(false);
            var positions = await _strategyRepository.GetPositionsByStrategyIdAsync(testId);

            var testResult = new TestResult
            {
                Id = testId,
                Asset = strategySettings.Asset,
                StartDate = strategySettings.StartDate,
                EndDate = strategySettings.EndDate,
                NumberOfPositions = positions.Count,
                NumberOfBuyPositions = positions.Count(p => p.Side == SideEnum.Buy),
                NumberOfSellPositions = positions.Count(p => p.Side == SideEnum.Sell),
                TotalProfitLoss = positions.Sum(p => p.ProfitLoss),
                BuyProfitLoss = positions.Where(p => p.Side == SideEnum.Buy).Sum(p => p.ProfitLoss),
                SellProfitLoss = positions.Where(p => p.Side == SideEnum.Sell).Sum(p => p.ProfitLoss)
            };        
            return Ok(testResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error GetTestResultsByStrategyId");
        }
        return Ok(false);
    }
}

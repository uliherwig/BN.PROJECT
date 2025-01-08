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

    [HttpGet("{strategyId}")]
    public async Task<IActionResult> GetStrategyById(Guid strategyId)
    {
        try
        {
            var test = await _strategyRepository.GetStrategyByIdAsync(strategyId);
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

    [HttpDelete("{strategyId}")]
    public async Task<IActionResult> DeleteTestAndPositions(Guid strategyId)
    {
        try
        {
            var test = await _strategyRepository.GetStrategyByIdAsync(strategyId);
            if (test == null)
            {
                return NotFound();
            }

            var positions = await _strategyRepository.GetPositionsByStrategyIdAsync(strategyId);
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

    [HttpGet("infos/{userId}/{strategyType}")]
    public async Task<IActionResult> GetStrategyInfosByUserId(Guid userId, StrategyEnum strategyType)
    {
        try
        {
            var settings = await _strategyRepository.GetStrategiesByUserIdAsync(userId, false);
            if (strategyType != StrategyEnum.None)
            {
                settings = settings.Where(s => s.StrategyType == strategyType).ToList();
            }
            var strategyInfos = settings.Select(s => new StrategyInfo
            {
                Id = s.Id,
                Label = s.Name,
            }).ToList();

            return Ok(strategyInfos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error GetTestSettingsByEmail");
        }
        return Ok(false);
    }

    [HttpGet("positions/{strategyId}")]
    public async Task<IActionResult> GetTestPositionsByStrategyId(Guid strategyId)
    {
        try
        {
            var positions = await _strategyRepository.GetPositionsByStrategyIdAsync(strategyId);

            return Ok(positions.Where(p => p.PriceClose > 0));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error GetTestPositionsByStrategyId");
        }
        return Ok(false);
    }

    [HttpGet("results/{strategyId}")]
    public async Task<IActionResult> GetTestResultsByStrategyId(Guid strategyId)
    {
        try
        {
            var strategySettings = await _strategyRepository.GetStrategyByIdAsync(strategyId);
            if (strategySettings == null) return Ok(false);
            var positions = await _strategyRepository.GetPositionsByStrategyIdAsync(strategyId);
            positions = positions.Where(p => p.PriceClose > 0).ToList();

            var testResult = new TestResult
            {
                Id = strategyId,
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
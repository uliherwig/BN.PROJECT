﻿namespace BN.PROJECT.StrategyService;

[Route("[controller]")]
[ApiController]
[AuthorizeUser(["user", "admin"])]
public class StrategyController : ControllerBase
{
    private readonly ILogger<StrategyController> _logger;
    private readonly IStrategyRepository _strategyRepository;

    // GlobalExceptionMiddleware for logging exceptions

    public StrategyController(ILogger<StrategyController> logger,
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

    [HttpGet("exists/{name}")]
    public async Task<IActionResult> GetStrategyNameExists(string name)
    {
        var userId = HttpContext.Items["UserId"]?.ToString();
        var strategies = await _strategyRepository.GetStrategiesByUserIdAsync(new Guid(userId!), false);
        var isStrategy = strategies.Any(s => s.Name == name);
        return Ok(isStrategy);
    }

    //[HttpPost]
    //public async Task<IActionResult> AddStrategy([FromBody] StrategySettingsModel testSettings)
    //{
    //    await _strategyRepository.AddStrategyAsync(testSettings);
    //    return Ok(true);
    //}

    [HttpPut]
    public async Task<IActionResult> UpdateStrategy([FromBody] StrategySettingsModel strategy)
    {
        var result = await _strategyRepository.UpdateStrategyAsync(strategy);
        return Ok(true);
    }

    [HttpDelete("{strategyId}")]
    public async Task<IActionResult> DeleteTestAndPositions(Guid strategyId)
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
 
    [HttpGet("settings")]
    public async Task<IActionResult> GetStrategiesByUserId(bool bookmarked = false)
    {
        var userId = HttpContext.Items["UserId"]?.ToString();
        var settings = await _strategyRepository.GetStrategiesByUserIdAsync(new Guid(userId!), bookmarked);
        return Ok(settings);
    }

    [HttpGet("infos/{strategyType}")]
    public async Task<IActionResult> GetStrategyInfosByUserId(StrategyEnum strategyType)
    {
        var userId = HttpContext.Items["UserId"]?.ToString();
        var settings = await _strategyRepository.GetStrategiesByUserIdAsync(new Guid(userId!), false);
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

    [HttpGet("positions/{strategyId}")]
    public async Task<IActionResult> GetTestPositionsByStrategyId(Guid strategyId)
    {
        var positions = await _strategyRepository.GetPositionsByStrategyIdAsync(strategyId);
        return Ok(positions.Where(p => p.PriceClose > 0));
    }

    [HttpGet("results/{strategyId}")]
    public async Task<IActionResult> GetTestResultsByStrategyId(Guid strategyId)
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

    [HttpDelete("remove-user-data")]
    public async Task<IActionResult> RemoveUserDataAsync()
    {
        var userId = HttpContext.Items["UserId"]?.ToString();
        await _strategyRepository.RemoveUserDataAsync(new Guid(userId!));
        return Ok(true);
    }
}
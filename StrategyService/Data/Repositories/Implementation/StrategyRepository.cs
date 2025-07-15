namespace BN.PROJECT.StrategyService;

public class StrategyRepository : IStrategyRepository
{
    private readonly StrategyDbContext _context;
    private readonly ILogger<StrategyRepository> _logger;

    public StrategyRepository(StrategyDbContext context, ILogger<StrategyRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<StrategySettingsModel>> GetStrategiesByUserIdAsync(Guid userId, bool bookmarked) =>
        bookmarked ? await _context.Strategies.Where(s => s.UserId == userId && s.Bookmarked).OrderByDescending(s => s.StampStart).ToListAsync()
        : await _context.Strategies.Where(s => s.UserId == userId).OrderByDescending(s => s.StampStart).ToListAsync();

    public async Task<List<StrategySettingsModel>> GetStartedStrategies()
    {
        var started = await _context.Strategies.Where(s => s.StampStart > s.StampEnd).ToListAsync();
        return started;
    }

    public async Task<StrategySettingsModel?> GetStrategyByIdAsync(Guid testId) =>
        await _context.Strategies.Where(s => s.Id == testId).FirstOrDefaultAsync();

    public async Task AddStrategyAsync(StrategySettingsModel strategySettings)
    {
        await _context.Strategies.AddAsync(strategySettings);
        await _context.SaveChangesAsync();
    }

    public async Task<int> UpdateStrategyAsync(StrategySettingsModel strategySettings)
    {
        try
        {
            _context.Strategies.Update(strategySettings);
            return await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error updating strategy settings");
        }
        return 0;
    }

    public async Task<List<PositionModel>> GetPositionsByStrategyIdAsync(Guid testId) =>
        await _context.Positions.Where(p => p.StrategyId == testId).ToListAsync();

    public async Task AddPositionAsync(PositionModel position)
    {
        await _context.Positions.AddAsync(position);
        await _context.SaveChangesAsync();
    }

    public async Task UpdatePositionAsync(PositionModel position)
    {
        _context.Positions.Update(position);
        await _context.SaveChangesAsync();
    }

    public async Task AddPositionsAsync(List<PositionModel> positions)
    {
        await _context.Positions.AddRangeAsync(positions);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteStrategyAsync(StrategySettingsModel strategySettings)
    {
        _context.Strategies.Remove(strategySettings);
        await _context.SaveChangesAsync();
    }

    public async Task DeletePositionsAsync(List<PositionModel> positions)
    {
        _context.Positions.RemoveRange(positions);
        await _context.SaveChangesAsync();
    }

    public async Task CleanupStrategiesAsync()
    {
        var threshold = DateTime.UtcNow.AddDays(-30);
        var strategies = await _context.Strategies.Where(t => t.StampEnd < threshold && t.Bookmarked == false).ToListAsync();
        var positions = await _context.Positions.Where(p => p.StrategyId == p.StrategyId).ToListAsync();
        _context.Strategies.RemoveRange(strategies);
        _context.Positions.RemoveRange(positions);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveUserDataAsync(Guid userId)
    {
        var strategies = await _context.Strategies.Where(t => t.UserId == userId).ToListAsync();
        var positions = await _context.Positions.Where(p => strategies.Select(t => t.Id).Contains(p.StrategyId)).ToListAsync();
        _context.Strategies.RemoveRange(strategies);
        _context.Positions.RemoveRange(positions);
        await _context.SaveChangesAsync();
    }
}
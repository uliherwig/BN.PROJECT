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

    public async Task<List<BacktestSettings>> GetBacktestsByEmailAsync(string email)
    {
        return await _context.BacktestSettings.Where(s => s.UserEmail == email).OrderByDescending(s => s.TestStamp).ToListAsync();
    }

    public async Task AddBacktestAsync(BacktestSettings backtestSettings)
    {
        try
        {
            await _context.BacktestSettings.AddAsync(backtestSettings);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error adding backtestsettings");
        }
    }

    public async Task<List<Position>> GetPositionsByTestId(Guid testId)
    {
        return await _context.Positions.Where(p => p.TestId == testId).ToListAsync();
    }

    public async Task AddPositionAsync(Position position)
    {
        try
        {
            await _context.Positions.AddAsync(position);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error adding backtestsettings");
        }
    }
    public async Task UpdatePositionAsync(Position position)
    {
        try
        {
            _context.Positions.Update(position);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error adding backtestsettings");
        }
    }
    public async Task AddPositionsAsync(List<Position> positions)
    {
        try
        {
            await _context.Positions.AddRangeAsync(positions);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error adding backtestsettings");
        }
    }

    public async Task DeleteBacktest(BacktestSettings backtestSettings)
    {
        _context.BacktestSettings.Remove(backtestSettings);
        await _context.SaveChangesAsync();
    }

    public async Task DeletePositions(List<Position> positions)
    {
        _context.Positions.RemoveRange(positions);
        await _context.SaveChangesAsync();
    }
}

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

    public async Task<List<BacktestSettings>> GetBacktestsByEmailAsync(string email) =>
        await _context.BacktestSettings.Where(s => s.UserEmail == email).OrderByDescending(s => s.TestStamp).ToListAsync();

    public async Task<BacktestSettings> GetBacktestByIdAsync(Guid testId) =>
        await _context.BacktestSettings.Where(s => s.Id == testId).FirstOrDefaultAsync();


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
    public async Task UpdateBacktestAsync(BacktestSettings backtestSettings)
    {
        try
        {
            _context.BacktestSettings.Update(backtestSettings);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error adding backtestsettings");
        }
    }




    public async Task<List<Position>> GetPositionsByTestId(Guid testId) => await _context.Positions.Where(p => p.TestId == testId).ToListAsync();

    public async Task AddPositionAsync(Position position)
    {
        try
        {
            await _context.Positions.AddAsync(position);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error adding backtest positions");
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
            _logger.LogError(e, "Error updating backtest positions");
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
            _logger.LogError(e, "Error adding backtest positions");
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

    public async Task CleanupBacktests()
    {
        try
        {
            var threshold = DateTime.UtcNow.AddDays(-30);
            var tests = await _context.BacktestSettings.Where(t => t.TestStamp < threshold && t.Bookmarked == false).ToListAsync();
            var positions = await _context.Positions.Where(p => tests.Select(t => t.Id).Contains(p.TestId)).ToListAsync();
            _context.BacktestSettings.RemoveRange(tests);
            _context.Positions.RemoveRange(positions);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error cleaning up backtests");
        }

    }
}

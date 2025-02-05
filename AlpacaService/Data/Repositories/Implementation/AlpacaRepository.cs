namespace BN.PROJECT.AlpacaService;

public class AlpacaRepository : IAlpacaRepository
{
    private readonly AlpacaDbContext _context;

    public AlpacaRepository(AlpacaDbContext context)
    {
        _context = context;
    }

    public async Task<List<AlpacaAsset>> GetAssets()
    {
        return await _context.Assets.OrderBy(a => a.Symbol).ToListAsync();
    }

    public async Task AddAssetsAsync(List<AlpacaAsset> assets)
    {
        await _context.Assets.AddRangeAsync(assets);
        await _context.SaveChangesAsync();
    }

    public async Task<AlpacaAsset?> GetAsset(string symbol)
    {
        return await _context.Assets.FirstOrDefaultAsync(a => a.Symbol == symbol);
    }

    public async Task<AlpacaBar?> GetLatestBar(string symbol)
    {
        return await _context.Bars.Where(b => b.Symbol == symbol).OrderByDescending(b => b.T).FirstOrDefaultAsync();
    }

    public async Task<List<AlpacaBar>> GetHistoricalBars(string symbol, DateTime startDate, DateTime endDate)
    {
        return await _context.Bars.Where(b => b.Symbol == symbol && b.T > startDate && b.T < endDate).OrderBy(b => b.T).ToListAsync();
    }

    public async Task AddBarsAsync(List<AlpacaBar> bars)
    {
        await _context.Bars.AddRangeAsync(bars);
        await _context.SaveChangesAsync();
    }

    public async Task<AlpacaQuote?> GetLatestQuote(string symbol)
    {
        var quotes = await _context.Quotes
            .Where(b => b.Symbol == symbol)
            .OrderByDescending(b => b.TimestampUtc)
            .FirstOrDefaultAsync();
        return quotes;
    }

    public async Task<List<AlpacaQuote>> GetHistoricalQuotes(string symbol, DateTime startDate, DateTime endDate)
    {
        return await _context.Quotes
            .Where(b => b.Symbol == symbol && b.TimestampUtc > startDate && b.TimestampUtc < endDate)
            .OrderBy(b => b.TimestampUtc).ToListAsync();
    }

    public async Task AddQuotesAsync(List<AlpacaQuote> quotes)
    {
        await _context.Quotes.AddRangeAsync(quotes);
        await _context.SaveChangesAsync();
    }

    // Create
    public async Task AddOrderAsync(AlpacaOrder order)
    {
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
    }

    // Read
    public async Task<AlpacaOrder?> GetOrderAsync(int id)
    {
        return await _context.Orders.FindAsync(id);
    }

    // Update
    public async Task UpdateOrderAsync(AlpacaOrder order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
    }

    public async Task AddUserSettingsAsync(UserSettingsModel userSettings)
    {
        await _context.UserSettings.AddAsync(userSettings);
        await _context.SaveChangesAsync();
    }

    public async Task<UserSettingsModel?> GetUserSettingsAsync(string userId)
    {
        return await _context.UserSettings.FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task UpdateUserSettingsAsync(UserSettingsModel userSettings)
    {
        _context.UserSettings.Update(userSettings);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteUserSettingsAsync(UserSettingsModel userSettings)
    {
        _context.UserSettings.Remove(userSettings);
        await _context.SaveChangesAsync();
    }

    public async Task AddAlpacaExecutionAsync(AlpacaExecutionModel execution)
    {
        await _context.Executions.AddAsync(execution);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAlpacaExecutionAsync(AlpacaExecutionModel execution)
    {
        _context.Executions.Update(execution);
        await _context.SaveChangesAsync();
    }

    public async Task<AlpacaExecutionModel?> GetAlpacaExecutionAsync(Guid id)
    {
        var execution = await _context.Executions.FindAsync(id);
        return execution;
    }

    public async Task<List<AlpacaExecutionModel>?> GetActiveAlpacaExecutionsAsync()
    {
        return await _context.Executions
               .Where(e => e.EndDate == DateTime.MinValue)
               .ToListAsync();
    }

    public async Task<AlpacaExecutionModel?> GetActiveAlpacaExecutionByUserIdAsync(Guid userId)
    {
        var execution = await _context.Executions.FirstOrDefaultAsync(e => e.UserId == userId && e.EndDate == DateTime.MinValue);
        return execution;
    }
}
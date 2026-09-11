namespace BN.PROJECT.AlpacaService;

public class AlpacaRepository : IAlpacaRepository
{
    private readonly AlpacaDbContext _context;

    public AlpacaRepository(AlpacaDbContext context)
    {
        _context = context;
    }

    // Calendar
    public async Task<List<AlpacaCalendar>> GetCalendarAsync(DateOnly startDate = default, DateOnly endDate = default)
    {
        if (startDate == default)
        {
            startDate = new DateOnly(2020, 1, 1);
        }
        if (endDate == default)
        {
            endDate = DateOnly.FromDateTime(DateTime.UtcNow);
        }
        return await _context.Calendars
            .Where(c => c.TradingDate >= startDate && c.TradingDate <= endDate)
            .OrderBy(c => c.TradingDate)
            .ToListAsync();
    }

    public async Task<AlpacaCalendar?> GetLatestCalendar()
    {
        return await _context.Calendars
            .OrderByDescending(c => c.TradingDate)
            .FirstOrDefaultAsync();
    }

    public async Task AddCalendarAsync(List<AlpacaCalendar> calendars)
    {
        await _context.Calendars.AddRangeAsync(calendars);
        await _context.SaveChangesAsync();
    }


    // Assets
    public async Task<AlpacaAsset?> GetAsset(string symbol)
    {
        return await _context.Assets.FirstOrDefaultAsync(a => a.Symbol == symbol);
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

    // Bars
    public async Task<AlpacaBar?> GetLatestBar(string symbol)
    {
        return await _context.Bars.Where(b => b.Symbol == symbol).OrderByDescending(b => b.T).FirstOrDefaultAsync();
    }
    public async Task<List<AlpacaBar>> GetHistoricalBars(string symbol, DateTime startDate, DateTime endDate)
    {
        return await _context.Bars.Where(b => b.Symbol == symbol && b.T > startDate && b.T <= endDate).OrderBy(b => b.T).ToListAsync();
    }
    public async Task AddBarsAsync(List<AlpacaBar> bars)
    {
        foreach (var bar in bars)
        {
            var existingBar = await _context.Bars.FirstOrDefaultAsync(b => b.Symbol == bar.Symbol && b.T == bar.T);
            if (existingBar == null)
            {       
                // Add new bar
                await _context.Bars.AddAsync(bar);
            }
        }   
        await _context.SaveChangesAsync();
    }

    // Quotes
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

    // Trades
    public async Task<AlpacaTrade?> GetLatestTrade(string symbol)
    {
        var trades = await _context.Trades
            .Where(b => b.Symbol == symbol)
            .OrderByDescending(b => b.TimestampUtc)
            .FirstOrDefaultAsync();
        return trades;
    }
    public async Task<List<AlpacaTrade>> GetHistoricalTrades(string symbol, DateTime startDate, DateTime endDate)
    {
        return await _context.Trades
            .Where(b => b.Symbol == symbol && b.TimestampUtc > startDate && b.TimestampUtc < endDate)
            .OrderBy(b => b.TimestampUtc).ToListAsync();
    }
    public async Task AddTradesAsync(List<AlpacaTrade> trades)
    {
        foreach (var trade in trades)
        {
            var existingTrade = await _context.Trades.FirstOrDefaultAsync(t => t.Id == trade.Id);
            if (existingTrade == null)
            {
                await _context.Trades.AddAsync(trade);
            }
        }
        await _context.SaveChangesAsync();
    }

    // Orders
    public async Task AddOrderAsync(AlpacaOrder order)
    {
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
    }
    public async Task<AlpacaOrder?> GetOrderAsync(int id)
    {
        return await _context.Orders.FindAsync(id);
    }
    public async Task UpdateOrderAsync(AlpacaOrder order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteOrderAsync(AlpacaOrder order)
    {
        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
    }
 
}
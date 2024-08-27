namespace BN.TRADER.AlpacaService
{
    public class DbRepository : IDbRepository
    {
        private readonly ApplicationDbContext _context;

        public DbRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<AlpacaAsset>> GetAssets() => await _context.Assets.OrderBy(a => a.Symbol).ToListAsync();

        public async Task AddAssetsAsync(List<AlpacaAsset> assets)
        {
            await _context.Assets.AddRangeAsync(assets);
            await _context.SaveChangesAsync();
        }

        public async Task<AlpacaAsset> GetAsset(string symbol) => await _context.Assets.FirstOrDefaultAsync(a => a.Symbol == symbol);

        public async Task<bool> ToggleAssetSelected(string symbol)
        {
            var asset = _context.Assets.FirstOrDefault(a => a.Symbol == symbol);
            if (asset == null)
            {
                return await Task.FromResult(false);
            }

            asset.IsSelected = !asset.IsSelected;
            var res = await _context.SaveChangesAsync();
            return await Task.FromResult(res > 0);
        }

        public async Task<AlpacaBar> GetLatestBar(string symbol)
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

        public async Task AddOrderAsync(AlpacaOrder order)
        {
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
        }
    }
}
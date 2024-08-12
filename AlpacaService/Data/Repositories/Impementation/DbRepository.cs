namespace BN.TRADER.AlpacaService
{
    public class DbRepository : IDbRepository
    {
        private readonly ApplicationDbContext _context;

        public DbRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<AlpacaAsset>> GetAssets()
        {
            try
            {
                return await _context.Assets.ToListAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task AddAssetsAsync(List<AlpacaAsset> assets)
        {
            await _context.Assets.AddRangeAsync(assets);
            await _context.SaveChangesAsync();
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
    }
}
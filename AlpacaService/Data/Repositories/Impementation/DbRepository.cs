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
    }
}
namespace BN.TRADER.AlpacaService
{
    public interface IDbRepository
    {
        Task AddAssetsAsync(List<AlpacaAsset> assets);

        Task<List<AlpacaAsset>> GetAssets();

        Task<AlpacaBar> GetLatestBar(string symbol);

        Task<List<AlpacaBar>> GetHistoricalBars(string symbol, DateTime startDate, DateTime endDate);

        Task AddBarsAsync(List<AlpacaBar> bars);

         Task AddOrderAsync(AlpacaOrder order);
    }
}
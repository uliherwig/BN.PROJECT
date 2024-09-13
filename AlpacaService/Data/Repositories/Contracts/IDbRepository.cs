namespace BN.PROJECT.AlpacaService
{
    public interface IDbRepository
    {
        Task AddAssetsAsync(List<AlpacaAsset> assets);

        Task<List<AlpacaAsset>> GetAssets();

        Task<AlpacaAsset> GetAsset(string symbol);

        Task<bool> ToggleAssetSelected(string symbol);

        Task<AlpacaBar> GetLatestBar(string symbol);

        Task<List<AlpacaBar>> GetHistoricalBars(string symbol, DateTime startDate, DateTime endDate);

        Task AddBarsAsync(List<AlpacaBar> bars);

        Task AddOrderAsync(AlpacaOrder order);
    }
}
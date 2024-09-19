namespace BN.PROJECT.DataService;

public interface IAlpacaRepository
{
    Task AddAssetsAsync(List<AlpacaAsset> assets);

    Task<List<AlpacaAsset>> GetAssets();

    Task<AlpacaAsset> GetAsset(string symbol);

    Task<AlpacaBar> GetLatestBar(string symbol);

    Task<List<AlpacaBar>> GetHistoricalBars(string symbol, DateTime startDate, DateTime endDate);

    Task AddBarsAsync(List<AlpacaBar> bars);

    Task AddOrderAsync(AlpacaOrder order);

    Task<AlpacaOrder> GetOrderAsync(int id);

    Task UpdateOrderAsync(AlpacaOrder order);
}
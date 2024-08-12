namespace BN.TRADER.AlpacaService
{
    public interface IDbRepository
    {
        Task AddAssetsAsync(List<AlpacaAsset> assets);

        Task<List<AlpacaAsset>> GetAssets();
    }
}
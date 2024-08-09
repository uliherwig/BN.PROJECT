namespace BN.TRADER.AlpacaService
{
    public interface IAlpacaRepository
    {
        Task AddAssetsAsync(List<AlpacaAsset> assets);

        Task<List<AlpacaAsset>> GetAssets();
    }
}
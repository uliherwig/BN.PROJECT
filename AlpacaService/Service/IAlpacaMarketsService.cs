namespace BN.TRADER.AlpacaService
{
    public interface IAlpacaMarketsService
    {
        Task CreateOrderAsync();

        Task<IAccount> GetAccountAsync();

        Task<IAsset> GetAssetBySymbolAsync(string symbol);

        Task<List<BnOhlc>> GetHistoricalBarsBySymbol(string symbol, DateTime startDate, DateTime endDate);

        Task<List<BnAsset>?> GetAssetsAsync();
    }
}
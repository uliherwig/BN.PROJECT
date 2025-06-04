namespace BN.PROJECT.AlpacaService;

public interface IAlpacaRepository
{
    Task AddAssetsAsync(List<AlpacaAsset> assets);

    Task<List<AlpacaAsset>> GetAssets();

    Task<AlpacaAsset?> GetAsset(string symbol);

    Task<AlpacaBar?> GetLatestBar(string symbol);

    Task<List<AlpacaBar>> GetHistoricalBars(string symbol, DateTime startDate, DateTime endDate);

    Task AddBarsAsync(List<AlpacaBar> bars);

    Task<AlpacaQuote?> GetLatestQuote(string symbol);

    Task<List<AlpacaQuote>> GetHistoricalQuotes(string symbol, DateTime startDate, DateTime endDate);

    Task AddQuotesAsync(List<AlpacaQuote> quotes);

    Task AddOrderAsync(AlpacaOrder order);

    Task<AlpacaOrder?> GetOrderAsync(int id);

    Task UpdateOrderAsync(AlpacaOrder order);

    Task AddUserSettingsAsync(UserSettingsModel userSettings);

    Task<UserSettingsModel?> GetUserSettingsAsync(string userId);

    Task UpdateUserSettingsAsync(UserSettingsModel userSettings);

    Task DeleteUserSettingsAsync(UserSettingsModel userSettings);

    Task AddAlpacaExecutionAsync(AlpacaExecutionModel execution);

    Task UpdateAlpacaExecutionAsync(AlpacaExecutionModel execution);
    
    Task<AlpacaExecutionModel> GetAlpacaExecutionAsync(Guid id);

    Task<List<AlpacaExecutionModel>?> GetActiveAlpacaExecutionsAsync();
    Task<AlpacaExecutionModel?> GetActiveAlpacaExecutionByUserIdAsync(Guid userId);
    Task DeleteAlpacaExecutionsAsync(Guid userId);



}
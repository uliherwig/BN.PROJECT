namespace BN.PROJECT.StrategyService;

public interface IStrategyRepository
{
    Task<List<StrategySettingsModel>> GetStrategiesByUserIdAsync(Guid userId, bool bookmarked);

    Task<StrategySettingsModel?> GetStrategyByIdAsync(Guid testId);

    Task AddStrategyAsync(StrategySettingsModel strategySettings);

    Task DeleteStrategyAsync(StrategySettingsModel strategySettings);

    Task<int> UpdateStrategyAsync(StrategySettingsModel strategySettings);

    Task<List<PositionModel>> GetPositionsByStrategyIdAsync(Guid testId);

    Task AddPositionAsync(PositionModel position);

    Task AddPositionsAsync(List<PositionModel> positions);

    Task UpdatePositionAsync(PositionModel position);

    Task DeletePositionsAsync(List<PositionModel> positions);

    Task CleanupStrategiesAsync();
}
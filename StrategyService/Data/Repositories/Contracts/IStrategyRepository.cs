namespace BN.PROJECT.StrategyService;

public interface IStrategyRepository
{
    Task<List<StrategySettingsModel>> GetStrategiesByUserIdAsync(Guid userId, bool bookmarked);

    Task<StrategySettingsModel?> GetStrategyByIdAsync(Guid testId);

    Task AddStrategyAsync(StrategySettingsModel strategySettings);

    Task DeleteStrategyAsync(StrategySettingsModel strategySettings);

    Task<int> UpdateStrategyAsync(StrategySettingsModel strategySettings);

    Task<List<Position>> GetPositionsByStrategyIdAsync(Guid testId);

    Task AddPositionAsync(Position position);

    Task AddPositionsAsync(List<Position> positions);

    Task UpdatePositionAsync(Position position);

    Task DeletePositionsAsync(List<Position> positions);

    Task CleanupStrategiesAsync();
}
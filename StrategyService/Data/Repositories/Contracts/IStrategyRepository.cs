namespace BN.PROJECT.StrategyService;

public interface IStrategyRepository
{
    Task<List<BacktestSettings>> GetBacktestsByEmailAsync(string email);
    Task<BacktestSettings> GetBacktestByIdAsync(Guid testId);
    Task AddBacktestAsync(BacktestSettings backtestSettings);
    Task DeleteBacktest(BacktestSettings backtestSettings);
    Task UpdateBacktestAsync(BacktestSettings backtestSettings);

    Task<List<Position>> GetPositionsByTestId(Guid testId);
    Task AddPositionAsync(Position position);
    Task AddPositionsAsync(List<Position> positions);
    Task UpdatePositionAsync(Position position);
    Task DeletePositions(List<Position> positions);
    Task CleanupBacktests();
}

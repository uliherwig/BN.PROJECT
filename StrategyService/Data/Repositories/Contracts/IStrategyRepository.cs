namespace BN.PROJECT.StrategyService;

public interface IStrategyRepository
{
    Task AddBacktestAsync(BacktestSettings backtestSettings);
    Task DeleteBacktest(BacktestSettings backtestSettings);
    Task AddPositionAsync(Position position);
    Task AddPositionsAsync(List<Position> positions);
    Task UpdatePositionAsync(Position position);
    Task DeletePositions(List<Position> positions);

}

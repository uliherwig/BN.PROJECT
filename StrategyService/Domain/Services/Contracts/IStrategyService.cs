namespace BN.PROJECT.StrategyService;

public interface IStrategyService
{
    Task StartTest(StrategyMessage message);
    Task EvaluateQuote(Guid strategyId, Quote quote);
    Task StopTest(StrategyMessage message);
    List<Position>? GetPositions(Guid strategyId);
    bool CanHandle(StrategyEnum strategy);
}

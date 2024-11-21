namespace BN.PROJECT.StrategyService;

public interface IStrategyService
{
    Task StartTest(StrategyMessage message);
    Task EvaluateQuote(Guid testId, Quote quote);
    Task StopTest(StrategyMessage message);
    bool CanHandle(StrategyEnum strategy);
}
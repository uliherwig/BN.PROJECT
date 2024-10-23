namespace BN.PROJECT.StrategyService;

public interface IStrategyTestService
{
    Task StartTest(StrategyMessage message);
    Task EvaluateQuote(Guid testId, Quote quote);
    Task StopTest(StrategyMessage message);
}
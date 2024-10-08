namespace BN.PROJECT.StrategyService;

public interface IStrategyService
{
    Task StartTest(BacktestSettings testSettings);
    Task EvaluateQuotes(StrategyMessage message);
    Task StopTest(StrategyMessage message);
}
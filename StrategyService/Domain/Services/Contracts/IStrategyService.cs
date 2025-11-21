namespace BN.PROJECT.StrategyService;

public interface IStrategyService
{
    Task Initialize(StrategyTaskEnum strategyTask, StrategySettingsModel strategySettings);

    Task StartTest(StrategyMessage message);
    Task EvaluateQuote(Guid strategyId, Guid userId, Quote quote);
    Task StopTest(StrategyMessage message);
    Task<TestResult> GetTestResult();
    List<PositionModel> GetPositions();
    bool CanHandle(StrategyEnum strategy);
}
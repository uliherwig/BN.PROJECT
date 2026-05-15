namespace BN.PROJECT.StrategyService;

public interface IIndicatorService
{
    Task<List<BarSignalModel>> StartTest(StrategySettingsModel strategySettings, List<Quote> quotes);
    // Task EvaluateQuote(Guid strategyId, Guid userId, PriceQuote quote);
    // Task StopTest(StrategyMessage message);
    Task<TestResult> GetTestResult();
    List<PositionModel> GetPositions();
    bool CanHandle(IndicatorEnum strategy);
}
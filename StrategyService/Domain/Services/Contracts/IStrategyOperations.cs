namespace BN.PROJECT.StrategyService;

public interface IStrategyOperations
{
    DateTime GetStartOfTimeSpan(DateTime dateTime, TimeSpan timeSpan);
    TimeSpan GetTimeSpanByBreakoutPeriod(BreakoutPeriodEnum tradeInterval);
    BreakoutModel GetBreakoutModel(StrategySettingsModel settings);
}
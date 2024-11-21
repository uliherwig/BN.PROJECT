
namespace BN.PROJECT.StrategyService;

public interface IStrategyOperations
{
    DateTime GetStartOfTimeSpan(DateTime dateTime, TimeSpan timeSpan);
    TimeSpan GetTimeSpanByBreakoutPeriod(BreakoutPeriodEnum tradeInterval);
    Position OpenPosition(BreakoutProcessModel tpm, Quote quote, SideEnum positionType);
    BreakoutModel GetBreakoutModel(StrategySettingsModel settings);
}
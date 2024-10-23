
namespace BN.PROJECT.StrategyService;

public interface IStrategyOperations
{
    DateTime GetStartOfTimeSpan(DateTime dateTime, TimeSpan timeSpan);
    TimeSpan GetTimeSpanByBreakoutPeriod(BreakoutPeriod tradeInterval);
    Position OpenPosition(BacktestSettings settings, StrategyProcessModel tpm, Quote quote, Side positionType);
}
namespace BN.PROJECT.StrategyService;

public interface IStrategyOperations
{
    DateTime GetStartOfTimeSpan(DateTime dateTime, TimeSpan timeSpan);

    TimeSpan GetTimeSpanByBreakoutPeriod(BreakoutPeriodEnum tradeInterval);

    void UpdateOrCloseOpenPosition(ref Position openPosition, Quote quote, decimal trailingStop, decimal takeProfitPercent);

    OrderMessage CreateOrderMessage(Guid strategyId, Guid userId, Position position);
}
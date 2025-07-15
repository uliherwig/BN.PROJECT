namespace BN.PROJECT.StrategyService;
public static class StrategyOperations
{
    public static DateTime GetStartOfTimeSpan(DateTime dateTime, TimeSpan timeSpan)
    {
        long ticksSinceMidnight = dateTime.TimeOfDay.Ticks / timeSpan.Ticks;
        TimeSpan startOfTimeSpan = TimeSpan.FromTicks(ticksSinceMidnight * timeSpan.Ticks);
        return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day).Add(startOfTimeSpan);
    }

    public static TimeSpan GetTimeSpanByBreakoutPeriod(BreakoutPeriodEnum breakoutPeriod)
    {
        return breakoutPeriod switch
        {
            BreakoutPeriodEnum.Day => TimeSpan.FromDays(1),
            BreakoutPeriodEnum.Hour => TimeSpan.FromHours(1),
            BreakoutPeriodEnum.Minute => TimeSpan.FromMinutes(1),
            BreakoutPeriodEnum.TenMinutes => TimeSpan.FromMinutes(10),
            _ => TimeSpan.FromDays(1)
        };
    }

    public static void UpdateOrCloseOpenPosition(ref PositionModel openPosition, Quote quote, decimal trailingStop, decimal takeProfitPercent)
    {
        if (openPosition.Side == SideEnum.Buy)
        {
            if (quote.BidPrice > openPosition.TakeProfit)
            {
                if (trailingStop > 0m)
                {
                    var tp = quote.BidPrice + (quote.BidPrice * takeProfitPercent / 100);
                    var sl = quote.BidPrice - (quote.BidPrice * trailingStop / 100);
                    openPosition.UpdateTakeProfitAndStopLoss(tp, sl);
                }
                else
                {
                    openPosition.ClosePosition(quote.TimestampUtc, quote.BidPrice, "TP");
                }
            }

            if (quote.BidPrice < openPosition.StopLoss)
            {
                openPosition.ClosePosition(quote.TimestampUtc, quote.BidPrice, "SL");
            }
        }

        if (openPosition.Side == SideEnum.Sell)
        {
            if (quote.BidPrice < openPosition.TakeProfit)
            {
                if (trailingStop > 0m)
                {
                    var tp = quote.BidPrice - (quote.BidPrice * takeProfitPercent / 100);
                    var sl = quote.AskPrice + (quote.AskPrice * trailingStop / 100);
                    openPosition.UpdateTakeProfitAndStopLoss(tp, sl);
                }
                else
                {
                    openPosition.ClosePosition(quote.TimestampUtc, quote.AskPrice, "TP");
                }
            }

            if (quote.AskPrice > openPosition.StopLoss)
            {
                openPosition.ClosePosition(quote.TimestampUtc, quote.AskPrice, "SL");
            }
        }
    }

    public static OrderMessage CreateOrderMessage(Guid strategyId, Guid userId, PositionModel position)
    {
        return new OrderMessage
        {
            MessageType = MessageTypeEnum.Order,
            StrategyId = strategyId,
            UserId = userId,
            Position = position
        };
    }
}

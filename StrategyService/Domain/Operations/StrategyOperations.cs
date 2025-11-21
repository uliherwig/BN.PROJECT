namespace BN.PROJECT.StrategyService;
public static class StrategyOperations
{
    public static DateTime GetStartOfTimeSpan(DateTime dateTime, TimeSpan timeSpan)
    {
        long ticksSinceMidnight = dateTime.TimeOfDay.Ticks / timeSpan.Ticks;
        TimeSpan startOfTimeSpan = TimeSpan.FromTicks(ticksSinceMidnight * timeSpan.Ticks);
        return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day).Add(startOfTimeSpan);
    }

    public static TimeSpan GetTimeSpanByBreakoutPeriod(TimeFrameEnum breakoutPeriod)
    {
        return breakoutPeriod switch
        {
            TimeFrameEnum.Day => TimeSpan.FromDays(1),
            TimeFrameEnum.Hour => TimeSpan.FromHours(1),
            TimeFrameEnum.Minute => TimeSpan.FromMinutes(1),
            TimeFrameEnum.TenMinutes => TimeSpan.FromMinutes(10),
            _ => TimeSpan.FromDays(1)
        };
    }
   
    public static decimal CalculateSMA(List<decimal> prices, int period)
    {
        return prices.Skip(prices.Count - period).Take(period).Average();
    }

    public static decimal CalculateSlope(List<decimal> shortSmas,  int window)
    {
        return (shortSmas.Last() - shortSmas[shortSmas.Count - 1 - window]) / window;
    }
}

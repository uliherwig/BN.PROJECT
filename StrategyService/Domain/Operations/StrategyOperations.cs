using Microsoft.Data.Analysis;

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

    public static decimal CalculateSlope(List<decimal> shortSmas, int window)
    {
        return (shortSmas.Last() - shortSmas[shortSmas.Count - 1 - window]) / window;
    }

    public record BarSignalModel(DateTime Timestamp, decimal Open, decimal High, decimal Low, decimal Close, int Signal);    

    public static List<BarSignalModel> DataFrameToBarSignalList(DataFrame df)
    {
        int n = (int)df.Rows.Count;
        var signals = new List<BarSignalModel>(n);

        var tsCol = df.Columns["DT"];
        var openCol = df.Columns["Open"];
        var highCol = df.Columns["High"];
        var lowCol = df.Columns["Low"];
        var closeCol = df.Columns["Close"];
        var signalCol = df.Columns["Signal"];


        for (int i = 0; i < n; i++)
        {
            var ts = tsCol[i] is DateTime dt ? dt : Convert.ToDateTime(tsCol[i]);
            var open = Convert.ToDecimal(openCol[i]);
            var high = Convert.ToDecimal(highCol[i]);
            var low = Convert.ToDecimal(lowCol[i]);
            var close = Convert.ToDecimal(closeCol[i]);
            var signal = Convert.ToInt32(signalCol[i]);

            signals.Add(new BarSignalModel(ts, open, high, low, close, signal));
        }

        return signals;
    }
}


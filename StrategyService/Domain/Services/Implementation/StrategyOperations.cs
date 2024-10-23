namespace BN.PROJECT.StrategyService;

public class StrategyOperations : IStrategyOperations
{
    private readonly ILogger<StrategyOperations> _logger;
    public StrategyOperations(ILogger<StrategyOperations> logger)
    {
        _logger = logger;
    }

    public DateTime GetStartOfTimeSpan(DateTime dateTime, TimeSpan timeSpan)
    {
        long ticksSinceMidnight = dateTime.TimeOfDay.Ticks / timeSpan.Ticks;
        TimeSpan startOfTimeSpan = TimeSpan.FromTicks(ticksSinceMidnight * timeSpan.Ticks);
        return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day).Add(startOfTimeSpan);
    }

    public TimeSpan GetTimeSpanByBreakoutPeriod(BreakoutPeriod breakoutPeriod)
    {
        return breakoutPeriod switch
        {
            BreakoutPeriod.Day => TimeSpan.FromDays(1),
            BreakoutPeriod.Hour => TimeSpan.FromHours(1),
            BreakoutPeriod.Minute => TimeSpan.FromMinutes(1),
            BreakoutPeriod.TenMinutes => TimeSpan.FromMinutes(10),
            _ => TimeSpan.FromDays(1)
        };
    }

    public Position OpenPosition(BacktestSettings settings, StrategyProcessModel tpm, Quote quote, Side positionType)
    {
        var stopLoss = positionType == Side.Buy ? tpm.PrevLow : tpm.PrevHigh;
        var takeProfit = positionType == Side.Buy ? quote.AskPrice + (quote.AskPrice * settings.TakeProfitPercent / 100) : quote.BidPrice - (quote.BidPrice * settings.TakeProfitPercent / 100);
        var position = PositionExtensions.CreatePosition(settings.Id,
                             settings.Symbol,
                             1,
                             positionType,
                             quote.AskPrice,
                             stopLoss,
                             takeProfit,
                             quote.TimestampUtc,
                             tpm.PrevLow,
                             tpm.PrevHigh,
                             tpm.PrevLowStamp,
                             tpm.PrevHighStamp);
        return position;
    }

}

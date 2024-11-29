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

    public TimeSpan GetTimeSpanByBreakoutPeriod(BreakoutPeriodEnum breakoutPeriod)
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

    public BreakoutModel GetBreakoutModel(StrategySettingsModel settings)
    {
        return JsonConvert.DeserializeObject<BreakoutModel>(settings.StrategyParams);
    }
}

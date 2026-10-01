namespace BN.PROJECT.AlpacaService;

public interface IPositionLifecycleService
{
    Task CloseExpiredPositionsAsync(TimeSpan maxHoldingPeriod);
    Task CloseAllPositionsEndOfDayAsync();
}

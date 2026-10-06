namespace BN.PROJECT.AlpacaService;

public interface IPositionLifecycleService
{
    Task CloseExpiredPositionsAsync(TimeSpan maxHoldingPeriod, bool testMode = false);
    Task CloseAllPositionsEndOfDayAsync();
}

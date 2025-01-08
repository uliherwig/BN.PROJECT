namespace BN.PROJECT.AlpacaService;

public interface IBacktestService
{
    Task RunBacktest(StrategySettingsModel testSettings);
}
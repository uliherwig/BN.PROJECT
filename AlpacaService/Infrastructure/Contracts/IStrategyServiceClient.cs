namespace BN.PROJECT.AlpacaService;

public interface IStrategyServiceClient
{
    Task<StrategySettingsModel?> GetStrategyAsync(string strategyId);

    Task<string> StartStrategyAsync(StrategySettingsModel testSettings);
}
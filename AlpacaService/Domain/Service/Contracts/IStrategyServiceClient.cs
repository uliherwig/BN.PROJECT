namespace BN.PROJECT.AlpacaService;

public interface IStrategyServiceClient
{
    Task<string> GetStrategyAsync();
    Task<string> StartStrategyAsync(BacktestSettings testSettings);
}

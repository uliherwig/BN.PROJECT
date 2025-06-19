namespace BN.PROJECT.AlpacaService
{
    public interface IOptimizerServiceClient
    {
        Task<string?> TestOptimizationAsync();
        Task<string> StartOptimizerAsync(StrategySettingsModel testSettings);
    }
}
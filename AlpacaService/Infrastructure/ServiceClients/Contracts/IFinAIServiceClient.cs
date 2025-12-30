namespace BN.PROJECT.AlpacaService
{
    public interface IFinAIServiceClient
    {
        Task<string?> TestOptimizationAsync();
        Task<string?> CreateIndicatorDataframeAsync(StrategySettingsModel testSettings);
        Task<string> StartOptimizerAsync(StrategySettingsModel testSettings);
    }
}
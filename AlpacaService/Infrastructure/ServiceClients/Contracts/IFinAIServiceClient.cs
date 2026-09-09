namespace BN.PROJECT.AlpacaService
{
    public interface IFinAIServiceClient
    {
        Task<string?> TestOptimizationAsync();
        Task<string?> CreateDataframeAsync(StrategySettingsModel testSettings);
        Task<string> StartOptimizerAsync(StrategySettingsModel testSettings);
        Task<string?> StartAlpacaPaperTradingAsync(string strategyName);
        Task<string?> StopAlpacaPaperTradingAsync();
    }
}
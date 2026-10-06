namespace BN.PROJECT.AlpacaService
{
    public interface IFinAIServiceClient
    {
        Task<StrategySettingsDTO[]> GetAiStrategies();
        Task<string?> TestOptimizationAsync();
        Task<string?> CreateDataframeAsync(StrategySettingsModel testSettings);
        Task<string> StartOptimizerAsync(StrategySettingsModel testSettings);
        Task<string?> StartAlpacaPaperTradingAsync(StrategySettingsDTO strategySettings);
        Task<string?> StopAlpacaPaperTradingAsync(StrategySettingsDTO strategySettings);
    }
}
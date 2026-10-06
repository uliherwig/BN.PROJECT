
namespace BN.PROJECT.StrategyService
{
    public interface IFinAIServiceClient
    {
        Task<string?> GetLgbModelById(string id);
        Task<StrategySettingsDTO[]> GetLgbModels();
        Task<string?> CreateIndicatorDataframeAsync(StrategySettingsModel testSettings);
    }
}
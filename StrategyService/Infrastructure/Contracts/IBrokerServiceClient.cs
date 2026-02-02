
namespace BN.PROJECT.StrategyService
{
    public interface IBrokerServiceClient
    {
        Task<string> GetStrategyAsync();
        Task<string> StartStrategyAsync(StrategySettingsModel testSettings);
    }
}
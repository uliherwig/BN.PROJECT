
namespace BN.PROJECT.StrategyService
{
    public interface IOptimizerService
    {
        Task<OptimizationResultModel> Finalize(Guid strategyId);
        Task Initialize(Guid strategyId);
        Task<bool> Run( StrategySettingsModel strategySettings);
    }
}
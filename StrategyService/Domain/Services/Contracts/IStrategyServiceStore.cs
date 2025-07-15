namespace BN.PROJECT.StrategyService;

public interface IStrategyServiceStore
{
    IOptimizerService GetOrCreateOptimizer(Guid strategyId);
    void RemoveOptimizer(Guid strategyId);
    IStrategyService GetOrCreateBacktester(Guid strategyId, StrategyEnum strategyEnum);
    void RemoveBacktester(Guid strategyId);
    void Clear();
}
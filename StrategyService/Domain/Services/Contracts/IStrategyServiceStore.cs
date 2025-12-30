namespace BN.PROJECT.StrategyService;

public interface IStrategyServiceStore
{ 
    IStrategyService GetOrCreateStrategyService(Guid strategyId, StrategyEnum strategyEnum);
    void RemoveStrategyService(Guid strategyId);
    void Clear();
}
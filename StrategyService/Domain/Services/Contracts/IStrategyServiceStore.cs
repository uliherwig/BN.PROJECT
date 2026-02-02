namespace BN.PROJECT.StrategyService;

public interface IStrategyServiceStore
{ 
    IStrategyService GetOrCreateStrategyService(Guid strategyId, IndicatorEnum strategyEnum);
    void RemoveStrategyService(Guid strategyId);
    void Clear();
}
namespace BN.PROJECT.StrategyService;

public interface IStrategyServiceStore
{
    IStrategyService GetOrCreateStrategyService(Guid strategyId, IndicatorEnum strategyEnum);
    void RemoveStrategyService(Guid strategyId);
    IIndicatorService GetOrCreateIndicatorService(Guid strategyId, IndicatorEnum indicatorEnum);
    void RemoveIndicatorService(Guid strategyId);
    void Clear();
}
namespace BN.PROJECT.StrategyService;

public interface IStrategyServiceStore
{
    IKafkaProducerService GetOrCreateKafkaProducer(Guid strategyId);
    void RemoveKafkaProducer(Guid strategyId);
    IStrategyService GetOrCreateStrategyService(Guid strategyId, StrategyEnum strategyEnum);
    void RemoveStrategyService(Guid strategyId);
    void Clear();
}
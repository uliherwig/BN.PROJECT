namespace BN.PROJECT.AlpacaService;

public interface IStrategyTestService
{

    Task RunBacktest(StrategySettingsModel testSettings);
    Task OptimizeStrategy(StrategySettingsModel testSettings);
    Task StartExecution(Guid userId, Guid strategyId);
    Task StopExecution(Guid userId, Guid strategyId);
    Task CreateAlpacaOrder(OrderMessage orderMessage);
}
namespace BN.PROJECT.AlpacaService;

public interface IStrategyTestService
{
    Task RunBacktest(StrategySettingsModel testSettings);
    Task RunExecution(Guid userId,Guid strategyId);
    Task StopExecution(Guid userId, Guid  strategyId);
    Task CreateAlpacaOrder(OrderMessage orderMessage);
}
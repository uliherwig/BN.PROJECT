
namespace BN.PROJECT.StrategyService
{
    public interface IStrategyService
    {
        Task BacktestWithBars(BacktestSettings testSettings);
        Task EvaluateQuote(string message);
        Task InitializeStrategyTest();
        Task RunStrategyTest(BacktestSettings testSettings);
        Task StopStrategyTest();
    }
}

namespace BN.PROJECT.AlpacaService;

public interface IBacktestService
{
    Task<string> RunBacktest(BacktestSettings testSettings);
}
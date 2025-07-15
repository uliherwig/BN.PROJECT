namespace BN.PROJECT.StrategyService;

public class StrategyServiceStore : IStrategyServiceStore
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<Guid, IOptimizerService> _optimizers = new();
    private readonly ConcurrentDictionary<Guid, IStrategyService> _backtesters = new();

    public StrategyServiceStore(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IOptimizerService GetOrCreateOptimizer(Guid strategyId)
    {
        return _optimizers.GetOrAdd(strategyId, _ =>
        {
            var scope = _serviceProvider.CreateScope();
            var optimizer = scope.ServiceProvider.GetService<IOptimizerService>();

            return optimizer;
        });
    }

    public void RemoveOptimizer(Guid strategyId)
    {
        if (_optimizers.TryRemove(strategyId, out var optimizer))
        {
            // Dispose the optimizer if it implements IDisposable
            if (optimizer is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    public IStrategyService GetOrCreateBacktester(Guid strategyId, StrategyEnum strategyEnum)
    {
        return _backtesters.GetOrAdd(strategyId, _ =>
        {
            var scope = _serviceProvider.CreateScope();
            var services = scope.ServiceProvider.GetServices<IStrategyService>();
            var backtester = services.Single(s => s.CanHandle(strategyEnum));

            return backtester;
        });
    }

    public void RemoveBacktester(Guid strategyId)
    {
        if (_backtesters.TryRemove(strategyId, out var backtester))
        {      
            if (backtester is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    public void Clear()
    {
        _backtesters.Clear();
    }
}

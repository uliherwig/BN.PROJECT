namespace BN.PROJECT.StrategyService;

public class StrategyServiceStore : IStrategyServiceStore
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<Guid, IStrategyService> _strategyServices = new();

    public StrategyServiceStore(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IStrategyService GetOrCreateStrategyService(Guid strategyId, StrategyEnum strategyEnum)
    {
        return _strategyServices.GetOrAdd(strategyId, _ =>
        {
            var scope = _serviceProvider.CreateScope();
            var services = scope.ServiceProvider.GetServices<IStrategyService>();
            var backtester = services.Single(s => s.CanHandle(strategyEnum));

            return backtester;
        });
    }

    public void RemoveStrategyService(Guid strategyId)
    {
        if (_strategyServices.TryRemove(strategyId, out var backtester))
        {      
            if (backtester is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    public void Clear()
    {
        _strategyServices.Clear();
    }
}

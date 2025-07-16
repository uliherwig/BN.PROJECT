namespace BN.PROJECT.StrategyService;

public class StrategyServiceStore : IStrategyServiceStore
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<Guid, IKafkaProducerService> _kafkaProducers = new();
    private readonly ConcurrentDictionary<Guid, IStrategyService> _strategyServices = new();

    public StrategyServiceStore(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IKafkaProducerService GetOrCreateKafkaProducer(Guid strategyId)
    {
        return _kafkaProducers.GetOrAdd(strategyId, _ =>
        {
            var scope = _serviceProvider.CreateScope();
            var producer = scope.ServiceProvider.GetService<IKafkaProducerService>();

            return producer == null ? throw new InvalidOperationException("IKafkaProducerService not found.") : producer;
        });
    }

    public void RemoveKafkaProducer(Guid strategyId)
    {
        if (_kafkaProducers.TryRemove(strategyId, out var optimizer))
        {
            // Dispose the optimizer if it implements IDisposable
            if (optimizer is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
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

namespace BN.PROJECT.StrategyService
{
    public interface ITestQueueService<TKey, TValue>
    {
        int Count { get; }

        void Add(TKey key, TValue value);
        void Remove(TKey key);
        bool TryGet(TKey key, out TValue value);
    }
}
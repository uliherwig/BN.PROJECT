using System.Collections.Concurrent;

namespace BN.PROJECT.StrategyService;

public class TestQueueService<TKey, TValue> : ITestQueueService<TKey, TValue>
{

    private readonly ConcurrentDictionary<TKey, TValue> _cache = new ConcurrentDictionary<TKey, TValue>();
    private readonly ConcurrentQueue<TKey> _keyQueue = new ConcurrentQueue<TKey>();
    private readonly int _capacity;

    public TestQueueService(int capacity)
    {
        _capacity = capacity;
    }

    public void Add(TKey key, TValue value)
    {
        if (_cache.TryAdd(key, value))
        {
            _keyQueue.Enqueue(key);

            // Remove the oldest item if capacity is exceeded
            if (_keyQueue.Count > _capacity)
            {
                if (_keyQueue.TryDequeue(out TKey oldestKey))
                {
                    _cache.TryRemove(oldestKey, out _);
                }
            }
        }
    }

    public bool TryGet(TKey key, out TValue value)
    {
        return _cache.TryGetValue(key, out value);
    }

    public void Remove(TKey key)
    {
        _cache.TryRemove(key, out _);
        // No need to update the queue for removal since it is naturally handled during dequeue.
    }

    public int Count => _cache.Count;
}

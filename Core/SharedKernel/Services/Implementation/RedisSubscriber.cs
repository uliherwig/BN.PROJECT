namespace BN.PROJECT.Core;

public class RedisSubscriber : IRedisSubscriber
{
    private readonly IConnectionMultiplexer _redis;

    public RedisSubscriber(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public void Subscribe(string channel, Action<RedisChannel, RedisValue> handler)
    {
        var subscriber = _redis.GetSubscriber();
        subscriber.Subscribe(new RedisChannel(channel, RedisChannel.PatternMode.Literal), handler);
    }
}

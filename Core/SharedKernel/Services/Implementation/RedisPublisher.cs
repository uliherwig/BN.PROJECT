namespace BN.PROJECT.Core;

public class RedisPublisher : IRedisPublisher
{
    private readonly IConnectionMultiplexer _redis;

    public RedisPublisher(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public Task Publish(string channel, string message)
    {
        var subscriber = _redis.GetSubscriber();
        subscriber.Publish(channel, message);
        return Task.CompletedTask;
    }

    public async Task PublishAsync(string channel, string message)
    {
        var subscriber = _redis.GetSubscriber();
        await subscriber.PublishAsync(channel, message);
    }
}

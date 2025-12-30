namespace BN.PROJECT.Core;

public interface IRedisSubscriber
{
    void Subscribe(string channel, Action<RedisChannel, RedisValue> handler);
}
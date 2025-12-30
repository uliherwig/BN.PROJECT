namespace BN.PROJECT.Core;

public interface IRedisPublisher
{
    Task Publish(string channel, string message);
    Task PublishAsync(string channel, string message);
}
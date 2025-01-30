namespace BN.PROJECT.Core;

public interface IKafkaProducerService
{
    Task SendMessageAsync(string topic, string message, CancellationToken cancellationToken = default);
}
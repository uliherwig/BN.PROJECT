namespace BN.PROJECT.Core;

public interface IKafkaProducerHostedService
{
    Task SendMessageAsync(string topic, string message, CancellationToken cancellationToken = default);
}
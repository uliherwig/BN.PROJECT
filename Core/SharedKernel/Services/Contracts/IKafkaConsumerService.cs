namespace BN.PROJECT.Core;

public interface IKafkaConsumerService
{
    void Start(string topic);

    void Stop();

    event Action<string> MessageReceived;

    Task DeleteMessagesAsync(string topic);
}
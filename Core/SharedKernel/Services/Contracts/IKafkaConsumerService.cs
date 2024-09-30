namespace BN.PROJECT.Core;
public interface IKafkaConsumerService
{
    void Start(string topic);
    void Stop();
    //Task DeleteMessagesAsync(string topic);

    event Action<string> MessageReceived;
}
namespace BN.PROJECT.Core;

public class KafkaProducerService : IKafkaProducerService
{
    private readonly IConfiguration _configuration;
    private IProducer<Null, string> _producer;

    public KafkaProducerService(IConfiguration configuration)
    {
        _configuration = configuration;
        var config = new ProducerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"],
        };
        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public async Task SendMessageAsync(string topic, string message, CancellationToken cancellationToken)
    {
        try
        {
            _ = await _producer.ProduceAsync(topic, new Message<Null, string> { Value = message }, cancellationToken);
            _ = _producer.Flush(TimeSpan.FromSeconds(10));
        }
        catch (Exception ex)
        {
            throw new Exception("Error sending Kafka message", ex);

        }
    }
}
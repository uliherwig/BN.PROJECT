namespace BN.PROJECT.Core;

public class KafkaProducerService : IKafkaProducerService
{
    private readonly ILogger<KafkaProducerService> _logger;
    private IProducer<Null, string> _producer;

    public KafkaProducerService(ILogger<KafkaProducerService> logger)
    {
        _logger = logger;
        var config = new ProducerConfig
        {
            BootstrapServers = "localhost:9092",
        };
        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public async Task SendMessageAsync(string topic, string message, CancellationToken cancellationToken)
    {
        try
        {
            _ = await _producer.ProduceAsync(topic, new Message<Null, string> { Value = message }, cancellationToken);
            _logger.LogInformation($"Produced message: {message}");

            _ = _producer.Flush(TimeSpan.FromSeconds(10));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Senden der Kafka-Nachricht.");
        }
    }
}
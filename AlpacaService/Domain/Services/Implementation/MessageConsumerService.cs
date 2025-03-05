using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace BN.PROJECT.AlpacaService;

public class MessageConsumerService : IHostedService
{
    private readonly ILogger<MessageConsumerService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    private readonly string[] topicNames = ["order", "strategy"];
    private readonly int numPartitions = 3;
    private readonly short replicationFactor = 1;



    public MessageConsumerService(
        IConfiguration configuration,
        ILogger<MessageConsumerService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {           
            var adminConfig = new AdminClientConfig { BootstrapServers = _configuration["Kafka:BootstrapServers"] };

            using var adminClient = new AdminClientBuilder(adminConfig).Build();

            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(5));

            foreach (var topicName in topicNames)
            {
                bool topicExists = metadata.Topics.Any(t => t.Topic == topicName);

                if (!topicExists)
                {
                    _logger.LogInformation($"Erstelle Kafka-Topic: {topicName}...");

                    await adminClient.CreateTopicsAsync(new[]
                    {
                    new TopicSpecification
                    {
                        Name = topicName,
                        NumPartitions = numPartitions,
                        ReplicationFactor = replicationFactor
                    }
                });

                    _logger.LogInformation($"Topic '{topicName}' wurde erfolgreich erstellt!");
                }
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                var kafkaConsumer = scope.ServiceProvider.GetRequiredService<IKafkaConsumerService>();

                kafkaConsumer.Start("order");
                kafkaConsumer.MessageReceived += ConsumeMessage;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in MessageConsumerService");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async void ConsumeMessage(string messageJson)
    {
        var message = JsonConvert.DeserializeObject<OrderMessage>(messageJson);
        if (message == null)
        {
            return;
        }

        using (var scope = _serviceProvider.CreateScope())
        {
            var strategyService = scope.ServiceProvider.GetRequiredService<IStrategyTestService>();

            if (message.MessageType == MessageTypeEnum.Order)
            {
                await strategyService.CreateAlpacaOrder(message);
            }
        }
    }
}
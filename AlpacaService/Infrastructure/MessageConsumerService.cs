namespace BN.PROJECT.AlpacaService;

public class MessageConsumerService : IHostedService
{
    private readonly ILogger<MessageConsumerService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;



    //private readonly string[] topicNames = ["order", "strategy"];
    private readonly string[] topicNames = Enum.GetNames(typeof(KafkaTopicEnum))
    .Select(x => x.ToLowerInvariant())
    .ToArray();


    private readonly int numPartitions = 1; // TODO should be configurable
    private readonly short replicationFactor = 1;  // TODO should be configurable

    public MessageConsumerService(
        IConfiguration configuration,
        ILogger<MessageConsumerService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var kafkaConsumer = scope.ServiceProvider.GetRequiredService<IKafkaConsumerService>();

                var topicName = KafkaUtilities.GetTopicName(KafkaTopicEnum.Order);
                if (string.IsNullOrEmpty(topicName))
                {
                    _logger.LogError("Kafka topic name is null or empty.");
                    return Task.CompletedTask;
                }

                kafkaConsumer.Start(topicName);
                kafkaConsumer.MessageReceived += ConsumeMessage;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in MessageConsumerService");
        }
        return Task.CompletedTask;
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
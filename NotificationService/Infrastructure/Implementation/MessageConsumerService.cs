using BN.PROJECT.Core;

namespace BN.PROJECT.NotificationService;

public class MessageConsumerService : IHostedService
{

    // TODO kafka setup service
    private readonly ILogger<MessageConsumerService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IDatabase _redisDatabase;
    private readonly string _notificationTopic = KafkaUtilities.GetTopicName(KafkaTopicEnum.Notification);

    public MessageConsumerService(
        IConfiguration configuration,
        ILogger<MessageConsumerService> logger,
        IServiceProvider serviceProvider,
        IHubContext<NotificationHub> hubContext,
        IConnectionMultiplexer redis
    )
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _hubContext = hubContext;
        _redisDatabase = redis.GetDatabase();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var kafkaConsumer = scope.ServiceProvider.GetRequiredService<IKafkaConsumerService>();

                kafkaConsumer.Start(_notificationTopic);
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
        var message = JsonConvert.DeserializeObject<StrategyMessage>(messageJson);
        if (message == null)
        {
            return;
        }

        _logger.LogInformation("Received message: {Message}", message.ToJson());

        var messageType = NotificationEnum.None;

        switch (message.MessageType)
        {
            case MessageTypeEnum.StartTest:
                messageType = NotificationEnum.StrategyStart;
                break;
            case MessageTypeEnum.StopTest:
                messageType = NotificationEnum.StrategyStop;
                break;
        }


        var notificationMessage = new NotificationMessage
        {
            UserId = message.UserId,
            NotificationType = messageType,
        };

        var connectionId = await _redisDatabase.StringGetAsync(message.UserId.ToString());
        if (!string.IsNullOrEmpty(connectionId.ToString()))
        {
            await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveNotification", notificationMessage.ToJson());
        }
    }
}
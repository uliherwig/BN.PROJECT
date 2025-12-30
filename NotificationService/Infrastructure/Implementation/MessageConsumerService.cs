namespace BN.PROJECT.NotificationService;

public class MessageConsumerService : IHostedService
{
    private readonly ILogger<MessageConsumerService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IDatabase _redisDatabase;
    private readonly string _notificationTopic = RedisUtilities.GetChannelName(RedisChannelEnum.Notification);

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
                var redisSubscriber = scope.ServiceProvider.GetRequiredService<IRedisSubscriber>();

             
                if (string.IsNullOrEmpty(_notificationTopic))
                {
                    return;
                }

                redisSubscriber.Subscribe(_notificationTopic, (channel, msg) =>
                {
                    ConsumeMessage(msg);
                });

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
        var notificationMessage = JsonConvert.DeserializeObject<NotificationMessage>(messageJson);
        if (notificationMessage == null)
        {
            return;
        }

        var connectionId = await _redisDatabase.StringGetAsync(notificationMessage.UserId.ToString());

        if (!string.IsNullOrEmpty(connectionId.ToString()))
        {
            await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveNotification", notificationMessage.ToJson());
        }
    }
}
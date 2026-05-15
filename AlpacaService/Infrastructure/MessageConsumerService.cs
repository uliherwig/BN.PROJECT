namespace BN.PROJECT.AlpacaService;

public class MessageConsumerService : IHostedService
{
    private readonly ILogger<MessageConsumerService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public MessageConsumerService(
        ILogger<MessageConsumerService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;    
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var redisSubscriber = scope.ServiceProvider.GetRequiredService<IRedisSubscriber>();

                var topicName = RedisUtilities.GetChannelName(RedisChannelEnum.Order);
                if (string.IsNullOrEmpty(topicName))
                {
                    return Task.CompletedTask;
                }

                redisSubscriber.Subscribe(topicName, (channel, msg) =>
                {                    
                    ConsumeMessage(msg);
                });
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
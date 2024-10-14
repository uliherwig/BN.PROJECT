namespace BN.PROJECT.StrategyService;

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

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var kafkaConsumer = scope.ServiceProvider.GetRequiredService<IKafkaConsumerService>();

                kafkaConsumer.Start("strategy");
                kafkaConsumer.MessageReceived += AddMessage;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in QuoteConsumerService");
            return;
        }

    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async void AddMessage(string messageJson)
    {
        var message = JsonConvert.DeserializeObject<StrategyMessage>(messageJson);
        _logger.LogInformation($"Nachricht empfangen: {message}");

        using (var scope = _serviceProvider.CreateScope())
        {
            var strategyService = scope.ServiceProvider.GetRequiredService<IStrategyService>();

            if (message?.Type == MessageType.StartTest)
            {
                await strategyService.StartTest(message.TestSettings);
            }

            if (message?.Type == MessageType.Quotes)
            {
                await strategyService.EvaluateQuotes(message);
            }

            if (message?.Type == MessageType.StopTest)
            {
                await strategyService.StopTest(message);
            }
        }
    }
}



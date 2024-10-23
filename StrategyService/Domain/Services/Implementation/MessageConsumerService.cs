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
                kafkaConsumer.MessageReceived += ConsumeMessage;
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

    private async void ConsumeMessage(string messageJson)
    {
        var message = JsonConvert.DeserializeObject<StrategyMessage>(messageJson);
        _logger.LogInformation($"Nachricht empfangen: {message}");

        using (var scope = _serviceProvider.CreateScope())
        {
            var strategyService = scope.ServiceProvider.GetRequiredService<IStrategyTestService>();

            if (message?.Type == MessageType.StartTest)
            {
                await strategyService.StartTest(message);
            }

            if (message?.Type == MessageType.Quotes)
            {
                foreach (var quote in message.Quotes)
                {
                    await strategyService.EvaluateQuote(message.TestId, quote);
                }
            }

            if (message?.Type == MessageType.StopTest)
            {
                await strategyService.StopTest(message);
            }
        }
    }
}



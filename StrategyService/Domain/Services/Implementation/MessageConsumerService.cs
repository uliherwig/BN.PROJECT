using System.Diagnostics;

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

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var kafkaConsumer = scope.ServiceProvider.GetRequiredService<IKafkaConsumerService>();

                var topicName = Enum.GetName(KafkaTopicEnum.Strategy)?.ToLowerInvariant();
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
        var message = JsonConvert.DeserializeObject<StrategyMessage>(messageJson);
        if (message == null)
        {
            return;
        }

        using (var scope = _serviceProvider.CreateScope())
        {
            var services = scope.ServiceProvider.GetServices<IStrategyService>();

            var strategyService = services.Single(s => s.CanHandle(message.Strategy));

            if (message.MessageType == MessageTypeEnum.StartTest)
            {
                await strategyService.StartTest(message);
            }

            if (message.MessageType == MessageTypeEnum.Quotes && message.Quotes != null)
            {
                var stopwatch = Stopwatch.StartNew();

                foreach (var quote in message.Quotes)
                {
                    await strategyService.EvaluateQuote(message.StrategyId,message.UserId, quote);
                }
                stopwatch.Stop();
                _logger.LogInformation($"EvaluateQuote done in {stopwatch.Elapsed.TotalMilliseconds} ms. Number of quotes = {message.Quotes.Count}");
            }

            if (message.MessageType == MessageTypeEnum.StopTest)
            {
                await strategyService.StopTest(message);
            }
        }
    }
}
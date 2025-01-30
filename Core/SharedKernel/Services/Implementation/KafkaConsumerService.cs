namespace BN.PROJECT.Core;

public class KafkaConsumerService : IKafkaConsumerService
{
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly string _bootstrapServers = "localhost:9092";
    private string _topic = "kafka-demo";
    private string _groupId = "BN.PROJECT";
    private CancellationTokenSource _cancellationTokenSource;
    private Task _consumingTask;

    public event Action<string> MessageReceived;

    public KafkaConsumerService(ILogger<KafkaConsumerService> logger)
    {
        _logger = logger;
    }

    public void Start(string topic)
    {
        if (_consumingTask == null || _consumingTask.IsCompleted)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _topic = topic;
            _consumingTask = Task.Run(() => Consume(_cancellationTokenSource.Token));
        }
    }

    private void Consume(CancellationToken cancellationToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _bootstrapServers,
            GroupId = _groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,

            //EnableAutoOffsetStore = false,
            //EnableAutoCommit = true,
            //StatisticsIntervalMs = 5000,
            //SessionTimeoutMs = 6000,
            //EnablePartitionEof = true,
            //// A good introduction to the CooperativeSticky assignor and incremental rebalancing:
            //// https://www.confluent.io/blog/cooperative-rebalancing-in-kafka-streams-consumer-ksqldb/
            //PartitionAssignmentStrategy = PartitionAssignmentStrategy.CooperativeSticky
        };

        using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
        {
            consumer.Subscribe(_topic);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var consumeResult = consumer.Consume(cancellationToken);
                    MessageReceived?.Invoke(consumeResult.Message.Value);
                }
            }
            catch (ConsumeException ex)
            {
                _logger.LogError($"Consume error: {ex.Error.Reason}");
                consumer.Close();
            }
            catch (OperationCanceledException)
            {
                consumer.Close();
            }

        }
    }

    public void Stop()
    {
        _cancellationTokenSource?.Cancel();
    }

    public async Task DeleteMessagesAsync(string topic)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _bootstrapServers,
            GroupId = _groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };
        using (var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = _bootstrapServers }).Build())
        {
            await adminClient.DeleteTopicsAsync(new List<string> { topic }, null);
        }
    }
}
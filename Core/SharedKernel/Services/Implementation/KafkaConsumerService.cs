using Confluent.Kafka.Admin;
using System.Threading.Tasks;

namespace BN.PROJECT.Core;

public class KafkaConsumerService : IKafkaConsumerService
{
    private string _topic = "kafka-demo";
    private string _groupId = "BN.PROJECT";
    private CancellationTokenSource _cancellationTokenSource;
    private Task _consumingTask;

    public event Action<string> MessageReceived;
    private readonly IConfiguration _configuration;

    public KafkaConsumerService(IConfiguration configuration) => _configuration = configuration;

    public void Start(string topic)
    {
        if (_consumingTask == null || _consumingTask.IsCompleted)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _topic = topic;
            _consumingTask = Task.Run(() => Consume(_cancellationTokenSource.Token));
        }
    }

    private async Task Consume(CancellationToken cancellationToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _configuration["KafkaServer"],
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
               throw new Exception($"Consume error: {ex.Error.Reason}");             
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
            BootstrapServers = _configuration["KafkaServer"],
            GroupId = _groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };
        using (var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = _configuration["KafkaServer"] }).Build())
        {
            await adminClient.DeleteTopicsAsync(new List<string> { topic }, null);
        }
    }
}
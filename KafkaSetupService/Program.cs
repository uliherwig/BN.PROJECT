using BN.PROJECT.Core;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var _topicNames = Enum.GetNames(typeof(KafkaTopicEnum))
.Select(x => x.ToLowerInvariant())
.ToArray();
var numPartitions = configuration.GetValue<int>("Kafka:NumPartitions", 1);
var replicationFactor = configuration.GetValue<short>("Kafka:ReplicationFactor", 1);

var adminConfig = new AdminClientConfig { BootstrapServers = configuration["Kafka:BootstrapServers"] };

using var adminClient = new AdminClientBuilder(adminConfig).Build();

var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(5));

foreach (var topicName in _topicNames)
{
    bool topicExists = metadata.Topics.Any(t => t.Topic == topicName);

    if (!topicExists)
    {
        await adminClient.CreateTopicsAsync(new[]
        {
                    new TopicSpecification
                    {
                        Name = topicName,
                        NumPartitions = numPartitions,
                        ReplicationFactor = replicationFactor
                    }
                });
    }
}
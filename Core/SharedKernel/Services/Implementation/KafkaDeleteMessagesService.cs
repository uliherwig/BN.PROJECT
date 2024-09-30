namespace BN.PROJECT.Core;

using Confluent.Kafka;
using Confluent.Kafka.Admin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class KafkaDeleteMessagesService
{
    private readonly string _bootstrapServers = "localhost:9092";
    private string _groupId = "BN.PROJECT"; 

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
            using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
            {
                // Assign the topic and get the partition offsets
                var partitions = consumer.Assignment;
                consumer.Subscribe(topic);
                consumer.Consume(TimeSpan.FromSeconds(1)); // Trigger assignment

                var deleteOffsets = new List<TopicPartitionOffset>();
                var metadata = adminClient.GetMetadata(topic, TimeSpan.FromSeconds(10));

                // Loop through each partition and set the offset for deletion
                foreach (var partition in metadata.Topics[0].Partitions)
                {
                    var tp = new TopicPartition(topic, partition.PartitionId);
                    var committed = consumer.Committed(new List<TopicPartition> { tp }, TimeSpan.FromSeconds(10));
                    var endOffset = consumer.GetWatermarkOffsets(tp).High;

                    // Specify up to which offset you want to delete
                    deleteOffsets.Add(new TopicPartitionOffset(tp, endOffset));
                }

                // Execute Delete Records request
                var deleteRecordsResult = await adminClient.DeleteRecordsAsync(deleteOffsets);

                // Print the results
                foreach (var result in deleteRecordsResult)
                {
                    Console.WriteLine($"Partition: {result.Offset.Value}");
                }
            }
        }
    }
}


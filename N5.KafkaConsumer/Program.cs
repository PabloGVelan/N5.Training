using Confluent.Kafka;
using N5.Kafka.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace N5.KafkaConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new Dictionary<string, object>()
            {
                { "bootstrap.servers", "localhost:9092" },
                { "group.id", "groupId" },
                { "enable.auto.commit", true },
                { "auto.commit.interval.ms", 5000 },
                { "statistics.interval.ms", 60000 }
            };

            bool cancelled = false;

            using (var consumer = new Consumer<string, string>(config, new ObjectSerializer<string>(), new ObjectSerializer<string>()))
            {
                consumer.OnMessage += (_, msg) =>
                {
                    var logMsg = $"OnMessage - Key: {msg.Key}, Partition: {msg.Partition}, Offset: {msg.Offset}";

                    Console.WriteLine(logMsg);

                    try
                    {
                        Console.WriteLine(msg);
                        //do something
                    }
                    catch (Exception ex)
                    {
                        var errorMsg = $"OnMessage.Error - Key: {msg.Key}, Partition: {msg.Partition}, Offset: {msg.Offset}, Ex: {ex.ToString()}, Value: {JsonConvert.SerializeObject(msg.Value, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })}";

                        Console.WriteLine(errorMsg);
                    }
                };

                consumer.OnPartitionEOF += (_, end) =>
                {
                    var msg = $"OnPartitionEOF - Reached end of topic {end.Topic} partition {end.Partition}, next message will be at offset {end.Offset}";

                    Console.WriteLine(msg);
                };

                consumer.OnError += (_, error) =>
                {
                    var msg = $"OnError - Error: {error}";

                    Console.WriteLine(msg);

                };

                consumer.OnConsumeError += (_, msg) =>
                {
                    var logMsg = $"OnConsumeError - Error consuming from topic {msg.Topic}/partition {msg.Partition}/offset {msg.Offset}: {msg.Error}";

                    Console.WriteLine(logMsg);
                };

                consumer.OnOffsetsCommitted += (_, commit) =>
                {
                    var offsetsMsg = $"OnOffsetsCommitted - [{string.Join(", ", commit.Offsets)}]";

                    Console.WriteLine(offsetsMsg);

                    if (commit.Error)
                    {
                        var offsetCommitErrorMsg = $"OnOffsetsCommitted.Error - Failed to commit offsets: {commit.Error}";

                        Console.WriteLine(offsetCommitErrorMsg);
                    }
                    else
                    {
                        var offsetCommitSuccessMsg = $"OnOffsetsCommitted.Success - Successfully committed offsets: [{string.Join(", ", commit.Offsets)}]";

                        Console.WriteLine(offsetCommitSuccessMsg);
                    }
                };

                consumer.OnPartitionsAssigned += (_, partitions) =>
                {
                    var msg = $"OnPartitionsAssigned - Assigned partitions: [{string.Join(", ", partitions)}], member id: {consumer.MemberId}";

                    Console.WriteLine(msg);
                    consumer.Assign(partitions);
                };

                consumer.OnPartitionsRevoked += (_, partitions) =>
                {
                    var msg = $"OnPartitionsRevoked - Revoked partitions: [{string.Join(", ", partitions)}]";

                    Console.WriteLine(msg);
                    consumer.Unassign();
                };

                consumer.OnStatistics += (_, json) =>
                {
                    var msg = $"OnStatistics - Statistics: {json}";

                    Console.WriteLine(msg);
                };

                consumer.Subscribe("topic");

                Console.CancelKeyPress += (_, e) =>
                {
                    e.Cancel = true;  // prevent the process from terminating.
                    cancelled = true;
                };

                Console.WriteLine("Ctrl-C to exit.");

                while (!cancelled)
                {
                    consumer.Poll(TimeSpan.FromMilliseconds(50));
                }
            }
        }
    }
}

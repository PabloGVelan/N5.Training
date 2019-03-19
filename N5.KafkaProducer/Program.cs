using System;
using System.Collections.Generic;
using Confluent.Kafka;
using N5.Kafka.Utils;

namespace N5.KafkaProducer
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new Dictionary<string, object>()
            {
                { "bootstrap.servers", "localhost:9092" },
                {  "api.version.request", true }
            };

            var producer = new Producer<string, string>(config, new ObjectSerializer<string>(), new ObjectSerializer<string>());

            producer.ProduceAsync("topic", "key", "a la grande le puse cuca");
        }
    }
}

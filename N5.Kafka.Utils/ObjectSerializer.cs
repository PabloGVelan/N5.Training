using Confluent.Kafka.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace N5.Kafka.Utils
{
    public class ObjectSerializer<T> : ISerializer<T>, IDeserializer<T> where T : class
    {
        private JsonSerializer _jsonSerializer;

        public ObjectSerializer()
        {
            _jsonSerializer = new JsonSerializer();
        }

        public IEnumerable<KeyValuePair<string, object>> Configure(IEnumerable<KeyValuePair<string, object>> config, bool isKey)
        {
            return config;
        }

        public T Deserialize(string topic, byte[] data)
        {
            try
            {
                return _jsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(data));
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public void Dispose()
        {
        }

        public byte[] Serialize(string topic, T data)
        {
            return Encoding.UTF8.GetBytes(_jsonSerializer.Serialize(data));
        }
    }
}

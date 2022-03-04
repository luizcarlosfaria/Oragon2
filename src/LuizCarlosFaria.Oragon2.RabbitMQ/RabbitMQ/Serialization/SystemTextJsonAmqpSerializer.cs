using RabbitMQ.Client;
using System;
using System.Diagnostics;
using System.Text;

namespace AmqpAdapters.Serialization;


public class SystemTextJsonAmqpSerializer : AmqpBaseSerializer
{

    public SystemTextJsonAmqpSerializer(ActivitySource activitySource) : base(activitySource, "SystemTextJsonAmqpSerializer") { }


    protected override TResponse DeserializeInternal<TResponse>(IBasicProperties basicProperties, ReadOnlyMemory<byte> body)
    {
        string message = Encoding.UTF8.GetString(body.ToArray());
        return System.Text.Json.JsonSerializer.Deserialize<TResponse>(message);
    }

    protected override byte[] SerializeInternal<T>(IBasicProperties basicProperties, T objectToSerialize)
    {
        return Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(objectToSerialize));
    }
}


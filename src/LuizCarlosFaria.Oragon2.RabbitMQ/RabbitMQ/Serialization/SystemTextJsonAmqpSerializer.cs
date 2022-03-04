using RabbitMQ.Client;
using System;
using System.Diagnostics;
using System.Text;

namespace LuizCarlosFaria.Oragon2.RabbitMQ.Serialization;

public class SystemTextJsonAmqpSerializer : AmqpBaseSerializer
{
    public SystemTextJsonAmqpSerializer(ActivitySource activitySource) : base(activitySource, "SystemTextJsonAmqpSerializer") { }

    protected override T DeserializeInternal<T>(IBasicProperties basicProperties, ReadOnlyMemory<byte> body)
    {
        if (body.IsEmpty) return default!;

        string message = Encoding.UTF8.GetString(body.ToArray());

        return System.Text.Json.JsonSerializer.Deserialize<T>(message)!;
    }

    protected override byte[] SerializeInternal<T>(IBasicProperties basicProperties, T objectToSerialize) where T : default
    {
        return Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(objectToSerialize));
    }
}

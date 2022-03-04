using RabbitMQ.Client;
using System;
using System.Diagnostics;
using System.Text;

namespace AmqpAdapters.Serialization;

public class NewtonsoftAmqpSerializer : AmqpBaseSerializer
{

    public NewtonsoftAmqpSerializer(ActivitySource activitySource) : base(activitySource, "NewtonsoftAmqpSerializer") { }


    protected override TResponse DeserializeInternal<TResponse>(IBasicProperties basicProperties, ReadOnlyMemory<byte> body)
    {
        string message = Encoding.UTF8.GetString(body.ToArray());
        return Newtonsoft.Json.JsonConvert.DeserializeObject<TResponse>(message);
    }

    protected override byte[] SerializeInternal<T>(IBasicProperties basicProperties, T objectToSerialize)
    {
        return Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(objectToSerialize));
    }
}

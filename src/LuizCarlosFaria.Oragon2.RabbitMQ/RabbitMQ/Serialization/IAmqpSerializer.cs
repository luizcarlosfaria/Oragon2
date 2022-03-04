using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace LuizCarlosFaria.Oragon2.RabbitMQ.Serialization;

public interface IAmqpSerializer
{
    T Deserialize<T>(BasicDeliverEventArgs eventArgs);

    byte[] Serialize<T>(IBasicProperties basicProperties, T objectToSerialize);
}

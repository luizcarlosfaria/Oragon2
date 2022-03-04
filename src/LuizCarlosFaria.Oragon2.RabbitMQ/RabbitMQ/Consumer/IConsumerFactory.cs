using RabbitMQ.Client;

namespace LuizCarlosFaria.Oragon2.RabbitMQ.Consumer;

public interface IConsumerFactory
{
    IBasicConsumer BuildConsumer(IModel model);
}

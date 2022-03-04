using RabbitMQ.Client;

namespace AmqpAdapters.Consumer;

public interface IConsumerFactory
{
    IBasicConsumer BuildConsumer(IModel model);

}

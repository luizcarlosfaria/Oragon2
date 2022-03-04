namespace LuizCarlosFaria.Oragon2.RabbitMQ.Bus;

public interface IEventBus
{
    void PublishEvent<TEvent>(TEvent @event) where TEvent : class, IEvent;
}

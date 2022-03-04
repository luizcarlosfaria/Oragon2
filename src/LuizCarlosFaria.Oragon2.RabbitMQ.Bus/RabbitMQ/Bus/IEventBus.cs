namespace AmqpAdapters.Bus;

public interface IEventBus
{
    void PublishEvent<TEvent>(TEvent @event) where TEvent : class, IEvent;
}

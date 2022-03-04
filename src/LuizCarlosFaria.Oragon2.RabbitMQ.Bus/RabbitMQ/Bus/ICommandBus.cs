namespace AmqpAdapters.Bus;

public interface ICommandBus
{
    void SendCommand<TCommand>(TCommand command) where TCommand : class, ICommand;
}

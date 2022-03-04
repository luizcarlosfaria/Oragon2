namespace LuizCarlosFaria.Oragon2.RabbitMQ.Bus;

public interface ICommandBus
{
    void SendCommand<TCommand>(TCommand command) where TCommand : class, ICommand;
}

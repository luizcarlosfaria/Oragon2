using LuizCarlosFaria.Oragon2.RabbitMQ;

namespace LuizCarlosFaria.Oragon2.RabbitMQ.Rpc;

public class AmqpResponse<T>
{
    public AmqpRpcRemoteException? Exception { get; set; }
    public T? Result { get; set; }

    public AmqpResponse(AmqpRpcRemoteException exception) { this.Exception = exception ?? throw new ArgumentNullException(nameof(exception)); }

    public AmqpResponse(T result) { this.Result = result; }
}

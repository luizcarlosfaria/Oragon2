namespace AmqpAdapters.Rpc;


public class AmqpResponse<T>
{

    public AmqpRpcRemoteException Exception { get; set; }
    public T Result { get; set; }

    public AmqpResponse(AmqpRpcRemoteException exception) { this.Exception = exception ?? throw new System.ArgumentNullException(nameof(exception)); }

    public AmqpResponse(T result) { this.Result = result; }

}

using System;

namespace LuizCarlosFaria.Oragon2.RabbitMQ;

[Serializable]
public class AmqpRpcRemoteException : Exception
{
    private readonly string remoteStackTrace;

    public AmqpRpcRemoteException() : this(message: null, inner: null, remoteStackTrace: null) { }

    public AmqpRpcRemoteException(string? message) : this(message: message, inner: null, remoteStackTrace: null) { }

    public AmqpRpcRemoteException(string? message, Exception? innerException) : this(message: message, inner: innerException, remoteStackTrace: null) { }

    public AmqpRpcRemoteException(string? message, Exception? inner, string? remoteStackTrace) : base(message, inner)
    {
        this.remoteStackTrace = remoteStackTrace ?? throw new ArgumentNullException(nameof(remoteStackTrace));
    }

    public override string StackTrace => this.remoteStackTrace;

    protected AmqpRpcRemoteException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context)
    {
    }
}

using AmqpAdapters;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Diagnostics;

namespace LuizCarlosFaria.Oragon2.RabbitMQ.Serialization;

public abstract class AmqpBaseSerializer : IAmqpSerializer
{
    private readonly ActivitySource activitySource;
    private readonly string name;

    protected AmqpBaseSerializer(ActivitySource activitySource, string name)
    {
        this.activitySource = activitySource;
        this.name = name;
    }

    protected abstract T DeserializeInternal<T>(IBasicProperties basicProperties, ReadOnlyMemory<byte> body);

    protected abstract byte[] SerializeInternal<T>(IBasicProperties basicProperties, T objectToSerialize);

    public T Deserialize<T>(BasicDeliverEventArgs eventArgs)
    {
        if (eventArgs is null) throw new ArgumentNullException(nameof(eventArgs));
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
        if (eventArgs.BasicProperties is null) throw new ArgumentNullException(nameof(eventArgs.BasicProperties));
#pragma warning restore CA2208 // Instantiate argument exceptions correctly

        using Activity receiveActivity = this.activitySource.SafeStartActivity($"{this.name}.Deserialize", ActivityKind.Internal);
        T? returnValue = default;
        try
        {
            returnValue = this.DeserializeInternal<T>(eventArgs.BasicProperties, eventArgs.Body);
        }
        catch (Exception ex)
        {
            receiveActivity?.SetStatus(ActivityStatusCode.Error, ex.ToString());
            throw;
        }
        finally
        {
            receiveActivity?.SetEndTime(DateTime.UtcNow);
        }
        return returnValue;
    }

    public byte[] Serialize<T>(IBasicProperties basicProperties, T? objectToSerialize)
    {
        if (basicProperties is null) throw new ArgumentNullException(nameof(basicProperties));

        using Activity receiveActivity = this.activitySource.SafeStartActivity($"{this.name}.Serialize", ActivityKind.Internal);
        byte[] returnValue;
        try
        {
            returnValue = this.SerializeInternal(basicProperties, objectToSerialize);
        }
        catch (Exception ex)
        {
            receiveActivity?.SetStatus(ActivityStatusCode.Error, ex.ToString());
            throw;
        }
        finally
        {
            receiveActivity?.SetEndTime(DateTime.UtcNow);
        }
        return returnValue;
    }
}

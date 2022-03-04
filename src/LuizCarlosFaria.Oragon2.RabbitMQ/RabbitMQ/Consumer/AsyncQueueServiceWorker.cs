using AmqpAdapters;
using LuizCarlosFaria.Oragon2.RabbitMQ.Serialization;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LuizCarlosFaria.Oragon2.RabbitMQ.Consumer;


public class AsyncQueueServiceWorker<TRequest, TResponse> : QueueServiceWorkerBase
    where TResponse : Task
{

    protected readonly IAmqpSerializer serializer;
    protected readonly ActivitySource activitySource;
    protected readonly Func<TRequest?, TResponse> dispatchFunc;


    #region Constructors 


    public AsyncQueueServiceWorker(ILogger logger, IConnection connection, IAmqpSerializer serializer, ActivitySource activitySource, string queueName, ushort prefetchCount, Func<TRequest?, TResponse> dispatchFunc)
        : base(logger, connection, queueName, prefetchCount)
    {
        this.serializer = serializer;
        this.activitySource = activitySource;
        this.dispatchFunc = dispatchFunc;
    }

    #endregion


    protected override IBasicConsumer BuildConsumer()
    {
        AsyncEventingBasicConsumer consumer = new(this.Model);

        consumer.Received += this.Receive;

        return consumer;
    }

    private async Task Receive(object sender, BasicDeliverEventArgs receivedItem)
    {
        if (receivedItem == null) throw new ArgumentNullException(nameof(receivedItem));
        if (receivedItem.BasicProperties == null) throw new ArgumentNullException("receivedItem.BasicProperties");

        using Activity receiveActivity = this.activitySource.SafeStartActivity("AsyncQueueServiceWorker.Receive", ActivityKind.Server);
        receiveActivity.SetParentId(receivedItem.BasicProperties.GetTraceId(), receivedItem.BasicProperties.GetSpanId(), ActivityTraceFlags.Recorded);
        receiveActivity.AddTag("Queue", this.QueueName);
        receiveActivity.AddTag("MessageId", receivedItem.BasicProperties.MessageId);
        receiveActivity.AddTag("CorrelationId", receivedItem.BasicProperties.CorrelationId);

        PostConsumeAction postReceiveAction = this.TryDeserialize(receivedItem, out TRequest? request);

        if (postReceiveAction == PostConsumeAction.None)
        {
            try
            {
                postReceiveAction = await this.Dispatch(receivedItem, receiveActivity, request);
            }
            catch (Exception exception)
            {
                postReceiveAction = PostConsumeAction.Nack;
                this.logger.LogWarning("Exception on processing message {queueName} {exception}", this.QueueName, exception);
            }
        }

        switch (postReceiveAction)
        {
            case PostConsumeAction.None: throw new InvalidOperationException("None is unsupported");
            case PostConsumeAction.Ack: this.Model.BasicAck(receivedItem.DeliveryTag, false); break;
            case PostConsumeAction.Nack: this.Model.BasicNack(receivedItem.DeliveryTag, false, false); break;
            case PostConsumeAction.Reject: this.Model.BasicReject(receivedItem.DeliveryTag, false); break;
        }

        receiveActivity?.SetEndTime(DateTime.UtcNow);
    }

    private PostConsumeAction TryDeserialize(BasicDeliverEventArgs receivedItem, out TRequest? request)
    {
        if (receivedItem is null) throw new ArgumentNullException(nameof(receivedItem));
        PostConsumeAction postReceiveAction = PostConsumeAction.None;

        request = default;
        try
        {
            request = this.serializer.Deserialize<TRequest>(receivedItem);
        }
        catch (Exception exception)
        {
            postReceiveAction = PostConsumeAction.Reject;

            this.logger.LogWarning("Message rejected during desserialization {exception}", exception);
        }

        return postReceiveAction;
    }

    protected virtual async Task<PostConsumeAction> Dispatch(BasicDeliverEventArgs receivedItem, Activity receiveActivity, TRequest? request)
    {
        if (receivedItem is null) throw new ArgumentNullException(nameof(receivedItem));

        using Activity dispatchActivity = this.activitySource.SafeStartActivity("AsyncQueueServiceWorker.Dispatch", ActivityKind.Internal, receiveActivity.Context);

        await this.dispatchFunc(request);

        dispatchActivity?.SetEndTime(DateTime.UtcNow);

        return PostConsumeAction.Ack;
    }
}

public enum PostConsumeAction
{
    None,
    Ack,
    Nack,
    Reject
}


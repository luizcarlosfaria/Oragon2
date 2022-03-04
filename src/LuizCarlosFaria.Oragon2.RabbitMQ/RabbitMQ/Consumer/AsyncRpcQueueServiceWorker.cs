using AmqpAdapters;
using LuizCarlosFaria.Oragon2.RabbitMQ.Serialization;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LuizCarlosFaria.Oragon2.RabbitMQ.Consumer;

public class AsyncRpcQueueServiceWorker<TRequest, TResponse> : AsyncQueueServiceWorker<TRequest, Task<TResponse>>
{
    public AsyncRpcQueueServiceWorker(ILogger logger, IConnection connection, IAmqpSerializer serializer, ActivitySource activitySource, string queueName, ushort prefetchCount, Func<TRequest?, Task<TResponse>> dispatchFunc) : base(logger, connection, serializer, activitySource, queueName, prefetchCount, dispatchFunc)
    {
    }

    protected override async Task<PostConsumeAction> Dispatch(BasicDeliverEventArgs receivedItem, Activity receiveActivity, TRequest? request)
    {
        if (receivedItem is null) throw new ArgumentNullException(nameof(receivedItem));
        if (request is null) throw new ArgumentNullException(nameof(request));
        using Activity dispatchActivity = this.activitySource.SafeStartActivity("AsyncQueueServiceWorker.Dispatch", ActivityKind.Internal, receiveActivity.Context);

        if (receivedItem.BasicProperties.ReplyTo == null)
        {
            this.logger.LogWarning("Message cannot be processed in RPC Flow because original message didn't have a ReplyTo.");

            return PostConsumeAction.Reject;
        }

        TResponse? responsePayload = default;

        try
        {
            responsePayload = await this.dispatchFunc(request);
        }
        catch (Exception ex)
        {
            this.SendReply(receivedItem, receiveActivity, ex);

            return PostConsumeAction.Nack;
        }

        dispatchActivity?.SetEndTime(DateTime.UtcNow);

        this.SendReply(receivedItem, receiveActivity, responsePayload);

        return PostConsumeAction.Ack;
    }

    private void SendReply(BasicDeliverEventArgs receivedItem, Activity receiveActivity, TResponse responsePayload)
    {
        if (receivedItem is null) throw new ArgumentNullException(nameof(receivedItem));
        if (responsePayload is null) throw new ArgumentNullException(nameof(responsePayload));

        using Activity replyActivity = this.activitySource.SafeStartActivity("AsyncQueueServiceWorker.Reply", ActivityKind.Client, receiveActivity.Context);

        IBasicProperties responseProperties = this.Model!.CreateBasicProperties()
                                                        .SetMessageId()
                                                        .SetTelemetry(replyActivity)
                                                        .SetCorrelationId(receivedItem.BasicProperties);

        replyActivity?.AddTag("Queue", receivedItem.BasicProperties.ReplyTo);
        replyActivity?.AddTag("MessageId", responseProperties.MessageId);
        replyActivity?.AddTag("CorrelationId", responseProperties.CorrelationId);

        this.Model.BasicPublish(string.Empty, receivedItem.BasicProperties.ReplyTo, responseProperties, this.serializer.Serialize(responseProperties, responsePayload));

        replyActivity?.SetEndTime(DateTime.UtcNow);
    }

    private void SendReply(BasicDeliverEventArgs receivedItem, Activity receiveActivity, Exception exception)
    {
        if (receivedItem is null) throw new ArgumentNullException(nameof(receivedItem));
        if (exception is null) throw new ArgumentNullException(nameof(exception));

        using Activity replyActivity = this.activitySource.SafeStartActivity("AsyncQueueServiceWorker.Reply", ActivityKind.Client, receiveActivity.Context);

        replyActivity.AddTag("Queue", receivedItem.BasicProperties.ReplyTo);

        IBasicProperties responseProperties = this.Model!.CreateBasicProperties()
                                                        .SetMessageId()
                                                        .SetException(exception)
                                                        .SetTelemetry(replyActivity)
                                                        .SetCorrelationId(receivedItem.BasicProperties);

        replyActivity.AddTag("MessageId", responseProperties.MessageId);

        replyActivity.AddTag("CorrelationId", responseProperties.CorrelationId);

        this.Model.BasicPublish(string.Empty, receivedItem.BasicProperties.ReplyTo, responseProperties, Array.Empty<byte>());

        replyActivity.SetEndTime(DateTime.UtcNow);
    }
}

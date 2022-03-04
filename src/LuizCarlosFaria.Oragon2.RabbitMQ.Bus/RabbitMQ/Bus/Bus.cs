using AmqpAdapters;
using LuizCarlosFaria.Oragon2.RabbitMQ.Bus.Routers;
using LuizCarlosFaria.Oragon2.RabbitMQ.Serialization;
using LuizCarlosFaria.Oragon2.RingBuffer;
using RabbitMQ.Client;
using System;
using System.Diagnostics;

namespace LuizCarlosFaria.Oragon2.RabbitMQ.Bus;

public class Bus : IEventBus, ICommandBus
{
    private readonly RingBuffer<IModel> modelBuffer;
    private readonly IAmqpSerializer serializer;
    private readonly ActivitySource activitySource;
    private readonly IRouteResolver routeResolver;

    public Bus(RingBuffer<IModel> modelBuffer, IAmqpSerializer serializer, ActivitySource activitySource, IRouteResolver routeResolver)
    {
        this.modelBuffer = modelBuffer;
        this.serializer = serializer;
        this.activitySource = activitySource;
        this.routeResolver = routeResolver;
    }

    public void SendCommand<TCommand>(TCommand command) where TCommand : class, ICommand
    {
        if (command == null) throw new ArgumentNullException(nameof(command));

        this.Send(this.routeResolver.ResolveRoute(command), command);
    }

    public void PublishEvent<TEvent>(TEvent @event) where TEvent : class, IEvent
    {
        if (@event == null) throw new ArgumentNullException(nameof(@event));

        this.Send(this.routeResolver.ResolveRoute(@event), @event);
    }

    protected virtual void Send<TRequest>(Route route, TRequest requestModel)
    {
        if (route == null) throw new ArgumentNullException(nameof(route));
        if (requestModel == null) throw new ArgumentNullException(nameof(requestModel));

        using Activity currentActivity = this.activitySource.SafeStartActivity($"{nameof(Bus)}.{nameof(Send)}", ActivityKind.Client);
        currentActivity.AddTag("Exchange", route.ExchangeName);
        currentActivity.AddTag("RoutingKey", route.RoutingKey);

        using IAccquisitonController<IModel> modelBuffered = this.modelBuffer.Accquire();

        IBasicProperties requestProperties = modelBuffered.Instance.CreateBasicProperties()
                                                .SetTelemetry(currentActivity)
                                                .SetMessageId();

        currentActivity.AddTag("MessageId", requestProperties.MessageId);
        currentActivity.AddTag("CorrelationId", requestProperties.CorrelationId);

        modelBuffered.Instance.BasicPublish(
            route.ExchangeName,
            route.RoutingKey,
            requestProperties,
            this.serializer.Serialize(requestProperties, requestModel)
        );

        currentActivity.SetEndTime(DateTime.UtcNow);
    }

}

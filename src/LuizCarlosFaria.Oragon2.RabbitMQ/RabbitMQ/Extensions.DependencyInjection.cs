using LuizCarlosFaria.Oragon2.RabbitMQ.Configuration;
using LuizCarlosFaria.Oragon2.RabbitMQ.Rpc;
using LuizCarlosFaria.Oragon2.RabbitMQ.Serialization;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;
using System.Diagnostics;

namespace AmqpAdapters;

public static partial class Extensions
{
    public static IServiceCollection AddRabbitMQ(this IServiceCollection services, Action<RabbitMQConfigurationBuilder> action)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));
        if (action is null) throw new ArgumentNullException(nameof(action));
        RabbitMQConfigurationBuilder builder = new(services);

        action(builder);

        builder.Build();

        return services;
    }

    public static IServiceCollection AddAmqpRpcClient(this IServiceCollection services)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));
        services.AddTransient(sp => sp.GetRequiredService<IConnection>().CreateModel());

        services.AddScoped(sp => new SimpleAmqpRpc(
                 sp.GetRequiredService<IModel>(),
                 sp.GetRequiredService<IAmqpSerializer>(),
                 sp.GetRequiredService<ActivitySource>(),
                 TimeSpan.FromMinutes(5) //default, but can be override
             )
         );

        return services;
    }
}

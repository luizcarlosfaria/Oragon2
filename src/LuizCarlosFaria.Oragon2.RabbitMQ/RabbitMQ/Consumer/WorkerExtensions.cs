﻿using LuizCarlosFaria.Oragon2.RabbitMQ.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LuizCarlosFaria.Oragon2.RabbitMQ.Consumer;

public static class WorkerExtensions
{
    /// <summary>
    /// Create a new QueueServiceWorker to bind a queue with an function
    /// </summary>
    /// <typeparam name="TService">Service Type will be used to determine which service will be used to connect on queue</typeparam>
    /// <typeparam name="TRequest">Type of message sent by publisher to consumer. Must be exactly same Type that functionToExecute parameter requests.</typeparam>
    /// <typeparam name="TResponse">Type of returned message sent by consumer to publisher. Must be exactly same Type that functionToExecute returns.</typeparam>
    /// <param name="services">Dependency Injection Service Collection</param>
    /// <param name="queueName">Name of queue</param>
    /// <param name="functionToExecute">Function to execute when any message are consumed from queue</param>
    public static void AddAsyncRpcQueueConsumer<TService, TRequest, TResponse>(this IServiceCollection services, string queueName, ushort prefetchCount, Func<TService, TRequest?, Task<TResponse>> functionToExecute)
        where TService : notnull
    {
        if (services is null) throw new ArgumentNullException(nameof(services));
        if (string.IsNullOrEmpty(queueName)) throw new ArgumentException($"'{nameof(queueName)}' cannot be null or empty.", nameof(queueName));
        if (prefetchCount < 1) throw new ArgumentOutOfRangeException(nameof(prefetchCount));
        if (functionToExecute is null) throw new ArgumentNullException(nameof(functionToExecute));

        services.AddSingleton<IHostedService>(sp =>
                new AsyncRpcQueueServiceWorker<TRequest, TResponse>(
                    sp.GetRequiredService<ILogger<AsyncRpcQueueServiceWorker<TRequest, TResponse>>>(),
                    sp.GetRequiredService<IConnection>(),
                    sp.GetRequiredService<IAmqpSerializer>(),
                    sp.GetRequiredService<ActivitySource>(),
                    queueName,
                    prefetchCount,
                    (request) => functionToExecute(sp.GetRequiredService<TService>(), request)
                )
            );
    }

    /// <summary>
    /// Create a new QueueServiceWorker to bind a queue with an function
    /// </summary>
    /// <typeparam name="TService">Service Type will be used to determine which service will be used to connect on queue</typeparam>
    /// <typeparam name="TRequest">Type of message sent by publisher to consumer. Must be exactly same Type that functionToExecute parameter requests.</typeparam>
    /// <typeparam name="TResponse">Type of returned message sent by consumer to publisher. Must be exactly same Type that functionToExecute returns.</typeparam>
    /// <param name="services">Dependency Injection Service Collection</param>
    /// <param name="queueName">Name of queue</param>
    /// <param name="functionToExecute">Function to execute when any message are consumed from queue</param>
    public static void AddAsyncQueueConsumer<TService, TRequest>(this IServiceCollection services, string queueName, ushort prefetchCount, Func<TService, TRequest?, Task> functionToExecute)
        where TService : notnull
    {
        if (services is null) throw new ArgumentNullException(nameof(services));
        if (string.IsNullOrEmpty(queueName)) throw new ArgumentException($"'{nameof(queueName)}' cannot be null or empty.", nameof(queueName));
        if (prefetchCount < 1) throw new ArgumentOutOfRangeException(nameof(prefetchCount));
        if (functionToExecute is null) throw new ArgumentNullException(nameof(functionToExecute));

        services.AddSingleton<IHostedService>(sp =>
                new AsyncQueueServiceWorker<TRequest, Task>(
                    sp.GetRequiredService<ILogger<AsyncQueueServiceWorker<TRequest, Task>>>(),
                    sp.GetRequiredService<IConnection>(),
                    sp.GetRequiredService<IAmqpSerializer>(),
                    sp.GetRequiredService<ActivitySource>(),
                    queueName,
                    prefetchCount,
                    async (request) =>
                        {
                            using (IServiceScope serviceScope = sp.GetRequiredService<IServiceScopeFactory>().CreateScope())
                            {
                                await functionToExecute(serviceScope.ServiceProvider.GetRequiredService<TService>(), request);
                            }
                        }
                )
            );
    }
}

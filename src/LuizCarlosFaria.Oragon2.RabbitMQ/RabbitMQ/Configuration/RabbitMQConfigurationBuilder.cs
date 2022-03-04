using AmqpAdapters.Serialization;
using Humanizer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;

namespace AmqpAdapters.Configuration;

public class RabbitMQConfigurationBuilder
{
    private readonly IServiceCollection services;
    private IConfiguration configuration;
    private string configurationPrefix = "RABBITMQ";
    private int connectMaxAttempts = 8;
    private Func<int, TimeSpan> produceWaitConnectWait = (retryAttempt) => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));

    public RabbitMQConfigurationBuilder(IServiceCollection services)
    {
        this.services = services;
    }

    public RabbitMQConfigurationBuilder WithSerializer<T>()
        where T : class, IAmqpSerializer
    {
        this.services.AddSingleton<IAmqpSerializer, T>();
        return this;
    }


    public RabbitMQConfigurationBuilder WithConfiguration(IConfiguration configuration)
    {
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));

        this.configuration = configuration;
        return this;
    }

    public RabbitMQConfigurationBuilder WithConfigurationPrefix(string configurationPrefix)
    {
        if (string.IsNullOrWhiteSpace(configurationPrefix)) throw new ArgumentNullException(nameof(configurationPrefix));
        this.configurationPrefix = configurationPrefix;
        return this;
    }

    public RabbitMQConfigurationBuilder WithConnectMaxAttempts(int connectMaxAttempts, Func<int, TimeSpan> produceWaitConnectWait = null)
    {
        if (connectMaxAttempts < 0) throw new ArgumentOutOfRangeException(nameof(connectMaxAttempts), "ConnectMaxAttempts must bem greater or equal zero.");
        if (produceWaitConnectWait == null) throw new ArgumentNullException(nameof(produceWaitConnectWait));

        this.connectMaxAttempts = connectMaxAttempts;
        if (produceWaitConnectWait != null)
        {
            this.produceWaitConnectWait = produceWaitConnectWait;
        }
        return this;
    }

    public void Build()
    {
        if(this.configuration == null) throw new ArgumentNullException(nameof(this.configuration));

        this.services.AddTransient(sp => sp.GetRequiredService<IConnection>().CreateModel());

        this.services.AddSingleton(sp =>
        {
            ConnectionFactory factory = new();
            this.configuration.Bind(this.configurationPrefix, factory);
            return factory;
        });

        this.services.AddSingleton(sp => Policy
               .Handle<BrokerUnreachableException>()
               .WaitAndRetry(this.connectMaxAttempts, retryAttempt =>
               {
                   TimeSpan wait = this.produceWaitConnectWait(retryAttempt);
                   Console.WriteLine($"Can't create a connection with RabbitMQ. We wil try again in {wait.Humanize()}.");
                   return wait;
               })
               .Execute(() =>
               {
                   System.Diagnostics.Debug.WriteLine("Trying to create a connection with RabbitMQ");

                   IConnection connection = sp.GetRequiredService<ConnectionFactory>().CreateConnection();

                   Console.WriteLine(@$"Connected on RabbitMQ '{connection}' with name '{connection.ClientProvidedName}'. 
....Local Port: {connection.LocalPort}
....Remote Port: {connection.RemotePort}
....cluster_name: {connection.ServerProperties.AsString("cluster_name")}
....copyright: {connection.ServerProperties.AsString("copyright")}
....information: {connection.ServerProperties.AsString("information")}
....platform: {connection.ServerProperties.AsString("platform")}
....product: {connection.ServerProperties.AsString("product")}
....version: {connection.ServerProperties.AsString("version")}");

                   return connection;
               })
       );

    }



}
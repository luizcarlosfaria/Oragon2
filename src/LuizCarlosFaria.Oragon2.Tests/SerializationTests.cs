using LuizCarlosFaria.Oragon2.RabbitMQ.Serialization;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using Xunit;
using RabbitMQ = RabbitMQ;

namespace LuizCarlosFaria.Oragon2;
public class SerializationTests
{
    readonly ActivitySource activitySource = new("a", "b");


    [Theory]
    [InlineData(typeof(NewtonsoftAmqpSerializer))]
    [InlineData(typeof(SystemTextJsonAmqpSerializer))]
    public void SerializationAndDesserialization(Type type)
    {
        var objeto1 = new ObjectToSerialize()
        {
            Int = 1,
            String = "",
            DateTime = DateTime.Now,
            Long = 1,
            Decimal = 12,
            TimeSpan = TimeSpan.FromTicks(1213212)
        };

        IAmqpSerializer? serializer = (IAmqpSerializer)Activator.CreateInstance(type, this.activitySource)!;

        if (serializer == null) throw new InvalidOperationException("Serializer criado não pode ser nulo");

        var mock = new Mock<IBasicProperties>();

        byte[] eventoJsonBin = serializer.Serialize(mock.Object, objeto1);
        var objeto2 = serializer.Deserialize<ObjectToSerialize>(new BasicDeliverEventArgs()
        {
            Body = eventoJsonBin,
            BasicProperties = mock.Object
        });


        Assert.Equal(objeto1.Int, objeto2.Int);
        Assert.Equal(objeto1.String, objeto2.String);
        Assert.Equal(objeto1.DateTime, objeto2.DateTime);
        Assert.Equal(objeto1.Long, objeto2.Long);
        Assert.Equal(objeto1.Decimal, objeto2.Decimal);
        Assert.Equal(objeto1.TimeSpan, objeto2.TimeSpan);
    }


    [Theory]
    [InlineData(typeof(NewtonsoftAmqpSerializer))]
    [InlineData(typeof(SystemTextJsonAmqpSerializer))]
    public void ExceptionsOnSerializeTests(Type type)
    {
        IAmqpSerializer serializer = (IAmqpSerializer)Activator.CreateInstance(type, this.activitySource)!;

        var mock = new Mock<IBasicProperties>();

        Assert.ThrowsAny<Exception>(() =>
        {
            serializer.Serialize(mock.Object, new SqlConnection());
        });


    }


    [Theory]
    [InlineData(typeof(NewtonsoftAmqpSerializer))]
    [InlineData(typeof(SystemTextJsonAmqpSerializer))]
    public void ExceptionsOnDeserializeTests(Type type)
    {
        IAmqpSerializer serializer = (IAmqpSerializer)Activator.CreateInstance(type, this.activitySource)!;

        var mock = new Mock<IBasicProperties>();

        Assert.ThrowsAny<Exception>(() =>
        {
            serializer.Deserialize<SqlConnection>(new BasicDeliverEventArgs()
            {
                BasicProperties = mock.Object,
                Body = System.Text.Encoding.UTF8.GetBytes(/*lang=json,strict*/ "{ \"Id\" : 1, \"ConnectionString\" : \"teste\" }")
            });
        });
    }

}
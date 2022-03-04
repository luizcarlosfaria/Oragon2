﻿using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmqpAdapters;

public static partial class Extensions
{
    public static IBasicProperties SetMessageId(this IBasicProperties basicProperties, string messageId = null)
    {
        if (basicProperties is null) throw new ArgumentNullException(nameof(basicProperties));
        basicProperties.MessageId = messageId ?? Guid.NewGuid().ToString("D");
        return basicProperties;
    }

    public static IBasicProperties SetCorrelationId(this IBasicProperties basicProperties, IBasicProperties originalBasicProperties)
    {
        if (basicProperties is null) throw new ArgumentNullException(nameof(basicProperties));
        if (originalBasicProperties is null) throw new ArgumentNullException(nameof(originalBasicProperties));

        return basicProperties.SetCorrelationId(originalBasicProperties.MessageId);
    }

    public static IBasicProperties SetCorrelationId(this IBasicProperties basicProperties, string correlationId)
    {
        if (basicProperties is null) throw new ArgumentNullException(nameof(basicProperties));
        if (string.IsNullOrEmpty(correlationId)) throw new ArgumentException($"'{nameof(correlationId)}' cannot be null or empty.", nameof(correlationId));

        basicProperties.CorrelationId = correlationId;
        return basicProperties;
    }

    public static IBasicProperties SetDurable(this IBasicProperties basicProperties, bool durable = true)
    {
        if (basicProperties is null) throw new ArgumentNullException(nameof(basicProperties));
        basicProperties.Persistent = durable;
        return basicProperties;
    }

    public static IBasicProperties SetReplyTo(this IBasicProperties basicProperties, string replyTo = null)
    {
        if (basicProperties is null) throw new ArgumentNullException(nameof(basicProperties));
        
        if (!string.IsNullOrEmpty(replyTo))
        {
            basicProperties.ReplyTo = replyTo;
        }

        return basicProperties;
    }

    public static IBasicProperties SetAppId(this IBasicProperties basicProperties, string appId = null)
    {
        if (basicProperties is null) throw new ArgumentNullException(nameof(basicProperties));
        
        if (!string.IsNullOrEmpty(appId))
        {
            basicProperties.AppId = appId;
        }

        return basicProperties;
    }

    private static string AsString(this object objectToConvert)
    {
        return objectToConvert != null ? Encoding.UTF8.GetString((byte[])objectToConvert) : null;
    }

    public static string AsString(this IDictionary<string, object> dic, string key)
    {
        object content = dic?[key];
        return (content != null) ? Encoding.UTF8.GetString((byte[])content) : null;
    }

    public static List<string> AsStringList(this object objectToConvert)
    {
        if (objectToConvert is null) throw new ArgumentNullException(nameof(objectToConvert));
        List<object> routingKeyList = ((List<object>)objectToConvert);

        List<string> items = routingKeyList.ConvertAll(key => key.AsString());

        return items;
    }

    public static IBasicProperties SetException(this IBasicProperties basicProperties, Exception exception)
    {
        if (basicProperties is null) throw new ArgumentNullException(nameof(basicProperties));
        if (exception is null) throw new ArgumentNullException(nameof(exception));

        if (basicProperties.Headers == null) basicProperties.Headers = new Dictionary<string, object>();

        Type exceptionType = exception.GetType();

        basicProperties.Headers.Add("exception.type", $"{exceptionType.Namespace}.{exceptionType.Name}, {exceptionType.Assembly.FullName}");
        basicProperties.Headers.Add("exception.message", exception.Message);
        basicProperties.Headers.Add("exception.stacktrace", exception.StackTrace);

        return basicProperties;
    }


    public static bool TryReconstructException(this IBasicProperties basicProperties, out AmqpRpcRemoteException remoteException)
    {
        remoteException = default;
        if (basicProperties?.Headers?.ContainsKey("exception.type") ?? false)
        {
            string exceptionTypeString = basicProperties.Headers.AsString("exception.type");
            string exceptionMessage = basicProperties.Headers.AsString("exception.message");
            string exceptionStackTrace = basicProperties.Headers.AsString("exception.stacktrace");
            Exception exceptionInstance = (Exception)Activator.CreateInstance(Type.GetType(exceptionTypeString) ?? typeof(Exception), exceptionMessage);
            remoteException = new AmqpRpcRemoteException("Remote consumer report a exception during execution", exceptionStackTrace, exceptionInstance);
            return true;
        }
        return false;
    }



    public static List<object> GetDeathHeader(this IBasicProperties basicProperties)
    {
        return (List<object>)basicProperties.Headers["x-death"];
    }

    public static string GetQueueName(this Dictionary<string, object> xdeath)
    {
        return xdeath.AsString("queue");
    }

    public static string GetExchangeName(this Dictionary<string, object> xdeath)
    {
        return xdeath.AsString("exchange");
    }

    public static List<string> GetRoutingKeys(this Dictionary<string, object> xdeath)
    {
        return xdeath["routing-keys"].AsStringList();
    }

    public static long Count(this Dictionary<string, object> xdeath)
    {
        return (long)xdeath["count"];
    }
}

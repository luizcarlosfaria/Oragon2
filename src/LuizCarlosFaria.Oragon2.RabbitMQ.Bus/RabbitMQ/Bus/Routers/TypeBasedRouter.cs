using LuizCarlosFaria.Oragon2.RabbitMQ.Bus;
using System;
using System.Collections.Generic;

namespace LuizCarlosFaria.Oragon2.RabbitMQ.Bus.Routers;

public class TypeBasedRouter : IRouteResolver
{
    private Dictionary<Type, Route> Routes { get; } = new Dictionary<Type, Route>();

    public TypeBasedRouter AddRoute<T>(Route route)
    {
        if (route == null) throw new ArgumentNullException(nameof(route));
        this.Routes.Add(typeof(T), route);
        return this;
    }

    public Route ResolveRoute(IRouteable routeable)
    {
        if (routeable == null) throw new ArgumentNullException(nameof(routeable));

        Type type = routeable.GetType();

        return this.Routes.ContainsKey(type)
            ? this.Routes[type]
            : throw new InvalidOperationException($"Route not found for type {type}.");
    }
}

using System;
using System.Collections.Generic;

namespace AmqpAdapters.Bus.Routers;
public class TypeAndFunctionBasedRouter : IRouteResolver
{
    private Dictionary<Type, Func<IRouteable, Route>> Routes { get; set; } = new Dictionary<Type, Func<IRouteable, Route>>();

    public TypeAndFunctionBasedRouter AddRoute<T>(Func<IRouteable, Route> func)
    {
        if (func == null) throw new ArgumentNullException(nameof(func));
        this.Routes.Add(typeof(T), func);
        return this;
    }

    public Route ResolveRoute(IRouteable routeable)
    {
        if (routeable == null) throw new ArgumentNullException(nameof(routeable));

        Type type = routeable.GetType();
        Route route = default;
        if (this.Routes.ContainsKey(type))
        {
            route = this.Routes[type](routeable);
        }
        return route ?? throw new InvalidOperationException("Route not found");
    }
}


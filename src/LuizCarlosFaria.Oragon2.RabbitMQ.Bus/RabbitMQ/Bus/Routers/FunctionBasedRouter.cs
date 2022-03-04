﻿using LuizCarlosFaria.Oragon2.RabbitMQ.Bus;
using System;
using System.Collections.Generic;

namespace LuizCarlosFaria.Oragon2.RabbitMQ.Bus.Routers;
public class FunctionBasedRouter : IRouteResolver
{
    private List<Func<IRouteable, Route?>> Routes { get; set; } = new();


    public FunctionBasedRouter AddRoute(Func<IRouteable, Route?> func)
    {
        if (func == null) throw new ArgumentNullException(nameof(func));
        this.Routes.Add(func);
        return this;
    }

    public Route ResolveRoute(IRouteable routeable)
    {
        if (routeable == null) throw new ArgumentNullException(nameof(routeable));

        foreach (var routeFunction in this.Routes)
        {
            Route? route = routeFunction(routeable);
            if (route != null)
                return route;
        }
        throw new InvalidOperationException("Route not found");
    }
}


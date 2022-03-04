using LuizCarlosFaria.Oragon2.RabbitMQ.Bus.Routers;
using System;
using Xunit;

namespace LuizCarlosFaria.Oragon2;
public class RouterTests
{
    readonly TypeBasedRouter typeBasedRouter = new();
    readonly TypeAndFunctionBasedRouter typeAndFunctionBasedRouter = new();
    readonly FunctionBasedRouter functionBasedRouter = new();

    public RouterTests()
    {
        typeBasedRouter.AddRoute<Exemplo1Event>(new Route(exchangeName: "a", routingKey: "b"));
        _ = typeAndFunctionBasedRouter.AddRoute<Exemplo1Event>(_ => new Route(exchangeName: "a", routingKey: "b"));
        functionBasedRouter.AddRoute(routable =>
        {
            return routable is Exemplo1Event
            ? new Route(exchangeName: "a", routingKey: "b")
            : null;
        });
    }

    [Fact]
    public void TesteSuccessResolution()
    {
        var evento = new Exemplo1Event() { Id = 1 };
        Assert.Equal(1, evento.Id);

        Assert.Equal("b", typeBasedRouter.ResolveRoute(evento).RoutingKey);
        Assert.Equal("a", typeBasedRouter.ResolveRoute(evento).ExchangeName);

        Assert.Equal("b", typeAndFunctionBasedRouter.ResolveRoute(evento).RoutingKey);
        Assert.Equal("a", typeAndFunctionBasedRouter.ResolveRoute(evento).ExchangeName);

        Assert.Equal("b", functionBasedRouter.ResolveRoute(evento).RoutingKey);
        Assert.Equal("a", functionBasedRouter.ResolveRoute(evento).ExchangeName);
    }

    [Fact]
    public void TesteNotFoundResolutions()
    {
        var evento = new Exemplo2Event() { Id = 1 };
        Assert.Equal(1, evento.Id);

        Assert.Throws<InvalidOperationException>(() => this.typeBasedRouter.ResolveRoute(evento));
        Assert.Throws<InvalidOperationException>(() => this.typeAndFunctionBasedRouter.ResolveRoute(evento));
        Assert.Throws<InvalidOperationException>(() => this.functionBasedRouter.ResolveRoute(evento));
    }
}
namespace LuizCarlosFaria.Oragon2.RabbitMQ.Bus.Routers;

public interface IRouteResolver
{
    Route ResolveRoute(IRouteable routeable);
}

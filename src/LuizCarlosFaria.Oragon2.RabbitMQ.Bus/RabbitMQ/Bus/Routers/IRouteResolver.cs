namespace AmqpAdapters.Bus.Routers;

public interface IRouteResolver
{
    Route ResolveRoute(IRouteable routeable);
}

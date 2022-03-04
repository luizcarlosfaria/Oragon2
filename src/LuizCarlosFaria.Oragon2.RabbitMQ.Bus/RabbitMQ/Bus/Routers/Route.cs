namespace LuizCarlosFaria.Oragon2.RabbitMQ.Bus.Routers;
public class Route
{

    public string ExchangeName { get; set; }

    public string RoutingKey { get; set; }

    public Route() : this(exchangeName: null!, routingKey: null!)
    {
    }

    public Route(string exchangeName, string routingKey)
    {
        this.ExchangeName = exchangeName;
        this.RoutingKey = routingKey;
    }
}

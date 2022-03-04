using LuizCarlosFaria.Oragon2.RabbitMQ.Bus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuizCarlosFaria.Oragon2;


public class ObjectToSerialize
{
    public int Int { get; set; }
    public string? String { get; set; }
    public DateTime? DateTime { get; set; }
    public long? Long { get; set; }
    public decimal? Decimal { get; set; }
    public TimeSpan? TimeSpan { get; set; }
}


public class SendEmailCommand : ICommand
{
    public string? ToName { get; set; }
    public string? ToEmail { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
}


public class Exemplo1Event : IEvent
{
    public int Id { get; set; }
}

public class Exemplo2Event : IEvent
{
    public int Id { get; set; }
}
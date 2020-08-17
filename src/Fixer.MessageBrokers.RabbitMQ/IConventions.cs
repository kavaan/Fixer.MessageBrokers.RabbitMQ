using System;
using System.Collections.Generic;
using System.Text;

namespace Fixer.MessageBrokers.RabbitMQ
{
    public interface IConventions
    {
        Type Type { get; }
        string RoutingKey { get; }
        string Exchange { get; }
        string Queue { get; }
    }
}

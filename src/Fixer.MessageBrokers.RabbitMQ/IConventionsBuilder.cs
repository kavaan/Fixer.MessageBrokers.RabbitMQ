using System;
using System.Collections.Generic;
using System.Text;

namespace Fixer.MessageBrokers.RabbitMQ
{
    public interface IConventionsBuilder
    {
        string GetRoutingKey(Type type);
        string GetExchange(Type type);
        string GetQueue(Type type);
    }
}

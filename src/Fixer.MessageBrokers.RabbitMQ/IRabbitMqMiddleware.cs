using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;

namespace Fixer.MessageBrokers.RabbitMQ
{
    public interface IRabbitMqMiddleware
    {
        Task HandleAsync(Func<Task> next, object message, object correlationContext, BasicDeliverEventArgs args);
    }
}

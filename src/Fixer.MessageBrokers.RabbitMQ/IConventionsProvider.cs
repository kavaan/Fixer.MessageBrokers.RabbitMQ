using System;
using System.Collections.Generic;
using System.Text;

namespace Fixer.MessageBrokers.RabbitMQ
{
    public interface IConventionsProvider
    {
        IConventions Get<T>();
        IConventions Get(Type type);
    }
}

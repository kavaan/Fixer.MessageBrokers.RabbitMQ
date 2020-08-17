using System;
using System.Collections.Generic;
using System.Text;

namespace Fixer.MessageBrokers.RabbitMQ
{
    public interface IExceptionToMessageMapper
    {
        object Map(Exception exception, object message);
    }
}

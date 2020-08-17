﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fixer.MessageBrokers.RabbitMQ.Publishers
{
    internal sealed class RabbitMqPublisher : IBusPublisher
    {
        private readonly IRabbitMqClient _client;
        private readonly IConventionsProvider _conventionsProvider;

        public RabbitMqPublisher(IRabbitMqClient client, IConventionsProvider conventionsProvider)
        {
            _client = client;
            _conventionsProvider = conventionsProvider;
        }

        public Task PublishAsync<T>(T message, string messageId = null, string correlationId = null,
            object context = null) where T : class
        {
            _client.Send(message, _conventionsProvider.Get<T>(), messageId, correlationId, context);

            return Task.CompletedTask;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fixer.MessageBrokers.RabbitMQ.Clients;
using Fixer.MessageBrokers.RabbitMQ.Contexts;
using Fixer.MessageBrokers.RabbitMQ.Conventions;
using Fixer.MessageBrokers.RabbitMQ.Middleware;
using Fixer.MessageBrokers.RabbitMQ.Processors;
using Fixer.MessageBrokers.RabbitMQ.Publishers;
using Fixer.MessageBrokers.RabbitMQ.Serializers;
using Fixer.MessageBrokers.RabbitMQ.Subscribers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Fixer.MessageBrokers.RabbitMQ
{
    public static class Extensions
    {
        private const string SectionName = "rabbitmq";
        private const string RegistryName = "messageBrokers.rabbitmq";

        public static IFixerBuilder AddRabbitMq(this IFixerBuilder builder, string sectionName = SectionName)
        {
            var options = builder.GetOptions<RabbitMqOptions>(sectionName);
            builder.Services.AddSingleton(options);
            if (!builder.TryRegister(RegistryName))
            {
                return builder;
            }

            builder.Services.AddSingleton<IContextProvider, ContextProvider>();
            builder.Services.AddSingleton<ICorrelationContextAccessor>(new CorrelationContextAccessor());
            builder.Services.AddSingleton<IMessagePropertiesAccessor>(new MessagePropertiesAccessor());
            builder.Services.AddSingleton<IConventionsBuilder, ConventionsBuilder>();
            builder.Services.AddSingleton<IConventionsProvider, ConventionsProvider>();
            builder.Services.AddSingleton<IConventionsRegistry, ConventionsRegistry>();
            builder.Services.AddSingleton<IRabbitMqSerializer, NewtonsoftJsonRabbitMqSerializer>();
            builder.Services.AddSingleton<IRabbitMqClient, RabbitMqClient>();
            builder.Services.AddSingleton<IBusPublisher, RabbitMqPublisher>();
            builder.Services.AddSingleton<IBusSubscriber, RabbitMqSubscriber>();
            if (options.MessageProcessor?.Enabled == true)
            {
                builder.Services.AddSingleton<IRabbitMqMiddleware, UniqueMessagesMiddleware>();
                builder.Services.AddSingleton<IUniqueMessagesMiddleware, UniqueMessagesMiddleware>();
                switch (options.MessageProcessor.Type?.ToLowerInvariant())
                {
                    default:
                        builder.Services.AddMemoryCache();
                        builder.Services.AddSingleton<IMessageProcessor, InMemoryMessageProcessor>();
                        break;
                }
            }
            else
            {
                builder.Services.AddSingleton<IMessageProcessor, EmptyMessageProcessor>();
            }

            builder.Services.AddSingleton(sp =>
            {
                var connectionFactory = new ConnectionFactory
                {
                    HostName = options.HostNames?.FirstOrDefault(),
                    Port = options.Port,
                    VirtualHost = options.VirtualHost,
                    UserName = options.Username,
                    Password = options.Password,
                    RequestedConnectionTimeout = options.RequestedConnectionTimeout,
                    SocketReadTimeout = options.SocketReadTimeout,
                    SocketWriteTimeout = options.SocketWriteTimeout,
                    RequestedChannelMax = options.RequestedChannelMax,
                    RequestedFrameMax = options.RequestedFrameMax,
                    RequestedHeartbeat = options.RequestedHeartbeat,
                    UseBackgroundThreadsForIO = options.UseBackgroundThreadsForIO,
                    DispatchConsumersAsync = true,
                    Ssl = options.Ssl is null
                        ? new SslOption()
                        : new SslOption(options.Ssl.ServerName, options.Ssl.CertificatePath, options.Ssl.Enabled)
                };

                var connection = connectionFactory.CreateConnection(options.ConnectionName);
                if (options.Exchange is null || !options.Exchange.Declare)
                {
                    return connection;
                }

                using (var channel = connection.CreateModel())
                {
                    if (options.Logger?.Enabled == true)
                    {
                        var logger = sp.GetService<ILogger<IConnection>>();
                        logger.LogInformation($"Declaring an exchange: '{options.Exchange.Name}', type: '{options.Exchange.Type}'.");
                    }

                    channel.ExchangeDeclare(options.Exchange.Name, options.Exchange.Type, options.Exchange.Durable,
                        options.Exchange.AutoDelete);
                    channel.Close();
                }

                return connection;
            });

            return builder;
        }

        public static IFixerBuilder AddExceptionToMessageMapper<T>(this IFixerBuilder builder)
            where T : class, IExceptionToMessageMapper
        {
            builder.Services.AddSingleton<IExceptionToMessageMapper, T>();

            return builder;
        }

        public static IBusSubscriber UseRabbitMq(this IApplicationBuilder app) => new RabbitMqSubscriber(app);

        private class EmptyMessageProcessor : IMessageProcessor
        {
            public Task<bool> TryProcessAsync(string id) => Task.FromResult(true);

            public Task RemoveAsync(string id) => Task.CompletedTask;

        }
    }
}

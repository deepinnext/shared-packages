using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Deepin.EventBus.RabbitMQ;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventBusRabbitMQ(this IServiceCollection services, RabbitMqOptions mqConfig, Assembly assembly)
    {
        if (mqConfig is null)
        {
            throw new ArgumentNullException(nameof(mqConfig), "RabbitMQ configuration cannot be null");
        }
        services.AddMassTransit(config =>
        {
            config.AddConsumers(assembly);
            config.UsingRabbitMq((ctx, mq) =>
            {
                mq.Host(mqConfig.Host, mqConfig.Port, mqConfig.VirtualHost, h =>
                {
                    h.Username(mqConfig.Username);
                    h.Password(mqConfig.Password);

                });
                mq.ReceiveEndpoint(mqConfig.QueueName, x =>
                {
                    x.ConfigureConsumers(ctx);
                });
            });
        });
        return services;
    }
}
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Deepin.EventBus;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventBusInMemory(this IServiceCollection services, Assembly[] assemblies)
    {
        services.AddMassTransit(cfg =>
        {
            cfg.AddConsumers(assemblies);
            cfg.UsingInMemory((ctx, cfg) =>
            {
                cfg.ConfigureEndpoints(ctx);
            });
        });
        return services;
    }
}

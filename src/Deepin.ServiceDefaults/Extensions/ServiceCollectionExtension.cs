using Deepin.Domain;
using Deepin.Infrastructure.Caching;
using Deepin.ServiceDefaults.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Deepin.ServiceDefaults.Extensions;

public static class ServiceCollectionExtension
{
    public static WebApplicationBuilder AddServiceDefaults(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddDefaultControllers()
            .AddDefaultCorsPolicy()
            .AddDefaultHealthChecks()
            .AddDefaultAuthentication(builder.Configuration)
            .AddDefaultOpenApi(builder.Configuration);

        return builder;
    }
    public static IServiceCollection AddDefaultCache(this IServiceCollection services, RedisCacheOptions? options = null)
    {
        if (options is null)
        {
            services.AddMemoryCache();
            services.AddSingleton<ICacheManager>(sp => new MemoryCacheManager(sp.GetRequiredService<IMemoryCache>(), new CacheOptions()));
        }
        else
        {
            services.AddSingleton(options);
            services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(options.ConnectionString));
            services.AddSingleton<ICacheManager, RedisCacheManager>();
        }
        return services;
    }
    public static IServiceCollection AddDefaultUserContexts(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddTransient<IUserContext, HttpUserContext>();
        return services;
    }

    public static IServiceCollection AddDefaultCorsPolicy(this IServiceCollection services)
    {
        return services.AddCors(options =>
        {
            options.AddPolicy("allow_any",
                builder => builder
                .SetIsOriginAllowed((host) => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
        });
    }
}

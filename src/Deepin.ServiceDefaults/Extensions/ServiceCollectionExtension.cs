using Deepin.Domain;
using Deepin.ServiceDefaults.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

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
    public static IServiceCollection AddDefaultCache(this IServiceCollection services, string? redisConnection = null)
    {
        if(string.IsNullOrEmpty(redisConnection))
        {
            services.AddDistributedMemoryCache();
        }
        else
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
            });
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

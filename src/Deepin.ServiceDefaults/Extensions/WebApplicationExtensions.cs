using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Deepin.ServiceDefaults.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseServiceDefaults(this WebApplication app)
    {

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        var pathBase = app.Configuration["PathBase"];

        if (!string.IsNullOrEmpty(pathBase))
        {
            app.UsePathBase(pathBase);
        }
        app.UseRouting();
        app.UseDefaultCorsPolicy();
        var identitySection = app.Configuration.GetSection("Identity");

        if (identitySection.Exists())
        {
            // We have to add the auth middleware to the pipeline here
            app.UseAuthentication();
            app.UseAuthorization();
        }

        app.UseDefaultOpenApi(app.Configuration);

        app.MapDefaultHealthChecks();

        app.MapControllers();

        return app;
    }
    public static IApplicationBuilder UseDefaultCorsPolicy(this IApplicationBuilder app)
    {
        app.UseCors("allow_any");
        return app;
    }
}

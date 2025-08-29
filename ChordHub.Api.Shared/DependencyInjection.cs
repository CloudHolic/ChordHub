using ChordHub.Api.Shared.Authentication;
using ChordHub.Api.Shared.Configuration;
using ChordHub.Api.Shared.Hubs;
using ChordHub.Api.Shared.Logging;
using ChordHub.Api.Shared.Middlewares;
using ChordHub.Api.Shared.Services;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

using Serilog;

namespace ChordHub.Api.Shared;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddChordHubApiServices(this WebApplicationBuilder builder, string apiTitle, string apiVersion)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        builder.AddChordHubSerilog();

        services.AddChordHubAuthentication(configuration);
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        services.AddSignalR();

        services.AddChordHubSwagger(apiTitle, apiVersion);

        services.AddCors(options =>
        {
            options.AddPolicy("AllowChordHubWeb", policy =>
            {
                policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy());

        return builder;
    }

    public static WebApplication UseChordHubApiMiddleware(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? "unknown");
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());

                if (httpContext.User?.Identity?.IsAuthenticated == true)
                    diagnosticContext.Set("UserId", httpContext.User.FindFirst("sub")?.Value ?? "unknown");
            };
        });

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ChordHub API v1");
                c.RoutePrefix = "swagger";
            });
        }

        app.UseCors("AllowChordHubWeb");

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapHealthChecks("/health");

        app.MapHub<AnalysisProgressHub>("/hubs/analysis");

        return app;
    }
}

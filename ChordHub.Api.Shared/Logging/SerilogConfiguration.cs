using System.Reflection;
using Elastic.Channels;
using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace ChordHub.Api.Shared.Logging;

public static class SerilogConfiguration
{
    public static WebApplicationBuilder AddChordHubSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, _, loggerConfiguration) =>
            ConfigureLogger(loggerConfiguration, context.Configuration, context.HostingEnvironment));

        return builder;
    }

    private static void ConfigureLogger(LoggerConfiguration loggerConfiguration, IConfiguration configuration, IHostEnvironment environment)
    {
        var serviceName = Assembly.GetExecutingAssembly().GetName().Name ?? "ChordHub";

        loggerConfiguration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)

            .Enrich.FromLogContext()
            .Enrich.WithProperty("ServiceName", serviceName)
            .Enrich.WithProperty("Environment", environment.EnvironmentName)
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId();

        if (environment.IsDevelopment())
        {
            loggerConfiguration
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3} {SourceContext}: {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    path: "logs/ChordHub-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3} {SourceContext}: {Message:lj} {Properties:j}{NewLine}{Exception}");
        }
        else
        {
            loggerConfiguration
                .WriteTo.Console(new CompactJsonFormatter())
                .WriteTo.File(new CompactJsonFormatter(),
                    path: "/var/log/chordhub/app-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30);

            var elasticsearchUri = configuration.GetConnectionString("Elasticsearch");
            if (!string.IsNullOrEmpty(elasticsearchUri))
            {
                loggerConfiguration
                    .WriteTo.Elasticsearch([new Uri(elasticsearchUri)], opts =>
                    {
                        opts.DataStream = new DataStreamName("logs", "chordhub", "application");
                        opts.BootstrapMethod = BootstrapMethod.Failure;
                        opts.ConfigureChannel = channelOpts =>
                        {
                            channelOpts.BufferOptions = new BufferOptions
                            {
                                InboundBufferMaxSize = 100_000,
                                OutboundBufferMaxSize = 1_000
                            };
                        };
                    });
            }
        }

        loggerConfiguration.ReadFrom.Configuration(configuration);
    }
}

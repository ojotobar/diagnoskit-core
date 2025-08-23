using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;

namespace DiagnosKit.Core.Configurations
{
    public static class SerilogESSinkConfiguration
    {
        public static IHostBuilder ConfigureSerilogESSink(this IHostBuilder hostBuilder)
        {
            hostBuilder.UseSerilog((context, services, configuration) =>
            {
                var env = context.HostingEnvironment;
                var appName = env.ApplicationName; // ✅ actual app name
                var elasticConfig = context.Configuration.GetSection("ElasticSearch");
                var elasticUrl = elasticConfig.GetValue<string>("Url") ?? throw new ArgumentNullException("ElasticSearch:Url");
                var indexFormat = elasticConfig.GetValue<string>("IndexFormat") ??
                    $"{appName?.ToLower().Replace(".", "-")}-{env.EnvironmentName.ToLower()}-{DateTime.UtcNow:yyyy-MM}";

                configuration
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.WithEnvironmentName()
                    .Enrich.WithThreadId()
                    .Enrich.WithProcessId()
                    .Enrich.WithCorrelationId()
                    .Enrich.WithExceptionDetails()
                    .Enrich.WithProperty("Environment", env.EnvironmentName)
                    .Enrich.WithProperty("Service", appName)
                    .WriteTo.Debug()
                    .WriteTo.Console()
                    .ReadFrom.Configuration(context.Configuration)
                    .WriteTo.Elasticsearch(ConfigureElasticSink(elasticUrl, indexFormat));
            });

            return hostBuilder;
        }

        public static IHostApplicationBuilder ConfigureSerilogESSink(this IHostApplicationBuilder builder)
        {
            var env = builder.Environment.EnvironmentName ?? string.Empty;

            var elasticConfig = builder.Configuration.GetSection("ElasticSearch");
            var elasticUrl = elasticConfig.GetValue<string>("Url")
                ?? throw new ArgumentNullException("ElasticSearch:Url");

            var indexFormat = elasticConfig.GetValue<string>("IndexFormat") ??
                $"{builder.Environment.ApplicationName?.ToLower().Replace(".", "-")}-{env.ToLower()}-{DateTime.UtcNow:yyyy-MM}";

            builder.Services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddSerilog(new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.WithEnvironmentName()
                    .Enrich.WithThreadId()
                    .Enrich.WithProcessId()
                    .Enrich.WithCorrelationId()
                    .Enrich.WithExceptionDetails()
                    .WriteTo.Debug()
                    .WriteTo.Console()
                    .Enrich.WithProperty("Environment", env)
                    .Enrich.WithProperty("Service", builder.Environment.ApplicationName)
                    .ReadFrom.Configuration(builder.Configuration)
                    .WriteTo.Elasticsearch(ConfigureElasticSink(elasticUrl, indexFormat))
                    .CreateLogger(), dispose: true);
            });

            return builder;
        }

        private static ElasticsearchSinkOptions ConfigureElasticSink(string url, string indexFormat)
        {
            return new ElasticsearchSinkOptions(new Uri(url))
            {
                AutoRegisterTemplate = true,
                IndexFormat = indexFormat,
                NumberOfReplicas = 1,
                NumberOfShards = 2,
            };
        }
    }
}

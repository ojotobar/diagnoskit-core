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
                var appName = env.ApplicationName;
                var elasticConfig = context.Configuration.GetSection("ElasticSearch");
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
                    .WriteTo.Elasticsearch(ConfigureElasticSink(elasticConfig, indexFormat));
            });

            return hostBuilder;
        }

        public static IHostApplicationBuilder ConfigureSerilogESSink(this IHostApplicationBuilder builder)
        {
            var env = builder.Environment.EnvironmentName ?? string.Empty;
            var elasticConfig = builder.Configuration.GetSection("ElasticSearch");
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
                    .WriteTo.Elasticsearch(ConfigureElasticSink(elasticConfig, indexFormat))
                    .CreateLogger(), dispose: true);
            });

            return builder;
        }

        private static ElasticsearchSinkOptions ConfigureElasticSink(IConfigurationSection section, string indexFormat)
        {
            var url = section.GetValue<string>("Url")
                ?? throw new ArgumentNullException("ElasticSearch:Url");

            var username = section.GetValue<string>("Username");
            var password = section.GetValue<string>("Password");
            var indexPrefix = section.GetValue<string>("IndexPrefix");
            if (!string.IsNullOrWhiteSpace(indexPrefix))
            {
                indexFormat = $"{indexPrefix.Replace(".", "")}-{indexFormat}";
            }

            return new ElasticsearchSinkOptions(new Uri(url))
            {
                AutoRegisterTemplate = false,
                IndexFormat = indexFormat,
                NumberOfReplicas = 1,
                NumberOfShards = 2,
                TypeName = null,
                ModifyConnectionSettings = x =>
                {
                    if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                        x.BasicAuthentication(username, password);

                    x.ServerCertificateValidationCallback((sender, cert, chain, errors) => true);

                    x.DisablePing();

                    x.ThrowExceptions();

                    return x;
                }
            };
        }
    }
}

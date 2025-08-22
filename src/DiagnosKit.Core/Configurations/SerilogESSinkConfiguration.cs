using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;
using System.Reflection;

namespace DiagnosKit.Core.Configurations
{
    public static class SerilogESSinkConfiguration
    {
        public static IHostBuilder ConfigureSerilogESSink(this IHostBuilder builder)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? string.Empty;
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env}.json", optional: true)
                .Build();

            Log.Logger = new LoggerConfiguration()
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
                .ReadFrom.Configuration(config)
                .Enrich.WithProperty("Service", Assembly.GetExecutingAssembly().GetName().Name)
                .WriteTo.Elasticsearch(ConfigureElasticSink(config, env))
                .CreateLogger();

            builder.UseSerilog();
            return builder;
        }

        public static IHostApplicationBuilder ConfigureSerilogESSink(this IHostApplicationBuilder builder)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? string.Empty;

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env}.json", optional: true)
                .Build();

            Log.Logger = new LoggerConfiguration()
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
                .ReadFrom.Configuration(config)
                .Enrich.WithProperty("Service", Assembly.GetExecutingAssembly().GetName().Name)
                .WriteTo.Elasticsearch(ConfigureElasticSink(config, env))
                .CreateLogger();

            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog();
            return builder;
        }

        private static ElasticsearchSinkOptions ConfigureElasticSink(IConfigurationRoot config, string environment)
        {
            return new ElasticsearchSinkOptions(new Uri(config["ElasticSearch:Url"] ?? string.Empty))
            {
                AutoRegisterTemplate = true,
                IndexFormat = $"{Assembly.GetExecutingAssembly().GetName()?.Name?.ToLower().Replace(".", "-")}-{environment.ToLower()}-{DateTime.UtcNow:yyyy-MM}",
                NumberOfReplicas = 1,
                NumberOfShards = 2,
            };
        }
    }
}

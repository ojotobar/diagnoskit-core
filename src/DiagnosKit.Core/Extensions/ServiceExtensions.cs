using DiagnosKit.Core.Logging.Contracts;
using DiagnosKit.Core.Logging.Implementations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;

namespace DiagnosKit.Core.Extensions
{
    public static class ServiceExtensions
    {
        /// <summary>
        /// Registers the <see cref="LoggerManager"/> implementation of <see cref="ILoggerManager"/> 
        /// in the application's dependency injection container.
        /// </summary>
        /// <param name="services">The service collection to add the logger manager to.</param>
        /// <remarks>
        /// This provides a single point of registration for DiagnosKit's logging abstraction, 
        /// ensuring that all consuming components receive a consistent, centralized 
        /// <see cref="ILoggerManager"/> instance for structured and enriched logging.
        /// </remarks>
        public static void AddLoggerManager(this IServiceCollection services)
        {
            services.AddSingleton<ILoggerManager, LoggerManager>();
        }

        /// <summary>
        /// Configures DiagnosKit observability by registering OpenTelemetry tracing and metrics
        /// for the application.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="serviceName">
        /// The logical name of the service. This name is used as the primary identifier for traces
        /// and metrics in distributed observability systems.
        /// </param>
        /// <param name="serviceVersion">
        /// An optional version for the service. Defaults to <c>"1.0.0"</c> if not provided.
        /// </param>
        /// <param name="traceConfig">
        /// An optional callback to further configure the <see cref="TracerProviderBuilder"/>.
        /// Use this to register custom activity sources or exporters.
        /// </param>
        /// <param name="metricsConfig">
        /// An optional callback to further configure the <see cref="MeterProviderBuilder"/>.
        /// Use this to register custom meters or exporters.
        /// </param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        /// <remarks>
        /// This extension method sets up a standard DiagnosKit observability pipeline:
        /// <list type="bullet">
        ///   <item>
        ///     <description>Configures OpenTelemetry resource attributes with <c>service.name</c>,
        ///     <c>service.version</c>, and <c>deployment.environment</c>.</description>
        ///   </item>
        ///   <item>
        ///     <description>Enables tracing for ASP.NET Core, HttpClient, and SQL client calls, 
        ///     with optional additional configuration via <paramref name="traceConfig"/>.</description>
        ///   </item>
        ///   <item>
        ///     <description>Enables metrics for ASP.NET Core, runtime statistics, and HttpClient, 
        ///     with a Prometheus exporter, and allows further customization via 
        ///     <paramref name="metricsConfig"/>.</description>
        ///   </item>
        /// </list>
        /// Call this once during application startup to enable consistent distributed tracing 
        /// and metrics across the service.
        /// </remarks>
        public static IServiceCollection AddDiagnosKitObservability(this IServiceCollection services,
                                                                   string serviceName,
                                                                   string? serviceVersion = null,
                                                                   Action<TracerProviderBuilder>? traceConfig = null,
                                                                   Action<MeterProviderBuilder>? metricsConfig = null)
        {
            services.AddOpenTelemetry()
                .ConfigureResource(r => r
                    .AddService(serviceName, serviceVersion ?? "1.0.0")
                    .AddAttributes(new Dictionary<string, object>
                    {
                        ["deployment.environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
                    }))
                .WithTracing(tracing =>
                {
                    tracing
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddSource(serviceName);

                    traceConfig?.Invoke(tracing);
                })
                .WithMetrics(metrics =>
                {
                    metrics
                        .AddAspNetCoreInstrumentation()
                        .AddRuntimeInstrumentation()
                        .AddHttpClientInstrumentation();
                    metricsConfig?.Invoke(metrics);
                });

            return services;
        }

        /// <summary>
        /// Enables Prometheus metrics collection for the application by:
        /// - Collecting default HTTP request metrics via middleware
        /// - Mapping the /metrics endpoint for scraping
        /// </summary>
        /// <param name="app">The WebApplication instance.</param>
        /// <returns>The updated WebApplication.</returns>
        public static IApplicationBuilder UseDiagnosKitPrometheus(this WebApplication app)
        {
            // Collects default HTTP request metrics (duration, count, etc.)
            app.UseHttpMetrics();

            // Exposes /metrics endpoint for Prometheus to scrape
            app.MapMetrics();

            return app;
        }

        /// <summary>
        /// Adds DiagnosKit logging integration using OpenTelemetry.
        /// </summary>
        /// <param name="logging">The logging builder to configure.</param>
        /// <returns>The updated <see cref="ILoggingBuilder"/>.</returns>
        /// <remarks>
        /// This extension method enables OpenTelemetry logging with DiagnosKit defaults:
        /// <list type="bullet">
        ///   <item>
        ///     <description><c>IncludeFormattedMessage</c> is enabled so that the fully formatted 
        ///     log message is included in the exported log record.</description>
        ///   </item>
        ///   <item>
        ///     <description><c>IncludeScopes</c> is enabled to capture ambient logging scopes 
        ///     (e.g., correlation IDs, user IDs).</description>
        ///   </item>
        ///   <item>
        ///     <description><c>ParseStateValues</c> is enabled so structured state values are 
        ///     serialized as key/value pairs in log records.</description>
        ///   </item>
        /// </list>
        /// Use this during host configuration to ensure that logs are exported consistently with 
        /// OpenTelemetry traces and metrics.
        /// </remarks>
        public static ILoggingBuilder AddDiagnosKitOpenTelemetryLogging(this ILoggingBuilder logging)
        {
            logging.AddOpenTelemetry(options =>
            {
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;
                options.ParseStateValues = true;
            });

            return logging;
        }
    }
}
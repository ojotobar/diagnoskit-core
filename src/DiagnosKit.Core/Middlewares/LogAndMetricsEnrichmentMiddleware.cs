using DiagnosKit.Core.Logging.Contracts;
using DiagnosKit.Core.Meters;
using Microsoft.AspNetCore.Http;
using Serilog.Context;
using System.Diagnostics.Metrics;
using System.Security.Claims;

namespace DiagnosKit.Core.Middlewares
{
    public class LogAndMetricsEnrichmentMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILoggerManager _logger;

        private static readonly Meter Meter = DiagnosKitMetrics.GetMeter("DiagnosKit.Requests");
        private static readonly Counter<int> RequestsByEndpoint =
            Meter.CreateCounter<int>("diagnoskit_http_requests_by_endpoint_total", "count",
                "Number of requests grouped by endpoint");

        public LogAndMetricsEnrichmentMiddleware(RequestDelegate next, ILoggerManager logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // --- CorrelationId ---
            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
                context.Response.Headers["X-Correlation-ID"] = correlationId; // echo it back
            }

            // --- UserId ---
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";

            // --- Endpoint ---
            var endpoint = context.GetEndpoint();
            var endpointName = endpoint?.DisplayName ?? "unknown";

            using (LogContext.PushProperty("CorrelationId", correlationId))
            using (LogContext.PushProperty("UserId", userId))
            using (LogContext.PushProperty("Endpoint", endpointName))
            {
                try
                {
                   
                    var routePattern = endpoint?.Metadata?
                        .GetMetadata<Microsoft.AspNetCore.Routing.RouteNameMetadata>()?.RouteName
                        ?? context.Request.Path.Value
                        ?? "unknown";

                    // Add to global requests counter
                    DiagnosKitMetrics.HttpRequests.Add(1);

                    // Increment per-endpoint counter with tags
                    RequestsByEndpoint.Add(1,
                        new KeyValuePair<string, object?>("endpoint", routePattern),
                        new KeyValuePair<string, object?>("status_code", context.Response.StatusCode));

                    await _next(context);

                    if (context.Response.StatusCode >= 400)
                        DiagnosKitMetrics.HttpFailures.Add(1);
                }
                catch
                {
                    DiagnosKitMetrics.HttpFailures.Add(1);
                    RequestsByEndpoint.Add(1,
                        new KeyValuePair<string, object?>("endpoint", "exception"),
                        new KeyValuePair<string, object?>("status_code", 500));
                }
                finally
                {
                    // Always capture the status code after execution
                    using (LogContext.PushProperty("StatusCode", context.Response.StatusCode))
                    {
                        _logger.LogInfo("Request completed");
                    }
                }
            }
        }
    }
}

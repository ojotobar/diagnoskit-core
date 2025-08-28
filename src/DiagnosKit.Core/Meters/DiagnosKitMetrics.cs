using System.Diagnostics.Metrics;

namespace DiagnosKit.Core.Meters
{
    public static class DiagnosKitMetrics
    {
        private static readonly Meter Meter = new("DiagnosKit.Core", "1.0.0");

        // Common counters (used across services)
        public static readonly Counter<int> HttpRequests =
            Meter.CreateCounter<int>("diagnoskit_http_requests_total", "count", "Total HTTP requests handled");

        public static readonly Counter<int> HttpFailures =
            Meter.CreateCounter<int>("diagnoskit_http_failures_total", "count", "Failed HTTP requests");

        // Allow services to register their own meters
        public static Meter GetMeter(string serviceName) =>
            new(serviceName, "1.0.0");
    }
}
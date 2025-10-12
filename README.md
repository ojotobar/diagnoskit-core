# DiagnosKit.Core

[![NuGet](https://img.shields.io/nuget/v/DiagnosKit.Core.svg)](https://www.nuget.org/packages/DiagnosKit.Core)  
[![NuGet Downloads](https://img.shields.io/nuget/dt/DiagnosKit.Core.svg)](https://www.nuget.org/packages/DiagnosKit.Core)

---
A lightweight diagnostics toolkit for .NET 8+ that provides:
- Structured logging with Serilog
- Elasticsearch sink support
- Easy integration for both **Web APIs** and **Worker Services**
- Unified global exception handling (middleware)
- OpenTelemetry tracing, metrics & logging integration
- Automatic request meters (requests per endpoint, duration, failures)
- Log enrichment with CorrelationId and UserId

---

## 📦 Installation

Install from NuGet:

```powershell
dotnet add package DiagnosKit.Core
```

---

## 🚀 Usage

### 1. Web API (`Program.cs`)

```csharp
using DiagnosKit.Core;

// For pre-bootstrap logging
SerilogBootstrapper.UseBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog + Elasticsearch
builder.Host.ConfigureSerilogESSink();

// Add DiagnosKit logger abstraction
builder.Services.AddLoggerManager();

// Add OpenTelemetry (traces + metrics + logging)
builder.Services.AddDiagnosKitObservability(
    serviceName: "MyWebApi",
    serviceVersion: "1.0.0"
);

// Optional: OTel logging
builder.Logging.AddDiagnosKitOpenTelemetryLogging();

builder.Services.AddControllers();

var app = builder.Build();

// Global exception handler
var logger = app.Services.GetRequiredServices<ILoggerManager>();
app.UseDiagnosKitExceptionHandler(logger);

app.MapControllers();

// Expose /metrics for Prometheus
app.MapPrometheusScrapingEndpoint();
// Enables Prometheus metrics collection for the application
app.UseDiagnosKitPrometheus();

app.Run();
```

---

### 2. Worker Service (`Program.cs`)

```csharp
using DiagnosKit.Core;

// Pre-bootstrap logging
SerilogBootstrapper.UseBootstrapLogger();

var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog + Elasticsearch
builder.ConfigureSerilogESSink();

// Add DiagnosKit logger abstraction
builder.Services.AddLoggerManager();

// Add OpenTelemetry (traces + metrics)
builder.Services.AddDiagnosKitObservability(
    serviceName: "MyWorker",
    serviceVersion: "1.0.0"
);

// Optional: OTel logging
builder.Logging.AddDiagnosKitOpenTelemetryLogging();

// Register worker
builder.Services.AddHostedService<MyWorker>();

var host = builder.Build();
host.Run();
```

Example Worker:

```csharp
public class MyWorker : BackgroundService
{
    private readonly ILoggerManager _logger;

    public MyWorker(ILoggerManager logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInfo("Worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogDebug("Heartbeat at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
    }
}
```

---

## ⚙️ Configuration

#### IndexFormat will default to this format: application-name-{0:yyyy.MM}, if no format is specified
In your `appsettings.json`: 

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.AspNetCore": "Information",
        "System": "Warning"
      }
    }
  },
  "ElasticSearch": {
    "Url": "http://localhost:9200",
    "IndexFormat": "myapp-{0:yyyy.MM}"
  },
}
```

---

## 📝 Features

✅ Web API support (app.UseDiagnosKitErrorHandler())
✅ Worker Service support with ILoggerManager abstraction
✅ Serilog + Elasticsearch integration
✅ OpenTelemetry-ready (Tracing, Metrics, Logging)
✅ Request meters (per endpoint: total requests, failures, duration)
✅ Automatic log enrichment with 
- CorrelationId
- UserId (when available)
- Environment
- Service

---

## 📊 Kibana Filtering

Because logs are enriched with structured properties (like `Environment`, `Service`, `CorrelationId`, and `Exception`),  
you can filter/search logs in **Kibana** by:

- `Environment: "Production"`  
- `Service: "UserService"`  
- `Exception exists`  
- `UserId: 12345`

---

## 📊 Kibana + Grafana

Kibana → filter structured logs by:
- Environment: "Production"
- Service: "UserService"
- CorrelationId: "abc123"
- UserId: 12345
- Exception exists

Grafana → visualize Prometheus metrics:
- http_requests_total{endpoint="/api/users"}
- http_requests_failed_total
- http_request_duration_seconds

## Prometheus Setup

By default, app.MapPrometheusScrapingEndpoint() in your ASP.NET Core app exposes metrics at /metrics.
In your Prometheus config (prometheus.yml), the metrics_path tells Prometheus where to scrape from. 
If you don’t set it, Prometheus automatically defaults to /metrics.

✅ Example (explicit):
```yaml
scrape_configs:
  - job_name: 'diagnoskit-api'
    scrape_interval: 5s
    metrics_path: /metrics
    static_configs:
      - targets: ['host.docker.internal:5000']
```

✅ Example (simpler, same result):
```json
scrape_configs:
  - job_name: 'diagnoskit-api'
    scrape_interval: 5s
    static_configs:
      - targets: ['host.docker.internal:5000']
```

Run Prometheus with this config, and you’ll see metrics like:
- http_server_requests_duration_seconds (per endpoint latency histograms)
- http_server_requests_count (per endpoint request count)
- .NET runtime metrics (GC, memory, threads, exceptions)

---

## Grafana Dashboard Example
Here’s a minimal Grafana dashboard JSON you can import to visualize key metrics:

```json
{
  "id": null,
  "title": "DiagnosKit API Dashboard",
  "timezone": "browser",
  "panels": [
    {
      "type": "graph",
      "title": "Request Count per Endpoint",
      "targets": [
        {
          "expr": "sum by (method, route) (http_server_requests_count)",
          "legendFormat": "{{method}} {{route}}"
        }
      ]
    },
    {
      "type": "graph",
      "title": "Request Duration (p95)",
      "targets": [
        {
          "expr": "histogram_quantile(0.95, sum(rate(http_server_requests_duration_seconds_bucket[5m])) by (le, route))",
          "legendFormat": "{{route}}"
        }
      ]
    },
    {
      "type": "graph",
      "title": ".NET GC Collections",
      "targets": [
        {
          "expr": "dotnet_gc_collections_count_total",
          "legendFormat": "Gen {{generation}}"
        }
      ]
    },
    {
      "type": "graph",
      "title": "Memory Usage",
      "targets": [
        {
          "expr": "process_private_memory_bytes",
          "legendFormat": "Private Memory"
        }
      ]
    }
  ],
  "schemaVersion": 30,
  "version": 1
}
```

### 👉 In Grafana:
- Go to Dashboards > Import
- Paste the JSON above
- Select your Prometheus datasource

---

## ⚖ License

This project is licensed under the MIT License - see the LICENSE file for details.
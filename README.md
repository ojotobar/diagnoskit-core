# DiagnosKit.Core

[![NuGet](https://img.shields.io/nuget/v/DiagnosKit.Core.svg)](https://www.nuget.org/packages/DiagnosKit.Core)  
[![NuGet Downloads](https://img.shields.io/nuget/dt/DiagnosKit.Core.svg)](https://www.nuget.org/packages/DiagnosKit.Core)

---
A lightweight diagnostics toolkit for .NET 8+ that provides:
- Unified global exception handling (middleware)
- Structured logging with Serilog
- Elasticsearch sink support
- Easy integration for both **Web APIs** and **Worker Services**

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

// Add DiagnosKit logging abstraction
builder.Services.AddLoggerManager();
builder.Services.AddControllers();

var app = builder.Build();

// Use global exception handler middleware
app.UseUnifiedErrorHandler();

app.MapControllers();

app.Run();
```

---

### 2. Worker Service (`Program.cs`)

```csharp
using DiagnosKit.Core;

// For pre-bootstrap logging
SerilogBootstrapper.UseBootstrapLogger();

var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog + Elasticsearch
builder.ConfigureSerilogESSink();

// Add DiagnosKit logging abstraction
builder.Services.AddLoggerManager();

// Add your worker
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

- **Web API support** via middleware (`app.UseUnifiedErrorHandler()`).
- **Worker Service support** with logger abstraction (`ILoggerManager`).
- **Serilog + Elasticsearch integration** out of the box.
- **Environment-aware index naming** (per app + environment).

---

## 📊 Kibana Filtering

Because logs are enriched with structured properties (like `Environment`, `Service`, `CorrelationId`, and `Exception`),  
you can filter/search logs in **Kibana** by:

- `Environment: "Production"`  
- `Service: "UserService"`  
- `Exception exists`  
- `UserId: 12345`

---

## ⚖ License

This project is licensed under the MIT License - see the LICENSE file for details.
# DiagnosKit.Core

[![NuGet](https://img.shields.io/nuget/v/DiagnosKit.Core.svg)](https://www.nuget.org/packages/DiagnosKit.Core)  
[![NuGet Downloads](https://img.shields.io/nuget/dt/DiagnosKit.Core.svg)](https://www.nuget.org/packages/DiagnosKit.Core)

---

## 📖 Overview

**DiagnosKit.Core** is a reusable .NET toolkit providing cross-cutting features for your services, including:

- **Global error handling middleware** to capture and format unhandled exceptions.  
- **Centralized logger manager** powered by **Serilog** with support for **Elasticsearch** and **Kibana**.  
- **Standardized error response models** for consistent API output.  

Designed for microservices, distributed systems, or any project that needs consistent **logging, error handling, and observability**.

---

## 📦 Installation

Install from NuGet:

```powershell
dotnet add package DiagnosKit.Core
```

---

## 🚀 Quick Start

### 1. Register the Logger Manager

In `Program.cs` or `Startup.ConfigureServices`:

```csharp
builder.Services.AddLoggerManager();
```

### 2. Configure Serilog with Elasticsearch Sink

In `Program.cs` (before `builder.Build()`):

```csharp
builder.Host.ConfigureSerilogESSink();
```

This will:

- Read settings from `appsettings.json` and environment-specific config.  
- Enrich logs with machine name, environment, correlation id, process id, and exception details.  
- Write logs to Console, Debug, and Elasticsearch.  
- Automatically index logs into Elasticsearch with an index format:

```
{service-name}-{environment}-{yyyy-MM}
```

### 3. Use the Global Error Handler

In `Program.cs` or `Startup.Configure`:

```csharp
app.UseUnifiedErrorHandler();
```

---

## 🛠 Logging Examples

Once configured, inject the `ILogger<LoggerManager>` into your service or use the `LoggerManager` wrapper methods:

```csharp
// Info
_logger.LogInfo("Application started");

// Debug
_logger.LogDebug("Processing request {RequestId}", requestId);

// Warning
_logger.LogWarn("User {UserId} attempted invalid action", userId);

// Error
_logger.LogError("Something went wrong");
_logger.LogError(exception, "Exception occurred while processing request");
```

All logs will be shipped to **Console**, **Elasticsearch**, and be queryable in **Kibana**.

---

## Sample appsettings.json for Serilog and Elasticsearch for work as expected

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
  }
}
```

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
# DiagnosKit.Core

[![NuGet](https://img.shields.io/nuget/v/DiagnosKit.Core.svg)](https://www.nuget.org/packages/DiagnosKit.Core)
[![NuGet Downloads](https://img.shields.io/nuget/dt/DiagnosKit.Core.svg)](https://www.nuget.org/packages/DiagnosKit.Core)

# DiagnosKit.Core

**DiagnosKit.Core** is a reusable .NET toolkit providing cross-cutting features for your services, including:

- **Global error handling middleware** to capture and format unhandled exceptions.
- **Centralized logger manager** for consistent logging across multiple services.
- **Standardized error response models** for consistent API output.

This library is designed for microservices, distributed systems, or any project that needs consistent error handling and logging.

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

### 2. Use the Global Error Handler
In `Program.cs` or `Startup.Configure`:

```csharp
app.UseUnifiedErrorHandler();
```

---

## 🛠 Example

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add Logger Manager
builder.Services.AddLoggerManager();

var app = builder.Build();

// Use Global Error Handler Middleware
app.UseUnifiedErrorHandler();

app.MapGet("/", () =>
{
    throw new Exception("Test unhandled exception");
});

app.Run();
```

```csharp
public class DiagnosKit
{
    private readonly ILoggerManager _logger;

    public DiagnosKit(ILoggerManager logger)
    {
        _logger = logger;
    }

    public void LogTest()
    {
        _logger.LogDebug("This is a debug");
        _logger.LogInfo("This is an info log");
        _logger.LogWarn("This is a warning log");
        _logger.LogError("This is an error log");
        _logger.LogCritical("This is a critical log");
    }
}
```

If an unhandled exception occurs, the middleware logs the error and returns a standardized JSON response like:

```json
{
  "statusCode": 500,
  "message": "Internal Server Error"
}
```

---

## 📂 Folder Structure

```
Middleware/
  UnifiedErrorHandlerMiddleware.cs
  MiddlewareExtensions.cs

Logging/
  LoggerManager.cs
  LoggerExtensions.cs
```
---
## ⚖ License

This project is licensed under the MIT License - see the LICENSE file for details.
using API.Common.Response.Model.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using DiagnosKit.Core.Logging.Contracts;
using System.Net;
using System.Text.Json;
using Serilog.Context;

namespace DiagnosKit.Core.Middlewares
{
    public class UnifiedErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILoggerManager _logger;

        public UnifiedErrorHandlerMiddleware(RequestDelegate next, ILoggerManager logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleAsync(context, ex);
            }
        }

        private async Task HandleAsync(HttpContext context, Exception exception)
        {
            var correlationId = context.Response.Headers["X-Correlation-ID"].FirstOrDefault()
                                ?? context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                                ?? Guid.NewGuid().ToString();

            context.Response.Headers["X-Correlation-ID"] = correlationId;

            using (LogContext.PushProperty("CorrelationId", correlationId))
            using (LogContext.PushProperty("UserId", context.User?.Identity?.Name ?? "anonymous"))
            using (LogContext.PushProperty("Endpoint", context.GetEndpoint()?.DisplayName ?? "unknown"))
            {
                _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";
                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (contextFeature != null)
                {
                    context.Response.StatusCode = contextFeature.Error switch
                    {
                        NotFoundException => StatusCodes.Status404NotFound,
                        UnauthorizedException => StatusCodes.Status401Unauthorized,
                        ForbiddenException => StatusCodes.Status403Forbidden,
                        BadRequestException => StatusCodes.Status400BadRequest,
                        _ => StatusCodes.Status500InternalServerError
                    };

                    await context.Response.WriteAsync(JsonSerializer.Serialize(new
                    {
                        context.Response.StatusCode,
                        CorrelationId = correlationId,
                        Message = "An unexpected error occurred. Please use the CorrelationId when contacting support."
                    }));
                }
            }
        }

    }
}
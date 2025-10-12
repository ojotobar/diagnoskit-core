using API.Common.Response.Model.Exceptions;
using DiagnosKit.Core.Logging.Contracts;
using KwikNesta.Contracts.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog.Context;
using System.Net;
using System.Text.Json;

namespace DiagnosKit.Core.Middlewares
{
    public static class Extensions
    {
        public static void UseDiagnosKitExceptionHandler(this WebApplication app, 
                                                         ILoggerManager logger)
        {
            app.UseExceptionHandler(builder =>
            {
                builder.Run(async context =>
                {
                    var correlationId = context.Response.Headers["X-Correlation-ID"].FirstOrDefault()
                                ?? context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                                ?? Guid.NewGuid().ToString();

                    context.Response.Headers["X-Correlation-ID"] = correlationId;

                    using (LogContext.PushProperty("CorrelationId", correlationId))
                    using (LogContext.PushProperty("UserId", context.User?.Identity?.Name ?? "anonymous"))
                    using (LogContext.PushProperty("Endpoint", context.GetEndpoint()?.DisplayName ?? "unknown"))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        context.Response.ContentType = "application/json";
                        var contextFeature = context.Features.Get<IExceptionHandlerFeature>();

                        if (contextFeature != null)
                        {
                            logger.LogError(contextFeature.Error, "An error occurred");
                            var message = contextFeature.Error.Message;
                            switch (contextFeature.Error)
                            {
                                case BadRequestException:
                                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                                    message = contextFeature.Error.Message;
                                    break;
                                case NotFoundException:
                                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                                    message = contextFeature.Error.Message;
                                    break;
                                case ForbiddenException:
                                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                                    message = contextFeature.Error.Message;
                                    break;
                                default:
                                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                    break;
                            }

                            await context.Response.WriteAsync(JsonSerializer.Serialize(new ApiResult<string>(message, context.Response.StatusCode)));
                        }
                    }
                });
            });
        }
    }
}

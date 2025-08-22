using DiagnosKit.Core.Logging.Contracts;
using DiagnosKit.Core.Logging.Implementations;
using DiagnosKit.Core.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DiagnosKit.Core.Extensions
{
    public static class ServiceExtensions
    {
        public static void UseUnifiedErrorHandler(this IApplicationBuilder app)
        {
            app.UseMiddleware<UnifiedErrorHandlerMiddleware>();
        }

        public static void AddLoggerManager(this IServiceCollection services)
        {
            services.AddSingleton<ILoggerManager, LoggerManager>();
        }
    }
}
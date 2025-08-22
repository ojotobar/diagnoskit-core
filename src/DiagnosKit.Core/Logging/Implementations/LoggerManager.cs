using DiagnosKit.Core.Logging.Contracts;
using Microsoft.Extensions.Logging;

namespace DiagnosKit.Core.Logging.Implementations
{
    public class LoggerManager : ILoggerManager
    {
        private readonly ILogger<LoggerManager> _logger;

        public LoggerManager(ILogger<LoggerManager> logger)
        {
            _logger = logger;
        }

        public void LogDebug(string message) => _logger.LogDebug(message);

        public void LogDebug(string message, params object?[] objects)
            => _logger.LogDebug(message, objects);

        public void LogError(string message) => _logger.LogError(message);

        public void LogError(Exception exception, string message)
            => _logger.LogError(exception, message);

        public void LogError(Exception exception, string message, params object?[] objects)
            => _logger.LogError(exception, message, objects);

        public void LogFatal(Exception exception, string message)
            => _logger.LogCritical(exception, message);

        public void LogFatal(Exception exception, string message, params object?[] objects)
            => _logger.LogCritical(exception, message, objects);

        public void LogInfo(string message)
            => _logger.LogInformation(message);

        public void LogInfo(string message, params object?[] objects)
            => _logger.LogInformation(message, objects);

        public void LogWarn(string message)
            => _logger.LogWarning(message);

        public void LogWarn(string message, params object?[] objects)
            => _logger.LogWarning(message, objects);
    }
}

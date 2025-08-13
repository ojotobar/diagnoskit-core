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

        public void LogError(string message) => _logger.LogInformation(message);

        public void LogInfo(string message) => _logger.LogInformation(message);

        public void LogWarn(string message) => _logger.LogWarning(message);

        public void LogCritical(string message) => _logger.LogCritical(message);
    }
}

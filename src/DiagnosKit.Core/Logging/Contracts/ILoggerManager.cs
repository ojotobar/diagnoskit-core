namespace DiagnosKit.Core.Logging.Contracts
{
    public interface ILoggerManager
    {
        void LogDebug(string message);
        void LogDebug(string message, params object?[] objects);
        void LogError(string message);
        void LogError(Exception exception, string message);
        void LogError(Exception exception, string message, params object?[] objects);
        void LogFatal(Exception exception, string message, params object?[] objects);
        void LogFatal(Exception exception, string message);
        void LogInfo(string message);
        void LogInfo(string message, params object?[] objects);
        void LogWarn(string message);
        void LogWarn(string message, params object?[] objects);
    }
}

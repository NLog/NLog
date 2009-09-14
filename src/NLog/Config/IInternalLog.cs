namespace NLog.Config
{
    public interface IInternalLog
    {
        bool IsTraceEnabled { get; }
        bool IsDebugEnabled { get; }
        bool IsInfoEnabled { get; }
        bool IsWarnEnabled { get; }
        bool IsErrorEnabled { get; }
        bool IsFatalEnabled { get; }

        void Trace(string message, params object[] parameters);
        void Debug(string message, params object[] parameters);
        void Info(string message, params object[] parameters);
        void Warn(string message, params object[] parameters);
        void Error(string message, params object[] parameters);
        void Fatal(string message, params object[] parameters);
    }
}

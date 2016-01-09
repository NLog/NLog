using Microsoft.Extensions.Logging;
using NLog;
using System;

using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace NLog.Asp
{
    public class NLogAdapter : ILogger
    {
        private NLog.ILogger logger;

        public NLogAdapter(string loggerName)
        {
            logger = LogManager.GetLogger(loggerName);
        }

        public IDisposable BeginScopeImpl(object state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Verbose:
                    return logger.IsTraceEnabled;
                case LogLevel.Debug:
                    return logger.IsDebugEnabled;
                case LogLevel.Information:
                    return logger.IsInfoEnabled;
                case LogLevel.Warning:
                    return logger.IsWarnEnabled;
                case LogLevel.Error:
                    return logger.IsErrorEnabled;
                case LogLevel.Critical:
                    return logger.IsFatalEnabled;
                default:
                    throw new ArgumentException($"Unknown log level {logLevel}.", nameof(logLevel));
            }
        }

        public void Log(LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }
            string message = null;
            if (null != formatter)
            {
                message = formatter(state, exception);
            }
            else
            {
                message = LogFormatter.Formatter(state, exception);
            }
            switch (logLevel)
            {
                case LogLevel.Verbose:
                    logger.Trace(exception, message);
                    break;
                case LogLevel.Debug:
                    logger.Debug(exception, message);
                    break;
                case LogLevel.Information:
                    logger.Info(exception, message);
                    break;
                case LogLevel.Warning:
                    logger.Warn(exception, message);
                    break;
                case LogLevel.Error:
                    logger.Error(exception, message);
                    break;
                case LogLevel.Critical:
                    logger.Fatal(exception, message);
                    break;
                default:
                    logger.Warn($"Encountered unknown log level {logLevel}, writing out as Info.");
                    logger.Info(exception, message);
                    break;
            }
        }
    }
}

using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace NLog.Fluent
{
    /// <summary>
    /// A global logging class using caller info to find the logger.
    /// </summary>
    public static class Log
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Starts building a log event with the specified <see cref="LogLevel" />.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="callerFilePath">The full path of the source file that contains the caller. This is the file path at the time of compile.</param>
        /// <returns>An instance of the fluent <see cref="LogBuilder"/>.</returns>
        public static LogBuilder Level(LogLevel logLevel, [CallerFilePath]string callerFilePath = null)
        {
            return Create(logLevel, callerFilePath);
        }
        
        /// <summary>
        /// Starts building a log event at the <c>Trace</c> level.
        /// </summary>
        /// <param name="callerFilePath">The full path of the source file that contains the caller. This is the file path at the time of compile.</param>
        /// <returns>An instance of the fluent <see cref="LogBuilder"/>.</returns>
        public static LogBuilder Trace([CallerFilePath]string callerFilePath = null)
        {
            return Create(LogLevel.Trace, callerFilePath);
        }

        /// <summary>
        /// Starts building a log event at the <c>Debug</c> level.
        /// </summary>
        /// <param name="callerFilePath">The full path of the source file that contains the caller. This is the file path at the time of compile.</param>
        /// <returns>An instance of the fluent <see cref="LogBuilder"/>.</returns>
        public static LogBuilder Debug([CallerFilePath]string callerFilePath = null)
        {
            return Create(LogLevel.Debug, callerFilePath);
        }

        /// <summary>
        /// Starts building a log event at the <c>Info</c> level.
        /// </summary>
        /// <param name="callerFilePath">The full path of the source file that contains the caller. This is the file path at the time of compile.</param>
        /// <returns>An instance of the fluent <see cref="LogBuilder"/>.</returns>
        public static LogBuilder Info([CallerFilePath]string callerFilePath = null)
        {
            return Create(LogLevel.Info, callerFilePath);
        }

        /// <summary>
        /// Starts building a log event at the <c>Warn</c> level.
        /// </summary>
        /// <param name="callerFilePath">The full path of the source file that contains the caller. This is the file path at the time of compile.</param>
        /// <returns>An instance of the fluent <see cref="LogBuilder"/>.</returns>
        public static LogBuilder Warn([CallerFilePath]string callerFilePath = null)
        {
            return Create(LogLevel.Warn, callerFilePath);
        }

        /// <summary>
        /// Starts building a log event at the <c>Error</c> level.
        /// </summary>
        /// <param name="callerFilePath">The full path of the source file that contains the caller. This is the file path at the time of compile.</param>
        /// <returns>An instance of the fluent <see cref="LogBuilder"/>.</returns>
        public static LogBuilder Error([CallerFilePath]string callerFilePath = null)
        {
            return Create(LogLevel.Error, callerFilePath);
        }

        /// <summary>
        /// Starts building a log event at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="callerFilePath">The full path of the source file that contains the caller. This is the file path at the time of compile.</param>
        /// <returns>An instance of the fluent <see cref="LogBuilder"/>.</returns>
        public static LogBuilder Fatal([CallerFilePath]string callerFilePath = null)
        {
            return Create(LogLevel.Fatal, callerFilePath);
        }

        private static LogBuilder Create(LogLevel logLevel, string callerFilePath)
        {
            string name = Path.GetFileNameWithoutExtension(callerFilePath ?? string.Empty);
            var logger = string.IsNullOrWhiteSpace(name) ? _logger : LogManager.GetLogger(name);

            var builder = new LogBuilder(logger, logLevel);
            if (callerFilePath != null)
                builder.Property("CallerFilePath", callerFilePath);

            return builder;
        }
    }
}

using System;

namespace NLog.Fluent
{
    /// <summary>
    /// Extension methods for NLog <see cref="Logger"/>.
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        /// Starts building a log event with the specified <see cref="LogLevel"/>.
        /// </summary>
        /// <param name="logger">The logger to write the log event to.</param>
        /// <param name="logLevel">The log level.</param>
        /// <returns></returns>
        public static LogBuilder Log(this Logger logger, LogLevel logLevel)
        {
            var builder = new LogBuilder(logger, logLevel);
            return builder;
        }
        
        /// <summary>
        /// Starts building a log event at the <c>Trace</c> level.
        /// </summary>
        /// <param name="logger">The logger to write the log event to.</param>
        /// <returns></returns>
        public static LogBuilder Trace(this Logger logger)
        {
            var builder = new LogBuilder(logger, LogLevel.Trace);
            return builder;
        }
        
        /// <summary>
        /// Starts building a log event at the <c>Debug</c> level.
        /// </summary>
        /// <param name="logger">The logger to write the log event to.</param>
        /// <returns></returns>
        public static LogBuilder Debug(this Logger logger)
        {
            var builder = new LogBuilder(logger, LogLevel.Debug);
            return builder;
        }
        
        /// <summary>
        /// Starts building a log event at the <c>Info</c> level.
        /// </summary>
        /// <param name="logger">The logger to write the log event to.</param>
        /// <returns></returns>
        public static LogBuilder Info(this Logger logger)
        {
            var builder = new LogBuilder(logger, LogLevel.Info);
            return builder;
        }
        
        /// <summary>
        /// Starts building a log event at the <c>Warn</c> level.
        /// </summary>
        /// <param name="logger">The logger to write the log event to.</param>
        /// <returns></returns>
        public static LogBuilder Warn(this Logger logger)
        {
            var builder = new LogBuilder(logger, LogLevel.Warn);
            return builder;
        }
        
        /// <summary>
        /// Starts building a log event at the <c>Error</c> level.
        /// </summary>
        /// <param name="logger">The logger to write the log event to.</param>
        /// <returns></returns>
        public static LogBuilder Error(this Logger logger)
        {
            var builder = new LogBuilder(logger, LogLevel.Error);
            return builder;
        }
        
        /// <summary>
        /// Starts building a log event at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="logger">The logger to write the log event to.</param>
        /// <returns></returns>
        public static LogBuilder Fatal(this Logger logger)
        {
            var builder = new LogBuilder(logger, LogLevel.Fatal);
            return builder;
        }
    }
}

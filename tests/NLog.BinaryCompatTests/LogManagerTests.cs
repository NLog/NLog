namespace NLog.BinaryCompatTests
{
    using System;
    using NLog.Config;

    public static class LogManagerTests
    {
        public static void Configuration()
        {
            LogManager.Configuration = new XmlLoggingConfiguration("NLog.config");
            LogManager.Configuration = null;
        }

        public static void CreateNullLogger()
        {
            Logger logger = LogManager.CreateNullLogger();
        }

        public static void EnableDisableLogging()
        {
            LogManager.DisableLogging();
            LogManager.EnableLogging();
            LogManager.IsLoggingEnabled();
        }

        public static void FlushTests()
        {
            LogManager.Flush();
            LogManager.Flush(100);
            LogManager.Flush(TimeSpan.FromSeconds(1));
        }

        public static void GetCurrentClassLoggerWithType()
        {
            LogManager.GetCurrentClassLogger(typeof(MyLogger));
        }

        public static void GetCurrentClassLoggerWithoutType()
        {
            LogManager.GetCurrentClassLogger();
        }

        public static void GetLoggerWithType()
        {
            LogManager.GetLogger("AAA", typeof(MyLogger));
        }

        public static void GetLoggerWithoutType()
        {
            LogManager.GetLogger("AAA");
        }

        public static void GlobalThreshold()
        {
            LogManager.GlobalThreshold = LogLevel.Off;
            LogManager.GlobalThreshold = LogLevel.Trace;
        }

        public static void ReconfigExistingLoggers()
        {
            LogManager.ReconfigExistingLoggers();
        }

        public static void ThrowExceptions()
        {
            LogManager.ThrowExceptions = true;
        }

        public class MyLogger : Logger
        {
        }
    }
}
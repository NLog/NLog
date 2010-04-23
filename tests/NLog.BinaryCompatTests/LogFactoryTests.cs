namespace NLog.BinaryCompatTests
{
    using System;
    using NLog.Config;

    public static class LogFactoryTests
    {
        public static void Configuration()
        {
            var factory = new LogFactory();
            factory.Configuration = new XmlLoggingConfiguration("NLog.config");
            factory.Configuration = null;
        }

        public static void CreateNullLogger()
        {
            var factory = new LogFactory();
            Logger logger = factory.CreateNullLogger();
        }

        public static void EnableDisableLogging()
        {
            var factory = new LogFactory();
            factory.DisableLogging();
            factory.EnableLogging();
            factory.IsLoggingEnabled();
        }

        public static void FlushTests()
        {
            var factory = new LogFactory();
            factory.Flush();
            factory.Flush(100);
            factory.Flush(TimeSpan.FromSeconds(1));
        }

        public static void GetCurrentClassLoggerWithType()
        {
            var factory = new LogFactory();
            factory.GetCurrentClassLogger(typeof(MyLogger));
        }

        public static void GetCurrentClassLoggerWithoutType()
        {
            var factory = new LogFactory();
            factory.GetCurrentClassLogger();
        }

        public static void GetLoggerWithType()
        {
            var factory = new LogFactory();
            factory.GetLogger("AAA", typeof(MyLogger));
        }

        public static void GetLoggerWithoutType()
        {
            var factory = new LogFactory();
            factory.GetLogger("AAA");
        }

        public static void GlobalThreshold()
        {
            var factory = new LogFactory();
            factory.GlobalThreshold = LogLevel.Off;
            factory.GlobalThreshold = LogLevel.Trace;
        }

        public static void ReconfigExistingLoggers()
        {
            var factory = new LogFactory();
            factory.ReconfigExistingLoggers();
        }

        public static void ThrowExceptions()
        {
            var factory = new LogFactory();
            factory.ThrowExceptions = true;
        }

        public class MyLogger : Logger
        {
        }
    }
}
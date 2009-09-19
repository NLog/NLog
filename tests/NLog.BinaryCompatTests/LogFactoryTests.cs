using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NLog.Config;
using NLog.Targets;

namespace NLog.BinaryCompatTests
{
    public static class LogFactoryTests
    {
        public class MyLogger : Logger
        {
        }

        public static void GetCurrentClassLoggerWithoutType()
        {
            var factory = new LogFactory();
            factory.GetCurrentClassLogger();
        }

        public static void GetCurrentClassLoggerWithType()
        {
            var factory = new LogFactory();
            factory.GetCurrentClassLogger(typeof(MyLogger));
        }

        public static void GetLoggerWithoutType()
        {
            var factory = new LogFactory();
            factory.GetLogger("AAA");
        }

        public static void GetLoggerWithType()
        {
            var factory = new LogFactory();
            factory.GetLogger("AAA", typeof(MyLogger));
        }

        public static void CreateNullLogger()
        {
            var factory = new LogFactory();
            var logger = factory.CreateNullLogger();
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

        public static void Events()
        {
            var factory = new LogFactory();
            factory.ConfigurationReloaded += new LoggingConfigurationReloaded(factory_ConfigurationReloaded);
            factory.ConfigurationChanged += new LoggingConfigurationChanged(factory_ConfigurationChanged);
            factory.ThrowExceptions = true;
        }

        static void factory_ConfigurationReloaded(bool succeeded, Exception ex)
        {
            throw new NotImplementedException();
        }

        static void factory_ConfigurationChanged(LoggingConfiguration oldConfig, LoggingConfiguration newConfig)
        {
            throw new NotImplementedException();
        }
    }
}

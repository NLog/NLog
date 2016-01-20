using System;
using System.Xml;
using NLog;
using NLog.Config;

namespace NlogDotNetCore
{
    public class Program
    {
        private static ILogger _logger;
        public static void LoadLoggerConfiguration()
        {
            var reader = XmlReader.Create("NLog.config");
            var config = new XmlLoggingConfiguration(reader, null); //filename is not required.
            LogManager.Configuration = config;
            _logger = LogManager.GetCurrentClassLogger();
        }

        public static void Main(string[] args)
        {
            LoadLoggerConfiguration();

            _logger.Trace("Sample trace message");
            _logger.Debug("Sample debug message");
            _logger.Info("Sample informational message");
            _logger.Warn("Sample warning message");
            _logger.Error("Sample error message");
            _logger.Fatal("Sample fatal error message");

            // alternatively you can call the Log() method 
            // and pass log level as the parameter.
            _logger.Log(LogLevel.Info, "Sample informational message");
        }
    }
}

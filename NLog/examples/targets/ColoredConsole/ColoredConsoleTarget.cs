using NLog;
using NLog.Win32.Targets;

class Example
{
    static void Main(string[] args)
    {
        ColoredConsoleTarget target = new ColoredConsoleTarget();
        target.Layout = "${date:format=HH\\:MM\\:ss} ${logger} ${message}";

        NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);

        Logger logger = LogManager.GetLogger("Example");
        logger.Trace("trace log message");
        logger.Debug("debug log message");
        logger.Info("info log message");
        logger.Warn("warn log message");
        logger.Error("error log message");
        logger.Fatal("fatal log message");
    }
}

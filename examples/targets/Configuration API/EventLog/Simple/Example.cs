using NLog;
using NLog.Targets;
using NLog.Win32.Targets;

class Example
{
    static void Main(string[] args)
    {
        EventLogTarget target = new EventLogTarget();
        target.Source = "My Source";
        target.Log = "Application";
        target.MachineName = ".";
        target.Layout = "${logger}: ${message}";

        NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Debug);

        Logger logger = LogManager.GetLogger("Example");
        logger.Debug("log message");
    }
}

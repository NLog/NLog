using NLog;
using NLog.Config;
using NLog.Targets;

class Example
{
    static void Main(string[] args)
    {
        EventLogTarget target = new EventLogTarget();
        target.Source = "My Source";
        target.Log = "Application";
        target.Layout = "${logger}: ${message} ${exception}";

        LoggingConfiguration nlogConfig = new LoggingConfiguration();
        nlogConfig.AddRuleForAllLevels(target);
        LogManager.Configuration = nlogConfig;

        Logger logger = LogManager.GetLogger("Example");
        logger.Debug("log message");
    }
}

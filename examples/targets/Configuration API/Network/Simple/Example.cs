using NLog;
using NLog.Config;
using NLog.Targets;

class Example
{
    static void Main(string[] args)
    {
        NetworkTarget target = new NetworkTarget();
        target.Layout = "${level} ${logger} ${message}${newline}";
        target.Address = "tcp://localhost:5555";

        LoggingConfiguration nlogConfig = new LoggingConfiguration();
        nlogConfig.AddRuleForAllLevels(target);
        LogManager.Configuration = nlogConfig;

        Logger logger = LogManager.GetLogger("Example");
        logger.Trace("log message 1");
        logger.Debug("log message 2");
        logger.Info("log message 3");
        logger.Warn("log message 4");
        logger.Error("log message 5");
        logger.Fatal("log message 6");

        LogManager.Flush();
    }
}

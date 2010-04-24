using NLog;
using NLog.Targets;

class Example
{
    static void Main(string[] args)
    {
        NetworkTarget target = new NetworkTarget();
        target.Layout = "${level} ${logger} ${message}${newline}";
        target.Address = "tcp://localhost:5555";

        NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Debug);

        Logger logger = LogManager.GetLogger("Example");
        logger.Trace("log message 1");
        logger.Debug("log message 2");
        logger.Info("log message 3");
        logger.Warn("log message 4");
        logger.Error("log message 5");
        logger.Fatal("log message 6");
    }
}

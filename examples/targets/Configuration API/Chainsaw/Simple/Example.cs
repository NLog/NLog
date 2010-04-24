using NLog;
using NLog.Targets;

class Example
{
    static void Main(string[] args)
    {
        ChainsawTarget target = new ChainsawTarget();
        target.Address = "udp://localhost:4000";

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

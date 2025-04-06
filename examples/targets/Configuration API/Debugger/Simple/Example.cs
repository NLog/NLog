using NLog;
using NLog.Config;
using NLog.Targets;

class Example
{
    static void Main(string[] args)
    {
        DebuggerTarget target = new DebuggerTarget();
        target.Layout = "${message}";

        LoggingConfiguration nlogConfig = new LoggingConfiguration();
        nlogConfig.AddRuleForAllLevels(target);
        LogManager.Configuration = nlogConfig;

        Logger logger = LogManager.GetLogger("Example");
        logger.Debug("log message");
    }
}

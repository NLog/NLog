using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;

class Example
{
    static void Main(string[] args)
    {
        FileTarget wrappedTarget = new FileTarget();
        wrappedTarget.FileName = "${basedir}/file.txt";

        FilteringTargetWrapper filteringTarget = new FilteringTargetWrapper();
        filteringTarget.WrappedTarget = wrappedTarget;

        filteringTarget.Condition = "contains('${message}','1')";

        LoggingConfiguration nlogConfig = new LoggingConfiguration();
        nlogConfig.AddRuleForAllLevels(filteringTarget);
        LogManager.Configuration = nlogConfig;

        Logger logger = LogManager.GetLogger("Example");
        logger.Debug("log message 0");
        logger.Debug("log message 1");
        logger.Debug("log message 2");
        logger.Debug("log message 11");
    }
}

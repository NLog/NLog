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

        RetryingTargetWrapper target = new RetryingTargetWrapper();
        target.WrappedTarget = wrappedTarget;
        target.RetryCount = 3;
        target.RetryDelayMilliseconds = 1000;

        LoggingConfiguration nlogConfig = new LoggingConfiguration();
        nlogConfig.AddRuleForAllLevels(target);
        LogManager.Configuration = nlogConfig;

        Logger logger = LogManager.GetLogger("Example");
        logger.Debug("log message");
    }
}

using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;

class Example
{
    static void Main(string[] args)
    {
        FileTarget target = new FileTarget();
        target.Layout = "${longdate} ${logger} ${message}";
        target.FileName = "${basedir}/logs/logfile.txt";
        target.KeepFileOpen = false;
        target.Encoding = System.Text.Encoding.UTF8;

        AsyncTargetWrapper wrapper = new AsyncTargetWrapper();
        wrapper.WrappedTarget = target;
        wrapper.QueueLimit = 5000;
        wrapper.OverflowAction = AsyncTargetWrapperOverflowAction.Discard;

        LoggingConfiguration nlogConfig = new LoggingConfiguration();
        nlogConfig.AddRuleForAllLevels(wrapper);
        LogManager.Configuration = nlogConfig;

        Logger logger = LogManager.GetLogger("Example");
        logger.Debug("log message");

        LogManager.Flush();
    }
}

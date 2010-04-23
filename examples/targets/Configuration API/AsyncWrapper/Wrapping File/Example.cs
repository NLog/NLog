using NLog;
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
        target.Encoding = "iso-8859-2";

        AsyncTargetWrapper wrapper = new AsyncTargetWrapper();
        wrapper.WrappedTarget = target;
        wrapper.QueueLimit = 5000;
        wrapper.OverflowAction = AsyncTargetWrapperOverflowAction.Discard;

        NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(wrapper, LogLevel.Debug);

        Logger logger = LogManager.GetLogger("Example");
        logger.Debug("log message");

        wrapper.Flush();
    }
}

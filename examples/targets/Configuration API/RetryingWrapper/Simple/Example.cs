using System;

using NLog;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System.Diagnostics;

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

        NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Debug);

        Logger logger = LogManager.GetLogger("Example");
        logger.Debug("log message");
    }
}

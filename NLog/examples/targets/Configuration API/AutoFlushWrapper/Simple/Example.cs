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

        AutoFlushTargetWrapper target = new AutoFlushTargetWrapper();
        target.WrappedTarget = wrappedTarget;

        NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Debug);

        Logger logger = LogManager.GetLogger("Example");
        logger.Debug("log message");
    }
}

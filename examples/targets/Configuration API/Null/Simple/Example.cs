using System;

using NLog;
using NLog.Targets;

class Example
{
    static void Main(string[] args)
    {
        NullTarget target = new NullTarget();
        target.Layout = "${message}";
        target.FormatMessage = true;

        NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Debug);

        Logger logger = LogManager.GetLogger("Example");
        logger.Debug("log message");
    }
}

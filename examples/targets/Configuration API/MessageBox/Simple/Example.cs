using System;

using NLog;
using NLog.Targets;

class Example
{
    static void Main(string[] args)
    {
        MessageBoxTarget target = new MessageBoxTarget();
        target.Layout = "${longdate}: ${message}";
        target.Caption = "${level} message";

        NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Debug);

        Logger logger = LogManager.GetLogger("Example");
        logger.Debug("log message");
    }
}

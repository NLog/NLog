using System;

using NLog;
using NLog.Targets;

class Example
{
    static void Main(string[] args)
    {
        DebugTarget target = new DebugTarget();
        target.Layout = "${message}";

        NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Debug);

        Logger logger = LogManager.GetLogger("Example");
        logger.Debug("log message");
        logger.Debug("another log message");

        Console.WriteLine("The debug target has been hit {0} times.", target.Counter);
        Console.WriteLine("The last message was '{0}'.", target.LastMessage);
    }
}

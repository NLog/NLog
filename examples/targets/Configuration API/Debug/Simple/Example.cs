using NLog;
using NLog.Config;
using NLog.Targets;

class Example
{
    static void Main(string[] args)
    {
        DebugTarget target = new DebugTarget();
        target.Layout = "${message}";

        LoggingConfiguration nlogConfig = new LoggingConfiguration();
        nlogConfig.AddRuleForAllLevels(target);
        LogManager.Configuration = nlogConfig;

        Logger logger = LogManager.GetLogger("Example");
        logger.Debug("log message");
        logger.Debug("another log message");

        Console.WriteLine("The debug target has been hit {0} times.", target.Counter);
        Console.WriteLine("The last message was '{0}'.", target.LastMessage);
    }
}

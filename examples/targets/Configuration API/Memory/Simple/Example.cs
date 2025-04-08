using NLog;
using NLog.Config;
using NLog.Targets;

class Example
{
    static void Main(string[] args)
    {
        MemoryTarget target = new MemoryTarget();
        target.Layout = "${message}";

        LoggingConfiguration nlogConfig = new LoggingConfiguration();
        nlogConfig.AddRuleForAllLevels(target);
        LogManager.Configuration = nlogConfig;

        Logger logger = LogManager.GetLogger("Example");
        logger.Debug("log message");

        foreach (string s in target.Logs)
        {
            Console.Write("logged: {0}", s);
        }
    }
}

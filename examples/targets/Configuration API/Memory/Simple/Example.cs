using System;

using NLog;
using NLog.Targets;

class Example
{
    static void Main(string[] args)
    {
        MemoryTarget target = new MemoryTarget();
        target.Layout = "${message}";

        NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Debug);

        Logger logger = LogManager.GetLogger("Example");
        logger.Debug("log message");

        foreach (string s in target.Logs)
        {
            Console.Write("logged: {0}", s);
        }
    }
}

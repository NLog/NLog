using System;

using NLog;
using NLog.Config;
using NLog.Targets;
using System.Diagnostics;

class Example
{
    static void Main(string[] args)
    {
        Trace.Listeners.Add(new ConsoleTraceListener());

        TraceTarget target = new TraceTarget();
        target.Layout = "${message}";

        LoggingConfiguration nlogConfig = new LoggingConfiguration();
        nlogConfig.AddRuleForAllLevels(target);
        LogManager.Configuration = nlogConfig;

        Logger logger = LogManager.GetLogger("Example");
        logger.Debug("log message");
    }
}

using System;

using NLog;
using NLog.Targets;
using NLog.Win32.Targets;
using System.Diagnostics;

class Example
{
    static void Main(string[] args)
    {
        PerfCounterTarget target = new PerfCounterTarget();
        target.AutoCreate = true;
        target.CategoryName = "My category";
        target.CounterName = "My counter";
        target.CounterType = PerformanceCounterType.NumberOfItems32;
        target.InstanceName = "My instance";

        NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Debug);

        Logger logger = LogManager.GetLogger("Example");
        logger.Debug("log message");
    }
}

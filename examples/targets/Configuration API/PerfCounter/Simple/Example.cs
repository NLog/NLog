using NLog;
using NLog.Config;
using NLog.Targets;

class Example
{
    static void Main(string[] args)
    {
        PerfCounterTarget target = new PerfCounterTarget();
        target.AutoCreate = true;
        target.CategoryName = "My category";
        target.CounterName = "My counter";
        target.CounterType = System.Diagnostics.PerformanceCounterType.NumberOfItems32;
        target.InstanceName = "My instance";

        LoggingConfiguration nlogConfig = new LoggingConfiguration();
        nlogConfig.AddRuleForAllLevels(target);
        LogManager.Configuration = nlogConfig;

        Logger logger = LogManager.GetLogger("Example");
        logger.Debug("log message");
    }
}

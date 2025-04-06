using NLog;
using NLog.Config;
using NLog.Targets;

class Example
{
    static void Main(string[] args)
    {
        MSMQTarget target = new MSMQTarget();
        target.Queue = ".\\private$\\nlog";
        target.Label = "${message}";
        target.Layout = "${message}";
        target.CreateQueueIfNotExists = true;
        target.Recoverable = true;

        LoggingConfiguration nlogConfig = new LoggingConfiguration();
        nlogConfig.AddRuleForAllLevels(target);
        LogManager.Configuration = nlogConfig;

        Logger l = LogManager.GetLogger("AAA");
        l.Error("This is an error. It goes to .\\private$\\nlog queue.");
        l.Debug("This is a debug information. It goes to .\\private$\\nlog queue.");
        l.Info("This is a information. It goes to .\\private$\\nlog queue.");
        l.Warn("This is a warn information. It goes to .\\private$\\nlog queue.");
        l.Fatal("This is a fatal information. It goes to .\\private$\\nlog queue.");
        l.Trace("This is a trace information. It goes to .\\private$\\nlog queue.");
    }
}

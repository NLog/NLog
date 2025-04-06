using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;

class Example
{
    static void Main(string[] args)
    {
        FileTarget wrappedTarget = new FileTarget();
        wrappedTarget.FileName = "${basedir}/file.txt";

        PostFilteringTargetWrapper postFilteringTarget = new PostFilteringTargetWrapper();
        postFilteringTarget.WrappedTarget = wrappedTarget;

        // set up default filter
        postFilteringTarget.DefaultFilter = "level >= LogLevel.Info";

        // if there are any warnings in the buffer
        // dump the messages whose level is Debug or higher

        FilteringRule rule = new FilteringRule();
        rule.Exists = "level >= LogLevel.Warn";
        rule.Filter = "level >= LogLevel.Debug";

        postFilteringTarget.Rules.Add(rule);

        BufferingTargetWrapper target = new BufferingTargetWrapper();
        target.BufferSize = 100;
        target.WrappedTarget = postFilteringTarget;

        LoggingConfiguration nlogConfig = new LoggingConfiguration();
        nlogConfig.AddRuleForAllLevels(target);
        LogManager.Configuration = nlogConfig;

        Logger logger = LogManager.GetLogger("Example");
        logger.Debug("log message");
    }
}

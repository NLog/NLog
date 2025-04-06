using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;

class Example
{
    static void Main(string[] args)
    {
        FileTarget file1 = new FileTarget();
        file1.FileName = "${basedir}/file1.txt";

        FileTarget file2 = new FileTarget();
        file2.FileName = "${basedir}/file2.txt";

        RandomizeTarget target = new RandomizeTarget();
        target.Targets.Add(file1);
        target.Targets.Add(file2);

        LoggingConfiguration nlogConfig = new LoggingConfiguration();
        nlogConfig.AddRuleForAllLevels(target);
        LogManager.Configuration = nlogConfig;

        Logger logger = LogManager.GetLogger("Example");
        logger.Debug("log message 1");
        logger.Debug("log message 2");
        logger.Debug("log message 3");
    }
}

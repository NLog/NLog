using NLog;
using NLog.Config;
using NLog.Targets;

class Example
{
    static void Main(string[] args)
    {
        FileTarget target = new FileTarget();
        target.Layout = "${longdate} ${logger} ${message}";
        target.FileName = "${basedir}/logs/logfile.txt";
        target.ArchiveFileName = "${basedir}/archives/log.{#####}.txt";
        target.ArchiveAboveSize = 10 * 1024; // archive files greater than 10 KB
        target.ArchiveNumbering = FileTarget.ArchiveNumberingMode.Sequence;

        LoggingConfiguration nlogConfig = new LoggingConfiguration();
        nlogConfig.AddRuleForAllLevels(target);
        LogManager.Configuration = nlogConfig;

        Logger logger = LogManager.GetLogger("Example");
        
        // generate a large volume of messages
        for (int i = 0; i < 1000; ++i)
        {
            logger.Debug("log message {0}", i);
        }
    }
}

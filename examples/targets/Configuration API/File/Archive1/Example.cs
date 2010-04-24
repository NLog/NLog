using NLog;
using NLog.Targets;
using NLog.Targets.Wrappers;

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

        // this speeds up things when no other processes are writing to the file
        target.ConcurrentWrites = true;

        NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Debug);

        Logger logger = LogManager.GetLogger("Example");
        
        // generate a large volume of messages
        for (int i = 0; i < 1000; ++i)
        {
            logger.Debug("log message {0}", i);
        }
    }
}

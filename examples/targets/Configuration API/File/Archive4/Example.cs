using NLog;
using NLog.Config;
using NLog.Targets;

class Example
{
    static void Main(string[] args)
    {
        FileTarget target = new FileTarget();
        target.Layout = "${longdate} ${logger} ${message}";
        target.FileName = "${basedir}/logs/logfile.${level}.txt";
        // where to store the archive files
        target.ArchiveFileName = "${basedir}/archives/${level}/log.{#####}.txt";
        target.ArchiveEvery = FileTarget.ArchiveEveryMode.Minute;
        target.ArchiveNumbering = FileTarget.ArchiveNumberingMode.Rolling;
        target.MaxArchiveFiles = 3;
        target.ArchiveAboveSize = 10000;

        LoggingConfiguration nlogConfig = new LoggingConfiguration();
        nlogConfig.AddRuleForAllLevels(target);
        LogManager.Configuration = nlogConfig;

        Logger logger = LogManager.GetLogger("Example");

        // generate a large number of messages, sleeping 1/10 of second between writes
        // to observe time-based archiving which occurs every minute
        // the volume is high enough to cause ArchiveAboveSize to be triggered
        // so that log files larger than 10000 bytes are archived as well

        // in this version, a single File target keeps track of 3 sets of log and 
        // archive files, one for each level

        // you get:
        //      logs/logfile.Debug.txt
        //      logs/logfile.Error.txt
        //      logs/logfile.Fatal.txt
        //
        // and your archives go to:
        //
        //      archives/Debug/log.00000.txt
        //      archives/Debug/log.00001.txt
        //      archives/Debug/log.00002.txt
        //      archives/Debug/log.00003.txt
        //      archives/Error/log.00000.txt
        //      archives/Error/log.00001.txt
        //      archives/Error/log.00002.txt
        //      archives/Error/log.00003.txt
        //      archives/Fatal/log.00000.txt
        //      archives/Fatal/log.00001.txt
        //      archives/Fatal/log.00002.txt
        //      archives/Fatal/log.00003.txt

        for (int i = 0; i < 2500; ++i)
        {
            logger.Debug("log message {i}", i);
            logger.Error("log message {i}", i);
            logger.Fatal("log message {i}", i);
            System.Threading.Thread.Sleep(100);
        }
    }
}

using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;

class Example
{
    static void Main(string[] args)
    {
        FileTarget file1 = new FileTarget();
        file1.FileName = "\\\\server1\\share\\file1.txt";

        FileTarget file2 = new FileTarget();
        file2.FileName = "\\\\server2\\share\\file1.txt";

        // write to server1, if it fails switch to server2
        FallbackTarget target = new FallbackTarget();

        target.ReturnToFirstOnSuccess = false;
        target.Targets.Add(file1);
        target.Targets.Add(file2);

        LoggingConfiguration nlogConfig = new LoggingConfiguration();
        nlogConfig.AddRuleForAllLevels(target);
        LogManager.Configuration = nlogConfig;

        Logger logger = LogManager.GetLogger("Example");
        logger.Debug("log message");
    }
}

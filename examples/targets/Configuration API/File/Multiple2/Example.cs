using NLog;
using NLog.Targets;

class Example
{
    static void Main(string[] args)
    {
        FileTarget target = new FileTarget();
        target.Layout = "${longdate} ${logger} ${message}";
        target.FileName = "${basedir}/${shortdate}/${windows-identity:domain=false}.${level}.log";
        target.KeepFileOpen = false;
        target.Encoding = "iso-8859-2";

        NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Debug);

        Logger logger = LogManager.GetLogger("Example");
        logger.Debug("log message");
    }
}

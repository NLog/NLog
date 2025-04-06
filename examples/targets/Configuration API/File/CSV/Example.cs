using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Layouts;

class Example
{
    static void Main(string[] args)
    {
        FileTarget target = new FileTarget();
        target.FileName = "${basedir}/file.csv";

        CsvLayout layout = new CsvLayout();
        layout.Columns.Add(new CsvColumn("time", "${longdate}"));
        layout.Columns.Add(new CsvColumn("message", "${message}"));
        layout.Columns.Add(new CsvColumn("logger", "${logger}"));
        layout.Columns.Add(new CsvColumn("level", "${level}"));

        target.Layout = layout;

        LoggingConfiguration nlogConfig = new LoggingConfiguration();
        nlogConfig.AddRuleForAllLevels(target);
        LogManager.Configuration = nlogConfig;

        Logger logger = LogManager.GetLogger("Example");
        logger.Debug("log message");
        logger.Debug("Message with \"quotes\" and \nnew line characters.");
    }
}

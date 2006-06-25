using System;

using NLog;
using NLog.Targets;

class Example
{
    static void Main(string[] args)
    {
        CsvFileTarget target = new CsvFileTarget();
        target.FileName = "${basedir}/file.csv";

        target.Columns.Add(new CsvFileColumn("time", "${longdate}"));
        target.Columns.Add(new CsvFileColumn("message", "${message}"));
        target.Columns.Add(new CsvFileColumn("logger", "${logger}"));
        target.Columns.Add(new CsvFileColumn("level", "${level}"));

        NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Debug);

        Logger logger = LogManager.GetLogger("Example");
        logger.Debug("log message");
        logger.Debug("Message with \"quotes\" and \nnew line characters.");
    }
}

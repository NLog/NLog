using System;

using NLog;
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

        target.CompiledLayout = layout;

        NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Debug);

        Logger logger = LogManager.GetLogger("Example");
        logger.Debug("log message");
        logger.Debug("Message with \"quotes\" and \nnew line characters.");
    }
}

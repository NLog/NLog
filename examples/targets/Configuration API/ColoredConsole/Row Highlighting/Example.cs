using NLog;
using NLog.Win32.Targets;

class Example
{
    static void Main(string[] args)
    {
        ColoredConsoleTarget target = new ColoredConsoleTarget();
        target.Layout = "${date:format=HH\\:MM\\:ss} ${logger} ${message}";
        target.RowHighlightingRules.Add(
                new ConsoleRowHighlightingRule(
                    "level >= LogLevel.Error and contains(message,'serious')", // condition
                    ConsoleOutputColor.White, // foreground color
                    ConsoleOutputColor.Red // background color
                    )
                );

        target.RowHighlightingRules.Add(
                new ConsoleRowHighlightingRule(
                    "starts-with(logger,'Example')", // condition
                    ConsoleOutputColor.Yellow, // foreground color
                    ConsoleOutputColor.DarkBlue) // background color
                );
        NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);

        // LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration("ColoredConsoleTargetRowHighlighting.nlog");

        Logger logger = LogManager.GetLogger("Example");
        logger.Trace("trace log message");
        logger.Debug("debug log message");
        logger.Info("info log message");
        logger.Warn("warn log message");
        logger.Error("very serious error log message");
        logger.Fatal("fatal log message, rather serious");

        Logger logger2 = LogManager.GetLogger("Another");
        logger2.Trace("trace log message");
        logger2.Debug("debug log message");
        logger2.Info("info log message");
        logger2.Warn("warn log message");
        logger2.Error("very serious error log message");
        logger2.Fatal("fatal log message");
    }
}

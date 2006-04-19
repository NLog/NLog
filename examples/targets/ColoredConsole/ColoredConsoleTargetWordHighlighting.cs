using NLog;
using NLog.Win32.Targets;

class Example
{
    static void Main(string[] args)
    {
        ColoredConsoleTarget target = new ColoredConsoleTarget();
        target.Layout = "${date:format=HH\\:MM\\:ss} ${logger} ${message}";
        target.WordHighlightingRules.Add(
                new ConsoleWordHighlightingRule("log", 
                    ConsoleOutputColor.NoChange, 
                    ConsoleOutputColor.DarkGreen));
        target.WordHighlightingRules.Add(
                new ConsoleWordHighlightingRule("abc", 
                    ConsoleOutputColor.Cyan, 
                    ConsoleOutputColor.NoChange));
                

        NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);

        Logger logger = LogManager.GetLogger("Example");
        logger.Trace("trace log message abcdefghijklmnopq");
        logger.Debug("debug log message");
        logger.Info("info log message abc abcdefghijklmnopq");
        logger.Warn("warn log message");
        logger.Error("error log abcdefghijklmnopq message abc");
        logger.Fatal("fatal log message abcdefghijklmnopq abc");
    }
}

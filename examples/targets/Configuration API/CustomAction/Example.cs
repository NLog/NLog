using NLog;
using NLog.Targets;
using System;

class Example
{
    static void Main(string[] args)
    {
        CustomActionTarget target = new CustomActionTarget();
        target.Name = "CustomActionTarget1";
        target.Layout = @"${date:format=HH\:MM\:ss} ${message}";

        ConsoleActionProvider provider = new ConsoleActionProvider();
        CustomActionTarget.Register(provider);

        NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Debug);

        Logger logger = LogManager.GetLogger("Example");
        logger.Debug("log message");
        logger.Error("error message");
        Console.ReadKey();
    }
}

public class ConsoleActionProvider : IActionProvider
{
    public void Action(TargetWithLayout target, LogEventInfo logEvent)
    {
        Console.WriteLine(" Action Povider: {0}; Target {1}; Event: {2}",
             this.GetType(), target.Name, target.Layout.Render(logEvent));
    }
}

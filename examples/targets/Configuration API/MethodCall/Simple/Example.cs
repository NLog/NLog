using System;

using NLog;
using NLog.Config;
using NLog.Targets;

public class Example
{
    public static void LogMethod(string level, string message)
    {
        Console.WriteLine("l: {0} m: {1}", level, message);
    }

    static void Main(string[] args)
    {
        MethodCallTarget target = new MethodCallTarget();
        target.ClassName = typeof(Example).AssemblyQualifiedName;
        target.MethodName = "LogMethod";
        target.Parameters.Add(new MethodCallParameter("${level}"));
        target.Parameters.Add(new MethodCallParameter("${message}"));

        LoggingConfiguration nlogConfig = new LoggingConfiguration();
        nlogConfig.AddRuleForAllLevels(target);
        LogManager.Configuration = nlogConfig;

        Logger logger = LogManager.GetLogger("Example");
        logger.Debug("log message");
        logger.Error("error message");
    }
}

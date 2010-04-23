using System;

using NLog;
using NLog.Targets;
using System.Diagnostics;

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

        NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Debug);

        Logger logger = LogManager.GetLogger("Example");
        logger.Debug("log message");
        logger.Error("error message");
    }
}

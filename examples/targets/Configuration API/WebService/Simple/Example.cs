using NLog;
using NLog.Targets;

class Example
{
    static void Main(string[] args)
    {
        WebServiceTarget target = new WebServiceTarget();
        target.Url = "http://localhost:2648/Service1.asmx";
        target.MethodName = "HelloWorld";
        target.Namespace = "http://www.nlog-project.org/example";
        target.Protocol = WebServiceTarget.WebServiceProtocol.Soap11;

        target.Parameters.Add(new MethodCallParameter("n1", "${message}"));
        target.Parameters.Add(new MethodCallParameter("n2", "${logger}"));
        target.Parameters.Add(new MethodCallParameter("n3", "${level}"));

        NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Debug);

        Logger logger = LogManager.GetLogger("Example");
        logger.Trace("log message 1");
        logger.Debug("log message 2");
        logger.Info("log message 3");
        logger.Warn("log message 4");
        logger.Error("log message 5");
        logger.Fatal("log message 6");
    }
}

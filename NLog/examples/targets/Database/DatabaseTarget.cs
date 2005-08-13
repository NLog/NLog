using NLog;
using NLog.Targets;

class Example
{
    static void Main(string[] args)
    {
        DatabaseTarget target = new DatabaseTarget();
        DatabaseParameterInfo param;
        
        target.DBProvider = "mssql";
        target.DBHost = ".";
        target.DBUserName = "nloguser";
        target.DBPassword = "pass";
        target.DBDatabase = "databasename";
        target.CommandText = "insert into LogTable(time_stamp,level,logger,message) values(@time_stamp, @level, @logger, @message);";

        param = new DatabaseParameterInfo();
        param.Name = "@time_stamp";
        param.Layout = "${date}";
        target.Parameters.Add(param);
        
        param = new DatabaseParameterInfo();
        param.Name = "@level";
        param.Layout = "${level}";
        target.Parameters.Add(param);
        
        param = new DatabaseParameterInfo();
        param.Name = "@logger";
        param.Layout = "${logger}";
        target.Parameters.Add(param);
        
        param = new DatabaseParameterInfo();
        param.Name = "@message";
        param.Layout = "${message}";
        target.Parameters.Add(param);

        NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Debug);

        Logger logger = LogManager.GetLogger("Example");
        logger.Debug("log message");
    }
}

using NLog;
using NLog.Targets;

class Example
{
    static void Main(string[] args)
    {
        DatabaseTarget target = new DatabaseTarget();
        DatabaseParameterInfo param;

        target.DBProvider = "oledb";
        target.ConnectionString = "Provider=msdaora;Data Source=MYORACLEDB;User Id=DBO;Password=MYPASSWORD;";
        target.CommandText = "insert into LOGTABLE( TIME_STAMP,LOGLEVEL,LOGGER,CALLSITE,MESSAGE) values(?,?,?,?,?)";

        target.Parameters.Add(new DatabaseParameterInfo("TIME_STAMP", "${longdate}"));
        target.Parameters.Add(new DatabaseParameterInfo("LOGLEVEL", "${level:uppercase=true}"));
        target.Parameters.Add(new DatabaseParameterInfo("LOGGER", "${logger}"));
        target.Parameters.Add(new DatabaseParameterInfo("CALLSITE", "${callsite:filename=true}"));
        target.Parameters.Add(new DatabaseParameterInfo("MESSAGE", "${message}"));
        
        NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Debug);

        Logger logger = LogManager.GetLogger("Example");
        logger.Debug("log message");
    }
}

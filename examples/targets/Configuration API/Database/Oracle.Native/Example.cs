using NLog;
using NLog.Targets;

class Example
{
    static void Main(string[] args)
    {
        DatabaseTarget target = new DatabaseTarget();
        DatabaseParameterInfo param;

        target.DBProvider = "System.Data.OracleClient.OracleConnection,System.Data.OracleClient, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
        target.ConnectionString = "Data Source=MYORACLEDB;User Id=DBO;Password=MYPASSWORD;Integrated Security=no;";
        target.CommandText = "insert into LOGTABLE( TIME_STAMP,LOGLEVEL,LOGGER,CALLSITE,MESSAGE) values( :TIME_STAMP,:LOGLEVEL,:LOGGER,:CALLSITE,:MESSAGE)";

        target.KeepConnection = true;

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

var logmanager = new ActiveXObject("NLog.LogManager");
WScript.Echo("Loading config from file 'config8.nlog' with internal logging to console...");
logmanager.InternalLogToConsole = true;
logmanager.InternalLogFile = "internal_log.txt";
logmanager.InternalLogLevel = "Info";
logmanager.LoadConfigFromFile("config8.nlog");

var logger = new ActiveXObject("NLog.Logger");
logger.LoggerName = "TestLogger";
logger.Log("Debug", "This is a debugging message");
logger.Log("Info", "This is an information message");
logger.Log("Warn", "This is a warning message");
logger.Log("Error", "This is an error");
logger.Log("Fatal", "This is a fatal message");

logger.Debug("This is a debugging message");
logger.Info("This is an information message");
logger.Warn("This is a warning message");
logger.Error("This is an error");
logger.Fatal("This is a fatal message");

WScript.Echo("Loading config from file 'config8a.nlog' without internal logging...");
logmanager.InternalLogToConsole = false;
logmanager.InternalLogFile = "internal_log.txt";
logmanager.InternalLogLevel = "Fatal";
logmanager.LoadConfigFromFile("config8a.nlog");

logger.Log("Debug", "This is a debugging message");
logger.Log("Info", "This is an information message");
logger.Log("Warn", "This is a warning message");
logger.Log("Error", "This is an error");
logger.Log("Fatal", "This is a fatal message");

logger.Debug("This is a debugging message");
logger.Info("This is an information message");
logger.Warn("This is a warning message");
logger.Error("This is an error");
logger.Fatal("This is a fatal message");



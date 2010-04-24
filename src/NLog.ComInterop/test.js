var lm = new ActiveXObject("NLog.LogManager");
lm.InternalLogToConsole = false;
lm.LoadConfigFromFile("config.nlog");

var l = new ActiveXObject("NLog.Logger");
lm.GetLogger("A").Debug("b");

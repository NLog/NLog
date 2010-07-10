try
{
	runTests();
	WScript.Echo("PASSED");
	WScript.Quit(0);
}
catch (e)
{
	WScript.Echo("ERROR: " + e.description);
	WScript.Quit(1);
}

function runTests() {
    var nlogConfigPath = WScript.Arguments(0);
    WScript.Echo("Using configuration file: '" + nlogConfigPath + "'");

    var manager = new ActiveXObject("NLog.LogManager");
    manager.LoadConfigFromFile(nlogConfigPath);

    var logger = new ActiveXObject("NLog.Logger");
    logger.LoggerName = "Foo";

    if (logger.IsTraceEnabled) {
        logger.Info("tracemsg");
    }

    if (logger.IsDebugEnabled) {
        logger.Info("debugmsg");
    }

    if (logger.IsInfoEnabled) {
        logger.Info("infomsg");
    }

    if (logger.IsWarnEnabled) {
        logger.Info("warnmsg");
    }

    if (logger.IsErrorEnabled) {
        logger.Info("errormsg");
    }

    if (logger.IsFatalEnabled) {
        logger.Info("fatalmsg");
    }

    var levels = ["Trace", "Debug", "Info", "Warn", "Error", "Fatal"];

    for (var i = 0; i < levels.length; ++i) {
        var level = levels[i];

        if (logger.IsEnabled(level)) {
            logger.Log(level, "msg");
        }
    }

    var fso = new ActiveXObject("Scripting.FileSystemObject");
    var fileContents = fso.OpenTextFile("file.txt").ReadAll();
    var expectedFileContents = "";

    expectedFileContents += "tracemsg 1 Info Foo\r\n";
    expectedFileContents += "debugmsg 2 Info Foo\r\n";
    expectedFileContents += "infomsg 3 Info Foo\r\n";
    expectedFileContents += "warnmsg 4 Info Foo\r\n";
    expectedFileContents += "errormsg 5 Info Foo\r\n";
    expectedFileContents += "fatalmsg 6 Info Foo\r\n";
    expectedFileContents += "msg 7 Trace Foo\r\n";
    expectedFileContents += "msg 8 Debug Foo\r\n";
    expectedFileContents += "msg 9 Info Foo\r\n";
    expectedFileContents += "msg 10 Warn Foo\r\n";
    expectedFileContents += "msg 11 Error Foo\r\n";
    expectedFileContents += "msg 12 Fatal Foo\r\n";

    if (fileContents != expectedFileContents) {
        throw new Error("File contents mismatch. Expected '" + expectedFileContents + "', got '" + fileContents + "'.");
    }
}
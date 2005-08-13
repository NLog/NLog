using System;
using System.Globalization;

using NLog;

class MyClass {
    static Logger logger = LogManager.GetLogger("MyClass");

    static void Main()
    {
        // you can use an interface known from log4net
        logger.Trace("This is a trace message");
        logger.Debug("This is a debugging message");
        logger.Info("This is a information message");
        logger.Warn("This is a warning message");
        logger.Error("This is an error");
        logger.Fatal("This is a fatal error message");

        // you can ask if the logging is enabled before writing
        if (logger.IsDebugEnabled) {
            logger.Debug("Some debug info");
        }

        // you can use WriteLine() style formatting
        logger.Debug("The result is {0} {1}", 1 + 2, "zzz");

        // you can even pass IFormatProvider for maximum flexibility
        logger.Debug(CultureInfo.InvariantCulture, 
                "The current time is {0}", DateTime.Now);

        // you can ask if the logging is enabled for specified level
        if (logger.IsEnabled(LogLevel.Warn)) {
            // and you can write the message for a particular level, too
            logger.Log(LogLevel.Warn, "Some warning info");
        }
    }
}

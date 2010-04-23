using System;

using NLog;
using NLog.Targets;
using NLog.Targets.Wrappers;

class Example
{
    static void Main(string[] args)
    {
        try
        {
            NLog.Internal.InternalLogger.LogToConsole = true;
            NLog.Internal.InternalLogger.LogLevel = LogLevel.Trace;
            Console.WriteLine("Setting up the target...");
            MailTarget target = new MailTarget();

            target.SmtpServer = "192.168.0.15";
            target.From = "jaak@jkowalski.net";
            target.To = "jaak@jkowalski.net";
            target.Subject = "sample subject";
            target.Body = "${message}${newline}";

            BufferingTargetWrapper buffer = new BufferingTargetWrapper(target, 5);

            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(buffer, LogLevel.Debug);

            Console.WriteLine("Sending...");
            Logger logger = LogManager.GetLogger("Example");
            logger.Debug("log message 1");
            logger.Debug("log message 2");
            logger.Debug("log message 3");
            logger.Debug("log message 4");
            logger.Debug("log message 5");
            logger.Debug("log message 6");
            logger.Debug("log message 7");
            logger.Debug("log message 8");

            // this should send 2 mails - one with messages 1..5, the other with messages 6..8
            Console.WriteLine("Sent.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("EX: {0}", ex);
                
        }
    }
}

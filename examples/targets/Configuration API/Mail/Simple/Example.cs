using System;

using NLog;
using NLog.Config;
using NLog.Targets;

class Example
{
    static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("Setting up the target...");
            MailTarget target = new MailTarget();

            target.SmtpServer = "192.168.0.15";
            target.From = "jaak@jkowalski.net";
            target.To = "jaak@jkowalski.net";
            target.Subject = "sample subject";

            LoggingConfiguration nlogConfig = new LoggingConfiguration();
            nlogConfig.AddRuleForAllLevels(target);
            LogManager.Configuration = nlogConfig;

            Console.WriteLine("Sending...");
            Logger logger = LogManager.GetLogger("Example");
            Console.WriteLine("Sent.");
            logger.Debug("log message");
        }
        catch (Exception ex)
        {
            Console.WriteLine("EX: {0}", ex);
        }
    }
}

using System;
using System.IO;
using System.Xml;
using nDumbster.smtp;
using System.Collections.Specialized;

using NLogViewer.Configuration;
using NLogViewer.Receivers;
using NLogViewer.Events;

namespace NLogViewer.Receivers
{
    [LogEventReceiver("SMTP", "SMTP Event Receiver based on NDumbster", "Receives XML events from the mock SMTP server")]
    public class NDumbsterSmtpEventReceiver : LogEventReceiverSkeleton
    {
        private SimpleSmtpServer _smtpServer;
        private int _port = SimpleSmtpServer.DEFAULT_SMTP_PORT;

        public NDumbsterSmtpEventReceiver()
        {
        }

        public override void Configure(NameValueCollection parameters)
        {
            base.Configure(parameters);
            
            if (parameters["port"] != null)
            {
                _port = Convert.ToInt32(parameters["port"]);
            }

        }

        public override void Start()
        {
            _smtpServer = SimpleSmtpServer.Start(_port);
            base.Start ();
        }


        public override void InputThread()
        {
            while (!QuitInputThread)
            {
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));

                if (_smtpServer.ReceivedEmailCount > 0)
                {
                    foreach(SmtpMessage message in _smtpServer.ReceivedEmail)
                    {
                        StringReader sr = new StringReader(message.Body);
                        XmlTextReader reader = new XmlTextReader(sr);
                        reader.Namespaces = false;
                        LogEvent logEventInfo;
                        reader.Read();
                        logEventInfo = LogEvent.ParseLog4JEvent(reader);
                        logEventInfo.ReceivedTime = DateTime.Now;
					
                        foreach (string header in message.Headers.AllKeys)
                        {
                            LogEventProperty lep = new LogEventProperty();
                            lep.Name = header;
                            lep.Value = message.Headers[header];
                            logEventInfo.Properties.Add(lep);
                        }

                        EventReceived(logEventInfo);
                    }

                    _smtpServer.ClearReceivedEmail();
                }
            }
        }

        ~NDumbsterSmtpEventReceiver()
        {
            _smtpServer.Stop();
        }
    }
}

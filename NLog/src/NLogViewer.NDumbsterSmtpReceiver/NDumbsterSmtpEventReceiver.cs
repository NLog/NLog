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
    [LogEventReceiver("SMTP", 
        "SMTP Event Receiver based on NDumbster", 
        "Receives XML events from the mock SMTP server")]
    public class NDumbsterSmtpEventReceiver : LogEventReceiverSkeleton
    {
        private SimpleSmtpServer _smtpServer = null;
        private int _port = SimpleSmtpServer.DEFAULT_SMTP_PORT;

        public NDumbsterSmtpEventReceiver()
        {
        }

        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }

        public override void Start()
        {
            NLogViewerTrace.Write("Starting SMTP server on port {0}", _port);
            _smtpServer = SimpleSmtpServer.Start(_port);
            base.Start ();
        }


        public override void InputThread()
        {
            while (!QuitInputThread)
            {
                System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(100));

                if (_smtpServer.ReceivedEmailCount > 0)
                {
                    foreach(SmtpMessage message in _smtpServer.ReceivedEmail)
                    {
                        NLogViewerTrace.Write("Received mail: {0}", message.Body);
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

        public override void Stop()
        {
            NLogViewerTrace.Write("Stopping SMTP Server on port {0}", _port);
            base.Stop ();
            if (_smtpServer != null)
            {
                _smtpServer.Stop();
                _smtpServer = null;
            }
        }
    }
}

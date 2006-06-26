using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using NLogViewer.Parsers;
using NLogViewer.Events;
using System.Xml;
using NLogViewer.Configuration;
using System.Collections.Specialized;

namespace NLogViewer.Parsers
{
    [LogEventParser("XML", "XML", "Log4J-Compatible XML (NLog, log4xxx)")]
    public class Log4JXmlLogEventParser : ILogEventParser
    {
        class Context : ILogEventParserInstance
        {
            public XmlTextReader _xtr;

            public Context(XmlTextReader xtr)
            {
                _xtr = xtr;
            }

            public LogEvent ReadNext()
            {
                while (_xtr.Read())
                {
                    if (_xtr.NodeType == XmlNodeType.Element && _xtr.LocalName == "log4j:event")
                    {
                        LogEvent logEvent = ParseLog4JEvent(_xtr);
                        logEvent.Properties["Received Time"] = DateTime.Now;
                        return logEvent;
                    }
                }
                return null;
            }

            public void Dispose()
            {
            }
        }

        public ILogEventParserInstance Begin(Stream stream)
        {
            //
            // a trick to handle multiple-root xml streams
            // as described by Oleg Tkachenko in his blog:
            //
            // http://www.tkachenko.com/blog/archives/000053.html
            //

            XmlParserContext context = new XmlParserContext(new NameTable(), null, null, XmlSpace.Default);
            XmlTextReader xtr = new XmlTextReader(stream, XmlNodeType.Element, context);
            xtr.Namespaces = false;
            return new Context(xtr);
        }

        public void Configure(NameValueCollection parameters)
        {
        }


        private static void ParseLog4JProperties(LogEvent ev, XmlTextReader reader, string namePrefix)
        {
            if (reader.IsEmptyElement)
                return;

            string elementName = reader.LocalName;

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == elementName)
                {
                    break;
                }
                if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "log4j:data")
                {
                    string name = reader.GetAttribute("name");
                    string value = reader.GetAttribute("value");

                    if (name == "log4japp")
                    {
                        ev["SourceApplication"] = value;
                        continue;
                    }

                    if (name == "log4jmachinename")
                    {
                        ev["SourceMachine"] = value;
                        continue;
                    }

                    ev.Properties[namePrefix + name] = value;
                }
            }
        }

        private static DateTime _log4jDateBase = new DateTime(1970, 1, 1);

        private static LogEvent ParseLog4JEvent(XmlTextReader reader)
        {
            LogEvent ev = new LogEvent();
            ev["Logger"] = reader.GetAttribute("logger");
            ev["Level"] = LogLevelMap.GetLevelForName(reader.GetAttribute("level"));
            ev["Thread"] = reader.GetAttribute("thread");
            ev.Properties["Time"] = _log4jDateBase.AddMilliseconds(Convert.ToDouble(reader.GetAttribute("timestamp"))).ToLocalTime();

            // System.Windows.Forms.MessageBox.Show(reader.ReadOuterXml());
            // Log.Write(reader.ReadOuterXml());

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.LocalName == "log4j:event")
                        return ev;
                }
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.LocalName)
                    {
                        case "log4j:message":
                            ev["Text"] = reader.ReadString();
                            continue;

                        case "log4j:properties":
                            ParseLog4JProperties(ev, reader, "");
                            continue;

                        case "log4j:MDC":
                            ParseLog4JProperties(ev, reader, "mdc:");
                            continue;

                        default:
                        case "log4j:locationinfo":
                            ev["SourceType"] = reader.GetAttribute("class");
                            ev["SourceMethod"] = reader.GetAttribute("method");
                            ev["SourceFile"] = reader.GetAttribute("file");
                            //ev.SourceLine = reader.GetAttribute("line");
                            continue;

                        case "nlog:locationinfo":
                            ev["SourceAssembly"] = reader.GetAttribute("assembly");
                            break;
                    }
                }
            }

            return ev;
        }
    }
}

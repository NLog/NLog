using System;
using System.Xml;
using System.Xml.Serialization;
using System.Collections;

namespace NLog.Viewer
{
    [XmlRoot("loginstance")]
	public class LogInstanceConfigurationInfo
	{
        private bool _dirty;

        [XmlElement("name")]
        public string Name;

        [XmlArray("columns")]
        [XmlArrayItem("column")]
        public LogColumnInfo[] Columns;

        [XmlIgnore]
        public bool Dirty
        {
            get { return _dirty; }
            set { _dirty = value; }
        }

        [XmlArray("loggers")]
        [XmlArrayItem("logger", typeof(LoggerConfigInfo))]
        public ArrayList loggerConfigInfos = new ArrayList();

        private Hashtable _loggerName2LoggerConfigInfo;

        public LoggerConfigInfo GetLoggerConfigInfo(string loggerName)
        {
            lock (this)
            {
                if (_loggerName2LoggerConfigInfo == null)
                {
                    _loggerName2LoggerConfigInfo = new Hashtable();
                }
                return (LoggerConfigInfo)_loggerName2LoggerConfigInfo[loggerName];
            }
        }

        public void AddLoggerConfigInfo(LoggerConfigInfo loggerConfigInfo)
        {
            lock (this)
            {
                _loggerName2LoggerConfigInfo[loggerConfigInfo.Name] = loggerConfigInfo;
                loggerConfigInfos.Add(loggerConfigInfo.Name);
            }
        }
	}
}

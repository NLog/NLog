using System;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;

namespace NLog.Viewer
{
    [XmlRoot("event", Namespace="http://nlog.sourceforge.net/NLogSchema.xsd")]
    [XmlType("event")]
	public struct LogEventInfo
	{
        private long _sendTimeTicks;
        private DateTime _receivedTime;
        private string _logger;
        private string _level;
        private string _message;
        private string _stackTrace;
        private string _sourceAssembly;
        private string _sourceType;
        private string _sourceMethod;
        private string _sourceFile;
        private int _sourceLine;
        private int _sourceColumn;
        private int _threadID;
        private int _processID;
        private string _threadName;

        [XmlIgnore]
        public DateTime SendTime
        {
            get { return new DateTime(_sendTimeTicks); }
            set { _sendTimeTicks = value.Ticks; }
        }

        [XmlElement("sendTime")]
        public long SendTimeTicks
        {
            get { return _sendTimeTicks; }
            set { _sendTimeTicks = value; }
        }

        [XmlIgnore]
        public DateTime ReceivedTime
        {
            get { return _receivedTime; }
            set { _receivedTime = value; }
        }

        [XmlElement("logger")]
        public string Logger
        {
            get { return _logger; }
            set { _logger = value; }
        }

        [XmlElement("level")]
        public string Level
        {
            get { return _level; }
            set { _level = value; }
        }

        [XmlElement("message")]
        public string MessageText
        {
            get { return _message; }
            set { _message = value; }
        }

        public string StackTrace
        {
            get { return _stackTrace; }
            set { _stackTrace = value; }
        }

        [XmlElement("assembly")]
        public string SourceAssembly
        {
            get { return _sourceAssembly; }
            set { _sourceAssembly = value; }
        }

        [XmlElement("sourceType")]
        public string SourceType
        {
            get { return _sourceType; }
            set { _sourceType = value; }
        }

        [XmlElement("sourceMethod")]
        public string SourceMethod
        {
            get { return _sourceMethod; }
            set { _sourceMethod = value; }
        }

        [XmlElement("sourceFile")]
        public string SourceFile
        {
            get { return _sourceFile; }
            set { _sourceFile = value; }
        }

        [XmlElement("sourceLine")]
        [DefaultValue(0)]
        public int SourceLine
        {
            get { return _sourceLine; }
            set { _sourceLine = value; }
        }

        [XmlElement("sourceColumn")]
        [DefaultValue(0)]
        public int SourceColumn
        {
            get { return _sourceColumn; }
            set { _sourceColumn = value; }
        }

        [XmlElement("threadName")]
        public string ThreadName
        {
            get { return _threadName; }
            set { _threadName = value; }
        }

        [XmlElement("threadID")]
        [DefaultValue(0)]
        public int ThreadID
        {
            get { return _threadID; }
            set { _threadID = value; }
        }

        [XmlElement("processID")]
        [DefaultValue(0)]
        public int ProcessID
        {
            get { return _processID; }
            set { _processID = value; }
        }

        [XmlArray("extra")]
        public LogEventExtraInfo[] ExtraInfo;
    }
}

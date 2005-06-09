// 
// Copyright (c) 2004,2005 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
// 
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

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

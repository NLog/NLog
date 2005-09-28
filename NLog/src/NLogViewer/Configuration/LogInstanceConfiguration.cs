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
using System.Collections;

using System.IO;

namespace NLogViewer.Configuration
{
    [XmlRoot("loginstance")]
	public class LogInstanceConfiguration
	{
        private bool _dirty;

        [XmlIgnore]
        public string FileName;

        [XmlElement("name")]
        public string Name;

        [XmlArray("columns")]
        [XmlArrayItem("column")]
        public LogColumnCollection Columns = new LogColumnCollection();

        [XmlIgnore]
        public bool Dirty
        {
            get { return _dirty; }
            set { _dirty = value; }
        }

        [XmlElement("max-log-entries")]
        [System.ComponentModel.DefaultValue(1000)]
        public int MaxLogEntries = 1000;

        [XmlElement("sortBy")]
        [System.ComponentModel.DefaultValue("ID")]
        public string OrderBy = "ID";

        [XmlElement("sortAscending")]
        [System.ComponentModel.DefaultValue(true)]
        public bool SortAscending = true;

        [XmlElement("receiver-type")]
        public string ReceiverType;

        [XmlArray("receiver-parameters")]
        [XmlArrayItem("param", typeof(ReceiverParameter))]
        public ReceiverParameterCollection ReceiverParameters = new ReceiverParameterCollection();

        [XmlArray("loggers")]
        [XmlArrayItem("logger", typeof(LoggerConfig))]
        public LoggerConfigCollection Loggers = new LoggerConfigCollection();

        private StringToLoggerConfigMap _loggerName2LoggerConfig;

        public LoggerConfig GetLoggerConfig(string loggerName)
        {
            lock (this)
            {
                if (_loggerName2LoggerConfig == null)
                {
                    _loggerName2LoggerConfig = new StringToLoggerConfigMap();
                }
                return (LoggerConfig)_loggerName2LoggerConfig[loggerName];
            }
        }

        public void AddLoggerConfig(LoggerConfig lc)
        {
            lock (this)
            {
                _loggerName2LoggerConfig[lc.Name] = lc;
                Loggers.Add(lc);
            }
        }

        private void Resolve()
        {
            if (Columns.Count == 0)
            {
                Columns.Add(new LogColumn("ID", 60));
                Columns.Add(new LogColumn("Time", 120));
                Columns.Add(new LogColumn("Logger", 200));
                Columns.Add(new LogColumn("Level", 50));
                Columns.Add(new LogColumn("Text", 300));
            }
        }

        private static XmlSerializer _serializer = new XmlSerializer(typeof(LogInstanceConfiguration));

        public void Save()
        {
            using (FileStream fs = File.OpenWrite(this.FileName))
            {
                _serializer.Serialize(fs, this);
            }
        }

        public static LogInstanceConfiguration Load(string fileName)
        {
            using (FileStream fs = File.OpenRead(fileName))
            {
                LogInstanceConfiguration c = (LogInstanceConfiguration)_serializer.Deserialize(fs);
                c.FileName = fileName;
                c.Resolve();
                return c;
            }
        }
    }
}

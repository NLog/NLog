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
using System.ComponentModel;
using System.Collections;

namespace NLog.Viewer
{
	public class LogEventInfo
	{
        private DateTime _sentTime;
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
        private string _thread;

        public DateTime SentTime
        {
            get { return _sentTime; }
            set { _sentTime = value; }
        }

        public DateTime ReceivedTime
        {
            get { return _receivedTime; }
            set { _receivedTime = value; }
        }

        public string Logger
        {
            get { return _logger; }
            set { _logger = value; }
        }

        public string Level
        {
            get { return _level; }
            set { _level = value; }
        }

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

        public string SourceAssembly
        {
            get { return _sourceAssembly; }
            set { _sourceAssembly = value; }
        }

        public string SourceType
        {
            get { return _sourceType; }
            set { _sourceType = value; }
        }

        public string SourceMethod
        {
            get { return _sourceMethod; }
            set { _sourceMethod = value; }
        }

        public string SourceFile
        {
            get { return _sourceFile; }
            set { _sourceFile = value; }
        }

        public int SourceLine
        {
            get { return _sourceLine; }
            set { _sourceLine = value; }
        }

        public int SourceColumn
        {
            get { return _sourceColumn; }
            set { _sourceColumn = value; }
        }

        public string Thread
        {
            get { return _thread; }
            set { _thread = value; }
        }


        public ArrayList ExtraInfo = new ArrayList();

        public static void ParseLog4JProperties(LogEventInfo ev, XmlTextReader reader, string namePrefix)
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

                    LogEventExtraInfo ei = new LogEventExtraInfo();
                    ei.Name = namePrefix + name;
                    ei.Value = value;
                    ev.ExtraInfo.Add(ei);

                }
            }
        }

        private static DateTime _log4jDateBase = new DateTime(1970, 1, 1);

        public static LogEventInfo ParseLog4JEvent(XmlTextReader reader)
        {
            LogEventInfo ev = new LogEventInfo();
            ev.Logger = reader.GetAttribute("logger");
            ev.Level = reader.GetAttribute("level");
            ev.Thread = reader.GetAttribute("thread");
            ev.SentTime = _log4jDateBase.AddMilliseconds(Convert.ToDouble(reader.GetAttribute("timestamp"))).ToLocalTime();

            // System.Windows.Forms.MessageBox.Show(reader.ReadOuterXml());

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.LocalName)
                    {
                        case "log4j:message":
                            ev.MessageText = reader.ReadString();
                            continue;

                        case "log4j:properties":
                            ParseLog4JProperties(ev, reader, "");
                            continue;

                        case "log4j:MDC":
                            ParseLog4JProperties(ev, reader, "mdc:");
                            continue;
                    }
                }
            }

            return ev;
        }
    }
}

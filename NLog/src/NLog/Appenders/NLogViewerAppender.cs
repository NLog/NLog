// 
// Copyright (c) 2004 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
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
using System.IO;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Diagnostics;

using NLog.Internal;
using System.Net;
using System.Net.Sockets;

namespace NLog.Appenders
{
    [Appender("NLogViewer")]
    public sealed class NLogViewerAppender: Appender
    {
        private Socket _socket;
        private IPEndPoint _endpoint = null;

        private bool _includeCallSite = false;
        private bool _includeSourceInfo = false;
        private string _appInfo;
        private int _port;

        public NLogViewerAppender()
        {
            _appInfo = AppDomain.CurrentDomain.FriendlyName;
        }

        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }

        public string AppInfo
        {
            get { return _appInfo; }
            set { _appInfo = value; }
        }

        public bool IncludeCallSite
        {
            get { return _includeCallSite; }
            set { _includeCallSite = value; }
        }

        public bool IncludeSourceInfo
        {
            get { return _includeSourceInfo; }
            set { _includeSourceInfo = value; }
        }

        protected internal override int NeedsStackTrace()
        {
            if (IncludeSourceInfo)
                return 2;
            if (IncludeCallSite)
                return 1;
            return 0;
        }


        protected internal override void Append(LogEventInfo ev)
        {
            StringBuilder sb = new StringBuilder(512);
            StringWriter sw = new StringWriter();

            XmlTextWriter xtw = new XmlTextWriter(sw);
            xtw.Formatting = Formatting.Indented;

            xtw.WriteStartElement("event", "http://nlog.sourceforge.net/NLogSchema.xsd");
            xtw.WriteElementString("sendTime", DateTime.Now.Ticks.ToString());
            xtw.WriteElementString("appInfo", AppInfo);
            xtw.WriteElementString("message", ev.FormattedMessage);
            xtw.WriteElementString("logger", ev.LoggerName);
            xtw.WriteElementString("level", ev.Level.ToString());
            xtw.WriteElementString("processId", ThreadIDHelper.CurrentProcessID.ToString());
            xtw.WriteElementString("threadId", ThreadIDHelper.CurrentThreadID.ToString());
            xtw.WriteElementString("threadName", System.Threading.Thread.CurrentThread.Name);

            if (IncludeCallSite)
            {
                System.Diagnostics.StackFrame frame = ev.UserStackFrame;
                MethodBase methodBase = frame.GetMethod();
                Type type = methodBase.DeclaringType;

                xtw.WriteElementString("assembly", type.Assembly.FullName);
                xtw.WriteElementString("sourceType", type.FullName);
                xtw.WriteElementString("sourceMethod", methodBase.ToString());
                if (IncludeSourceInfo)
                {
                    xtw.WriteElementString("sourceFile", frame.GetFileName());
                    xtw.WriteElementString("sourceLine", frame.GetFileLineNumber().ToString());
                    xtw.WriteElementString("sourceColumn", frame.GetFileColumnNumber().ToString());
                }
            }
            xtw.WriteEndElement();
            xtw.Flush();
            byte[] data = System.Text.Encoding.UTF8.GetBytes(sw.ToString());

            if (data.Length <= 64000)
            {
                SendToViewer(data);
            }
        }

        private void SendToViewer(byte[] buffer)
        {
            if (_socket == null)
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _endpoint = new IPEndPoint(IPAddress.Loopback, 40000);
            }
            _socket.SendTo(buffer, _endpoint);
        }
    }
}

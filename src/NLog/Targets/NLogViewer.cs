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
using System.IO;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Diagnostics;

using NLog.Internal;
using System.Net;
using System.Net.Sockets;

using NLog.Config;

namespace NLog.Targets
{
    /// <summary>
    /// Sends logging messages to the remote instance of NLog Viewer. 
    /// NOT OPERATIONAL YET.
    /// </summary>
    [Target("NLogViewer", IgnoresLayout=true)]
    public class NLogViewerTarget: NetworkTarget
    {
        private string _appInfo;
        private bool _includeMDC = false;
        private bool _includeNDC = false;
        private static DateTime _log4jDateBase = new DateTime(1970, 1, 1);
        private NLogViewerParameterInfoCollection _parameters = new NLogViewerParameterInfoCollection();

        /// <summary>
        /// Include NLog-specific extensions to log4j schema.
        /// </summary>
        public bool IncludeNLogData = true;

        /// <summary>
        /// Creates a new instance of the <see cref="NLogViewerTarget"/> 
        /// and initializes default property values.
        /// </summary>
        public NLogViewerTarget()
        {
#if NETCF
            _appInfo = ".NET CF Application";
#else
            _appInfo = String.Format("{0}({1})", AppDomain.CurrentDomain.FriendlyName,
                NLog.Internal.ThreadIDHelper.CurrentProcessID);
#endif
        }

        /// <summary>
        /// The AppInfo field. By default it's the friendly name of the current AppDomain.
        /// </summary>
        public string AppInfo
        {
            get { return _appInfo; }
            set { _appInfo = value; }
        }

#if !NETCF
        private bool _includeCallSite = false;
        private bool _includeSourceInfo = false;

        /// <summary>
        /// Include call site (class and method name) in the information sent over the network.
        /// </summary>
        public bool IncludeCallSite
        {
            get { return _includeCallSite; }
            set { _includeCallSite = value; }
        }

        /// <summary>
        /// Include source info (file name and line number) in the information sent over the network.
        /// </summary>
        public bool IncludeSourceInfo
        {
            get { return _includeSourceInfo; }
            set { _includeSourceInfo = value; }
        }

        /// <summary>
        /// Returns the value indicating whether call site and/or source information should be gathered.
        /// </summary>
        /// <returns>2 - when IncludeSourceInfo is set, 1 when IncludeCallSite is set, 0 otherwise</returns>
        protected internal override int NeedsStackTrace()
        {
            if (IncludeSourceInfo)
                return 2;
            if (IncludeCallSite)
                return 1;

            int max = 0;
            for (int i = 0; i < Parameters.Count; ++i)
            {
                max = Math.Max(max, Parameters[i].NeedsStackTrace());
                if (max == 2)
                    break;
            }

            return max;
        }
#endif

        /// <summary>
        /// Include MDC dictionary in the information sent over the network.
        /// </summary>
        public bool IncludeMDC
        {
            get { return _includeMDC; }
            set { _includeMDC = value; }
        }

        /// <summary>
        /// Include NDC stack.
        /// </summary>
        public bool IncludeNDC
        {
            get { return _includeNDC; }
            set { _includeNDC = value; }
        }
        /// <summary>
        /// The collection of paramters. Each parameter contains a mapping
        /// between NLog layout and a database named or positional parameter.
        /// </summary>
        [ArrayParameter(typeof(NLogViewerParameterInfo), "parameter")]
        public NLogViewerParameterInfoCollection Parameters
        {
            get
            {
                return _parameters;
            }
        }

        /// <summary>
        /// Constructs an XML packet including the logging event information and sends it over the network.
        /// </summary>
        /// <param name="ev">Logging event information.</param>
        protected internal override void Append(LogEventInfo ev)
        {
            StringBuilder sb = new StringBuilder(512);
            StringWriter sw = new StringWriter();

            XmlTextWriter xtw = new XmlTextWriter(sw);
            xtw.Formatting = Formatting.Indented;

            xtw.WriteStartElement("log4j:event");
            xtw.WriteAttributeString("logger", ev.LoggerName);
            xtw.WriteAttributeString("level", ev.Level.Name.ToUpper());
            xtw.WriteAttributeString("timestamp", Convert.ToString((long)(ev.TimeStamp.ToUniversalTime() - _log4jDateBase).TotalMilliseconds));
#if !NETCF
            xtw.WriteAttributeString("thread", NLog.Internal.ThreadIDHelper.CurrentThreadID.ToString());
#else
            xtw.WriteElementString("thread", "");
#endif

            xtw.WriteElementString("log4j:message", ev.FormattedMessage);
            if (IncludeNDC)
            {
                xtw.WriteElementString("log4j:NDC", NDC.GetAllMessages(" "));
            }
#if !NETCF
            if (IncludeCallSite || IncludeSourceInfo)
            {
                System.Diagnostics.StackFrame frame = ev.UserStackFrame;
                MethodBase methodBase = frame.GetMethod();
                Type type = methodBase.DeclaringType;

                xtw.WriteStartElement("log4j:locationinfo");
                xtw.WriteAttributeString("class", type.FullName);
                xtw.WriteAttributeString("method", methodBase.ToString());
                if (IncludeSourceInfo)
                {
                    xtw.WriteAttributeString("file", frame.GetFileName());
                    xtw.WriteAttributeString("line", frame.GetFileLineNumber().ToString());
                }
                xtw.WriteEndElement();

                if (IncludeNLogData)
                {
                    xtw.WriteStartElement("nlog:locationinfo");
                    xtw.WriteAttributeString("assembly", type.Assembly.FullName);
                    xtw.WriteEndElement();
                }
            }
#endif
            xtw.WriteStartElement("log4j:properties");
            if (IncludeMDC)
            {
                foreach (System.Collections.DictionaryEntry entry in MDC.GetThreadDictionary())
                {
                    xtw.WriteStartElement("log4j:data");
                    xtw.WriteAttributeString("name", Convert.ToString(entry.Key));
                    xtw.WriteAttributeString("value", Convert.ToString(entry.Value));
                    xtw.WriteEndElement();
                }
            }
            foreach (NLogViewerParameterInfo parameter in Parameters)
            {
                xtw.WriteStartElement("log4j:data");
                xtw.WriteAttributeString("name", parameter.Name);
                xtw.WriteAttributeString("value", parameter.CompiledLayout.GetFormattedMessage(ev));
                xtw.WriteEndElement();
            }

            xtw.WriteStartElement("log4j:data");
            xtw.WriteAttributeString("name", "log4japp");
            xtw.WriteAttributeString("value", AppInfo);
            xtw.WriteEndElement();

            xtw.WriteStartElement("log4j:data");
            xtw.WriteAttributeString("name", "log4jmachinename");
#if NETCF
                xtw.WriteAttributeString("value", "netcf");
#else
            xtw.WriteAttributeString("value", NLog.LayoutRenderers.MachineNameLayoutRenderer.MachineName);
#endif
            xtw.WriteEndElement();
            xtw.WriteEndElement();

            xtw.WriteEndElement();
            xtw.Flush();
            // Console.WriteLine(sw.ToString());
            NetworkSend(AddressLayout.GetFormattedMessage(ev), sw.ToString());
        }
    }
}

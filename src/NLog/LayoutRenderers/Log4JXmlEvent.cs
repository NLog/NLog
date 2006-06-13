// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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
// * Neither the name of Jaroslaw Kowalski nor the names of its 
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
using System.Text;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Reflection;

using NLog.Config;
using NLog.Targets;

namespace NLog.LayoutRenderers
{
    /// <summary>
    /// XML event description compatible with log4j, Chainsaw and NLogViewer
    /// </summary>
    [LayoutRenderer("log4jxmlevent",UsingLogEventInfo=true)]
    public class Log4JXmlEventLayoutRenderer: LayoutRenderer
    {
        private string _appInfo;
        private bool _includeMDC = false;
        private bool _includeNDC = false;
        private bool _includeNLogData = true;
        private bool _indentXml = false;
        private static DateTime _log4jDateBase = new DateTime(1970, 1, 1);
        private NLogViewerParameterInfoCollection _parameters = new NLogViewerParameterInfoCollection();

        /// <summary>
        /// Creates a new instance of <see cref="Log4JXmlEventLayoutRenderer"/> and initializes default values.
        /// </summary>
        public Log4JXmlEventLayoutRenderer()
        {
#if NETCF
            AppInfo = ".NET CF Application";
#else
            AppInfo = String.Format("{0}({1})", AppDomain.CurrentDomain.FriendlyName, NLog.Internal.ThreadIDHelper.Instance.CurrentProcessID);
#endif
        }

        /// <summary>
        /// Include NLog-specific extensions to log4j schema.
        /// </summary>
        [System.ComponentModel.DefaultValue(true)]
        public bool IncludeNLogData
        {
            get { return _includeNLogData; }
            set { _includeNLogData = value; }
        }

        /// <summary>
        /// Whether the XML should use spaces for indentation.
        /// </summary>
        public bool IndentXml
        {
            get { return _indentXml; }
            set { _indentXml = value; }
        }

        /// <summary>
        /// Returns the estimated number of characters that are needed to
        /// hold the rendered value for the specified logging event.
        /// </summary>
        /// <param name="logEvent">Logging event information.</param>
        /// <returns>The number of characters.</returns>
        /// <remarks>
        /// If the exact number is not known or
        /// expensive to calculate this function should return a rough estimate
        /// that's big enough in most cases, but not too big, in order to conserve memory.
        /// </remarks>
        protected internal override int GetEstimatedBufferSize(LogEventInfo logEvent)
        {
            return 512;
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

        internal NLogViewerParameterInfoCollection Parameters
        {
            get { return _parameters; }
            set { _parameters = value; }
        }

        /// <summary>
        /// Renders the XML logging event and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected internal override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            StringWriter sw = new StringWriter(builder);
            XmlTextWriter xtw = new XmlTextWriter(sw);
            if (IndentXml)
                xtw.Formatting = Formatting.Indented;

            xtw.WriteStartElement("log4j:event");
            xtw.WriteAttributeString("logger", logEvent.LoggerName);
            xtw.WriteAttributeString("level", logEvent.Level.Name.ToUpper());
            xtw.WriteAttributeString("timestamp", Convert.ToString((long)(logEvent.TimeStamp.ToUniversalTime() - _log4jDateBase).TotalMilliseconds));
#if !NETCF
            xtw.WriteAttributeString("thread", NLog.Internal.ThreadIDHelper.Instance.CurrentThreadID.ToString());
#else
            xtw.WriteElementString("thread", "");
#endif

            xtw.WriteElementString("log4j:message", logEvent.FormattedMessage);
            if (IncludeNDC)
            {
                xtw.WriteElementString("log4j:NDC", NDC.GetAllMessages(" "));
            }
#if !NETCF
            if (IncludeCallSite || IncludeSourceInfo)
            {
                System.Diagnostics.StackFrame frame = logEvent.UserStackFrame;
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
                    xtw.WriteElementString("nlog:eventSequenceNumber", logEvent.SequenceID.ToString());
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
                xtw.WriteAttributeString("value", parameter.CompiledLayout.GetFormattedMessage(logEvent));
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
        }
    }
}

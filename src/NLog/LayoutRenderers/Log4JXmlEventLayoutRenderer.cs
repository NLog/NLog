// 
// Copyright (c) 2004-2009 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using NLog.Contexts;
using NLog.Internal;
using NLog.Targets;

namespace NLog.LayoutRenderers
{
    /// <summary>
    /// XML event description compatible with log4j, Chainsaw and NLogViewer.
    /// </summary>
    [LayoutRenderer("log4jxmlevent")]
    public class Log4JXmlEventLayoutRenderer : LayoutRenderer
    {
        private static DateTime log4jDateBase = new DateTime(1970, 1, 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4JXmlEventLayoutRenderer" /> class.
        /// </summary>
        public Log4JXmlEventLayoutRenderer()
        {
            this.IncludeNLogData = true;
#if NET_CF
            this.AppInfo = ".NET CF Application";
#elif SILVERLIGHT
            this.AppInfo = "Silverlight Application";
#else
            this.AppInfo = String.Format(
                CultureInfo.InvariantCulture,
                "{0}({1})", 
                AppDomain.CurrentDomain.FriendlyName, 
                ThreadIDHelper.Instance.CurrentProcessID);
#endif
            this.Parameters = new List<NLogViewerParameterInfo>();
        }

        /// <summary>
        /// Gets or sets a value indicating whether to include NLog-specific extensions to log4j schema.
        /// </summary>
        [DefaultValue(true)]
        public bool IncludeNLogData { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the XML should use spaces for indentation.
        /// </summary>
        public bool IndentXml { get; set; }

        /// <summary>
        /// Gets or sets the AppInfo field. By default it's the friendly name of the current AppDomain.
        /// </summary>
        public string AppInfo { get; set; }

#if !NET_CF
        /// <summary>
        /// Gets or sets a value indicating whether to include call site (class and method name) in the information sent over the network.
        /// </summary>
        public bool IncludeCallSite { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include source info (file name and line number) in the information sent over the network.
        /// </summary>
        public bool IncludeSourceInfo { get; set; }
#endif

        /// <summary>
        /// Gets or sets a value indicating whether to include contents of the <see cref="MappedDiagnosticsContext"/> dictionary.
        /// </summary>
        public bool IncludeMDC { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include contents of the <see cref="NestedDiagnosticsContext"/> stack.
        /// </summary>
        public bool IncludeNDC { get; set; }

        internal ICollection<NLogViewerParameterInfo> Parameters { get; set; }

        /// <summary>
        /// Renders the XML logging event and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected internal override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            StringWriter sw = new StringWriter(builder);
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = this.IndentXml;
            XmlWriter xtw = XmlWriter.Create(sw, settings);

            xtw.WriteStartElement("log4j:event");
            xtw.WriteAttributeString("logger", logEvent.LoggerName);
            xtw.WriteAttributeString("level", logEvent.Level.Name.ToUpper(CultureInfo.InvariantCulture));
            xtw.WriteAttributeString("timestamp", Convert.ToString((long)(logEvent.TimeStamp.ToUniversalTime() - log4jDateBase).TotalMilliseconds));
            xtw.WriteAttributeString("thread", System.Threading.Thread.CurrentThread.ManagedThreadId.ToString());

            xtw.WriteElementString("log4j:message", logEvent.FormattedMessage);
            if (this.IncludeNDC)
            {
                xtw.WriteElementString("log4j:NDC", String.Join(" ", NestedDiagnosticsContext.GetAllMessages()));
            }
#if !NET_CF
            if (this.IncludeCallSite || this.IncludeSourceInfo)
            {
                System.Diagnostics.StackFrame frame = logEvent.UserStackFrame;
                MethodBase methodBase = frame.GetMethod();
                Type type = methodBase.DeclaringType;

                xtw.WriteStartElement("log4j:locationinfo");
                if (type != null)
                {
                    xtw.WriteAttributeString("class", type.FullName);
                }

                xtw.WriteAttributeString("method", methodBase.ToString());
                if (this.IncludeSourceInfo)
                {
                    xtw.WriteAttributeString("file", frame.GetFileName());
                    xtw.WriteAttributeString("line", frame.GetFileLineNumber().ToString(CultureInfo.InvariantCulture));
                }

                xtw.WriteEndElement();

                if (this.IncludeNLogData)
                {
                    xtw.WriteElementString("nlog:eventSequenceNumber", logEvent.SequenceID.ToString(CultureInfo.InvariantCulture));
                    xtw.WriteStartElement("nlog:locationinfo");
                    xtw.WriteAttributeString("assembly", type.Assembly.FullName);
                    xtw.WriteEndElement();
                }
            }
#endif
            xtw.WriteStartElement("log4j:properties");
            if (this.IncludeMDC)
            {
                foreach (KeyValuePair<string, string> entry in MappedDiagnosticsContext.ThreadDictionary)
                {
                    xtw.WriteStartElement("log4j:data");
                    xtw.WriteAttributeString("name", Convert.ToString(entry.Key));
                    xtw.WriteAttributeString("value", Convert.ToString(entry.Value));
                    xtw.WriteEndElement();
                }
            }

            foreach (NLogViewerParameterInfo parameter in this.Parameters)
            {
                xtw.WriteStartElement("log4j:data");
                xtw.WriteAttributeString("name", parameter.Name);
                xtw.WriteAttributeString("value", parameter.Layout.GetFormattedMessage(logEvent));
                xtw.WriteEndElement();
            }

            xtw.WriteStartElement("log4j:data");
            xtw.WriteAttributeString("name", "log4japp");
            xtw.WriteAttributeString("value", this.AppInfo);
            xtw.WriteEndElement();

            xtw.WriteStartElement("log4j:data");
            xtw.WriteAttributeString("name", "log4jmachinename");
#if NET_CF
            xtw.WriteAttributeString("value", "netcf");
#elif SILVERLIGHT
            xtw.WriteAttributeString("value", "silverlight");
#else
            xtw.WriteAttributeString("value", NLog.LayoutRenderers.MachineNameLayoutRenderer.MachineName);
#endif
            xtw.WriteEndElement();
            xtw.WriteEndElement();

            xtw.WriteEndElement();
            xtw.Flush();
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
        /// Determines whether the layout renderer is volatile.
        /// </summary>
        /// <returns>
        /// A boolean indicating whether the layout renderer is volatile.
        /// </returns>
        /// <remarks>
        /// Volatile layout renderers are dependent on information not contained
        /// in <see cref="LogEventInfo"/> (such as thread-specific data, MDC data, NDC data).
        /// </remarks>
        protected internal override bool IsVolatile()
        {
            return false;
        }
    }
}

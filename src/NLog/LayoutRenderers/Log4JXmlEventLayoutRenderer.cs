// 
// Copyright (c) 2004-2017 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.LayoutRenderers
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Text;
    using System.Xml;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Internal.Fakeables;
    using NLog.Layouts;
    using NLog.Targets;

    /// <summary>
    /// XML event description compatible with log4j, Chainsaw and NLogViewer.
    /// </summary>
    [LayoutRenderer("log4jxmlevent")]
    public class Log4JXmlEventLayoutRenderer : LayoutRenderer, IUsesStackTrace, IIncludeContext
    {
        private static readonly DateTime log4jDateBase = new DateTime(1970, 1, 1);

        private static readonly string dummyNamespace = "http://nlog-project.org/dummynamespace/" + Guid.NewGuid();
        private static readonly string dummyNamespaceRemover = " xmlns:log4j=\"" + dummyNamespace + "\"";

        private static readonly string dummyNLogNamespace = "http://nlog-project.org/dummynamespace/" + Guid.NewGuid();
        private static readonly string dummyNLogNamespaceRemover = " xmlns:nlog=\"" + dummyNLogNamespace + "\"";

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4JXmlEventLayoutRenderer" /> class.
        /// </summary>
        public Log4JXmlEventLayoutRenderer() : this(LogFactory.CurrentAppDomain)
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Log4JXmlEventLayoutRenderer" /> class.
        /// </summary>
        public Log4JXmlEventLayoutRenderer(IAppDomain appDomain)
        {
            IncludeNLogData = true; // TODO NLog ver. 5 - Disable this by default, as mostly duplicate data is added
            NdcItemSeparator = " ";
#if !SILVERLIGHT
            NdlcItemSeparator = " ";
#endif

#if SILVERLIGHT
            this.AppInfo = "Silverlight Application";
#elif __IOS__
			this.AppInfo = "MonoTouch Application";
#else
            AppInfo = string.Format(
                CultureInfo.InvariantCulture,
                "{0}({1})", 
                appDomain.FriendlyName, 
                ThreadIDHelper.Instance.CurrentProcessID);
#endif

            Parameters = new List<NLogViewerParameterInfo>();

            try
            {
#if SILVERLIGHT
                this._machineName = "silverlight";
#else
                _machineName = Environment.MachineName;
#endif
            }
            catch (System.Security.SecurityException)
            {
                _machineName = string.Empty;
            }
        }

        /// <summary>
        /// Initializes the layout renderer.
        /// </summary>
        protected override void InitializeLayoutRenderer()
        {
            base.InitializeLayoutRenderer();

            _xmlWriterSettings = new XmlWriterSettings
            {
                Indent = IndentXml,
                ConformanceLevel = ConformanceLevel.Fragment,
                IndentChars = "  ",
            };
        }

        /// <summary>
        /// Gets or sets a value indicating whether to include NLog-specific extensions to log4j schema.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        [DefaultValue(true)]
        public bool IncludeNLogData { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the XML should use spaces for indentation.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        public bool IndentXml { get; set; }

        /// <summary>
        /// Gets or sets the AppInfo field. By default it's the friendly name of the current AppDomain.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        public string AppInfo { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include call site (class and method name) in the information sent over the network.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        public bool IncludeCallSite { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include source info (file name and line number) in the information sent over the network.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        public bool IncludeSourceInfo { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include contents of the <see cref="MappedDiagnosticsContext"/> dictionary.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        public bool IncludeMdc { get; set; }

#if !SILVERLIGHT
        /// <summary>
        /// Gets or sets a value indicating whether to include contents of the <see cref="MappedDiagnosticsLogicalContext"/> dictionary.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        public bool IncludeMdlc { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include contents of the <see cref="NestedDiagnosticsLogicalContext"/> stack.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        public bool IncludeNdlc { get; set; }

        /// <summary>
        /// Gets or sets the NDLC item separator.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        [DefaultValue(" ")]
        public string NdlcItemSeparator { get; set; }
#endif

        /// <summary>
        /// Gets or sets the option to include all properties from the log events
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        public bool IncludeAllProperties { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include contents of the <see cref="NestedDiagnosticsContext"/> stack.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        public bool IncludeNdc { get; set; }

        /// <summary>   
        /// Gets or sets the NDC item separator.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        [DefaultValue(" ")]
        public string NdcItemSeparator { get; set; }

        /// <summary>
        /// Gets or sets the log4j:event logger-xml-attribute (Default ${logger})
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        public Layout LoggerName { get; set; }

        private readonly string _machineName;

        private XmlWriterSettings _xmlWriterSettings;

        /// <summary>
        /// Gets the level of stack trace information required by the implementing class.
        /// </summary>
        StackTraceUsage IUsesStackTrace.StackTraceUsage
        {
            get
            {
                if (IncludeSourceInfo)
                {
                    return StackTraceUsage.Max;
                }

                if (IncludeCallSite)
                {
                    return StackTraceUsage.WithoutSource;
                }

                return StackTraceUsage.None;
            }
        }

        internal IList<NLogViewerParameterInfo> Parameters { get; set; }

        internal void AppendToStringBuilder(StringBuilder sb, LogEventInfo logEvent)
        {
            Append(sb, logEvent);
        }

        /// <summary>
        /// Renders the XML logging event and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            StringBuilder sb = new StringBuilder();
            using (XmlWriter xtw = XmlWriter.Create(sb, _xmlWriterSettings))
            {
                xtw.WriteStartElement("log4j", "event", dummyNamespace);
                xtw.WriteAttributeSafeString("xmlns", "nlog", null, dummyNLogNamespace);
                xtw.WriteAttributeSafeString("logger", LoggerName != null ? LoggerName.Render(logEvent) : logEvent.LoggerName);
                xtw.WriteAttributeSafeString("level", logEvent.Level.Name.ToUpperInvariant());
                xtw.WriteAttributeSafeString("timestamp", Convert.ToString((long)(logEvent.TimeStamp.ToUniversalTime() - log4jDateBase).TotalMilliseconds, CultureInfo.InvariantCulture));
                xtw.WriteAttributeSafeString("thread", System.Threading.Thread.CurrentThread.ManagedThreadId.ToString(CultureInfo.InvariantCulture));

                xtw.WriteElementSafeString("log4j", "message", dummyNamespace, logEvent.FormattedMessage);
                if (logEvent.Exception != null)
                {
                    // TODO Why twice the exception details?
                    xtw.WriteElementSafeString("log4j", "throwable", dummyNamespace, logEvent.Exception.ToString());
                }

                string ndcContent = null;
                if (IncludeNdc)
                {
                    ndcContent = string.Join(NdcItemSeparator, NestedDiagnosticsContext.GetAllMessages());
                }
#if !SILVERLIGHT
                if (IncludeNdlc)
                {
                    if (ndcContent != null)
                    {
                        //extra separator
                        ndcContent += NdcItemSeparator;
                    }
                    ndcContent += string.Join(NdlcItemSeparator, NestedDiagnosticsLogicalContext.GetAllMessages());
                }
#endif

                if (ndcContent != null)
                {
                    //NDLC and NDC should be in the same element
                    xtw.WriteElementSafeString("log4j", "NDC", dummyNamespace, ndcContent);
                }

                if (logEvent.Exception != null)
                {
                    // TODO Why twice the exception details?
                    xtw.WriteStartElement("log4j", "throwable", dummyNamespace);
                    xtw.WriteSafeCData(logEvent.Exception.ToString());
                    xtw.WriteEndElement();
                }

                if (IncludeCallSite || IncludeSourceInfo)
                {
                    if (logEvent.CallSiteInformation != null)
                    {
                        MethodBase methodBase = logEvent.CallSiteInformation.GetCallerStackFrameMethod(0);
                        string callerClassName = logEvent.CallSiteInformation.GetCallerClassName(methodBase, true, true, true);
                        string callerMemberName = logEvent.CallSiteInformation.GetCallerMemberName(methodBase, true, true, true);

                        xtw.WriteStartElement("log4j", "locationInfo", dummyNamespace);
                        if (!string.IsNullOrEmpty(callerClassName))
                        {
                            xtw.WriteAttributeSafeString("class", callerClassName);
                        }

                        xtw.WriteAttributeSafeString("method", callerMemberName);
#if !SILVERLIGHT
                        if (IncludeSourceInfo)
                        {
                            xtw.WriteAttributeSafeString("file", logEvent.CallSiteInformation.GetCallerFilePath(0));
                            xtw.WriteAttributeSafeString("line", logEvent.CallSiteInformation.GetCallerLineNumber(0).ToString(CultureInfo.InvariantCulture));
                        }
#endif
                        xtw.WriteEndElement();

                        if (IncludeNLogData)
                        {
                            xtw.WriteElementSafeString("nlog", "eventSequenceNumber", dummyNLogNamespace, logEvent.SequenceID.ToString(CultureInfo.InvariantCulture));
                            xtw.WriteStartElement("nlog", "locationInfo", dummyNLogNamespace);
                            var type = methodBase?.DeclaringType;
                            if (type != null)
                            {
                                xtw.WriteAttributeSafeString("assembly", type.GetAssembly().FullName);
                            }
                            xtw.WriteEndElement();

                            xtw.WriteStartElement("nlog", "properties", dummyNLogNamespace);
                            AppendProperties("nlog", xtw, logEvent);
                            xtw.WriteEndElement();
                        }
                    }
                }

                xtw.WriteStartElement("log4j", "properties", dummyNamespace);
                if (IncludeMdc)
                {
                    foreach (string key in MappedDiagnosticsContext.GetNames())
                    {
                        string propertyValue = XmlHelper.XmlConvertToString(MappedDiagnosticsContext.GetObject(key));
                        if (propertyValue == null)
                            continue;

                        xtw.WriteStartElement("log4j", "data", dummyNamespace);
                        xtw.WriteAttributeSafeString("name", key);
                        xtw.WriteAttributeSafeString("value", propertyValue);
                        xtw.WriteEndElement();
                    }
                }

#if !SILVERLIGHT
                if (IncludeMdlc)
                {
                    foreach (string key in MappedDiagnosticsLogicalContext.GetNames())
                    {
                        string propertyValue = XmlHelper.XmlConvertToString(MappedDiagnosticsLogicalContext.GetObject(key));
                        if (propertyValue == null)
                            continue;

                        xtw.WriteStartElement("log4j", "data", dummyNamespace);
                        xtw.WriteAttributeSafeString("name", key);
                        xtw.WriteAttributeSafeString("value", propertyValue);
                        xtw.WriteEndElement();
                    }
                }
#endif

                if (IncludeAllProperties)
                {
                    AppendProperties("log4j", xtw, logEvent);
                }

                if (Parameters.Count > 0)
                {
                    foreach (NLogViewerParameterInfo parameter in Parameters)
                    {
                        xtw.WriteStartElement("log4j", "data", dummyNamespace);
                        xtw.WriteAttributeSafeString("name", parameter.Name);
                        xtw.WriteAttributeSafeString("value", parameter.Layout.Render(logEvent));
                        xtw.WriteEndElement();
                    }
                }

                xtw.WriteStartElement("log4j", "data", dummyNamespace);
                xtw.WriteAttributeSafeString("name", "log4japp");
                xtw.WriteAttributeSafeString("value", AppInfo);
                xtw.WriteEndElement();

                xtw.WriteStartElement("log4j", "data", dummyNamespace);
                xtw.WriteAttributeSafeString("name", "log4jmachinename");
                xtw.WriteAttributeSafeString("value", _machineName);
                xtw.WriteEndElement();

                xtw.WriteEndElement();

                xtw.WriteEndElement();
                xtw.Flush();

                // get rid of 'nlog' and 'log4j' namespace declarations
                sb.Replace(dummyNamespaceRemover, string.Empty);
                sb.Replace(dummyNLogNamespaceRemover, string.Empty);
                builder.Append(sb.ToString());  // StringBuilder.Replace is not good when reusing the StringBuilder
            }
        }

        private void AppendProperties(string prefix, XmlWriter xtw, LogEventInfo logEvent)
        {
            if (logEvent.HasProperties)
            {
                foreach (var contextProperty in logEvent.Properties)
                {
                    string propertyKey = XmlHelper.XmlConvertToString(contextProperty.Key);
                    if (string.IsNullOrEmpty(propertyKey))
                        continue;

                    string propertyValue = XmlHelper.XmlConvertToString(contextProperty.Value);
                    if (propertyValue == null)
                        continue;

                    xtw.WriteStartElement(prefix, "data", dummyNamespace);
                    xtw.WriteAttributeSafeString("name", propertyKey);
                    xtw.WriteAttributeSafeString("value", propertyValue);
                    xtw.WriteEndElement();
                }
            }
        }
    }
}

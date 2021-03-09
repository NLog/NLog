// 
// Copyright (c) 2004-2020 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using NLog.Common;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Internal.Fakeables;
    using NLog.Layouts;
    using NLog.Targets;

    /// <summary>
    /// XML event description compatible with log4j, Chainsaw and NLogViewer.
    /// </summary>
    [LayoutRenderer("log4jxmlevent")]
    [ThreadSafe]
    [MutableUnsafe]
    public class Log4JXmlEventLayoutRenderer : LayoutRenderer, IUsesStackTrace, IIncludeContext
    {
        private static readonly DateTime log4jDateBase = new DateTime(1970, 1, 1);

        private static readonly string dummyNamespace = "http://nlog-project.org/dummynamespace/" + Guid.NewGuid();
        private static readonly string dummyNamespaceRemover = " xmlns:log4j=\"" + dummyNamespace + "\"";

        private static readonly string dummyNLogNamespace = "http://nlog-project.org/dummynamespace/" + Guid.NewGuid();
        private static readonly string dummyNLogNamespaceRemover = " xmlns:nlog=\"" + dummyNLogNamespace + "\"";

        private readonly ScopeContextNestedStatesLayoutRenderer _scopeNestedLayoutRenderer = new ScopeContextNestedStatesLayoutRenderer() { Separator = " " };

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4JXmlEventLayoutRenderer" /> class.
        /// </summary>
        public Log4JXmlEventLayoutRenderer() : this(LogFactory.DefaultAppEnvironment)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4JXmlEventLayoutRenderer" /> class.
        /// </summary>
        internal Log4JXmlEventLayoutRenderer(IAppEnvironment appEnvironment)
        {

#if NETSTANDARD1_3
            AppInfo = "NetCore Application";
#else
            AppInfo = string.Format(
                CultureInfo.InvariantCulture,
                "{0}({1})",
                appEnvironment.AppDomain.FriendlyName,
                appEnvironment.CurrentProcessId);
#endif

            Parameters = new List<NLogViewerParameterInfo>();

            try
            {
                _machineName = EnvironmentHelper.GetMachineName();
                if (string.IsNullOrEmpty(_machineName))
                {
                    InternalLogger.Info("MachineName is not available.");
                }
            }
            catch (Exception exception)
            {
                InternalLogger.Error(exception, "Error getting machine name.");
                if (exception.MustBeRethrown())
                {
                    throw;
                }

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
#if !NET35
                NamespaceHandling = NamespaceHandling.OmitDuplicates,
#endif
                IndentChars = "  ",
            };
        }

        /// <summary>
        /// Gets or sets a value indicating whether to include NLog-specific extensions to log4j schema.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        [DefaultValue(false)]
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
        public Layout AppInfo { get; set; }

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
        [Obsolete("Replaced by IncludeScopeProperties. Marked obsolete on NLog 5.0")]
        public bool IncludeMdc { get => _includeMdc ?? false; set => _includeMdc = value; }
        private bool? _includeMdc;

        /// <summary>
        /// Gets or sets a value indicating whether to include contents of the <see cref="MappedDiagnosticsLogicalContext"/> dictionary.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        [Obsolete("Replaced by IncludeScopeProperties. Marked obsolete on NLog 5.0")]
        public bool IncludeMdlc { get => _includeMdlc ?? false; set => _includeMdlc = value; }
        private bool? _includeMdlc;

        /// <summary>
        /// Gets or sets a value indicating whether to include contents of the <see cref="NestedDiagnosticsLogicalContext"/> stack.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        [Obsolete("Replaced by IncludeScopeNestedStates. Marked obsolete on NLog 5.0")]
        public bool IncludeNdlc { get => _includeNdlc ?? false; set => _includeNdlc = value; }
        private bool? _includeNdlc;

        /// <summary>
        /// Gets or sets a value indicating whether to include contents of the <see cref="NestedDiagnosticsContext"/> stack.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        [Obsolete("Replaced by IncludeScopeNestedStates. Marked obsolete on NLog 5.0")]
        public bool IncludeNdc { get => _includeNdc ?? false; set => _includeNdc = value; }
        private bool? _includeNdc;

        /// <summary>
        /// Gets or sets whether to include the contents of the <see cref="ScopeContext"/> properties-dictionary.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        public bool IncludeScopeProperties { get => _includeScopeProperties ?? (_includeMdlc == true || _includeMdc == true); set => _includeScopeProperties = value; }
        private bool? _includeScopeProperties;

        /// <summary>
        /// Gets or sets whether to include the contents of the <see cref="ScopeContext"/> operation-call-stack.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        public bool IncludeScopeNestedStates { get => _includeScopeNestedStates ?? (_includeNdlc == true || _includeNdc == true); set => _includeScopeNestedStates = value; }
        private bool? _includeScopeNestedStates;

        /// <summary>
        /// Gets or sets the stack separator for <see cref="ScopeContext"/> operation-call-stack.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        public Layout ScopeNestedStateSeparator
        {
            get => _scopeNestedLayoutRenderer.Separator;
            set => _scopeNestedLayoutRenderer.Separator = value;
        }

        /// <summary>
        /// Gets or sets the NDLC item separator.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        [DefaultValue(" ")]
        [Obsolete("Replaced by ScopeNestedStateSeparator. Marked obsolete on NLog 5.0")]
        public string NdlcItemSeparator { get => (ScopeNestedStateSeparator as SimpleLayout)?.OriginalText; set => ScopeNestedStateSeparator = new SimpleLayout(value); }

        /// <summary>
        /// Gets or sets the option to include all properties from the log events
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        [Obsolete("Replaced by IncludeEventProperties. Marked obsolete on NLog 5.0")]
        public bool IncludeAllProperties { get => IncludeEventProperties; set => IncludeEventProperties = value; }

        /// <summary>
        /// Gets or sets the option to include all properties from the log events
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        public bool IncludeEventProperties { get; set; }

        /// <summary>   
        /// Gets or sets the NDC item separator.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        [Obsolete("Replaced by ScopeNestedStateSeparator. Marked obsolete on NLog 5.0")]
        [DefaultValue(" ")]
        public string NdcItemSeparator { get => (ScopeNestedStateSeparator as SimpleLayout)?.OriginalText; set => ScopeNestedStateSeparator = new SimpleLayout(value); }

        /// <summary>
        /// Gets or sets the log4j:event logger-xml-attribute (Default ${logger})
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        public Layout LoggerName { get; set; }

        /// <summary>
        ///  Gets or sets whether the log4j:throwable xml-element should be written as CDATA
        /// </summary>
        public bool WriteThrowableCData { get; set; }

        private readonly string _machineName;

        private XmlWriterSettings _xmlWriterSettings;

        /// <summary>
        /// Gets the level of stack trace information required by the implementing class.
        /// </summary>
        StackTraceUsage IUsesStackTrace.StackTraceUsage => (IncludeCallSite || IncludeSourceInfo) ? (StackTraceUsageUtils.GetStackTraceUsage(IncludeSourceInfo, 0, true) | StackTraceUsage.WithCallSiteClassName) : StackTraceUsage.None;

        internal IList<NLogViewerParameterInfo> Parameters { get; set; }

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
                bool includeNLogCallsite = (IncludeCallSite || IncludeSourceInfo) && logEvent.CallSiteInformation != null;
                if (includeNLogCallsite && IncludeNLogData)
                {
                    xtw.WriteAttributeString("xmlns", "nlog", null, dummyNLogNamespace);
                }
                xtw.WriteAttributeSafeString("logger", LoggerName != null ? LoggerName.Render(logEvent) : logEvent.LoggerName);
                xtw.WriteAttributeString("level", logEvent.Level.Name.ToUpperInvariant());
                xtw.WriteAttributeString("timestamp", Convert.ToString((long)(logEvent.TimeStamp.ToUniversalTime() - log4jDateBase).TotalMilliseconds, CultureInfo.InvariantCulture));
                xtw.WriteAttributeString("thread", AsyncHelpers.GetManagedThreadId().ToString(CultureInfo.InvariantCulture));

                xtw.WriteElementSafeString("log4j", "message", dummyNamespace, logEvent.FormattedMessage);
                if (logEvent.Exception != null)
                {
                    if (WriteThrowableCData)
                    {
                        // CDATA correctly preserves newlines and indention, but not all viewers support this
                        xtw.WriteStartElement("log4j", "throwable", dummyNamespace);
                        xtw.WriteSafeCData(logEvent.Exception.ToString());
                        xtw.WriteEndElement();
                    }
                    else
                    {
                        xtw.WriteElementSafeString("log4j", "throwable", dummyNamespace, logEvent.Exception.ToString());
                    }
                }

                AppendScopeContextNestedStates(xtw, logEvent);

                if (includeNLogCallsite)
                {
                    AppendCallSite(logEvent, xtw);
                }

                xtw.WriteStartElement("log4j", "properties", dummyNamespace);

                AppendScopeContextProperties("log4j", dummyNamespaceRemover, xtw);

                if (IncludeEventProperties)
                {
                    AppendProperties("log4j", dummyNamespaceRemover, xtw, logEvent);
                }

                AppendParameters(logEvent, xtw);

                xtw.WriteStartElement("log4j", "data", dummyNamespace);
                xtw.WriteAttributeString("name", "log4japp");
                xtw.WriteAttributeSafeString("value", AppInfo?.Render(logEvent) ?? string.Empty);
                xtw.WriteEndElement();

                xtw.WriteStartElement("log4j", "data", dummyNamespace);
                xtw.WriteAttributeString("name", "log4jmachinename");
                xtw.WriteAttributeSafeString("value", _machineName);
                xtw.WriteEndElement();

                xtw.WriteEndElement();  // properties

                xtw.WriteEndElement();  // event
                xtw.Flush();

                // get rid of 'nlog' and 'log4j' namespace declarations
                sb.Replace(dummyNamespaceRemover, string.Empty);
                if (includeNLogCallsite && IncludeNLogData)
                {
                    sb.Replace(dummyNLogNamespaceRemover, string.Empty);
                }
                sb.CopyTo(builder); // StringBuilder.Replace is not good when reusing the StringBuilder
            }
        }

        private void AppendScopeContextProperties(string prefix, string propertiesNamespace, XmlWriter xtw)
        {
            if (IncludeScopeProperties)
            {
                using (var scopeEnumerator = ScopeContext.GetAllPropertiesEnumerator())
                {
                    while (scopeEnumerator.MoveNext())
                    {
                        var scopeProperty = scopeEnumerator.Current;
                        if (string.IsNullOrEmpty(scopeProperty.Key))
                            continue;

                        string propertyValue = XmlHelper.XmlConvertToStringSafe(scopeProperty.Value);
                        if (propertyValue == null)
                            continue;

                        xtw.WriteStartElement(prefix, "data", propertiesNamespace);
                        xtw.WriteAttributeSafeString("name", scopeProperty.Key);
                        xtw.WriteAttributeString("value", propertyValue);
                        xtw.WriteEndElement();
                    }
                }
            }
        }

        private void AppendScopeContextNestedStates(XmlWriter xtw, LogEventInfo logEvent)
        {
            if (IncludeScopeNestedStates)
            {
                var nestedStates = _scopeNestedLayoutRenderer.Render(logEvent);
                //NDLC and NDC should be in the same element
                xtw.WriteElementSafeString("log4j", "NDC", dummyNamespace, nestedStates);
            }
        }

        private void AppendParameters(LogEventInfo logEvent, XmlWriter xtw)
        {
            for (int i = 0; i < Parameters?.Count; ++i)
            {
                var parameter = Parameters[i];
                if (string.IsNullOrEmpty(parameter?.Name))
                    continue;

                var parameterValue = parameter.Layout?.Render(logEvent) ?? string.Empty;
                if (!parameter.IncludeEmptyValue && string.IsNullOrEmpty(parameterValue))
                    continue;

                xtw.WriteStartElement("log4j", "data", dummyNamespace);
                xtw.WriteAttributeSafeString("name", parameter.Name);
                xtw.WriteAttributeSafeString("value", parameterValue);
                xtw.WriteEndElement();
            }
        }

        private void AppendCallSite(LogEventInfo logEvent, XmlWriter xtw)
        {
            MethodBase methodBase = logEvent.CallSiteInformation.GetCallerStackFrameMethod(0);
            string callerClassName = logEvent.CallSiteInformation.GetCallerClassName(methodBase, true, true, true);
            string callerMethodName = logEvent.CallSiteInformation.GetCallerMethodName(methodBase, true, true, true);

            xtw.WriteStartElement("log4j", "locationInfo", dummyNamespace);
            if (!string.IsNullOrEmpty(callerClassName))
            {
                xtw.WriteAttributeSafeString("class", callerClassName);
            }

            xtw.WriteAttributeSafeString("method", callerMethodName);

            if (IncludeSourceInfo)
            {
                xtw.WriteAttributeSafeString("file", logEvent.CallSiteInformation.GetCallerFilePath(0));
                xtw.WriteAttributeString("line", logEvent.CallSiteInformation.GetCallerLineNumber(0).ToString(CultureInfo.InvariantCulture));
            }

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
                AppendProperties("nlog", dummyNLogNamespace, xtw, logEvent);
                xtw.WriteEndElement();
            }
        }

        private void AppendProperties(string prefix, string propertiesNamespace, XmlWriter xtw, LogEventInfo logEvent)
        {
            if (logEvent.HasProperties)
            {
                foreach (var contextProperty in logEvent.Properties)
                {
                    string propertyKey = XmlHelper.XmlConvertToStringSafe(contextProperty.Key);
                    if (string.IsNullOrEmpty(propertyKey))
                        continue;

                    string propertyValue = XmlHelper.XmlConvertToStringSafe(contextProperty.Value);
                    if (propertyValue == null)
                        continue;

                    xtw.WriteStartElement(prefix, "data", propertiesNamespace);
                    xtw.WriteAttributeString("name", propertyKey);
                    xtw.WriteAttributeString("value", propertyValue);
                    xtw.WriteEndElement();
                }
            }
        }
    }
}

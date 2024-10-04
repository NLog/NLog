//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/Log4JXMLEvent-Layout-Renderer">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/Log4JXMLEvent-Layout-Renderer">Documentation on NLog Wiki</seealso>
    [LayoutRenderer("log4jxmlevent")]
    public class Log4JXmlEventLayoutRenderer : LayoutRenderer, IUsesStackTrace, IIncludeContext
    {
        private static readonly DateTime log4jDateBase = new DateTime(1970, 1, 1);

        private static readonly string dummyNamespace = "http://nlog-project.org/dummynamespace/" + Guid.NewGuid();
        private static readonly string dummyNamespaceRemover = " xmlns:log4j=\"" + dummyNamespace + "\"";

        private static readonly string dummyNLogNamespace = "http://nlog-project.org/dummynamespace/" + Guid.NewGuid();
        private static readonly string dummyNLogNamespaceRemover = " xmlns:nlog=\"" + dummyNLogNamespace + "\"";

        private readonly ScopeContextNestedStatesLayoutRenderer _scopeNestedLayoutRenderer = new ScopeContextNestedStatesLayoutRenderer();

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
            AppInfo = string.Format(
                CultureInfo.InvariantCulture,
                "{0}({1})",
                appEnvironment.AppDomainFriendlyName,
                appEnvironment.CurrentProcessId);

            Parameters = new List<NLogViewerParameterInfo>();

            try
            {
                _machineName = XmlHelper.XmlConvertToStringSafe(EnvironmentHelper.GetMachineName());
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

        /// <inheritdoc/>
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
        /// <docgen category='Layout Options' order='10' />
        public bool IncludeNLogData { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the XML should use spaces for indentation.
        /// </summary>
        /// <docgen category='Layout Options' order='50' />
        public bool IndentXml { get; set; }

        /// <summary>
        /// Gets or sets the log4j:event logger-xml-attribute. Default: ${logger}
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public Layout LoggerName { get; set; }

        /// <summary>
        /// Gets or sets the log4j:event message-xml-element. Default: ${message}
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public Layout FormattedMessage { get; set; }

        /// <summary>
        /// Gets or sets the log4j:event log4japp-xml-element. By default it's the friendly name of the current AppDomain.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public Layout AppInfo { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include call site (class and method name) in the information sent over the network.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public bool IncludeCallSite { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include source info (file name and line number) in the information sent over the network.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public bool IncludeSourceInfo { get; set; }

        /// <summary>
        /// Obsolete and replaced by <see cref="IncludeScopeProperties"/> with NLog v5.
        ///
        /// Gets or sets a value indicating whether to include contents of the <see cref="MappedDiagnosticsContext"/> dictionary.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [Obsolete("Replaced by IncludeScopeProperties. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IncludeMdc { get => _includeMdc ?? false; set => _includeMdc = value; }
        private bool? _includeMdc;

        /// <summary>
        /// Obsolete and replaced by <see cref="IncludeScopeProperties"/> with NLog v5.
        ///
        /// Gets or sets a value indicating whether to include contents of the <see cref="MappedDiagnosticsLogicalContext"/> dictionary.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [Obsolete("Replaced by IncludeScopeProperties. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IncludeMdlc { get => _includeMdlc ?? false; set => _includeMdlc = value; }
        private bool? _includeMdlc;

        /// <summary>
        /// Obsolete and replaced by <see cref="IncludeNdc"/> with NLog v5.
        ///
        /// Gets or sets a value indicating whether to include contents of the <see cref="NestedDiagnosticsLogicalContext"/> stack.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [Obsolete("Replaced by IncludeNdc. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IncludeNdlc { get => _includeNdlc ?? false; set => _includeNdlc = value; }
        private bool? _includeNdlc;

        /// <summary>
        /// Gets or sets whether to include log4j:NDC in output from <see cref="ScopeContext"/> nested context.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public bool IncludeNdc { get => _includeNdc ?? false; set => _includeNdc = value; }
        private bool? _includeNdc;

        /// <summary>
        /// Gets or sets whether to include the contents of the <see cref="ScopeContext"/> properties-dictionary.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public bool IncludeScopeProperties { get => _includeScopeProperties ?? (_includeMdlc == true || _includeMdc == true); set => _includeScopeProperties = value; }
        private bool? _includeScopeProperties;

        /// <summary>
        /// Gets or sets whether to include log4j:NDC in output from <see cref="ScopeContext"/> nested context.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public bool IncludeScopeNested { get => _includeScopeNested ?? (_includeNdlc == true || _includeNdc == true); set => _includeScopeNested = value; }
        private bool? _includeScopeNested;

        /// <summary>
        /// Gets or sets the stack separator for log4j:NDC in output from <see cref="ScopeContext"/> nested context.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public string ScopeNestedSeparator
        {
            get => _scopeNestedLayoutRenderer.Separator;
            set => _scopeNestedLayoutRenderer.Separator = value;
        }

        /// <summary>
        /// Obsolete and replaced by <see cref="NdcItemSeparator"/> with NLog v5.
        ///
        /// Gets or sets the stack separator for log4j:NDC in output from <see cref="ScopeContext"/> nested context.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [Obsolete("Replaced by NdcItemSeparator. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string NdlcItemSeparator { get => ScopeNestedSeparator; set => ScopeNestedSeparator = value; }

        /// <summary>
        /// Obsolete and replaced by <see cref="IncludeEventProperties"/> with NLog v5.
        ///
        /// Gets or sets the option to include all properties from the log events
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [Obsolete("Replaced by IncludeEventProperties. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IncludeAllProperties { get => IncludeEventProperties; set => IncludeEventProperties = value; }

        /// <summary>
        /// Gets or sets the option to include all properties from the log events
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public bool IncludeEventProperties { get; set; } = true;

        /// <summary>
        /// Gets or sets the stack separator for log4j:NDC in output from <see cref="ScopeContext"/> nested context.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public string NdcItemSeparator { get => ScopeNestedSeparator; set => ScopeNestedSeparator = value; }

        /// <summary>
        ///  Gets or sets whether the log4j:throwable xml-element should be written as CDATA
        /// </summary>
        /// <docgen category='Layout Options' order='50' />
        public bool WriteThrowableCData { get; set; }

        private readonly string _machineName;

        private XmlWriterSettings _xmlWriterSettings;

        /// <inheritdoc/>
        StackTraceUsage IUsesStackTrace.StackTraceUsage => (IncludeCallSite || IncludeSourceInfo) ? (StackTraceUsageUtils.GetStackTraceUsage(IncludeSourceInfo, 0, true) | StackTraceUsage.WithCallSiteClassName) : StackTraceUsage.None;

        internal IList<NLogViewerParameterInfo> Parameters { get; set; }

        /// <inheritdoc/>
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
                xtw.WriteAttributeSafeString("logger", LoggerName?.Render(logEvent) ?? logEvent.LoggerName);
                xtw.WriteAttributeString("level", logEvent.Level.Name.ToUpperInvariant());
                xtw.WriteAttributeString("timestamp", Convert.ToString((long)(logEvent.TimeStamp.ToUniversalTime() - log4jDateBase).TotalMilliseconds, CultureInfo.InvariantCulture));
                xtw.WriteAttributeString("thread", AsyncHelpers.GetManagedThreadId().ToString(CultureInfo.InvariantCulture));

                xtw.WriteElementSafeString("log4j", "message", dummyNamespace, FormattedMessage?.Render(logEvent) ?? logEvent.FormattedMessage);
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

                AppendScopeContextProperties(xtw);

                if (IncludeEventProperties)
                {
                    AppendDataProperties("log4j", dummyNamespace, xtw, logEvent);
                }

                AppendParameters(logEvent, xtw);

                var appInfo = XmlHelper.XmlConvertToStringSafe(AppInfo?.Render(logEvent) ?? string.Empty);
                AppendDataProperty(xtw, "log4japp", appInfo, dummyNamespace);

                AppendDataProperty(xtw, "log4jmachinename", _machineName, dummyNamespace);

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

        private void AppendScopeContextProperties(XmlWriter xtw)
        {
            if (IncludeScopeProperties)
            {
                using (var scopeEnumerator = ScopeContext.GetAllPropertiesEnumerator())
                {
                    while (scopeEnumerator.MoveNext())
                    {
                        var scopeProperty = scopeEnumerator.Current;

                        string propertyKey = XmlHelper.XmlConvertToStringSafe(scopeProperty.Key);
                        if (string.IsNullOrEmpty(propertyKey))
                            continue;

                        string propertyValue = XmlHelper.XmlConvertToStringSafe(scopeProperty.Value);
                        if (propertyValue is null)
                            continue;

                        AppendDataProperty(xtw, propertyKey, propertyValue, dummyNamespace);
                    }
                }
            }
        }

        private void AppendScopeContextNestedStates(XmlWriter xtw, LogEventInfo logEvent)
        {
            if (IncludeScopeNested)
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

                string parameterName = parameter?.Name; // property-setter has ensured safe xml-string
                if (string.IsNullOrEmpty(parameterName))
                    continue;

                var parameterValue = XmlHelper.XmlConvertToStringSafe(parameter.Layout?.Render(logEvent) ?? string.Empty);
                if (!parameter.IncludeEmptyValue && string.IsNullOrEmpty(parameterValue))
                    continue;

                AppendDataProperty(xtw, parameterName, parameterValue, dummyNamespace);
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
                    xtw.WriteAttributeSafeString("assembly", type.Assembly.FullName);
                }
                xtw.WriteEndElement();

                xtw.WriteStartElement("nlog", "properties", dummyNLogNamespace);
                AppendDataProperties("nlog", dummyNLogNamespace, xtw, logEvent);
                xtw.WriteEndElement();
            }
        }

        private static void AppendDataProperties(string prefix, string propertiesNamespace, XmlWriter xtw, LogEventInfo logEvent)
        {
            if (logEvent.HasProperties)
            {
                foreach (var contextProperty in logEvent.Properties)
                {
                    string propertyKey = XmlHelper.XmlConvertToStringSafe(contextProperty.Key);
                    if (string.IsNullOrEmpty(propertyKey))
                        continue;

                    string propertyValue = XmlHelper.XmlConvertToStringSafe(contextProperty.Value);
                    if (propertyValue is null)
                        continue;

                    AppendDataProperty(xtw, propertyKey, propertyValue, propertiesNamespace, prefix);
                }
            }
        }

        private static void AppendDataProperty(XmlWriter xtw, string propertyKey, string propertyValue, string propertiesNamespace, string prefix = "log4j")
        {
            xtw.WriteStartElement(prefix, "data", propertiesNamespace);
            xtw.WriteAttributeString("name", propertyKey);
            xtw.WriteAttributeString("value", propertyValue);
            xtw.WriteEndElement();
        }
    }
}

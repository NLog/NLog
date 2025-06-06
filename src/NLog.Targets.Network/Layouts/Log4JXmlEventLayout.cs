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

namespace NLog.Layouts
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text;
    using NLog.Config;

    /// <summary>
    /// A specialized layout that renders Log4j-compatible XML events.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/Log4JXmlEventLayout">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/Log4JXmlEventLayout">Documentation on NLog Wiki</seealso>
    [Layout("Log4JXmlEventLayout")]
    [Layout("Log4JXmlLayout")]
    [ThreadAgnostic]
    public class Log4JXmlEventLayout : CompoundLayout
    {
        private static readonly DateTime UnixDateStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private readonly XmlLayout _innerXml = new XmlLayout() { ElementName = "log4j:event" };

        /// <summary>
        /// Gets the collection of parameters. Each parameter contains a mapping
        /// between NLog layout and a named parameter.
        /// </summary>
        [ArrayParameter(typeof(Log4JXmlEventParameter), "parameter")]
        public IList<Log4JXmlEventParameter> Parameters { get; } = new List<Log4JXmlEventParameter>();

        /// <summary>
        /// Gets or sets the option to include all properties from the log events
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public bool IncludeEventProperties { get; set; }

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
        /// Gets or sets a value indicating whether to include NLog-specific extensions to log4j schema.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [Obsolete("Non standard extension to the Log4j-XML format. Marked obsolete with NLog v5.4")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IncludeNLogData { get; set; }

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
        /// Gets or sets the stack separator for log4j:NDC in output from <see cref="ScopeContext"/> nested context.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public string ScopeNestedSeparator { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the stack separator for log4j:NDC in output from <see cref="ScopeContext"/> nested context.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public string NdcItemSeparator { get => ScopeNestedSeparator; set => ScopeNestedSeparator = value; }

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
        /// Gets or sets whether to include log4j:NDC in output from <see cref="ScopeContext"/> nested context.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public bool IncludeNdc { get => _includeNdc ?? false; set => _includeNdc = value; }
        private bool? _includeNdc;

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
        /// Gets or sets the log4j:event logger-xml-attribute. Default: ${logger}
        /// </summary>
        /// <docgen category='Layout Options' order='100' />
        public Layout LoggerName { get => _loggerName.Layout; set => _loggerName.Layout = value; }
        private readonly XmlAttribute _loggerName = new XmlAttribute("logger", "${logger}");

        private readonly XmlAttribute _levelValue = new XmlAttribute("level", "${level:uppercase=true}");

        private readonly XmlAttribute _timestampValue = new XmlAttribute("timestamp", Layout.FromMethod(evt => (long)(evt.TimeStamp.ToUniversalTime() - UnixDateStart).TotalMilliseconds, LayoutRenderOptions.ThreadAgnostic));

        private readonly XmlAttribute _threadIdValue = new XmlAttribute("thread", "${threadid}");

        /// <summary>
        /// Gets or sets the log4j:event message-xml-element. Default: ${message}
        /// </summary>
        /// <docgen category='Layout Options' order='100' />
        public Layout FormattedMessage { get => _formattedMessage.Layout; set => _formattedMessage.Layout = value; }
        private readonly XmlElement _formattedMessage = new XmlElement("log4j:message", "${message}");

        /// <summary>
        /// Gets or sets the log4j:event log4japp-xml-element. By default it's the friendly name of the current AppDomain.
        /// </summary>
        /// <docgen category='Layout Options' order='100' />
        public Layout AppInfo { get => _log4jAppName.Attributes[1].Layout; set => _log4jAppName.Attributes[1].Layout = value; }
        private readonly XmlElement _log4jAppName = new XmlElement("log4j:data", Layout.Empty)
        {
            Attributes =
            {
                new XmlAttribute("name", "log4japp"),
                new XmlAttribute("value", "${appdomain:format=Friendly}(${processid})")
            }
        };

        private readonly XmlElement _log4jMachineName = new XmlElement("log4j:data", Layout.Empty)
        {
            Attributes =
            {
                new XmlAttribute("name", "log4jmachinename"),
                new XmlAttribute("value", "${hostname}")
            }
        };

        /// <summary>
        ///  Gets or sets whether the log4j:throwable xml-element should be written as CDATA
        /// </summary>
        /// <docgen category='Layout Options' order='50' />
        public bool WriteThrowableCData { get => _exceptionThrowable.CDataEncode; set => _exceptionThrowable.CDataEncode = value; }
        private readonly XmlElement _exceptionThrowable = new XmlElement("log4j:throwable", "${exception:format=ToString}");

        /// <summary>
        /// Gets or sets a value indicating whether to include call site (class and method name) in the information sent over the network.
        /// </summary>
        /// <docgen category='Layout Options' order='100' />
        public bool IncludeCallSite { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include source info (file name and line number) in the information sent over the network.
        /// </summary>
        /// <docgen category='Layout Options' order='100' />
        public bool IncludeSourceInfo { get; set; }

        /// <inheritdoc/>
        protected override void InitializeLayout()
        {
            _innerXml.Attributes.Clear();
            _innerXml.Elements.Clear();

            _innerXml.Attributes.Add(_loggerName);
            _innerXml.Attributes.Add(_levelValue);
            _innerXml.Attributes.Add(_timestampValue);
            _innerXml.Attributes.Add(_threadIdValue);

            _innerXml.Elements.Add(_formattedMessage);
            _innerXml.Elements.Add(_exceptionThrowable);

            if (IncludeCallSite || IncludeSourceInfo)
            {
                var locationInfo = new XmlElement("log4j:locationInfo", Layout.Empty)
                {
                    Attributes =
                    {
                        new XmlAttribute("class", "${callsite:methodName=false}"),
                        new XmlAttribute("method", "${callsite:className=false}")
                    }
                };

                if (IncludeSourceInfo)
                {
                    locationInfo.Attributes.Add(new XmlAttribute("file", "${callsite-filename}"));
                    locationInfo.Attributes.Add(new XmlAttribute("line", "${callsite-linenumber}"));
                }

                _innerXml.Elements.Add(locationInfo);
            }

#pragma warning disable CS0618 // Type or member is obsolete
            if (IncludeNLogData)
            {
                _innerXml.Elements.Add(new XmlElement("nlog:eventSequenceNumber", "${sequenceid}"));
                _innerXml.Elements.Add(new XmlElement("nlog:locationInfo", Layout.Empty)
                {
                    Attributes =
                    {
                        new XmlAttribute("assembly", "${callsite:fileName=true:className=false:methodName=false}")
                    }
                });
                _innerXml.Elements.Add(new XmlElement("nlog:properties", Layout.Empty)
                {
                    PropertiesElementName = "nlog:data",
                    PropertiesElementKeyAttribute = "name",
                    PropertiesElementValueAttribute = "value",
                    IncludeEventProperties = true,
                });
            }
#pragma warning restore CS0618 // Type or member is obsolete

            if (IncludeScopeNested)
            {
                var separator = ScopeNestedSeparator;
                Layout scopeNested = string.IsNullOrEmpty(separator) ? "${scopenested}" : ("${scopenested:separator=" + separator.Replace(":", "\\:") + "}");
                _innerXml.Elements.Add(new XmlElement("log4j:NDC", scopeNested));
            }

            var dataProperties = new XmlElement("log4j:properties", Layout.Empty)
            {
                PropertiesElementName = "log4j:data",
                PropertiesElementKeyAttribute = "name",
                PropertiesElementValueAttribute = "value",
                IncludeEventProperties = IncludeEventProperties,
                IncludeScopeProperties = IncludeScopeProperties
            };

            dataProperties.ContextProperties?.Clear();
            if (Parameters.Count > 0)
            {
                if (dataProperties.ContextProperties is null)
                    dataProperties.ContextProperties = new List<Targets.TargetPropertyWithContext>();
                foreach (var parameter in Parameters)
                {
                    dataProperties.ContextProperties.Add(new Targets.TargetPropertyWithContext(parameter.Name, parameter.Layout) { IncludeEmptyValue = parameter.IncludeEmptyValue });
                }
            }

            dataProperties.Elements.Add(_log4jAppName);
            dataProperties.Elements.Add(_log4jMachineName);

            _innerXml.Elements.Add(dataProperties);

            // CompoundLayout includes optimization, so only doing precalculate/caching of relevant Layouts (instead of the entire LOG4J-message)
            Layouts.Clear();
            foreach (var xmlAttribute in _innerXml.Attributes)
                Layouts.Add(xmlAttribute.Layout);
            foreach (var xmlElement in _innerXml.Elements)
                Layouts.Add(xmlElement);

            base.InitializeLayout();
        }

        /// <inheritdoc/>
        protected override string GetFormattedMessage(LogEventInfo logEvent)
        {
            var sb = new StringBuilder(1024);
            RenderFormattedMessage(logEvent, sb);
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override void RenderFormattedMessage(LogEventInfo logEvent, StringBuilder target)
        {
            _innerXml.Render(logEvent, target);
        }
    }
}

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
    using NLog.LayoutRenderers;

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
    public class Log4JXmlEventLayout : Layout
    {

        private static readonly DateTime log4jDateBase = new DateTime(1970, 1, 1);
        private IList<Log4JXmlEventParameter> _parameters = new List<Log4JXmlEventParameter>();


        /// <summary>
        /// Gets inner XML layout.
        /// </summary>
        public XmlLayout InnerXml { get; } = new XmlLayout();

        /// <inheritdoc/>
        protected override void InitializeLayout()
        {
            InnerXml.Attributes.Clear();
            InnerXml.Elements.Clear();

            InnerXml.ElementName = "log4j:event";

            InnerXml.Attributes.Add(new XmlAttribute("logger", LoggerName));
            InnerXml.Attributes.Add(new XmlAttribute("level", "${level:uppercase=true}") { IncludeEmptyValue = true });
            InnerXml.Attributes.Add(new XmlAttribute("timestamp", Layout.FromMethod(evt => (long)(evt.TimeStamp.ToUniversalTime() - log4jDateBase).TotalMilliseconds, LayoutRenderOptions.ThreadAgnostic)));
            InnerXml.Attributes.Add(new XmlAttribute("thread", "${threadid}") { IncludeEmptyValue = true });

            InnerXml.Elements.Add(new XmlElement("log4j:message", FormattedMessage ?? "${message}"));
            InnerXml.Elements.Add(new XmlElement("log4j:throwable", "${exception:format=ToString}")
            {
                CDataEncode  = WriteThrowableCData,
            });

            if (IncludeCallSite || IncludeSourceInfo)
            {
                var locationInfo = new XmlElement("log4j:locationInfo", null)
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

                InnerXml.Elements.Add(locationInfo);
            }

            if (IncludeScopeNested)
            {
                var separator = ScopeNestedSeparator;
                Layout scopeNested = string.IsNullOrEmpty(separator) ? "${scopenested}" : ("${scopenested:separator=" + separator + "}");
                InnerXml.Elements.Add(new XmlElement("log4j:NDC", scopeNested));
            }

            var dataProperties = new XmlElement("log4j:properties", null)
            {
                PropertiesElementName = "log4j:data",
                PropertiesElementKeyAttribute = "name",
                PropertiesElementValueAttribute = "value",
                IncludeEventProperties = IncludeEventProperties,
                IncludeScopeProperties = IncludeScopeProperties
            };

            foreach (var parameter in Parameters)
            {
                var propertyElement = new XmlElement("log4j:data", null)
                {
                    IncludeEmptyValue = parameter.IncludeEmptyValue
                };

                propertyElement.Attributes.Add(new XmlAttribute("name", parameter.Name));
                propertyElement.Attributes.Add(new XmlAttribute("value", parameter.Layout));
                dataProperties.Elements.Add(propertyElement);
            }


            var appProperty = new XmlElement("log4j:data", null);
            appProperty.Attributes.Add(new XmlAttribute("name", "log4japp"));
            appProperty.Attributes.Add(new XmlAttribute("value", AppInfo));
            dataProperties.Elements.Add(appProperty);

            var machineProperty = new XmlElement("log4j:data", null);
            machineProperty.Attributes.Add(new XmlAttribute("name", "log4jmachinename"));
            machineProperty.Attributes.Add(new XmlAttribute("value", "${machinename}"));
            dataProperties.Elements.Add(machineProperty);

            InnerXml.Elements.Add(dataProperties);

            base.InitializeLayout();
        }

        /// <summary>
        /// Gets the collection of parameters. Each parameter contains a mapping
        /// between NLog layout and a named parameter.
        /// </summary>
        [ArrayParameter(typeof(Log4JXmlEventParameter), "parameter")]
        public IList<Log4JXmlEventParameter> Parameters
        {
            get => _parameters;
            set => _parameters = value ?? new List<Log4JXmlEventParameter>();
        }

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
        [Obsolete("Non standard extension to the Log4j-XML format. Marked obsolete with NLog 6.0")]
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
        public string ScopeNestedSeparator { get; set; }

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
        public Layout LoggerName { get; set; } = "${logger}";

        /// <summary>
        /// Gets or sets the log4j:event message-xml-element. Default: ${message}
        /// </summary>
        /// <docgen category='Layout Options' order='100' />
        public Layout FormattedMessage { get; set; }

        /// <summary>
        /// Gets or sets the log4j:event log4japp-xml-element. By default it's the friendly name of the current AppDomain.
        /// </summary>
        /// <docgen category='Layout Options' order='100' />
        public Layout AppInfo { get; set; }

        /// <summary>
        ///  Gets or sets whether the log4j:throwable xml-element should be written as CDATA
        /// </summary>
        /// <docgen category='Layout Options' order='50' />
        public bool WriteThrowableCData { get; set; }

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
        protected override string GetFormattedMessage(LogEventInfo logEvent)
        {
            var sb = new StringBuilder(1024);
            RenderFormattedMessage(logEvent, sb);
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override void RenderFormattedMessage(LogEventInfo logEvent, StringBuilder target)
        {
            InnerXml.Render(logEvent, target);
        }
    }
}

// 
// Copyright (c) 2004-2018 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// A specialized layout that renders XML-formatted events.
    /// </summary>
    [Layout("XmlLayout")]
    [ThreadAgnostic]
    [ThreadSafe]
    public class XmlLayout : Layout
    {
        /// <summary>
        /// Name of the top level XML element
        /// </summary>
        /// <docgen category='XML Options' order='10' />
        [DefaultValue("logevent")]
        public string NodeName { get => _nodename; set => _nodename = XmlHelper.XmlConvertToElementName(value?.Trim(), true); }
        private string _nodename;

        /// <summary>
        /// Value inside the top level XML element
        /// </summary>
        /// <docgen category='XML Options' order='10' />
        public Layout NodeValue { get => NodeValueWrapper.Inner; set => NodeValueWrapper.Inner = value; }
        internal readonly LayoutRenderers.Wrappers.XmlEncodeLayoutRendererWrapper NodeValueWrapper = new LayoutRenderers.Wrappers.XmlEncodeLayoutRendererWrapper();

        /// <summary>
        /// Auto indent and create new lines
        /// </summary>
        /// <docgen category='XML Options' order='10' />
        public bool IndentXml { get; set; }

        /// <summary>
        /// Gets the array of 'nodes' configurations.
        /// </summary>
        /// <docgen category='XML Options' order='10' />
        [ArrayParameter(typeof(XmlLayout), "node")]
        public IList<XmlLayout> Nodes { get; private set; }

        /// <summary>
        /// Gets the array of 'attributes' configurations for the <see cref="NodeName"/>
        /// </summary>
        /// <docgen category='XML Options' order='10' />
        [ArrayParameter(typeof(XmlAttribute), "attribute")]
        public IList<XmlAttribute> Attributes { get; private set; }

        /// <summary>
        /// Gets or sets whether a NodeValue with empty value should be included in the output
        /// </summary>
        /// <docgen category='XML Options' order='10' />
        public bool IncludeEmptyValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include contents of the <see cref="MappedDiagnosticsContext"/> dictionary.
        /// </summary>
        /// <docgen category='LogEvent Properties XML Options' order='10' />
        public bool IncludeMdc { get; set; }

#if !SILVERLIGHT
        /// <summary>
        /// Gets or sets a value indicating whether to include contents of the <see cref="MappedDiagnosticsLogicalContext"/> dictionary.
        /// </summary>
        /// <docgen category='LogEvent Properties XML Options' order='10' />
        public bool IncludeMdlc { get; set; }
#endif

        /// <summary>
        /// Gets or sets the option to include all properties from the log event (as XML)
        /// </summary>
        /// <docgen category='LogEvent Properties XML Options' order='10' />
        public bool IncludeAllProperties { get; set; }

        /// <summary>
        /// List of property names to exclude when <see cref="IncludeAllProperties"/> is true
        /// </summary>
        /// <docgen category='LogEvent Properties XML Options' order='10' />
#if NET3_5
        public HashSet<string> ExcludeProperties { get; set; }
#else
        public ISet<string> ExcludeProperties { get; set; }
#endif

        /// <summary>
        /// XML tag name to use when rendering properties
        /// </summary>
        /// <remarks>
        /// Support string-format where {0} means property-key-name
        /// 
        /// Skips closing element tag when having configured <see cref="PropertiesNodeValueAttribute"/>
        /// </remarks>
        /// <docgen category='LogEvent Properties XML Options' order='10' />
        public string PropertiesNodeName { get { return _propertiesNodeName; } set { _propertiesNodeName = value; _propertiesNodeNameFormat = value?.IndexOf('{') >= 0; } }
        private string _propertiesNodeName = "property";
        private bool _propertiesNodeNameFormat;

        /// <summary>
        /// XML attribute format to use when rendering property-key
        /// </summary>
        /// <remarks>
        /// Support string-format where {0} means property-key-name
        /// 
        /// Replaces newlines with underscore (_)
        /// </remarks>
        /// <docgen category='LogEvent Properties XML Options' order='10' />
        public string PropertiesNodeKeyAttribute { get; set; } = "key=\"{0}\"";

        /// <summary>
        /// XML attribute format to use when rendering property-value
        /// 
        /// When null (or empty) then value is formatted as XML-element-value
        /// </summary>
        /// <remarks>
        /// Support string-format where {0} means property-value
        /// 
        /// Replaces newlines with &#13;&#10;
        /// 
        /// Skips closing element tag when using attribute for value
        /// </remarks>
        /// <docgen category='LogEvent Properties XML Options' order='10' />
        public string PropertiesNodeValueAttribute { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLayout"/> class.
        /// </summary>
        public XmlLayout()
            :this("logevent", null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLayout"/> class.
        /// </summary>
        /// <param name="nodeName">The name of the top XML node</param>
        /// <param name="nodeValue">The value of the top XML node</param>
        public XmlLayout(string nodeName, Layout nodeValue)
        {
            NodeName = nodeName;
            NodeValue = nodeValue;
            Attributes = new List<XmlAttribute>();
            Nodes = new List<XmlLayout>();
            ExcludeProperties = new HashSet<string>();
        }

        /// <summary>
        /// Initializes the layout.
        /// </summary>
        protected override void InitializeLayout()
        {
            base.InitializeLayout();
            if (IncludeMdc)
            {
                ThreadAgnostic = false;
            }
#if !SILVERLIGHT
            if (IncludeMdlc)
            {
                ThreadAgnostic = false;
            }
#endif
        }

        /// <summary>
        /// Closes the layout.
        /// </summary>
        protected override void CloseLayout()
        {
            base.CloseLayout();
        }

        internal override void PrecalculateBuilder(LogEventInfo logEvent, StringBuilder target)
        {
            if (!ThreadAgnostic) RenderAppendBuilder(logEvent, target, true);
        }

        /// <summary>
        /// Formats the log event as a XML document for writing.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        /// <param name="target"><see cref="StringBuilder"/> for the result</param>
        protected override void RenderFormattedMessage(LogEventInfo logEvent, StringBuilder target)
        {
            int orgLength = target.Length;
            RenderXmlFormattedMessage(logEvent, target);
            if (target.Length == orgLength && IncludeEmptyValue && !string.IsNullOrEmpty(NodeName))
            {
                target.Append('<');
                target.Append(NodeName);
                target.Append("/>");
            }
        }

        /// <summary>
        /// Formats the log event as a XML document for writing.
        /// </summary>
        /// <param name="logEvent">The log event to be formatted.</param>
        /// <returns>A XML string representation of the log event.</returns>
        protected override string GetFormattedMessage(LogEventInfo logEvent)
        {
            return RenderAllocateBuilder(logEvent);
        }

        private void RenderXmlFormattedMessage(LogEventInfo logEvent, StringBuilder sb)
        {
            int orgLength = sb.Length;

            // Attributes without element-names should be added to the top XML element
            if (!string.IsNullOrEmpty(NodeName))
            {
                for (int i = 0; i < Attributes.Count; i++)
                {
                    var attrib = Attributes[i];
                    int beforeAttribLength = sb.Length;
                    if (!RenderAppendXmlAttributeValue(attrib, logEvent, sb, sb.Length == orgLength))
                    {
                        sb.Length = beforeAttribLength;
                    }
                }
                if (sb.Length != orgLength)
                {
                    bool hasNodes = 
                        NodeValue != null ||
                        Nodes.Count > 0 ||
                        IncludeMdc ||
#if !SILVERLIGHT
                        IncludeMdlc ||
#endif
                        (IncludeAllProperties && logEvent.HasProperties);
                    if (!hasNodes)
                    {
                        sb.Append(" />");
                        return;
                    }
                    else
                    {
                        sb.Append('>');
                        if (IndentXml)
                            sb.AppendLine();
                    }
                }

                if (NodeValue != null)
                {
                    int beforeNodeLength = sb.Length;
                    if (sb.Length == orgLength)
                    {
                        BeginXmlDocument(sb, NodeName);
                    }
                    int beforeValueLength = sb.Length;
                    NodeValue.RenderAppendBuilder(logEvent, sb);
                    if (beforeValueLength == sb.Length && !IncludeEmptyValue)
                    {
                        sb.Length = beforeNodeLength;
                    }
                }
            }

            //Memory profiling pointed out that using a foreach-loop was allocating
            //an Enumerator. Switching to a for-loop avoids the memory allocation.
            for (int i = 0; i < Nodes.Count; i++)
            {
                var node = Nodes[i];
                int beforeAttribLength = sb.Length;
                if (!RenderAppendXmlNodeValue(node, logEvent, sb, sb.Length == orgLength))
                {
                    sb.Length = beforeAttribLength;
                }
            }

            if (IncludeMdc)
            {
                foreach (string key in MappedDiagnosticsContext.GetNames())
                {
                    if (string.IsNullOrEmpty(key))
                        continue;
                    object propertyValue = MappedDiagnosticsContext.GetObject(key);
                    AppendXmlPropertyValue(key, propertyValue, sb, sb.Length == orgLength);
                }
            }

#if !SILVERLIGHT
            if (IncludeMdlc)
            {
                foreach (string key in MappedDiagnosticsLogicalContext.GetNames())
                {
                    if (string.IsNullOrEmpty(key))
                        continue;
                    object propertyValue = MappedDiagnosticsLogicalContext.GetObject(key);
                    AppendXmlPropertyValue(key, propertyValue, sb, sb.Length == orgLength);
                }
            }
#endif

            if (IncludeAllProperties && logEvent.HasProperties)
            {
                var propertiesList = logEvent.CreateOrUpdatePropertiesInternal(true) as IEnumerable<MessageTemplates.MessageTemplateParameter>;
                foreach (var prop in propertiesList)
                {
                    if (string.IsNullOrEmpty(prop.Name))
                        continue;

                    if (ExcludeProperties.Contains(prop.Name))
                        continue;

                    AppendXmlPropertyValue(prop.Name, prop.Value, sb, sb.Length == orgLength);
                }
            }

            if (sb.Length > orgLength && !string.IsNullOrEmpty(NodeName))
            {
                EndXmlDocument(sb, NodeName);
            }
        }

        private void AppendXmlPropertyValue(string propName, object propertyValue, StringBuilder sb, bool beginXmlDocument)
        {
            if (string.IsNullOrEmpty(PropertiesNodeName))
                return; // Not supported

            propName = propName?.Trim();
            if (string.IsNullOrEmpty(propName))
                return; // Not supported

            if (beginXmlDocument && !string.IsNullOrEmpty(NodeName))
            {
                BeginXmlDocument(sb, NodeName);
            }

            if (IndentXml && !string.IsNullOrEmpty(NodeName))
                sb.Append("  ");

            sb.Append('<');
            string propNameElement = null;
            if (_propertiesNodeNameFormat)
            {
                propNameElement = XmlHelper.XmlConvertToStringSafe(propName);
                sb.AppendFormat(PropertiesNodeName, propNameElement);
            }
            else
            {
                sb.Append(PropertiesNodeName);
            }

            if (!string.IsNullOrEmpty(PropertiesNodeKeyAttribute))
            {
                string propNameAttribute = ReferenceEquals(propName, propNameElement) ? propName : XmlHelper.EscapeXmlString(propName, true);
                sb.Append(' ');
                sb.AppendFormat(PropertiesNodeKeyAttribute, propNameAttribute);
            }

            string xmlValueString = XmlHelper.XmlConvertToStringSafe(propertyValue);
            if (!string.IsNullOrEmpty(PropertiesNodeValueAttribute))
            {
                xmlValueString = XmlHelper.EscapeXmlString(xmlValueString, true);
                sb.Append(' ');
                sb.AppendFormat(PropertiesNodeValueAttribute, xmlValueString);
                sb.Append(" />");
            }
            else
            {
                sb.Append('>');
                XmlHelper.EscapeXmlString(xmlValueString, false, sb);
                sb.Append("</");
                if (_propertiesNodeNameFormat)
                    sb.AppendFormat(PropertiesNodeName, propNameElement);
                else
                    sb.AppendFormat(PropertiesNodeName, PropertiesNodeName);
                sb.Append('>');
            }
            if (IndentXml)
                sb.AppendLine();
        }

        private bool RenderAppendXmlNodeValue(XmlLayout xmlNode, LogEventInfo logEvent, StringBuilder sb, bool beginXmlDocument)
        {
            string xmlElementName = xmlNode.NodeName;
            if (string.IsNullOrEmpty(xmlElementName))
                return false;

            if (beginXmlDocument && !string.IsNullOrEmpty(NodeName))
            {
                BeginXmlDocument(sb, NodeName);
            }

            if (IndentXml && !string.IsNullOrEmpty(NodeName))
                sb.Append("  ");

            int beforeValueLength = sb.Length;
            xmlNode.RenderAppendBuilder(logEvent, sb, false);
            if (sb.Length == beforeValueLength && !xmlNode.IncludeEmptyValue)
                return false;

            if (IndentXml)
                sb.AppendLine();
            return true;
        }

        private bool RenderAppendXmlAttributeValue(XmlAttribute attrib, LogEventInfo logEvent, StringBuilder sb, bool beginXmlDocument)
        {
            string xmlKeyString = attrib.Name;
            if (string.IsNullOrEmpty(xmlKeyString))
                return false;

            if (beginXmlDocument)
            {
                sb.Append('<');
                sb.Append(NodeName);
            }

            sb.Append(' ');
            sb.Append(xmlKeyString);
            sb.Append("=\"");

            int beforeValueLength = sb.Length;
            attrib.LayoutWrapper.RenderAppendBuilder(logEvent, sb);
            if (sb.Length == beforeValueLength && !attrib.IncludeEmptyValue)
                return false;

            sb.Append('\"');
            return true;
        }

        private void BeginXmlDocument(StringBuilder sb, string xmlName)
        {
            sb.Append('<');
            sb.Append(xmlName);
            sb.Append('>');
            if (IndentXml)
                sb.AppendLine();
        }

        private void EndXmlDocument(StringBuilder sb, string xmlName)
        {
            sb.Append("</");
            sb.Append(xmlName);
            sb.Append('>');
        }
    }
}

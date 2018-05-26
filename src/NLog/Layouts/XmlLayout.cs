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
    using System;
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
        /// Initializes a new instance of the <see cref="XmlLayout"/> class.
        /// </summary>
        public XmlLayout()
            : this("logevent", null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLayout"/> class.
        /// </summary>
        /// <param name="elementName">The name of the top XML node</param>
        /// <param name="elementValue">The value of the top XML node</param>
        public XmlLayout(string elementName, Layout elementValue)
        {
            ElementName = elementName;
            ElementValue = elementValue;
            Attributes = new List<XmlAttribute>();
            Elements = new List<XmlLayout>();
            ExcludeProperties = new HashSet<string>();
        }

        /// <summary>
        /// Name of the top level XML element
        /// </summary>
        /// <docgen category='XML Options' order='10' />
        [DefaultValue("logevent")]
        public string ElementName { get => _elementName; set => _elementName = XmlHelper.XmlConvertToElementName(value?.Trim(), true); }
        private string _elementName;

        /// <summary>
        /// Value inside the top level XML element
        /// </summary>
        /// <docgen category='XML Options' order='10' />
        public Layout ElementValue { get => _elementValueWrapper.Inner; set => _elementValueWrapper.Inner = value; }
        private readonly LayoutRenderers.Wrappers.XmlEncodeLayoutRendererWrapper _elementValueWrapper = new LayoutRenderers.Wrappers.XmlEncodeLayoutRendererWrapper();

        /// <summary>
        /// Xml Encode the value for the top level XML element
        /// </summary>
        /// <remarks>Ensures always valid XML, but gives a performance hit</remarks>
        [DefaultValue(true)]
        public bool ElementEncode { get => _elementValueWrapper.XmlEncode; set => _elementValueWrapper.XmlEncode = value; }

        /// <summary>
        /// Auto indent and create new lines
        /// </summary>
        /// <docgen category='XML Options' order='10' />
        public bool IndentXml { get; set; }

        /// <summary>
        /// Gets the array of xml 'elements' configurations.
        /// </summary>
        /// <docgen category='XML Options' order='10' />
        [ArrayParameter(typeof(XmlLayout), "element")]
        public IList<XmlLayout> Elements { get; private set; }

        /// <summary>
        /// Gets the array of 'attributes' configurations for the <see cref="ElementName"/>
        /// </summary>
        /// <docgen category='XML Options' order='10' />
        [ArrayParameter(typeof(XmlAttribute), "attribute")]
        public IList<XmlAttribute> Attributes { get; private set; }

        /// <summary>
        /// Gets or sets whether a ElementValue with empty value should be included in the output
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
        /// Skips closing element tag when having configured <see cref="PropertiesElementValueAttribute"/>
        /// </remarks>
        /// <docgen category='LogEvent Properties XML Options' order='10' />
        public string PropertiesElementName { get { return _propertiesElementName; } set { _propertiesElementName = value; _propertiesElementNameHasFormat = value?.IndexOf('{') >= 0; } }
        private string _propertiesElementName = "property";
        private bool _propertiesElementNameHasFormat;

        /// <summary>
        /// XML attribute name to use when rendering property-key
        ///
        /// When null (or empty) then key-attribute is not included
        /// </summary>
        /// <remarks>
        /// Will replace newlines in attribute-value with &#13;&#10;
        /// </remarks>
        /// <docgen category='LogEvent Properties XML Options' order='10' />
        public string PropertiesElementKeyAttribute { get; set; } = "key";

        /// <summary>
        /// XML attribute name to use when rendering property-value
        /// 
        /// When null (or empty) then value-attribute is not included and
        /// value is formatted as XML-element-value
        /// </summary>
        /// <remarks>
        /// Skips closing element tag when using attribute for value
        ///
        /// Will replace newlines in attribute-value with &#13;&#10;
        /// </remarks>
        /// <docgen category='LogEvent Properties XML Options' order='10' />
        public string PropertiesElementValueAttribute { get; set; }

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

            if (IncludeAllProperties)
            {
                MutableUnsafe = true;
            }

            if (Attributes.Count > 1)
            {
                HashSet<string> attributeValidator = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var attribute in Attributes)
                {
                    if (string.IsNullOrEmpty(attribute.Name))
                    {
                        Common.InternalLogger.Warn("XmlLayout(ElementName={0}): Contains attribute with missing name (Ignored)");
                    }
                    else if (attributeValidator.Contains(attribute.Name))
                    {
                        Common.InternalLogger.Warn("XmlLayout(ElementName={0}): Contains duplicate attribute name: {1} (Invalid xml)", ElementName, attribute.Name);
                    }
                    else
                    {
                        attributeValidator.Add(attribute.Name);
                    }
                }
            }
        }

        internal override void PrecalculateBuilder(LogEventInfo logEvent, StringBuilder target)
        {
            PrecalculateBuilderInternal(logEvent, target);
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
            if (target.Length == orgLength && IncludeEmptyValue && !string.IsNullOrEmpty(ElementName))
            {
                target.Append('<');
                target.Append(ElementName);
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
            if (!string.IsNullOrEmpty(ElementName))
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
                    bool hasElements = 
                        ElementValue != null ||
                        Elements.Count > 0 ||
                        IncludeMdc ||
#if !SILVERLIGHT
                        IncludeMdlc ||
#endif
                        (IncludeAllProperties && logEvent.HasProperties);
                    if (!hasElements)
                    {
                        sb.Append("/>");
                        return;
                    }
                    else
                    {
                        sb.Append('>');
                        if (IndentXml)
                            sb.AppendLine();
                    }
                }

                if (ElementValue != null)
                {
                    int beforeElementLength = sb.Length;
                    if (sb.Length == orgLength)
                    {
                        BeginXmlDocument(sb, ElementName);
                    }
                    int beforeValueLength = sb.Length;
                    ElementValue.RenderAppendBuilder(logEvent, sb);
                    if (beforeValueLength == sb.Length && !IncludeEmptyValue)
                    {
                        sb.Length = beforeElementLength;
                    }
                }
            }

            //Memory profiling pointed out that using a foreach-loop was allocating
            //an Enumerator. Switching to a for-loop avoids the memory allocation.
            for (int i = 0; i < Elements.Count; i++)
            {
                var element = Elements[i];
                int beforeAttribLength = sb.Length;
                if (!RenderAppendXmlElementValue(element, logEvent, sb, sb.Length == orgLength))
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
                foreach (var property in logEvent.Properties)
                {
                    string propertyKey = property.Key.ToString();
                    if (string.IsNullOrEmpty(propertyKey))
                        continue;

                    if (ExcludeProperties.Contains(propertyKey))
                        continue;

                    AppendXmlPropertyValue(propertyKey, property.Value, sb, sb.Length == orgLength);
                }
            }

            if (sb.Length > orgLength && !string.IsNullOrEmpty(ElementName))
            {
                EndXmlDocument(sb, ElementName);
            }
        }

        private void AppendXmlPropertyValue(string propName, object propertyValue, StringBuilder sb, bool beginXmlDocument)
        {
            if (string.IsNullOrEmpty(PropertiesElementName))
                return; // Not supported

            propName = propName?.Trim();
            if (string.IsNullOrEmpty(propName))
                return; // Not supported

            if (beginXmlDocument && !string.IsNullOrEmpty(ElementName))
            {
                BeginXmlDocument(sb, ElementName);
            }

            if (IndentXml && !string.IsNullOrEmpty(ElementName))
                sb.Append("  ");

            sb.Append('<');
            string propNameElement = null;
            if (_propertiesElementNameHasFormat)
            {
                propNameElement = XmlHelper.XmlConvertToStringSafe(propName);
                sb.AppendFormat(PropertiesElementName, propNameElement);
            }
            else
            {
                sb.Append(PropertiesElementName);
            }

            if (!string.IsNullOrEmpty(PropertiesElementKeyAttribute))
            {
                sb.Append(' ');
                sb.Append(PropertiesElementKeyAttribute);
                sb.Append("=\"");
                XmlHelper.EscapeXmlString(propName, true, sb);
                sb.Append('\"');
            }

            string xmlValueString = XmlHelper.XmlConvertToStringSafe(propertyValue);
            if (!string.IsNullOrEmpty(PropertiesElementValueAttribute))
            {
                sb.Append(' ');
                sb.Append(PropertiesElementValueAttribute);
                sb.Append("=\"");
                XmlHelper.EscapeXmlString(xmlValueString, true, sb);
                sb.Append('\"');
                sb.Append("/>");
            }
            else
            {
                sb.Append('>');
                XmlHelper.EscapeXmlString(xmlValueString, false, sb);
                sb.Append("</");
                if (_propertiesElementNameHasFormat)
                    sb.AppendFormat(PropertiesElementName, propNameElement);
                else
                    sb.AppendFormat(PropertiesElementName, PropertiesElementName);
                sb.Append('>');
            }
            if (IndentXml)
                sb.AppendLine();
        }

        private bool RenderAppendXmlElementValue(XmlLayout xmlElement, LogEventInfo logEvent, StringBuilder sb, bool beginXmlDocument)
        {
            string xmlElementName = xmlElement.ElementName;
            if (string.IsNullOrEmpty(xmlElementName))
                return false;

            if (beginXmlDocument && !string.IsNullOrEmpty(ElementName))
            {
                BeginXmlDocument(sb, ElementName);
            }

            if (IndentXml && !string.IsNullOrEmpty(ElementName))
                sb.Append("  ");

            int beforeValueLength = sb.Length;
            xmlElement.RenderAppendBuilder(logEvent, sb, false);
            if (sb.Length == beforeValueLength && !xmlElement.IncludeEmptyValue)
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
                sb.Append(ElementName);
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

        private void BeginXmlDocument(StringBuilder sb, string elementName)
        {
            sb.Append('<');
            sb.Append(elementName);
            sb.Append('>');
            if (IndentXml)
                sb.AppendLine();
        }

        private void EndXmlDocument(StringBuilder sb, string elementName)
        {
            sb.Append("</");
            sb.Append(elementName);
            sb.Append('>');
        }

        /// <summary>
        /// Generate description of XML Layout
        /// </summary>
        /// <returns>XML Layout String Description</returns>
        public override string ToString()
        {
            if (Elements.Count > 0)
                return ToStringWithNestedItems(Elements, l => l.ToString());
            else if (Attributes.Count > 0)
                return ToStringWithNestedItems(Attributes, a => "Attrib:" + a.Name);
            else if (ElementName != null)
                return ToStringWithNestedItems(new[] { this }, n => "Element:" + n.ElementName);
            else
                return GetType().Name;
        }
    }
}

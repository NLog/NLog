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
    using System.Linq;
    using System.Text;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Targets;

    /// <summary>
    /// A specialized layout that renders XML-formatted events.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/XmlLayout">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/XmlLayout">Documentation on NLog Wiki</seealso>
    public abstract class XmlElementBase : Layout
    {
        private Layout[]? _precalculateLayouts;
        private const string DefaultPropertyName = "property";
        private const string DefaultPropertyKeyAttribute = "key";
        private const string DefaultCollectionItemName = "item";

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlElementBase"/> class.
        /// </summary>
        /// <param name="elementName">The name of the top XML node</param>
        /// <param name="elementValue">The value of the top XML node</param>
        protected XmlElementBase(string elementName, Layout elementValue)
        {
            ElementNameInternal = elementName;
            LayoutWrapper.Inner = elementValue;
            ExcludeProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Name of the XML element
        /// </summary>
        internal string ElementNameInternal { get => _elementNameInternal; set => _elementNameInternal = XmlHelper.XmlConvertToElementName(value?.Trim() ?? string.Empty); }
        private string _elementNameInternal = string.Empty;

        /// <summary>
        /// Value inside the XML element
        /// </summary>
        /// <remarks>Upgrade to private protected when using C# 7.2 </remarks>
        internal readonly LayoutRenderers.Wrappers.XmlEncodeLayoutRendererWrapper LayoutWrapper = new LayoutRenderers.Wrappers.XmlEncodeLayoutRendererWrapper();

        /// <summary>
        /// Auto indent and create new lines
        /// </summary>
        /// <remarks>Default: <see langword="false"/></remarks>
        /// <docgen category='Layout Options' order='50' />
        public bool IndentXml { get; set; }

        /// <summary>
        /// Gets the array of xml 'elements' configurations.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [ArrayParameter(typeof(XmlElement), "element")]
        public IList<XmlElement> Elements => _elements;
        private readonly List<XmlElement> _elements = new List<XmlElement>();

        /// <summary>
        /// Gets the array of 'attributes' configurations for the element
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [ArrayParameter(typeof(XmlAttribute), "attribute")]
        public IList<XmlAttribute> Attributes => _attributes;
        private readonly List<XmlAttribute> _attributes = new List<XmlAttribute>();

        /// <summary>
        /// Gets the collection of context properties that should be included with the other properties.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [ArrayParameter(typeof(TargetPropertyWithContext), "contextproperty")]
        public List<TargetPropertyWithContext>? ContextProperties { get; set; }

        /// <summary>
        /// Gets or sets whether empty XML-element should be included in the output.
        /// </summary>
        /// <remarks>Default: <see langword="false"/> . Empty value is either null or empty string</remarks>
        /// <docgen category='Layout Output' order='10' />
        public bool IncludeEmptyValue { get; set; }

        /// <summary>
        /// Gets or sets the option to include all properties from the log event (as XML)
        /// </summary>
        /// <remarks>Default: <see langword="false"/></remarks>
        /// <docgen category='Layout Output' order='10' />
        public bool IncludeEventProperties { get; set; }

        /// <summary>
        /// Gets or sets whether to include the contents of the <see cref="ScopeContext"/> dictionary.
        /// </summary>
        /// <remarks>Default: <see langword="false"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        public bool IncludeScopeProperties { get => _includeScopeProperties ?? (_includeMdlc == true || _includeMdc == true); set => _includeScopeProperties = value; }
        private bool? _includeScopeProperties;

        /// <summary>
        /// Obsolete and replaced by <see cref="IncludeScopeProperties"/> with NLog v5.
        ///
        /// Gets or sets a value indicating whether to include contents of the <see cref="MappedDiagnosticsContext"/> dictionary.
        /// </summary>
        /// <remarks>Default: <see langword="false"/></remarks>
        /// <docgen category='Layout Options' order='100' />
        [Obsolete("Replaced by IncludeScopeProperties. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IncludeMdc { get => _includeMdc ?? false; set => _includeMdc = value; }
        private bool? _includeMdc;

        /// <summary>
        /// Obsolete and replaced by <see cref="IncludeScopeProperties"/> with NLog v5.
        ///
        /// Gets or sets a value indicating whether to include contents of the <see cref="MappedDiagnosticsLogicalContext"/> dictionary.
        /// </summary>
        /// <remarks>Default: <see langword="false"/></remarks>
        /// <docgen category='Layout Options' order='100' />
        [Obsolete("Replaced by IncludeScopeProperties. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IncludeMdlc { get => _includeMdlc ?? false; set => _includeMdlc = value; }
        private bool? _includeMdlc;

        /// <summary>
        /// Obsolete and replaced by <see cref="IncludeEventProperties"/> with NLog v5.
        ///
        /// Gets or sets the option to include all properties from the log event (as XML)
        /// </summary>
        /// <remarks>Default: <see langword="false"/></remarks>
        /// <docgen category='Layout Options' order='100' />
        [Obsolete("Replaced by IncludeEventProperties. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IncludeAllProperties { get => IncludeEventProperties; set => IncludeEventProperties = value; }

        /// <summary>
        /// List of property names to exclude when <see cref="IncludeEventProperties"/> is <see langword="true"/>
        /// </summary>
        /// <docgen category='Layout Options' order='50' />
#if !NET35
        public ISet<string> ExcludeProperties { get; set; }
#else
        public HashSet<string> ExcludeProperties { get; set; }
#endif

        /// <summary>
        /// XML element name to use when rendering properties
        /// </summary>
        /// <remarks>
        /// Support string-format where {0} means property-key-name
        ///
        /// Skips closing element tag when having configured <see cref="PropertiesElementValueAttribute"/>
        /// </remarks>
        /// <docgen category='Layout Options' order='50' />
        public string PropertiesElementName
        {
            get => _propertiesElementName;
            set
            {
                _propertiesElementName = value;
                _propertiesElementNameHasFormat = value?.IndexOf('{') >= 0;
                if (!_propertiesElementNameHasFormat)
                    _propertiesElementName = XmlHelper.XmlConvertToElementName(value?.Trim() ?? string.Empty);
            }
        }
        private string _propertiesElementName = DefaultPropertyName;
        private bool _propertiesElementNameHasFormat;

        /// <summary>
        /// XML attribute name to use when rendering property-key
        ///
        /// When null (or empty) then key-attribute is not included
        /// </summary>
        /// <remarks>Default: <c>key</c> . Newlines in attribute-value will be replaced with <c>&#13;&#10;</c></remarks>
        /// <docgen category='Layout Options' order='50' />
        public string PropertiesElementKeyAttribute { get; set; } = DefaultPropertyKeyAttribute;

        /// <summary>
        /// XML attribute name to use when rendering property-value
        ///
        /// When null (or empty) then value-attribute is not included and
        /// value is formatted as XML-element-value.
        /// </summary>
        /// <remarks>Default: <see cref="string.Empty"/> . Newlines in attribute-value will be replaced with <c>&#13;&#10;</c></remarks>
        /// <docgen category='Layout Options' order='50' />
        public string PropertiesElementValueAttribute { get; set; } = string.Empty;

        /// <summary>
        /// XML element name to use for rendering IList-collections items
        /// </summary>
        /// <remarks>Default: <c>item</c></remarks>
        /// <docgen category='Layout Options' order='50' />
        public string PropertiesCollectionItemName { get; set; } = DefaultCollectionItemName;

        /// <summary>
        /// How far should the XML serializer follow object references before backing off
        /// </summary>
        /// <remarks>Default: <see langword="1"/></remarks>
        /// <docgen category='Layout Options' order='50' />
        public int MaxRecursionLimit { get; set; } = 1;

        private ObjectReflectionCache ObjectReflectionCache => _objectReflectionCache ?? (_objectReflectionCache = new ObjectReflectionCache(LoggingConfiguration.GetServiceProvider()));
        private ObjectReflectionCache? _objectReflectionCache;
        private static readonly IEqualityComparer<object> _referenceEqualsComparer = SingleItemOptimizedHashSet<object>.ReferenceEqualityComparer.Default;
        private const int MaxXmlLength = 512 * 1024;

        /// <inheritdoc/>
        protected override void InitializeLayout()
        {
            base.InitializeLayout();

            if (string.IsNullOrEmpty(ElementNameInternal))
                throw new NLogConfigurationException("XmlLayout Name-property must be assigned. Name is required for valid XML element.");

            if (IncludeScopeProperties)
                ThreadAgnostic = false;

            if (IncludeEventProperties)
                ThreadAgnosticImmutable = true;

            if (_attributes.Count > 0)
            {
                HashSet<string> attributeValidator = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var attribute in _attributes)
                {
                    if (string.IsNullOrEmpty(attribute.Name))
                        throw new NLogConfigurationException($"XmlElement(Name={ElementNameInternal}): Contains invalid XmlAttribute with unassigned Name-property");

                    if (attributeValidator.Contains(attribute.Name))
                    {
                        Common.InternalLogger.Warn("XmlElement(ElementName={0}): Contains duplicate XmlAttribute(Name={1}) (Invalid xml)", ElementNameInternal, attribute.Name);
                    }
                    else
                    {
                        attributeValidator.Add(attribute.Name);
                    }
                }
            }

            if (ContextProperties != null)
            {
                foreach (var contextProperty in ContextProperties)
                {
                    if (string.IsNullOrEmpty(contextProperty.Name))
                        throw new NLogConfigurationException($"XmlElement(Name={ElementNameInternal}): Contains invalid ContextProperty with unassigned Name-property");
                }
            }

            var innerLayouts = LayoutWrapper.Inner is null ? ArrayHelper.Empty<Layout>() : new[] { LayoutWrapper.Inner };
            _precalculateLayouts = (IncludeEventProperties || IncludeScopeProperties) ? null : ResolveLayoutPrecalculation(_attributes.Select(atr => atr.Layout).Concat(_elements.Where(elm => elm.Layout != null).Select(elm => elm.Layout)).Concat(ContextProperties?.Select(ctx => ctx.Layout) ?? Enumerable.Empty<Layout>()).Concat(innerLayouts));
        }

        /// <inheritdoc/>
        protected override void CloseLayout()
        {
            _precalculateLayouts = null;
            base.CloseLayout();
        }

        internal override void PrecalculateBuilder(LogEventInfo logEvent, StringBuilder target)
        {
            PrecalculateBuilderInternal(logEvent, target, _precalculateLayouts);
        }

        /// <inheritdoc/>
        protected override void RenderFormattedMessage(LogEventInfo logEvent, StringBuilder target)
        {
            int orgLength = target.Length;
            RenderXmlFormattedMessage(logEvent, target);
            if (target.Length == orgLength && IncludeEmptyValue && !string.IsNullOrEmpty(ElementNameInternal))
            {
                RenderSelfClosingElement(target, ElementNameInternal);
            }
        }

        /// <inheritdoc/>
        protected override string GetFormattedMessage(LogEventInfo logEvent)
        {
            return RenderAllocateBuilder(logEvent);
        }

        private void RenderXmlFormattedMessage(LogEventInfo logEvent, StringBuilder sb)
        {
            int orgLength = sb.Length;

            // Attributes without element-names should be added to the top XML element
            if (!string.IsNullOrEmpty(ElementNameInternal))
            {
                foreach (var attribute in _attributes)
                {
                    int beforeAttributeLength = sb.Length;
                    if (!RenderAppendXmlAttributeValue(attribute, logEvent, sb, sb.Length == orgLength))
                    {
                        sb.Length = beforeAttributeLength;
                    }
                }

                if (sb.Length != orgLength)
                {
                    bool hasElements = HasNestedXmlElements(logEvent);
                    if (!hasElements)
                    {
                        sb.Append("/>");
                        return;
                    }
                    else
                    {
                        sb.Append('>');
                    }
                }

                if (LayoutWrapper.Inner != null)
                {
                    int beforeElementLength = sb.Length;
                    if (sb.Length == orgLength)
                    {
                        RenderStartElement(sb, ElementNameInternal);
                    }
                    int beforeValueLength = sb.Length;
                    LayoutWrapper.RenderAppendBuilder(logEvent, sb);
                    if (beforeValueLength == sb.Length && !IncludeEmptyValue)
                    {
                        sb.Length = beforeElementLength;
                    }
                }

                if (IndentXml && sb.Length != orgLength)
                    sb.AppendLine();
            }

            foreach (var element in _elements)
            {
                int beforeAttributeLength = sb.Length;
                if (!RenderAppendXmlElementValue(element, logEvent, sb, sb.Length == orgLength))
                {
                    sb.Length = beforeAttributeLength;
                }
            }

            AppendLogEventXmlProperties(logEvent, sb, orgLength);

            if (sb.Length > orgLength && !string.IsNullOrEmpty(ElementNameInternal))
            {
                EndXmlDocument(sb, ElementNameInternal);
            }
        }

        private bool HasNestedXmlElements(LogEventInfo logEvent)
        {
            var innerText = LayoutWrapper.Inner;
            if (!ReferenceEquals(innerText, null) && !ReferenceEquals(innerText, Layout.Empty))
                return true;

            if (_elements.Count > 0)
                return true;

            if (ContextProperties?.Count > 0)
                return true;

            if (IncludeScopeProperties)
                return true;

            if (IncludeEventProperties && logEvent.HasProperties)
                return true;

            return false;
        }

        private void AppendLogEventXmlProperties(LogEventInfo logEventInfo, StringBuilder sb, int orgLength)
        {
            if (ContextProperties != null)
            {
                foreach (var contextProperty in ContextProperties)
                {
                    var propertyValue = contextProperty.RenderValue(logEventInfo);
                    if (!contextProperty.IncludeEmptyValue && StringHelpers.IsNullOrEmptyString(propertyValue))
                        continue;

                    AppendXmlPropertyValue(contextProperty.Name, propertyValue, sb, orgLength);
                }
            }

            if (IncludeScopeProperties)
            {
                bool checkExcludeProperties = ExcludeProperties.Count > 0;
                using (var scopeEnumerator = ScopeContext.GetAllPropertiesEnumerator())
                {
                    while (scopeEnumerator.MoveNext())
                    {
                        var scopeProperty = scopeEnumerator.Current;
                        if (string.IsNullOrEmpty(scopeProperty.Key))
                            continue;

                        if (checkExcludeProperties && ExcludeProperties.Contains(scopeProperty.Key))
                            continue;

                        AppendXmlPropertyValue(scopeProperty.Key, scopeProperty.Value, sb, orgLength);
                    }
                }
            }

            if (IncludeEventProperties)
            {
                AppendLogEventProperties(logEventInfo, sb, orgLength);
            }
        }

        private void AppendLogEventProperties(LogEventInfo logEventInfo, StringBuilder sb, int orgLength)
        {
            if (!logEventInfo.HasProperties)
                return;

            bool checkExcludeProperties = ExcludeProperties.Count > 0;
            using (var propertyEnumerator = logEventInfo.CreatePropertiesInternal().GetPropertyEnumerator())
            {
                while (propertyEnumerator.MoveNext())
                {
                    var prop = propertyEnumerator.CurrentParameter;

                    if (string.IsNullOrEmpty(prop.Name))
                        continue;

                    if (checkExcludeProperties && ExcludeProperties.Contains(prop.Name))
                        continue;

                    var propertyValue = prop.Value;
                    if (!string.IsNullOrEmpty(prop.Format) && propertyValue is IFormattable formattedProperty)
                        propertyValue = formattedProperty.ToString(prop.Format, System.Globalization.CultureInfo.InvariantCulture);
                    else if (prop.CaptureType == MessageTemplates.CaptureType.Stringify)
                        propertyValue = Convert.ToString(prop.Value ?? string.Empty, System.Globalization.CultureInfo.InvariantCulture);

                    AppendXmlPropertyObjectValue(prop.Name, propertyValue, sb, orgLength, default(SingleItemOptimizedHashSet<object>), 0);
                }
            }
        }

        private bool AppendXmlPropertyObjectValue(string propName, object? propertyValue, StringBuilder sb, int orgLength, SingleItemOptimizedHashSet<object> objectsInPath, int depth, bool ignorePropertiesElementName = false)
        {
            if (propertyValue is IConvertible convertibleValue)
            {
                var objTypeCode = convertibleValue.GetTypeCode();
                if (objTypeCode != TypeCode.Object)
                {
                    string xmlValueString = XmlHelper.XmlConvertToString(convertibleValue, objTypeCode, true);
                    AppendXmlPropertyStringValue(propName, xmlValueString, sb, orgLength, false, ignorePropertiesElementName);
                    return true;
                }
            }
            else if (propertyValue is null)
            {
                string xmlValueString = XmlHelper.XmlConvertToString(null, TypeCode.Empty, true);
                AppendXmlPropertyStringValue(propName, xmlValueString, sb, orgLength, false, ignorePropertiesElementName);
                return true;
            }

            int beforeValueLength = sb.Length;
            if (beforeValueLength > MaxXmlLength)
            {
                return false;
            }

            int nextDepth = objectsInPath.Count == 0 ? depth : (depth + 1); // Allow serialization of list-items
            if (nextDepth > MaxRecursionLimit)
            {
                return false;
            }

            if (objectsInPath.Contains(propertyValue))
            {
                return false;
            }

            if (MaxRecursionLimit == 0 || (nextDepth == MaxRecursionLimit && !(propertyValue is System.Collections.IEnumerable)))
            {
                string xmlValueString = XmlHelper.XmlConvertToStringSafe(propertyValue);
                AppendXmlPropertyStringValue(propName, xmlValueString, sb, orgLength, false, ignorePropertiesElementName);
                return true;
            }

            if (propertyValue is System.Collections.IDictionary dict)
            {
                using (StartCollectionScope(ref objectsInPath, dict))
                {
                    AppendXmlDictionaryObject(propName, dict, sb, orgLength, objectsInPath, nextDepth, ignorePropertiesElementName);
                }
            }
            else if (propertyValue is System.Collections.IEnumerable collection)
            {
                if (ObjectReflectionCache.TryLookupExpandoObject(propertyValue, out var propertyValues))
                {
                    using (new SingleItemOptimizedHashSet<object>.SingleItemScopedInsert(propertyValue, ref objectsInPath, false, _referenceEqualsComparer))
                    {
                        AppendXmlObjectPropertyValues(propName, ref propertyValues, sb, orgLength, ref objectsInPath, nextDepth, ignorePropertiesElementName);
                    }
                }
                else
                {
                    using (StartCollectionScope(ref objectsInPath, collection))
                    {
                        AppendXmlCollectionObject(propName, collection, sb, orgLength, objectsInPath, nextDepth, ignorePropertiesElementName);
                    }
                }
            }
            else
            {
                using (new SingleItemOptimizedHashSet<object>.SingleItemScopedInsert(propertyValue, ref objectsInPath, false, _referenceEqualsComparer))
                {
                    var propertyValues = ObjectReflectionCache.LookupObjectProperties(propertyValue);
                    AppendXmlObjectPropertyValues(propName, ref propertyValues, sb, orgLength, ref objectsInPath, nextDepth, ignorePropertiesElementName);
                }
            }

            return true;
        }

        private static SingleItemOptimizedHashSet<object>.SingleItemScopedInsert StartCollectionScope(ref SingleItemOptimizedHashSet<object> objectsInPath, object value)
        {
            return new SingleItemOptimizedHashSet<object>.SingleItemScopedInsert(value, ref objectsInPath, true, _referenceEqualsComparer);
        }

        private void AppendXmlCollectionObject(string propName, System.Collections.IEnumerable collection, StringBuilder sb, int orgLength, SingleItemOptimizedHashSet<object> objectsInPath, int depth, bool ignorePropertiesElementName)
        {
            string propNameElement = AppendXmlPropertyValue(propName, string.Empty, sb, orgLength, true);
            if (!string.IsNullOrEmpty(propNameElement))
            {
                foreach (var item in collection)
                {
                    int beforeValueLength = sb.Length;
                    if (beforeValueLength > MaxXmlLength)
                        break;

                    if (!AppendXmlPropertyObjectValue(PropertiesCollectionItemName, item, sb, orgLength, objectsInPath, depth, true))
                    {
                        sb.Length = beforeValueLength;
                    }
                }
                AppendClosingPropertyTag(propNameElement, sb, ignorePropertiesElementName);
            }
        }

        private void AppendXmlDictionaryObject(string propName, System.Collections.IDictionary dictionary, StringBuilder sb, int orgLength, SingleItemOptimizedHashSet<object> objectsInPath, int depth, bool ignorePropertiesElementName)
        {
            string propNameElement = AppendXmlPropertyValue(propName, string.Empty, sb, orgLength, true, ignorePropertiesElementName);
            if (!string.IsNullOrEmpty(propNameElement))
            {
                foreach (var item in new DictionaryEntryEnumerable(dictionary))
                {
                    int beforeValueLength = sb.Length;
                    if (beforeValueLength > MaxXmlLength)
                        break;

                    var propertyName = item.Key?.ToString() ?? string.Empty;
                    if (string.IsNullOrEmpty(propertyName))
                        continue;

                    if (!AppendXmlPropertyObjectValue(propertyName, item.Value, sb, orgLength, objectsInPath, depth))
                    {
                        sb.Length = beforeValueLength;
                    }
                }
                AppendClosingPropertyTag(propNameElement, sb, ignorePropertiesElementName);
            }
        }

        private void AppendXmlObjectPropertyValues(string propName, ref ObjectReflectionCache.ObjectPropertyList propertyValues, StringBuilder sb, int orgLength, ref SingleItemOptimizedHashSet<object> objectsInPath, int depth, bool ignorePropertiesElementName = false)
        {
            if (propertyValues.IsSimpleValue)
            {
                AppendXmlPropertyValue(propName, propertyValues.ObjectValue, sb, orgLength, false, ignorePropertiesElementName);
                return;
            }

            string propNameElement = AppendXmlPropertyValue(propName, string.Empty, sb, orgLength, true, ignorePropertiesElementName);
            if (string.IsNullOrEmpty(propNameElement))
                return;

            foreach (var property in propertyValues)
            {
                int beforeValueLength = sb.Length;
                if (beforeValueLength > MaxXmlLength)
                    break;

                if (string.IsNullOrEmpty(property.Name) || (!IncludeEmptyValue && StringHelpers.IsNullOrEmptyString(property.Value)))
                    continue;

                var propertyTypeCode = property.TypeCode;
                if (propertyTypeCode != TypeCode.Object)
                {
                    string xmlValueString = XmlHelper.XmlConvertToString((IConvertible?)property.Value, propertyTypeCode, true);
                    AppendXmlPropertyStringValue(property.Name, xmlValueString, sb, orgLength, false, ignorePropertiesElementName);
                }
                else
                {
                    if (!AppendXmlPropertyObjectValue(property.Name, property.Value, sb, orgLength, objectsInPath, depth))
                    {
                        sb.Length = beforeValueLength;
                    }
                }
            }

            AppendClosingPropertyTag(propNameElement, sb, ignorePropertiesElementName);
        }

        private string AppendXmlPropertyValue(string propName, object? propertyValue, StringBuilder sb, int orgLength, bool ignoreValue = false, bool ignorePropertiesElementName = false)
        {
            string xmlValueString = ignoreValue ? string.Empty : XmlHelper.XmlConvertToStringSafe(propertyValue);
            return AppendXmlPropertyStringValue(propName, xmlValueString, sb, orgLength, ignoreValue, ignorePropertiesElementName);
        }

        private string AppendXmlPropertyStringValue(string propName, string xmlValueString, StringBuilder sb, int orgLength, bool ignoreValue = false, bool ignorePropertiesElementName = false)
        {
            if (string.IsNullOrEmpty(PropertiesElementName))
                return string.Empty; // Not supported

            propName = propName?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(propName))
                return string.Empty; // Not supported

            if (sb.Length == orgLength && !string.IsNullOrEmpty(ElementNameInternal))
            {
                BeginXmlDocument(sb, ElementNameInternal);
            }

            if (IndentXml && !string.IsNullOrEmpty(ElementNameInternal))
                sb.Append("  ");

            sb.Append('<');
            string propNameElement;
            if (ignorePropertiesElementName)
            {
                propNameElement = XmlHelper.XmlConvertToElementName(propName);
                sb.Append(propNameElement);
            }
            else
            {
                if (_propertiesElementNameHasFormat)
                {
                    propNameElement = XmlHelper.XmlConvertToElementName(propName);
                    sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, PropertiesElementName, propNameElement);
                }
                else
                {
                    propNameElement = PropertiesElementName;
                    sb.Append(PropertiesElementName);
                }

                RenderAttribute(sb, PropertiesElementKeyAttribute, propName);
            }

            if (ignoreValue)
            {
                sb.Append('>');
                if (IndentXml)
                    sb.AppendLine();
            }
            else if (RenderAttribute(sb, PropertiesElementValueAttribute, xmlValueString))
            {
                sb.Append("/>");
                if (IndentXml)
                    sb.AppendLine();
            }
            else
            {
                sb.Append('>');
                XmlHelper.EscapeXmlString(xmlValueString, false, sb);
                AppendClosingPropertyTag(propNameElement, sb, ignorePropertiesElementName);
            }

            return propNameElement;
        }

        private void AppendClosingPropertyTag(string propNameElement, StringBuilder sb, bool ignorePropertiesElementName = false)
        {
            sb.Append("</");
            if (ignorePropertiesElementName)
                sb.Append(propNameElement);
            else
                sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, PropertiesElementName, propNameElement);
            sb.Append('>');
            if (IndentXml)
                sb.AppendLine();
        }

        /// <summary>
        /// write attribute, only if <paramref name="attributeName"/> is not empty
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="attributeName"></param>
        /// <param name="value"></param>
        /// <returns>rendered</returns>
        private static bool RenderAttribute(StringBuilder sb, string attributeName, string value)
        {
            if (!string.IsNullOrEmpty(attributeName))
            {
                sb.Append(' ');
                sb.Append(attributeName);
                sb.Append("=\"");
                XmlHelper.EscapeXmlString(value, true, sb);
                sb.Append('\"');
                return true;
            }

            return false;
        }

        private bool RenderAppendXmlElementValue(XmlElementBase xmlElement, LogEventInfo logEvent, StringBuilder sb, bool beginXmlDocument)
        {
            string xmlElementName = xmlElement.ElementNameInternal;
            if (string.IsNullOrEmpty(xmlElementName))
                return false;

            if (beginXmlDocument && !string.IsNullOrEmpty(ElementNameInternal))
            {
                BeginXmlDocument(sb, ElementNameInternal);
            }

            if (IndentXml && !string.IsNullOrEmpty(ElementNameInternal))
                sb.Append("  ");

            int beforeValueLength = sb.Length;
            xmlElement.Render(logEvent, sb);
            if (sb.Length == beforeValueLength && !xmlElement.IncludeEmptyValue)
                return false;

            if (IndentXml)
                sb.AppendLine();
            return true;
        }

        private bool RenderAppendXmlAttributeValue(XmlAttribute xmlAttribute, LogEventInfo logEvent, StringBuilder sb, bool beginXmlDocument)
        {
            string xmlKeyString = xmlAttribute.Name;
            if (string.IsNullOrEmpty(xmlKeyString))
                return false;

            if (beginXmlDocument)
            {
                sb.Append('<');
                sb.Append(ElementNameInternal);
            }

            sb.Append(' ');
            sb.Append(xmlKeyString);
            sb.Append("=\"");

            if (!xmlAttribute.RenderAppendXmlValue(logEvent, sb))
                return false;

            sb.Append('\"');
            return true;
        }

        private void BeginXmlDocument(StringBuilder sb, string elementName)
        {
            RenderStartElement(sb, elementName);
            if (IndentXml)
                sb.AppendLine();
        }

        private void EndXmlDocument(StringBuilder sb, string elementName)
        {
            RenderEndElement(sb, elementName);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (_elements.Count > 0)
                return ToStringWithNestedItems(_elements, l => string.IsNullOrEmpty(l.ElementNameInternal) ? l.ToString() : ("TagName=" + l.ElementNameInternal));
            else if (!string.IsNullOrEmpty(ElementNameInternal))
                return ToStringWithNestedItems(new[] { this }, l => "TagName=" + l.ElementNameInternal);
            else if (_attributes.Count > 0)
                return ToStringWithNestedItems(_attributes, a => "Attribute=" + a.Name);
            else if (ContextProperties != null && ContextProperties.Count > 0)
                return ToStringWithNestedItems(ContextProperties, n => "Property=" + n.Name);
            else
                return GetType().Name;
        }

        private static void RenderSelfClosingElement(StringBuilder target, string elementName)
        {
            target.Append('<');
            target.Append(elementName);
            target.Append("/>");
        }

        private static void RenderStartElement(StringBuilder sb, string elementName)
        {
            sb.Append('<');
            sb.Append(elementName);
            sb.Append('>');
        }

        private static void RenderEndElement(StringBuilder sb, string elementName)
        {
            sb.Append("</");
            sb.Append(elementName);
            sb.Append('>');
        }
    }
}

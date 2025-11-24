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
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using NLog.Config;

    /// <summary>
    /// A specialized layout that renders JSON-formatted events.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/JsonLayout">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/JsonLayout">Documentation on NLog Wiki</seealso>
    [Layout("JsonLayout")]
    [ThreadAgnostic]
    public class JsonLayout : Layout
    {
        private const int SpacesPerIndent = 2;
        private Layout[]? _precalculateLayouts;

        private LimitRecursionJsonConvert JsonConverter => _jsonConverter ?? (_jsonConverter = new LimitRecursionJsonConvert(MaxRecursionLimit, SuppressSpaces, ResolveService<IJsonConverter>()));
        private LimitRecursionJsonConvert? _jsonConverter;
        private IValueFormatter ValueFormatter => _valueFormatter ?? (_valueFormatter = ResolveService<IValueFormatter>());
        private IValueFormatter? _valueFormatter;
        private Internal.ObjectReflectionCache ObjectReflectionCache => _objectReflectionCache ?? (_objectReflectionCache = new Internal.ObjectReflectionCache(ResolveService<System.IServiceProvider>()));
        private Internal.ObjectReflectionCache? _objectReflectionCache;

        private sealed class LimitRecursionJsonConvert : IJsonConverter
        {
            private readonly IJsonConverter _converter;
            private readonly Targets.DefaultJsonSerializer? _serializer;
            private readonly Targets.JsonSerializeOptions _serializerOptions;

            public LimitRecursionJsonConvert(int maxRecursionLimit, bool suppressSpaces, IJsonConverter converter)
            {
                _converter = converter;
                _serializer = converter as Targets.DefaultJsonSerializer;
                _serializerOptions = new Targets.JsonSerializeOptions() { MaxRecursionLimit = Math.Max(0, maxRecursionLimit), SuppressSpaces = suppressSpaces, SanitizeDictionaryKeys = true };
            }

            public bool SerializeObject(object? value, StringBuilder builder)
            {
                if (_serializer != null)
                    return _serializer.SerializeObject(value, builder, _serializerOptions);
                else
                    return _converter.SerializeObject(value, builder);
            }

            public bool SerializeObjectNoLimit(object? value, StringBuilder builder)
            {
                return _converter.SerializeObject(value, builder);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonLayout"/> class.
        /// </summary>
        public JsonLayout()
        {
            ExcludeProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the array of attributes' configurations.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [ArrayParameter(typeof(JsonAttribute), "attribute")]
        public IList<JsonAttribute> Attributes => _attributes;
        private readonly List<JsonAttribute> _attributes = new List<JsonAttribute>();

        /// <summary>
        /// Gets or sets the option to suppress the extra spaces in the output json.
        /// </summary>
        /// <remarks>Default: <see langword="true"/></remarks>
        /// <docgen category='Layout Options' order='100' />
        public bool SuppressSpaces
        {
            get => _suppressSpaces;
            set
            {
                if (_suppressSpaces != value)
                {
                    _suppressSpaces = value;
                    RefreshJsonDelimiters();
                }
            }
        }
        private bool _suppressSpaces = true;

        /// <summary>
        /// Gets or sets the option to render the empty object value {}
        /// </summary>
        /// <remarks>Default: <see langword="true"/></remarks>
        /// <docgen category='Layout Options' order='100' />
        public bool RenderEmptyObject { get => _renderEmptyObject ?? true; set => _renderEmptyObject = value; }
        private bool? _renderEmptyObject;

        /// <summary>
        /// Auto indent and create new lines
        /// </summary>
        /// <remarks>Default: <see langword="false"/></remarks>
        /// <docgen category='Layout Options' order='100' />
        public bool IndentJson
        {
            get => _indentJson;
            set
            {
                if (_indentJson != value)
                {
                    _indentJson = value;
                    if (_indentJson)
                        _suppressSpaces = false;
                    RefreshJsonDelimiters();
                }
            }
        }
        private bool _indentJson;

        /// <summary>
        /// Gets or sets whether to flatten nested object properties using dotted notation
        /// </summary>
        /// <remarks>Default: <see langword="false"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        public bool DottedRecursion { get; set; }

        /// <summary>
        /// Gets or sets the option to include all properties from the log event (as JSON)
        /// </summary>
        /// <remarks>Default: <see langword="false"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        public bool IncludeEventProperties { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include contents of the <see cref="GlobalDiagnosticsContext"/> dictionary.
        /// </summary>
        /// <remarks>Default: <see langword="false"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        public bool IncludeGdc { get; set; }

        /// <summary>
        /// Gets or sets whether to include the contents of the <see cref="ScopeContext"/> dictionary.
        /// </summary>
        /// <remarks>Default: <see langword="false"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        public bool IncludeScopeProperties { get => _includeScopeProperties ?? (_includeMdlc == true || _includeMdc == true); set => _includeScopeProperties = value; }
        private bool? _includeScopeProperties;

        /// <summary>
        /// Obsolete and replaced by <see cref="IncludeEventProperties"/> with NLog v5.
        ///
        /// Gets or sets the option to include all properties from the log event (as JSON)
        /// </summary>
        /// <remarks>Default: <see langword="false"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        [Obsolete("Replaced by IncludeEventProperties. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IncludeAllProperties { get => IncludeEventProperties; set => IncludeEventProperties = value; }

        /// <summary>
        /// Obsolete and replaced by <see cref="IncludeScopeProperties"/> with NLog v5.
        ///
        /// Gets or sets a value indicating whether to include contents of the <see cref="MappedDiagnosticsContext"/> dictionary.
        /// </summary>
        /// <remarks>Default: <see langword="false"/></remarks>
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
        /// <remarks>Default: <see langword="false"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        [Obsolete("Replaced by IncludeScopeProperties. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IncludeMdlc { get => _includeMdlc ?? false; set => _includeMdlc = value; }
        private bool? _includeMdlc;

        /// <summary>
        /// Gets or sets the option to exclude null/empty properties from the log event (as JSON)
        /// </summary>
        /// <remarks>Default: <see langword="false"/></remarks>
        /// <docgen category='Layout Options' order='100' />
        public bool ExcludeEmptyProperties { get; set; }

        /// <summary>
        /// List of property names to exclude when <see cref="IncludeAllProperties"/> is true
        /// </summary>
        /// <docgen category='Layout Options' order='100' />
#if !NET35
        public ISet<string> ExcludeProperties { get; set; }
#else
        public HashSet<string> ExcludeProperties { get; set; }
#endif

        /// <summary>
        /// How far should the JSON serializer follow object references before backing off
        /// </summary>
        /// <remarks>Default: <see langword="1"/></remarks>
        /// <docgen category='Layout Options' order='100' />
        public int MaxRecursionLimit { get; set; } = 1;

        /// <summary>
        /// Should forward slashes be escaped? If <see langword="true"/>, / will be converted to \/
        /// </summary>
        /// <remarks>Default: <see langword="false"/></remarks>
        /// <docgen category='Layout Options' order='100' />
        [Obsolete("Marked obsolete with NLog 5.5. Should never escape forward slash")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool EscapeForwardSlash { get; set; }

        /// <inheritdoc/>
        protected override void InitializeLayout()
        {
            base.InitializeLayout();

            if (IncludeScopeProperties)
            {
                ThreadAgnostic = false;
            }
            if (IncludeEventProperties)
            {
                ThreadAgnosticImmutable = true;
            }

            _precalculateLayouts = (IncludeScopeProperties || IncludeEventProperties) ? null : ResolveLayoutPrecalculation(Attributes.Select(atr => atr.Layout));

            foreach (var attribute in _attributes)
            {
                if (string.IsNullOrEmpty(attribute.Name))
                    throw new NLogConfigurationException("JsonLayout: Contains invalid JsonAttribute with unassigned Name-property");

                if (!attribute.Encode && attribute.Layout is JsonLayout jsonLayout)
                {
                    if (!attribute.IncludeEmptyValue && !jsonLayout._renderEmptyObject.HasValue)
                        jsonLayout.RenderEmptyObject = false;

                    if (!SuppressSpaces || IndentJson)
                        jsonLayout.SuppressSpaces = false;
                }
            }
        }

        /// <inheritdoc/>
        protected override void CloseLayout()
        {
            _jsonConverter = null;
            _valueFormatter = null;
            _objectReflectionCache = null;
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
            RenderJsonFormattedMessage(logEvent, target);
            if (target.Length == orgLength && RenderEmptyObject)
            {
                target.Append(SuppressSpaces ? "{}" : "{ }");
            }
        }

        /// <inheritdoc/>
        protected override string GetFormattedMessage(LogEventInfo logEvent)
        {
            return RenderAllocateBuilder(logEvent);
        }

        private void RenderJsonFormattedMessage(LogEventInfo logEvent, StringBuilder sb)
        {
            int orgLength = sb.Length;

            foreach (var attribute in _attributes)
            {
                RenderAppendJsonPropertyValue(attribute, logEvent, sb, sb.Length == orgLength);
            }

            if (IncludeGdc)
            {
                var gdcKeys = GlobalDiagnosticsContext.GetNames();
                if (gdcKeys.Count > 0)
                {
                    foreach (string key in gdcKeys)
                    {
                        if (string.IsNullOrEmpty(key))
                            continue;

                        var propertyValue = GlobalDiagnosticsContext.GetObject(key);
                        AppendJsonPropertyValue(key, propertyValue, sb, sb.Length == orgLength);
                    }
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

                        AppendJsonPropertyValue(scopeProperty.Key, scopeProperty.Value, sb, sb.Length == orgLength);
                    }
                }
            }

            if (IncludeEventProperties && logEvent.HasProperties)
            {
                bool checkExcludeProperties = ExcludeProperties.Count > 0;
                using (var propertyEnumerator = logEvent.CreatePropertiesInternal().GetPropertyEnumerator())
                {
                    while (propertyEnumerator.MoveNext())
                    {
                        var prop = propertyEnumerator.CurrentParameter;
                        if (string.IsNullOrEmpty(prop.Name))
                            continue;

                        if (checkExcludeProperties && ExcludeProperties.Contains(prop.Name))
                            continue;

                        if (DottedRecursion)
                        {
                            AppendFlattenedPropertyValue(prop.Name, prop.Value, prop.Format, logEvent.FormatProvider, prop.CaptureType, sb, sb.Length == orgLength);
                        }
                        else
                        {
                            AppendJsonPropertyValue(prop.Name, prop.Value, prop.Format, logEvent.FormatProvider, prop.CaptureType, sb, sb.Length == orgLength);
                        }
                    }
                }
            }

            if (sb.Length > orgLength)
                sb.Append(_completeJsonMessage);
        }

        private void BeginJsonProperty(StringBuilder sb, string propName, bool beginJsonMessage, bool ensureStringEscape)
        {
            sb.Append(beginJsonMessage ? _beginJsonMessage : _beginJsonPropertyName);

            if (ensureStringEscape)
                Targets.DefaultJsonSerializer.AppendStringEscape(sb, propName, false);
            else
                sb.Append(propName);

            sb.Append(_completeJsonPropertyName);
        }

        private string _beginJsonMessage = "{\"";
        private string _completeJsonMessage = "}";
        private string _beginJsonPropertyName = ",\"";
        private string _completeJsonPropertyName = "\":";

        private void RefreshJsonDelimiters()
        {
            if (IndentJson)
                _beginJsonMessage = new StringBuilder().Append('{').AppendLine().Append(' ', SpacesPerIndent).Append('"').ToString();
            else
                _beginJsonMessage = SuppressSpaces ? "{\"" : "{ \"";

            if (IndentJson)
                _completeJsonMessage = new StringBuilder().AppendLine().Append('}').ToString().ToString();
            else
                _completeJsonMessage = SuppressSpaces ? "}" : " }";

            if (IndentJson)
                _beginJsonPropertyName = new StringBuilder().Append(',').AppendLine().Append(' ', SpacesPerIndent).Append('"').ToString();
            else
                _beginJsonPropertyName = SuppressSpaces ? ",\"" : ", \"";

            _completeJsonPropertyName = SuppressSpaces ? "\":" : "\": ";
        }

        private void AppendJsonPropertyValue(string propName, object? propertyValue, StringBuilder sb, bool beginJsonMessage)
        {
            if (ExcludeEmptyProperties && (propertyValue is null || ReferenceEquals(propertyValue, string.Empty)))
                return;

            var initialLength = sb.Length;
            BeginJsonProperty(sb, propName, beginJsonMessage, true);
            if (!JsonConverter.SerializeObject(propertyValue, sb))
            {
                sb.Length = initialLength;
                return;
            }

            if (ExcludeEmptyProperties && sb[sb.Length - 1] == '"' && sb[sb.Length - 2] == '"' && sb[sb.Length - 3] != '\\')
            {
                sb.Length = initialLength;
            }
        }
        private void AppendJsonPropertyValue(string propName, object? propertyValue, string? format, IFormatProvider? formatProvider, MessageTemplates.CaptureType captureType, StringBuilder sb, bool beginJsonMessage)
        {
            AppendPropertyValueInternal(propName, propertyValue, format, formatProvider, captureType, sb, beginJsonMessage);
        }

        private void AppendFlattenedPropertyValue(string propName, object? propertyValue, string? format, IFormatProvider? formatProvider, MessageTemplates.CaptureType captureType, StringBuilder sb, bool beginJsonMessage)
        {
            if (captureType == MessageTemplates.CaptureType.Stringify)
            {
                AppendPropertyValueInternal(propName, propertyValue, format, formatProvider, captureType, sb, beginJsonMessage);
            }
            else
            {
                // Allow flattening also for Serialize, by starting at a negative depth to effectively loosen depth bound
                int startDepth = captureType == MessageTemplates.CaptureType.Serialize
                    ? Math.Min(0, MaxRecursionLimit - 10)
                    : 0;

                FlattenObjectProperties(propName, propertyValue, sb, beginJsonMessage, startDepth);
            }
        }

        private void AppendPropertyValueInternal(string propName, object? propertyValue, string? format, IFormatProvider? formatProvider, MessageTemplates.CaptureType captureType, StringBuilder sb, bool beginJsonMessage)
        {
            if (captureType == MessageTemplates.CaptureType.Serialize && MaxRecursionLimit <= 1)
            {
                if (ExcludeEmptyProperties && propertyValue is null)
                    return;

                var initialLength = sb.Length;
                BeginJsonProperty(sb, propName, beginJsonMessage, true);

                // Overrides MaxRecursionLimit as message-template tells us it is safe
                if (!JsonConverter.SerializeObjectNoLimit(propertyValue, sb))
                {
                    sb.Length = initialLength;
                }
            }
            else if (captureType == MessageTemplates.CaptureType.Stringify)
            {
                if (ExcludeEmptyProperties && Internal.StringHelpers.IsNullOrEmptyString(propertyValue))
                    return;

                BeginJsonProperty(sb, propName, beginJsonMessage, true);

                sb.Append('"');
                int valueStart = sb.Length;
                ValueFormatter.FormatValue(propertyValue, format, captureType, formatProvider, sb);
                Targets.DefaultJsonSerializer.PerformJsonEscapeWhenNeeded(sb, valueStart, false);
                sb.Append('"');
            }
            else
            {
                AppendJsonPropertyValue(propName, propertyValue, sb, beginJsonMessage);
            }
        }

        private void FlattenObjectProperties(string basePropertyName, object? propertyValue, StringBuilder sb, bool beginJsonMessage, int depth = 0)
        {
            if (depth >= MaxRecursionLimit)
            {
                AppendJsonPropertyValue(basePropertyName, propertyValue, sb, beginJsonMessage);
                return;
            }

            if (ExcludeEmptyProperties && (propertyValue is null || ReferenceEquals(propertyValue, string.Empty)))
                return;

            if (propertyValue is null || propertyValue is string || (propertyValue is IConvertible c && c.GetTypeCode() != TypeCode.Object))
            {
                AppendJsonPropertyValue(basePropertyName, propertyValue, sb, beginJsonMessage);
                return;
            }

            if (propertyValue is IEnumerable && !ObjectReflectionCache.TryLookupExpandoObject(propertyValue, out _))
            {
                AppendJsonPropertyValue(basePropertyName, propertyValue, sb, beginJsonMessage);
                return;
            }
            var objectPropertyList = ObjectReflectionCache.LookupObjectProperties(propertyValue);
            if (objectPropertyList.IsSimpleValue)
            {
                AppendJsonPropertyValue(basePropertyName, objectPropertyList.ObjectValue, sb, beginJsonMessage);
                return;
            }

            bool isFirstChild = beginJsonMessage;
            foreach (var property in objectPropertyList)
            {
                if (!property.HasNameAndValue)
                    continue;

                string dottedPropertyName = string.Concat(basePropertyName, ".", property.Name);
                int beforeLength = sb.Length;
                FlattenObjectProperties(dottedPropertyName, property.Value, sb, isFirstChild, depth + 1);
                if (sb.Length != beforeLength)
                {
                    isFirstChild = false;
                }
            }
        }

        private void RenderAppendJsonPropertyValue(JsonAttribute attrib, LogEventInfo logEvent, StringBuilder sb, bool beginJsonMessage)
        {
            var initialLength = sb.Length;
            BeginJsonProperty(sb, attrib.Name, beginJsonMessage, false);
            if (!attrib.RenderAppendJsonValue(logEvent, JsonConverter, sb))
            {
                sb.Length = initialLength;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (_attributes.Count > 0)
                return ToStringWithNestedItems(_attributes, a => string.Concat(a.Name, "=", a.Layout?.ToString()));
            else if (IncludeEventProperties)
                return $"{GetType().Name}: IncludeEventProperties=true";
            else
                return GetType().Name;
        }
    }
}

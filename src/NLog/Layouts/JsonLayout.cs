// 
// Copyright (c) 2004-2019 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.Text;
    using NLog.Config;

    /// <summary>
    /// A specialized layout that renders JSON-formatted events.
    /// </summary>
    [Layout("JsonLayout")]
    [ThreadAgnostic]
    [ThreadSafe]
    public class JsonLayout : Layout
    {
        private LimitRecursionJsonConvert JsonConverter
        {
            get => _jsonConverter ?? (_jsonConverter = new LimitRecursionJsonConvert(MaxRecursionLimit, ConfigurationItemFactory.Default.JsonConverter));
            set => _jsonConverter = value;
        }
        private LimitRecursionJsonConvert _jsonConverter;
        private IValueFormatter ValueFormatter
        {
            get => _valueFormatter ?? (_valueFormatter = ConfigurationItemFactory.Default.ValueFormatter);
            set => _valueFormatter = value;
        }
        private IValueFormatter _valueFormatter;

        class LimitRecursionJsonConvert : IJsonConverter
        {
            readonly IJsonConverter _converter;
            readonly Targets.DefaultJsonSerializer _serializer;
            readonly Targets.JsonSerializeOptions _serializerOptions;

            public LimitRecursionJsonConvert(int maxRecursionLimit, IJsonConverter converter)
            {
                _converter = converter;
                _serializer = converter as Targets.DefaultJsonSerializer;
                _serializerOptions = new Targets.JsonSerializeOptions() { MaxRecursionLimit = Math.Max(0, maxRecursionLimit) };
            }

            public bool SerializeObject(object value, StringBuilder builder)
            {
                if (_serializer != null)
                    return _serializer.SerializeObject(value, builder, _serializerOptions);
                else
                    return _converter.SerializeObject(value, builder);
            }

            public void SerializeObjectNoLimit(object value, StringBuilder builder)
            {
                _converter.SerializeObject(value, builder);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonLayout"/> class.
        /// </summary>
        public JsonLayout()
        {
            Attributes = new List<JsonAttribute>();
            RenderEmptyObject = true;
            IncludeAllProperties = false;
            ExcludeProperties = new HashSet<string>();
            MaxRecursionLimit = 0;  // Will enumerate simple collections but not object properties. TODO NLog 5.0 change to 1 (or higher)
        }

        /// <summary>
        /// Gets the array of attributes' configurations.
        /// </summary>
        /// <docgen category='JSON Options' order='10' />
        [ArrayParameter(typeof(JsonAttribute), "attribute")]
        public IList<JsonAttribute> Attributes { get; private set; }

        /// <summary>
        /// Gets or sets the option to suppress the extra spaces in the output json
        /// </summary>
        /// <docgen category='JSON Options' order='10' />
        public bool SuppressSpaces { get; set; }

        /// <summary>
        /// Gets or sets the option to render the empty object value {}
        /// </summary>
        /// <docgen category='JSON Options' order='10' />
        public bool RenderEmptyObject { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include contents of the <see cref="GlobalDiagnosticsContext"/> dictionary.
        /// </summary>
        /// <docgen category='JSON Options' order='10' />
        public bool IncludeGdc { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include contents of the <see cref="MappedDiagnosticsContext"/> dictionary.
        /// </summary>
        /// <docgen category='JSON Options' order='10' />
        public bool IncludeMdc { get; set; }

#if !SILVERLIGHT
        /// <summary>
        /// Gets or sets a value indicating whether to include contents of the <see cref="MappedDiagnosticsLogicalContext"/> dictionary.
        /// </summary>
        /// <docgen category='JSON Options' order='10' />
        public bool IncludeMdlc { get; set; }
#endif

        /// <summary>
        /// Gets or sets the option to include all properties from the log event (as JSON)
        /// </summary>
        /// <docgen category='JSON Options' order='10' />
        public bool IncludeAllProperties { get; set; }

        /// <summary>
        /// List of property names to exclude when <see cref="IncludeAllProperties"/> is true
        /// </summary>
        /// <docgen category='JSON Options' order='10' />
#if NET3_5
        public HashSet<string> ExcludeProperties { get; set; }
#else
        public ISet<string> ExcludeProperties { get; set; }
#endif

        /// <summary>
        /// How far should the JSON serializer follow object references before backing off
        /// </summary>
        /// <docgen category='JSON Options' order='10' />
        public int MaxRecursionLimit { get; set; }

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
        }

        /// <summary>
        /// Closes the layout.
        /// </summary>
        protected override void CloseLayout()
        {
            JsonConverter = null;
            ValueFormatter = null;
            base.CloseLayout();
        }

        internal override void PrecalculateBuilder(LogEventInfo logEvent, StringBuilder target)
        {
            PrecalculateBuilderInternal(logEvent, target);
        }

        /// <summary>
        /// Formats the log event as a JSON document for writing.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        /// <param name="target"><see cref="StringBuilder"/> for the result</param>
        protected override void RenderFormattedMessage(LogEventInfo logEvent, StringBuilder target)
        {
            int orgLength = target.Length;
            RenderJsonFormattedMessage(logEvent, target);
            if (target.Length == orgLength && RenderEmptyObject)
            {
                target.Append(SuppressSpaces ? "{}" : "{  }");
            }
        }

        /// <summary>
        /// Formats the log event as a JSON document for writing.
        /// </summary>
        /// <param name="logEvent">The log event to be formatted.</param>
        /// <returns>A JSON string representation of the log event.</returns>
        protected override string GetFormattedMessage(LogEventInfo logEvent)
        {
            return RenderAllocateBuilder(logEvent);
        }

        private void RenderJsonFormattedMessage(LogEventInfo logEvent, StringBuilder sb)
        {
            int orgLength = sb.Length;

            //Memory profiling pointed out that using a foreach-loop was allocating
            //an Enumerator. Switching to a for-loop avoids the memory allocation.
            for (int i = 0; i < Attributes.Count; i++)
            {
                var attrib = Attributes[i];
                int beforeAttribLength = sb.Length;
                if (!RenderAppendJsonPropertyValue(attrib, logEvent, sb, sb.Length == orgLength))
                {
                    sb.Length = beforeAttribLength;
                }
            }

            if(IncludeGdc)
            {
                foreach (string key in GlobalDiagnosticsContext.GetNames())
                {
                    if (string.IsNullOrEmpty(key))
                        continue;
                    object propertyValue = GlobalDiagnosticsContext.GetObject(key);
                    AppendJsonPropertyValue(key, propertyValue, null, null, MessageTemplates.CaptureType.Unknown, sb, sb.Length == orgLength);
                }
            }

            if (IncludeMdc)
            {
                foreach (string key in MappedDiagnosticsContext.GetNames())
                {
                    if (string.IsNullOrEmpty(key))
                        continue;
                    object propertyValue = MappedDiagnosticsContext.GetObject(key);
                    AppendJsonPropertyValue(key, propertyValue, null, null, MessageTemplates.CaptureType.Unknown, sb, sb.Length == orgLength);
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
                    AppendJsonPropertyValue(key, propertyValue, null, null, MessageTemplates.CaptureType.Unknown, sb, sb.Length == orgLength);
                }
            }
#endif

            if (IncludeAllProperties && logEvent.HasProperties)
            {
                IEnumerable<MessageTemplates.MessageTemplateParameter> propertiesList = logEvent.CreateOrUpdatePropertiesInternal(true);
                foreach (var prop in propertiesList)
                {
                    if (string.IsNullOrEmpty(prop.Name))
                        continue;

                    if (ExcludeProperties.Contains(prop.Name))
                        continue;

                    AppendJsonPropertyValue(prop.Name, prop.Value, prop.Format, logEvent.FormatProvider, prop.CaptureType, sb, sb.Length == orgLength);
                }
            }

            if (sb.Length > orgLength)
                CompleteJsonMessage(sb);
        }

        private void BeginJsonProperty(StringBuilder sb, string propName, bool beginJsonMessage)
        {
            if (beginJsonMessage)
            {
                sb.Append(SuppressSpaces ? "{" : "{ ");
            }
            else
            {
                sb.Append(',');
                if (!SuppressSpaces)
                    sb.Append(' ');
            }

            sb.Append('"');
            sb.Append(propName);
            sb.Append('"');
            sb.Append(':');
            if (!SuppressSpaces)
                sb.Append(' ');
        }

        private void CompleteJsonMessage(StringBuilder sb)
        {
            sb.Append(SuppressSpaces ? "}" : " }");
        }

        private void AppendJsonPropertyValue(string propName, object propertyValue, string format, IFormatProvider formatProvider, MessageTemplates.CaptureType captureType, StringBuilder sb, bool beginJsonMessage)
        {
            BeginJsonProperty(sb, propName, beginJsonMessage);
            if (MaxRecursionLimit <= 1 && captureType == MessageTemplates.CaptureType.Serialize)
            {
                // Overrides MaxRecursionLimit as message-template tells us it is safe
                JsonConverter.SerializeObjectNoLimit(propertyValue, sb);
            }
            else if (captureType == MessageTemplates.CaptureType.Stringify)
            {
                // Overrides MaxRecursionLimit as message-template tells us it is unsafe
                int originalStart = sb.Length;
                ValueFormatter.FormatValue(propertyValue, format, captureType, formatProvider, sb);
                PerformJsonEscapeIfNeeded(sb, originalStart);
            }
            else
            {
                JsonConverter.SerializeObject(propertyValue, sb);
            }
        }

        private static void PerformJsonEscapeIfNeeded(StringBuilder sb, int valueStart)
        {
            if (sb.Length - valueStart <= 2)
                return;

            for (int i = valueStart + 1; i < sb.Length - 1; ++i)
            {
                if (Targets.DefaultJsonSerializer.RequiresJsonEscape(sb[i], false))
                {
                    var jsonEscape = sb.ToString(valueStart + 1, sb.Length - valueStart - 2);
                    sb.Length = valueStart;
                    sb.Append('"');
                    Targets.DefaultJsonSerializer.AppendStringEscape(sb, jsonEscape, false);
                    sb.Append('"');
                    break;
                }
            }
        }

        private bool RenderAppendJsonPropertyValue(JsonAttribute attrib, LogEventInfo logEvent, StringBuilder sb, bool beginJsonMessage)
        {
            BeginJsonProperty(sb, attrib.Name, beginJsonMessage);
            if (attrib.Encode)
            {
                // "\"{0}\":{1}\"{2}\""
                sb.Append('"');
            }
            int beforeValueLength = sb.Length;
            attrib.LayoutWrapper.RenderAppendBuilder(logEvent, sb);
            if (!attrib.IncludeEmptyValue && beforeValueLength == sb.Length)
            {
                return false;
            }
            if (attrib.Encode)
            {
                sb.Append('"');
            }
            return true;
        }

        /// <summary>
        /// Generate description of JSON Layout
        /// </summary>
        /// <returns>JSON Layout String Description</returns>
        public override string ToString()
        {
            return ToStringWithNestedItems(Attributes, a => string.Concat(a.Name, "-", a.Layout?.ToString()));
        }
    }
}
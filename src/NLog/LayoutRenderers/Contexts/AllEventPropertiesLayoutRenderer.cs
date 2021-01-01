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
    using System.Text;
    using System.Collections.Generic;
    using System.ComponentModel;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// Log event context data.
    /// </summary>
    [LayoutRenderer("all-event-properties")]
    [ThreadAgnostic]
    [ThreadSafe]
    [MutableUnsafe]
    public class AllEventPropertiesLayoutRenderer : LayoutRenderer
    {
        private string _format;
        private string _beforeKey;
        private string _afterKey;
        private string _afterValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="AllEventPropertiesLayoutRenderer"/> class.
        /// </summary>
        public AllEventPropertiesLayoutRenderer()
        {
            Separator = ", ";
            Format = "[key]=[value]";
            Exclude = new HashSet<string>(
                ArrayHelper.Empty<string>(),
                StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets or sets string that will be used to separate key/value pairs.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        public string Separator { get; set; }

        /// <summary>
        /// Get or set if empty values should be included.
        ///
        /// A value is empty when null or in case of a string, null or empty string.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(false)]
        public bool IncludeEmptyValues { get; set; } = false;

        /// <summary>
        /// Gets or sets the keys to exclude from the output. If omitted, none are excluded.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
#if !NET35
        public ISet<string> Exclude { get; set; }
#else
        public HashSet<string> Exclude { get; set; }   
#endif

        /// <summary>
        /// Gets or sets how key/value pairs will be formatted.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        public string Format
        {
            get => _format;
            set
            {
                if (!value.Contains("[key]"))
                    throw new ArgumentException("Invalid format: [key] placeholder is missing.");

                if (!value.Contains("[value]"))
                    throw new ArgumentException("Invalid format: [value] placeholder is missing.");

                _format = value;

                var formatSplit = _format.Split(new[] { "[key]", "[value]" }, StringSplitOptions.None);
                if (formatSplit.Length == 3)
                {
                    _beforeKey = formatSplit[0];
                    _afterKey = formatSplit[1];
                    _afterValue = formatSplit[2];
                }
                else
                {
                    _beforeKey = null;
                    _afterKey = null;
                    _afterValue = null;
                }
            }
        }

        /// <summary>
        /// Renders all log event's properties and appends them to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (!logEvent.HasProperties)
                return;

            var formatProvider = GetFormatProvider(logEvent);
            bool checkForExclude = Exclude?.Count > 0;
            bool nonStandardFormat = _beforeKey == null || _afterKey == null || _afterValue == null;

            bool first = true;
            foreach (var property in logEvent.Properties)
            {
                if (!IncludeEmptyValues && IsEmptyPropertyValue(property.Value))
                    continue;

                if (checkForExclude && property.Key is string propertyKey && Exclude.Contains(propertyKey))
                    continue;

                if (!first)
                {
                    builder.Append(Separator);
                }

                first = false;

                if (nonStandardFormat)
                {
                    var key = Convert.ToString(property.Key, formatProvider);
                    var value = Convert.ToString(property.Value, formatProvider);
                    var pair = Format.Replace("[key]", key)
                                     .Replace("[value]", value);
                    builder.Append(pair);
                }
                else
                {
                    builder.Append(_beforeKey);
                    builder.AppendFormattedValue(property.Key, null, formatProvider, ValueFormatter);
                    builder.Append(_afterKey);
                    builder.AppendFormattedValue(property.Value, null, formatProvider, ValueFormatter);
                    builder.Append(_afterValue);
                }
            }
        }

        private static bool IsEmptyPropertyValue(object value)
        {
            if (value is string s)
            {
                return string.IsNullOrEmpty(s);
            }

            return value == null;
        }
    }
}
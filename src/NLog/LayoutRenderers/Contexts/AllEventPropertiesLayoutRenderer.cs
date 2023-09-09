// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using NLog.Config;
    using NLog.Internal;
    using System.Globalization;

    /// <summary>
    /// Log event context data.
    /// </summary>
    [LayoutRenderer("all-event-properties")]
    [ThreadAgnostic]
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
            Exclude = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets or sets string that will be used to separate key/value pairs.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public string Separator { get; set; }

        /// <summary>
        /// Get or set if empty values should be included.
        ///
        /// A value is empty when null or in case of a string, null or empty string.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public bool IncludeEmptyValues { get; set; }

        /// <summary>
        /// Gets or sets whether to include the contents of the <see cref="ScopeContext"/> properties-dictionary.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public bool IncludeScopeProperties { get; set; }

        /// <summary>
        /// Gets or sets the keys to exclude from the output. If omitted, none are excluded.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
#if !NET35
        public ISet<string> Exclude { get; set; }
#else
        public HashSet<string> Exclude { get; set; }   
#endif

        /// <summary>
        /// Enables capture of ScopeContext-properties from active thread context
        /// </summary>
        public LayoutRenderer FixScopeContext => IncludeScopeProperties ? _fixScopeContext : null;
        private static readonly LayoutRenderer _fixScopeContext = new ScopeContextPropertyLayoutRenderer() { Item = string.Empty };

        /// <summary>
        /// Gets or sets how key/value pairs will be formatted.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public string Format
        {
            get => _format;
            set
            {
                if (string.IsNullOrEmpty(value) || value.IndexOf("[key]", StringComparison.Ordinal) < 0)
                    throw new ArgumentException("Invalid format: [key] placeholder is missing.");

                if (value.IndexOf("[value]", StringComparison.Ordinal) < 0)
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
        /// Gets or sets the culture used for rendering. 
        /// </summary>
        /// <docgen category='Layout Options' order='100' />
        public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

        /// <inheritdoc/>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (!logEvent.HasProperties && !IncludeScopeProperties)
                return;

            var formatProvider = GetFormatProvider(logEvent, Culture);
            bool checkForExclude = Exclude?.Count > 0;
            bool nonStandardFormat = _beforeKey is null || _afterKey is null || _afterValue is null;

            bool includeSeparator = false;
            if (logEvent.HasProperties)
            {
                IEnumerable<MessageTemplates.MessageTemplateParameter> propertiesList = logEvent.CreateOrUpdatePropertiesInternal(true);
                foreach (var property in propertiesList)
                {
                    if (AppendProperty(builder, property.Name, property.Value, property.Format, formatProvider, includeSeparator, checkForExclude, nonStandardFormat))
                    {
                        includeSeparator = true;
                    }
                }
            }

            if (IncludeScopeProperties)
            {
                using (var scopeEnumerator = ScopeContext.GetAllPropertiesEnumerator())
                {
                    while (scopeEnumerator.MoveNext())
                    {
                        var property = scopeEnumerator.Current;
                        if (AppendProperty(builder, property.Key, property.Value, null, formatProvider, includeSeparator, checkForExclude, nonStandardFormat))
                        {
                            includeSeparator = true;
                        }
                    }
                }
            }
        }

        private bool AppendProperty(StringBuilder builder, object propertyKey, object propertyValue, string propertyFormat, IFormatProvider formatProvider, bool includeSeparator, bool checkForExclude, bool nonStandardFormat)
        {
            if (!IncludeEmptyValues && IsEmptyPropertyValue(propertyValue))
                return false;

            if (checkForExclude && Exclude.Contains(propertyKey as string ?? string.Empty))
                return false;

            if (includeSeparator)
            {
                builder.Append(Separator);
            }

            if (nonStandardFormat)
            {
                var key = Convert.ToString(propertyKey, formatProvider);
                var value = Convert.ToString(propertyValue, formatProvider);
                var pair = Format.Replace("[key]", key)
                                 .Replace("[value]", value);
                builder.Append(pair);
            }
            else
            {
                builder.Append(_beforeKey);
                builder.AppendFormattedValue(propertyKey, null, formatProvider, ValueFormatter);
                builder.Append(_afterKey);
                builder.AppendFormattedValue(propertyValue, propertyFormat, formatProvider, ValueFormatter);
                builder.Append(_afterValue);
            }

            return true;
        }

        private static bool IsEmptyPropertyValue(object value)
        {
            if (value is string s)
            {
                return string.IsNullOrEmpty(s);
            }

            return value is null;
        }
    }
}
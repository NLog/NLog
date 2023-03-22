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
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using NLog.Internal;
    using NLog.Layouts;

    /// <summary>
    /// Renders the nested states from <see cref="ScopeContext"/> like a callstack
    /// </summary>
    [LayoutRenderer("scopenested")]
    public sealed class ScopeContextNestedStatesLayoutRenderer : LayoutRenderer
    {
        /// <summary>
        /// Gets or sets the number of top stack frames to be rendered.
        /// </summary>
        /// <docgen category='Layout Options' order='100' />
        public int TopFrames { get; set; } = -1;

        /// <summary>
        /// Gets or sets the number of bottom stack frames to be rendered.
        /// </summary>
        /// <docgen category='Layout Options' order='100' />
        public int BottomFrames { get; set; } = -1;

        /// <summary>
        /// Gets or sets the separator to be used for concatenating nested logical context output.
        /// </summary>
        /// <docgen category='Layout Options' order='100' />
        public string Separator { get => _separator?.OriginalText; set => _separator = new SimpleLayout(value ?? string.Empty); } 
        private SimpleLayout _separator = new SimpleLayout(" ");

        /// <summary>
        /// Gets or sets how to format each nested state. Ex. like JSON = @
        /// </summary>
        /// <docgen category='Layout Options' order='50' />
        public string Format { get; set; }

        /// <summary>
        /// Gets or sets the culture used for rendering. 
        /// </summary>
        /// <docgen category='Layout Options' order='100' />
        public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

        /// <inheritdoc/>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (TopFrames == 1)
            {
                // Allows fast rendering of topframes=1
                var topFrame = ScopeContext.PeekNestedState();
                if (topFrame != null)
                    builder.AppendFormattedValue(topFrame, Format, GetFormatProvider(logEvent, Culture), ValueFormatter);
                return;
            }

            var messages = ScopeContext.GetAllNestedStateList();
            if (messages.Count == 0)
                return;

            int startPos = 0;
            int endPos = messages.Count;

            if (TopFrames != -1)
            {
                endPos = Math.Min(TopFrames, messages.Count);
            }
            else if (BottomFrames != -1)
            {
                startPos = messages.Count - Math.Min(BottomFrames, messages.Count);
            }

            AppendNestedStates(builder, messages, startPos, endPos, logEvent);
        }

        private void AppendNestedStates(StringBuilder builder, IList<object> messages, int startPos, int endPos, LogEventInfo logEvent)
        {
            bool formatAsJson = MessageTemplates.ValueFormatter.FormatAsJson.Equals(Format, StringComparison.Ordinal);
            var formatProvider = GetFormatProvider(logEvent, Culture);

            string separator = null;
            string itemSeparator = null;
            if (formatAsJson)
            {
                separator = _separator?.Render(logEvent) ?? string.Empty;
                builder.Append('[');
                builder.Append(separator);
                itemSeparator = "," + separator;
            }

            try
            {
                string currentSeparator = null;
                for (int i = endPos - 1; i >= startPos; --i)
                {
                    builder.Append(currentSeparator);
                    if (formatAsJson)
                        AppendJsonFormattedValue(messages[i], formatProvider, builder, separator, itemSeparator);
                    else if (messages[i] is IEnumerable<KeyValuePair<string, object>>)
                        builder.Append(Convert.ToString(messages[i]));   // Special support for Microsoft Extension Logging ILogger.BeginScope
                    else
                        builder.AppendFormattedValue(messages[i], Format, formatProvider, ValueFormatter);
                    currentSeparator = itemSeparator ?? _separator?.Render(logEvent) ?? string.Empty;
                }
            }
            finally
            {
                if (formatAsJson)
                {
                    builder.Append(separator);
                    builder.Append(']');
                }
            }
        }

        private void AppendJsonFormattedValue(object nestedState, IFormatProvider formatProvider, StringBuilder builder, string separator, string itemSeparator)
        {
            if (nestedState is IEnumerable<KeyValuePair<string, object>> propertyList && HasUniqueCollectionKeys(propertyList))
            {
                // Special support for Microsoft Extension Logging ILogger.BeginScope where property-states are rendered as expando-objects
                builder.Append('{');
                builder.Append(separator);

                string currentSeparator = string.Empty;

                using (var scopeEnumerator = new ScopeContextPropertyEnumerator<object>(propertyList))
                {
                    while (scopeEnumerator.MoveNext())
                    {
                        var property = scopeEnumerator.Current;

                        int orgLength = builder.Length;
                        if (!AppendJsonProperty(property.Key, property.Value, builder, currentSeparator))
                        {
                            builder.Length = orgLength;
                            continue;
                        }

                        currentSeparator = itemSeparator;
                    }
                }

                builder.Append(separator);
                builder.Append('}');
            }
            else
            {
                builder.AppendFormattedValue(nestedState, Format, formatProvider, ValueFormatter);
            }
        }

        bool AppendJsonProperty(string propertyName, object propertyValue, StringBuilder builder, string itemSeparator)
        {
            if (string.IsNullOrEmpty(propertyName))
                return false;

            builder.Append(itemSeparator);

            if (!ValueFormatter.FormatValue(propertyName, null, MessageTemplates.CaptureType.Serialize, null, builder))
            {
                return false;
            }

            builder.Append(": ");
            if (!ValueFormatter.FormatValue(propertyValue, null, MessageTemplates.CaptureType.Serialize, null, builder))
            {
                return false;
            }

            return true;
        }

        bool HasUniqueCollectionKeys(IEnumerable<KeyValuePair<string, object>> propertyList)
        {
            if (propertyList is IDictionary<string, object>)
            {
                return true;
            }
#if !NET35
            else if (propertyList is IReadOnlyDictionary<string, object>)
            {
                return true;
            }
            else if (propertyList is IReadOnlyCollection<KeyValuePair<string, object>> propertyCollection)
            {
                if (propertyCollection.Count <= 1)
                    return true;
                else if (propertyCollection.Count > 10)
                    return false;   // Too many combinations
            }
#endif
            return ScopeContextPropertyEnumerator<object>.HasUniqueCollectionKeys(propertyList, StringComparer.Ordinal);
        }
    }
}

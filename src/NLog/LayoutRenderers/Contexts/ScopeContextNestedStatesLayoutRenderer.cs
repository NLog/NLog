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
    using System.Collections.Generic;
    using System.Text;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Layouts;

    /// <summary>
    /// Renders the nested states from <see cref="ScopeContext"/> like a callstack
    /// </summary>
    [LayoutRenderer("scopenested")]
    [ThreadSafe]
    public sealed class ScopeContextNestedStatesLayoutRenderer : LayoutRenderer
    {
        /// <summary>
        /// Gets or sets the number of top stack frames to be rendered.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        public int TopFrames { get; set; } = -1;

        /// <summary>
        /// Gets or sets the number of bottom stack frames to be rendered.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        public int BottomFrames { get; set; } = -1;

        /// <summary>
        /// Gets or sets the separator to be used for concatenating nested logical context output.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        public Layout Separator { get; set; } = new SimpleLayout(new[] { new LiteralLayoutRenderer(" ") }, " ", ConfigurationItemFactory.Default);

        /// <summary>
        /// Gets or sets how to format each nested state. Ex. like JSON = @
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// Renders the specified Nested Logical Context item and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (TopFrames == 1)
            {
                // Allows fast rendering of topframes=1
                var topFrame = ScopeContext.PeekNestedState();
                if (topFrame != null)
                    builder.AppendFormattedValue(topFrame, Format, GetFormatProvider(logEvent), ValueFormatter);
                return;
            }

            var messages = ScopeContext.GetAllNestedStates();
            if (messages.Length == 0)
                return;

            int startPos = 0;
            int endPos = messages.Length;

            if (TopFrames != -1)
            {
                endPos = Math.Min(TopFrames, messages.Length);
            }
            else if (BottomFrames != -1)
            {
                startPos = messages.Length - Math.Min(BottomFrames, messages.Length);
            }

            string separator = Separator?.Render(logEvent) ?? string.Empty;
            string itemSeparator = separator;
            if (Format == MessageTemplates.ValueFormatter.FormatAsJson)
            {
                builder.Append("[");
                builder.Append(separator);
                itemSeparator = "," + separator;
            }

            try
            {
                var formatProvider = GetFormatProvider(logEvent);
                string currentSeparator = string.Empty;
                for (int i = endPos - 1; i >= startPos; --i)
                {
                    builder.Append(currentSeparator);
                    AppendFormattedValue(messages[i], formatProvider, builder, separator, itemSeparator);
                    currentSeparator = itemSeparator;
                }
            }
            finally
            {
                if (Format == MessageTemplates.ValueFormatter.FormatAsJson)
                {
                    builder.Append(separator);
                    builder.Append("]");
                }
            }
        }

        private void AppendFormattedValue(object nestedState, IFormatProvider formatProvider, StringBuilder builder, string separator, string itemSeparator)
        {
            if (Format == MessageTemplates.ValueFormatter.FormatAsJson)
            {
                AppendJsonFormattedValue(nestedState, formatProvider, builder, separator, itemSeparator);
            }
            else
            {
                builder.AppendFormattedValue(nestedState, Format, formatProvider, ValueFormatter);
            }
        }

        private void AppendJsonFormattedValue(object nestedState, IFormatProvider formatProvider, StringBuilder builder, string separator, string itemSeparator)
        {
            if (nestedState is IEnumerable<KeyValuePair<string, object>> propertyList && HasUniqueCollectionKeys(propertyList))
            {
                // Special support for Microsoft Extension Logging ILogger.BeginScope where property-states are rendered as expando-objects
                builder.Append("{");
                builder.Append(separator);

                string currentSeparator = string.Empty;

                using (var scopeEnumerator = new ScopeContext.ScopePropertiesEnumerator<object>(propertyList))
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
                builder.Append("}");
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
            return ScopeContext.ScopePropertiesEnumerator<object>.HasUniqueCollectionKeys(propertyList, StringComparer.Ordinal);
        }
    }
}

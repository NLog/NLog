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

using System.Collections.Generic;
using System.Linq;

namespace NLog.LayoutRenderers
{
    using System;
    using System.Globalization;
    using System.Text;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// Log event context data. See <see cref="LogEventInfo.Properties"/>.
    /// </summary>
    [LayoutRenderer("event-properties")]
    [ThreadAgnostic]
    [ThreadSafe]
    [MutableUnsafe]
    public class EventPropertiesLayoutRenderer : LayoutRenderer, IRawValue, IStringValueRenderer
    {
        private string _item;
        private string _propertyName;
        private bool _evaluateAsNestedProperties;
        private List<string> _propertyChain;
        /// <summary>
        /// Gets or sets the name of the item.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [RequiredParameter]
        [DefaultParameter]
        public string Item
        {
            get => _item;
            set
            {
                _item = value;
                UpdateProperties();
            }
        }

        /// <summary>
        /// Format string for conversion from object to string.
        /// </summary>
        /// <docgen category='Rendering Options' order='50' />
        public string Format { get; set; }

        /// <summary>
        /// Gets or sets the culture used for rendering. 
        /// </summary>
        /// <docgen category='Rendering Options' order='100' />
        public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;


        /// <summary>
        /// Gets or sets whether <see cref="Item"/> with a dot are evaluated as properties or not
        /// </summary>
        /// <docgen category='Rendering Options' order='100' />
        public bool EvaluateAsNestedProperties
        {
            get => _evaluateAsNestedProperties;
            set
            {
                _evaluateAsNestedProperties = value;
                UpdateProperties();
            }
        }

        private void UpdateProperties()
        {
            if (_evaluateAsNestedProperties)
            {
                var itemParts = Item.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                _propertyName = itemParts.FirstOrDefault();
                _propertyChain = itemParts.Skip(1).ToList();
            }
            else
            {
                _propertyName = Item;
                _propertyChain = null;
            }
        }

        /// <inheritdoc/>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (GetValue(logEvent, out var value))
            {
                var formatProvider = GetFormatProvider(logEvent, Culture);
                builder.AppendFormattedValue(value, Format, formatProvider);
            }
        }

        /// <inheritdoc/>
        bool IRawValue.TryGetRawValue(LogEventInfo logEvent, out object value)
        {
            GetValue(logEvent, out value);
            return true;
        }

        /// <inheritdoc/>
        string IStringValueRenderer.GetFormattedString(LogEventInfo logEvent) => GetStringValue(logEvent);

        private bool GetValue(LogEventInfo logEvent, out object value)
        {
            value = null;
            if (!logEvent.HasProperties)
            {
                return false;
            }
            if (logEvent.Properties.TryGetValue(_propertyName, out value))
            {
                if (_evaluateAsNestedProperties)
                {
                    value = PropertyReader.GetNestedPropertyOfValue(value, _propertyChain);
                }

                return true;
            }
            return false;
        }

        private string GetStringValue(LogEventInfo logEvent)
        {
            if (Format != MessageTemplates.ValueFormatter.FormatAsJson)
            {
                if (GetValue(logEvent, out var value))
                {
                    string stringValue = FormatHelper.TryFormatToString(value, Format, GetFormatProvider(logEvent, Culture));
                    return stringValue;
                }
                return string.Empty;
            }
            return null;
        }
    }
}
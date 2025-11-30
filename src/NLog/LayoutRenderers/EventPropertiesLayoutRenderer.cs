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

namespace NLog.LayoutRenderers
{
    using System.Globalization;
    using System.Text;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// Log event context data. See <see cref="LogEventInfo.Properties"/>.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/EventProperties-Layout-Renderer">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/EventProperties-Layout-Renderer">Documentation on NLog Wiki</seealso>
    [LayoutRenderer("event-properties")]
    [LayoutRenderer("event-property")]
    [LayoutRenderer("event-context")]
    [ThreadAgnostic]
    [ThreadAgnosticImmutable]
    public class EventPropertiesLayoutRenderer : LayoutRenderer, IRawValue, IStringValueRenderer
    {
        private ObjectReflectionCache ObjectReflectionCache => _objectReflectionCache ?? (_objectReflectionCache = new ObjectReflectionCache(LoggingConfiguration.GetServiceProvider()));
        private ObjectReflectionCache? _objectReflectionCache;
        private ObjectPropertyPath _objectPropertyPath;

        /// <summary>
        /// Gets or sets the name of the item.
        /// </summary>
        /// <remarks><b>[Required]</b> Default: <see cref="string.Empty"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        [DefaultParameter]
        public string Item { get => _item?.ToString() ?? string.Empty; set => _item = (value is null || !IgnoreCase) ? (value ?? string.Empty) : new PropertiesDictionary.IgnoreCasePropertyKey(value); }
        private object _item = string.Empty;

        /// <summary>
        /// Format string for conversion from object to string.
        /// </summary>
        /// <remarks>Default: <see langword="null"/></remarks>
        /// <docgen category='Layout Options' order='50' />
        public string? Format { get; set; }

        /// <summary>
        /// Gets or sets the culture used for rendering.
        /// </summary>
        /// <remarks>Default: <see cref="CultureInfo.InvariantCulture"/></remarks>
        /// <docgen category='Layout Options' order='100' />
        public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

        /// <summary>
        /// Gets or sets the object-property-navigation-path for lookup of nested property
        /// </summary>
        /// <remarks>Default: <see cref="string.Empty"/></remarks>
        /// <docgen category='Layout Options' order='20' />
        public string ObjectPath
        {
            get => _objectPropertyPath.Value ?? string.Empty;
            set => _objectPropertyPath.Value = value;
        }

        /// <summary>
        /// Gets or sets whether to perform case-sensitive property-name lookup
        /// </summary>
        /// <remarks>Default: <see langword="true"/></remarks>
        /// <docgen category='Layout Options' order='100' />
        public bool IgnoreCase
        {
            get => _ignoreCase;
            set
            {
                if (value != _ignoreCase)
                {
                    _ignoreCase = value;
                    Item = _item?.ToString() ?? string.Empty;
                }
            }
        }
        private bool _ignoreCase = true;

        /// <inheritdoc/>
        protected override void InitializeLayoutRenderer()
        {
            base.InitializeLayoutRenderer();

            if (StringHelpers.IsNullOrEmptyString(_item))
                throw new NLogConfigurationException("EventProperty-LayoutRenderer Item-property must be assigned. Lookup blank value not supported.");
        }

        /// <inheritdoc/>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (TryGetValue(logEvent, out var value))
            {
                AppendFormattedValue(builder, logEvent, value, Format, Culture);
            }
        }

        bool IRawValue.TryGetRawValue(LogEventInfo logEvent, out object? value)
        {
            TryGetValue(logEvent, out value);
            return true;
        }

        string? IStringValueRenderer.GetFormattedString(LogEventInfo logEvent)
        {
            if (!MessageTemplates.ValueFormatter.FormatAsJson.Equals(Format))
            {
                if (TryGetValue(logEvent, out var value))
                {
                    var stringValue = FormatHelper.TryFormatToString(value, Format, GetFormatProvider(logEvent, Culture));
                    return stringValue;
                }
                return string.Empty;
            }
            return null;
        }

        private bool TryGetValue(LogEventInfo logEvent, out object? value)
        {
            value = null;

            if (!logEvent.HasProperties)
                return false;

            if (!logEvent.Properties.TryGetValue(_item, out value))
                return false;

            if (_objectPropertyPath.PathNames != null)
            {
                if (ObjectReflectionCache.TryGetObjectProperty(value, _objectPropertyPath.PathNames, out var rawValue))
                {
                    value = rawValue;
                }
                else
                {
                    value = null;
                }
            }

            return true;
        }
    }
}

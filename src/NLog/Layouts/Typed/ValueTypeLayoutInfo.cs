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

#nullable enable

namespace NLog.Layouts
{
    using System;
    using System.Globalization;
    using System.Text;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// Typed Value that is easily configured from NLog.config file
    /// </summary>
    [NLogConfigurationItem]
    public sealed class ValueTypeLayoutInfo
    {
        private static readonly Layout<string> _fixedNullValue = new Layout<string>(null);

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueTypeLayoutInfo" /> class.
        /// </summary>
        public ValueTypeLayoutInfo()
        {
        }

        /// <summary>
        /// Gets or sets the layout that will render the result value
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public Layout Layout
        {
            get => _layout;
            set
            {
                _layout = value ?? Layout.Empty;
                if (ValueType is null && _layout is ILayoutTypeValue layoutTyped)
                {
                    ValueType = layoutTyped.InnerType;
                }
                _layoutValue = null;
            }
        }
        private Layout _layout = Layout.Empty;

        /// <summary>
        /// Gets or sets the result value type, for conversion of layout rendering output
        /// </summary>
        /// <docgen category='Layout Options' order='50' />
        public Type? ValueType
        {
            get => _valueType;
            [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming - Allow ValueType assign from config", "IL2067")]
            set
            {
                _valueType = value;
                if (value?.IsValueType == true)
                    _createDefaultValue = () => Activator.CreateInstance(value);
                else
                    _createDefaultValue = null;
                _layoutValue = null;
                _defaultLayoutValue = null;
                _useDefaultWhenEmptyString = UseDefaultWhenEmptyString(_valueType, _defaultValue);
            }
        }
        private Type? _valueType;
        private Func<object>? _createDefaultValue;

        /// <summary>
        /// Gets or sets the fallback value when result value is not available
        /// </summary>
        /// <docgen category='Layout Options' order='50' />
        public Layout? DefaultValue
        {
            get => _defaultValue;
            set
            {
                _defaultValue = value;
                _defaultLayoutValue = null;
                _useDefaultWhenEmptyString = UseDefaultWhenEmptyString(_valueType, _defaultValue);
            }
        }
        private Layout? _defaultValue;
        private bool _useDefaultWhenEmptyString;

        private static bool UseDefaultWhenEmptyString(Type? valueType, Layout? defaultValue)
        {
            if (valueType is null || typeof(string).Equals(valueType) || typeof(object).Equals(valueType))
            {
                if (defaultValue is null || (defaultValue is SimpleLayout simpleLayout && string.Empty.Equals(simpleLayout.Text)) || (ReferenceEquals(defaultValue, _fixedNullValue) && !typeof(object).Equals(valueType)))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Gets or sets the fallback value should be null (instead of default value of <see cref="ValueType"/>) when result value is not available
        /// </summary>
        /// <docgen category='Layout Options' order='100' />
        public bool ForceDefaultValueNull
        {
            get => ReferenceEquals(DefaultValue, _fixedNullValue);
            set
            {
                if (value)
                    DefaultValue = _fixedNullValue;
                else if (ReferenceEquals(DefaultValue, _fixedNullValue))
                    DefaultValue = null;
                _useDefaultWhenEmptyString = UseDefaultWhenEmptyString(_valueType, _defaultValue);
            }
        }

        /// <summary>
        /// Gets or sets format used for parsing parameter string-value for type-conversion
        /// </summary>
        /// <docgen category='Layout Options' order='100' />
        public string? ValueParseFormat
        {
            get => _valueParseFormat;
            set
            {
                _valueParseFormat = value;
                _layoutValue = null;
                _defaultLayoutValue = null;
            }
        }
        private string? _valueParseFormat;

        /// <summary>
        /// Gets or sets the culture used for parsing parameter string-value for type-conversion
        /// </summary>
        /// <docgen category='Layout Options' order='100' />
        public CultureInfo? ValueParseCulture
        {
            get => _valueParseCulture;
            set
            {
                _valueParseCulture = value;
                _layoutValue = null;
                _defaultLayoutValue = null;
            }
        }
        private CultureInfo? _valueParseCulture;

        /// <summary>
        /// Render Result Value
        /// </summary>
        /// <param name="logEvent">Log event for rendering</param>
        /// <returns>Result value when available, else fallback to defaultValue</returns>
        public object? RenderValue(LogEventInfo logEvent)
        {
            var objectValue = LayoutValue.RenderObjectValue(logEvent, null);
            if (objectValue is null || (_useDefaultWhenEmptyString && StringHelpers.IsNullOrEmptyString(objectValue)))
            {
                objectValue = DefaultLayoutValue.RenderObjectValue(logEvent, null);
            }

            return objectValue;
        }

        private ILayoutTypeValue LayoutValue => _layoutValue ?? (_layoutValue = BuildLayoutTypeValue(Layout));
        private ILayoutTypeValue? _layoutValue;

        private ILayoutTypeValue DefaultLayoutValue => _defaultLayoutValue ?? (_defaultLayoutValue = BuildLayoutTypeValue(DefaultValue));
        private ILayoutTypeValue? _defaultLayoutValue;

        private ILayoutTypeValue BuildLayoutTypeValue(Layout? layout)
        {
            if (layout is null)
            {
                if (_createDefaultValue != null)
                {
                    var fixedDefaultValue = _createDefaultValue.Invoke();
                    return new FixedLayoutTypeValue(fixedDefaultValue);
                }
                else if (ValueType is null || typeof(string).Equals(ValueType))
                {
                    return new FixedLayoutTypeValue(string.Empty);
                }
                else
                {
                    layout = Layout.Empty;
                }
            }

            if (layout is ILayoutTypeValue typedLayout)
                return typedLayout.InnerLayout;

            if (ValueType is null || typeof(string).Equals(ValueType))
                return new StringLayoutTypeValue(layout);

            var logFactory = layout.LoggingConfiguration?.LogFactory ?? LogManager.LogFactory;
            var valueTypeConverter = logFactory.ServiceRepository.ResolveService<IPropertyTypeConverter>();
            var layoutValue = new LayoutTypeValue(layout, ValueType, ValueParseFormat, ValueParseCulture, valueTypeConverter);
            var fixedValue = layoutValue.TryParseFixedValue();
            if (fixedValue is null)
                return layoutValue;
            else
                return new FixedLayoutTypeValue(fixedValue);
        }

        private sealed class FixedLayoutTypeValue : ILayoutTypeValue
        {
            private readonly object? _fixedValue;
            public ILayoutTypeValue InnerLayout => this;
            public Type? InnerType => _fixedValue?.GetType();

            public FixedLayoutTypeValue(object fixedValue)
            {
                _fixedValue = fixedValue;
            }

            public object? RenderObjectValue(LogEventInfo logEvent, StringBuilder? stringBuilder)
            {
                return _fixedValue;
            }

            public override string ToString()
            {
                return _fixedValue?.ToString() ?? "null";
            }
        }

        private sealed class StringLayoutTypeValue : ILayoutTypeValue
        {
            private readonly Layout _innerLayout;
            public ILayoutTypeValue InnerLayout => this;
            public Type InnerType => typeof(string);

            public StringLayoutTypeValue(Layout layout)
            {
                _innerLayout = layout;
            }

            public object RenderObjectValue(LogEventInfo logEvent, StringBuilder? stringBuilder)
            {
                return _innerLayout.Render(logEvent);
            }

            public override string ToString()
            {
                return _innerLayout.ToString();
            }
        }
    }
}

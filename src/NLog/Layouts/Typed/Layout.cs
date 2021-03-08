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

using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;
using NLog.Common;
using NLog.Config;
using NLog.Internal;

namespace NLog.Layouts
{
    /// <summary>
    /// Typed Layout for easy conversion from NLog Layout logic to a simple value (ex. integer or enum)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [ThreadAgnostic]
    [ThreadSafe]
    [AppDomainFixedOutput]
    public sealed class Layout<T> : Layout, ITypedLayout
    {
        private readonly Layout _innerLayout;
        private readonly CultureInfo _parseFormatCulture;
        private readonly string _parseFormat;
        private string _previousStringValue;
        private object _previousValue;
        private readonly T _fixedValue;
        private T _staticValue;
        private bool _createdStaticValue;

        /// <summary>
        /// Is fixed value?
        /// </summary>
        public bool IsFixed => ReferenceEquals(_innerLayout, null);

        Layout ITypedLayout.InnerLayout => _innerLayout;

        Type ITypedLayout.ValueType => typeof(T);

        object ITypedLayout.StaticValue => FixedObjectValue ?? StaticValue;

        private object FixedObjectValue => IsFixed ? (_previousValue ?? (_previousValue = _fixedValue)) : null;

        private IPropertyTypeConverter ValueTypeConverter => _valueTypeConverter ?? (_valueTypeConverter = ResolveService<IPropertyTypeConverter>());
        IPropertyTypeConverter _valueTypeConverter;

        /// <summary>
        /// Static value independent of LogEvent being processed. Remains static until LoggingConfiguration is closed.
        /// </summary>
        public T StaticValue
        {
            get
            {
                if (IsFixed)
                    return _fixedValue;

                if (_createdStaticValue)
                    return _staticValue;

                // Would be great if ReconfigExistingLoggers() caused all StaticValue's to reset
                var staticValue = RenderTypedValue(LogEventInfo.CreateNullEvent(), default(T));
                _staticValue = staticValue;
                _createdStaticValue = true;
                return staticValue;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Layout{T}" /> class.
        /// </summary>
        /// <param name="layout">Dynamic NLog Layout</param>
        public Layout(Layout layout)
            :this(layout, null, CultureInfo.InvariantCulture)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Layout{T}" /> class.
        /// </summary>
        /// <param name="layout">Dynamic NLog Layout</param>
        /// <param name="parseValueFormat">Format used for parsing string-value into result value type</param>
        /// <param name="parseValueCulture">Culture used for parsing string-value into result value type</param>
        internal Layout(Layout layout, string parseValueFormat, CultureInfo parseValueCulture)
        {
            if (PropertyTypeConverter.IsComplexType(typeof(T)))
            {
                throw new NLogConfigurationException($"Layout<{typeof(T).ToString()}> not supported. Immutable value type is recommended");
            }

            if (layout is SimpleLayout simpleLayout && simpleLayout.IsFixedText)
            {
                if (TryParseValueFromString(simpleLayout.FixedText, parseValueFormat, parseValueCulture, out var value) && value != null)
                {
                    _fixedValue = (T)value;
                }
                else
                {
                    _innerLayout = simpleLayout;
                    _parseFormat = parseValueFormat;
                    _parseFormatCulture = parseValueCulture;
                }
            }
            else if (ReferenceEquals(layout, null))
            {
                _fixedValue = default(T);
            }
            else
            {
                _innerLayout = layout;
                _parseFormat = parseValueFormat;
                _parseFormatCulture = parseValueCulture;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Layout{T}" /> class.
        /// </summary>
        /// <param name="value">Fixed value</param>
        public Layout(T value)
        {
            _fixedValue = value;
        }

        /// <summary>
        /// Render Value
        /// </summary>
        /// <param name="logEvent">Log event for rendering</param>
        /// <param name="defaultValue">Fallback value when no value available</param>
        /// <returns>Result value when available, else fallback to defaultValue</returns>
        internal T RenderTypedValue([CanBeNull] LogEventInfo logEvent, T defaultValue = default(T))
        {
            return RenderTypedValue(logEvent, null, defaultValue);
        }

        internal T RenderTypedValue([CanBeNull] LogEventInfo logEvent, [CanBeNull] StringBuilder stringBuilder, T defaultValue)
        {
            if (IsFixed)
                return _fixedValue;
            else
                return RenderTypedValue<T>(logEvent, stringBuilder, defaultValue);
        }

        object ITypedLayout.RenderValue(LogEventInfo logEvent, object defaultValue)
        {
            var value = FixedObjectValue ?? RenderTypedValue(logEvent, null, defaultValue);
            if (ReferenceEquals(value, defaultValue) && ReferenceEquals(defaultValue, string.Empty) && typeof(T) != typeof(string))
                return default(T);  // Translate defaultValue = string.Empty into unspecified defaultValue
            else
                return value;
        }

        /// <summary>
        /// Renders the value and converts the value into string format
        /// </summary>
        /// <remarks>
        /// Only to implement abstract method from <see cref="Layout"/>, and only used when calling <see cref="Layout.Render(LogEventInfo)"/>
        /// </remarks>
        protected override string GetFormattedMessage(LogEventInfo logEvent)
        {
            var value = IsFixed ? FixedObjectValue : RenderTypedValue<object>(logEvent, null, null);
            var formatProvider = logEvent.FormatProvider ?? LoggingConfiguration?.DefaultCultureInfo;
            return Convert.ToString(value, formatProvider);
        }

        /// <inheritdoc />
        protected override void InitializeLayout()
        {
            base.InitializeLayout();
            _innerLayout?.Initialize(LoggingConfiguration ?? _innerLayout.LoggingConfiguration);
            ThreadSafe = _innerLayout?.ThreadSafe ?? true;
            ThreadAgnostic = _innerLayout?.ThreadAgnostic ?? true;
            MutableUnsafe = _innerLayout?.MutableUnsafe ?? false;
            _valueTypeConverter = null;
            _createdStaticValue = false;
            _previousStringValue = null;
            _previousValue = null;
        }

        /// <inheritdoc />
        protected override void CloseLayout()
        {
            _innerLayout?.Close();
            _valueTypeConverter = null;
            _createdStaticValue = false;
            _previousStringValue = null;
            _previousValue = null;
            base.CloseLayout();
        }

        /// <inheritdoc />
        public override void Precalculate(LogEventInfo logEvent)
        {
            PrecalculateInnerLayout(logEvent, null);
        }

        /// <inheritdoc />
        internal override void PrecalculateBuilder(LogEventInfo logEvent, StringBuilder target)
        {
            PrecalculateInnerLayout(logEvent, target);
        }

        private void PrecalculateInnerLayout(LogEventInfo logEvent, StringBuilder target)
        {
            if (IsFixed || (_innerLayout.ThreadAgnostic && !_innerLayout.MutableUnsafe))
                return;

            if (TryRenderObjectValue(logEvent, target, out var cachedValue))
            {
                logEvent.AddCachedLayoutValue(this, cachedValue);
            }
        }

        private TValueType RenderTypedValue<TValueType>(LogEventInfo logEvent, StringBuilder stringBuilder, TValueType defaultValue)
        {
            if (logEvent.TryGetCachedLayoutValue(this, out var cachedValue))
            {
                if (cachedValue != null)
                    return (TValueType)cachedValue;
                else
                    return defaultValue;
            }

            if (TryRenderObjectValue(logEvent, stringBuilder, out var value))
            {
                if (value != null)
                    return (TValueType)value;
                else
                    return defaultValue;
            }

            return defaultValue;
        }

        private bool TryRenderObjectValue(LogEventInfo logEvent, StringBuilder stringBuilder, out object value)
        {
            if (!IsInitialized)
            {
                Initialize(LoggingConfiguration ?? _innerLayout?.LoggingConfiguration);
            }

            if (_innerLayout.TryGetRawValue(logEvent, out var rawValue))
            {
                if (rawValue is string rawStringValue)
                {
                    if (string.IsNullOrEmpty(rawStringValue))
                    {
                        return TryParseValueFromString(rawStringValue, _parseFormat, _parseFormatCulture, out value);
                    }
                }
                else
                {
                    if (ReferenceEquals(rawValue, null))
                    {
                        value = null;
                        return true;
                    }

                    return TryParseValueFromObject(rawValue, _parseFormat, _parseFormatCulture, out value);
                }
            }

            var previousStringValue = _previousStringValue;
            var previousValue = _previousValue;

            var stringValue = RenderStringValue(logEvent, stringBuilder, previousStringValue);
            if (previousStringValue != null && previousStringValue == stringValue)
            {
                value = previousValue;
                return true;
            }

            if (TryParseValueFromString(stringValue, _parseFormat, _parseFormatCulture, out value))
            {
                _previousValue = value;
                _previousStringValue = stringValue;
                return true;
            }

            return false;
        }

        private string RenderStringValue(LogEventInfo logEvent, StringBuilder stringBuilder, string previousStringValue)
        {
            SimpleLayout simpleLayout = _innerLayout as SimpleLayout;
            if (simpleLayout != null && simpleLayout.IsSimpleStringText)
            {
                return simpleLayout.Render(logEvent);
            }

            if (stringBuilder?.Length == 0)
            {
                _innerLayout.RenderAppendBuilder(logEvent, stringBuilder);
                if (stringBuilder.Length == 0)
                    return string.Empty;
                else if (!string.IsNullOrEmpty(previousStringValue) && stringBuilder.EqualTo(previousStringValue))
                    return previousStringValue;
                else
                    return stringBuilder.ToString();
            }
            else
            {
                return _innerLayout.Render(logEvent);
            }
        }

        private bool TryParseValueFromString(string stringValue, string parseValueFormat, CultureInfo parseValueCulture, out object parsedValue)
        {
            if (string.IsNullOrEmpty(stringValue))
            {
                parsedValue = typeof(T) == typeof(string) ? stringValue : null;
                return true;
            }

            return TryParseValueFromObject(stringValue, parseValueFormat, parseValueCulture, out parsedValue);
        }

        bool ITypedLayout.TryParseValueFromString(string stringValue, out object parsedValue)
        {
            return TryParseValueFromString(stringValue, _parseFormat, _parseFormatCulture, out parsedValue);
        }

        bool TryParseValueFromObject(object rawValue, string parseValueFormat, CultureInfo parseValueCulture, out object parsedValue)
        {
            try
            {
                parsedValue = ValueTypeConverter.Convert(rawValue, typeof(T), parseValueFormat, parseValueCulture);
                return true;
            }
            catch (Exception ex)
            {
                parsedValue = null;
                InternalLogger.Warn(ex, "Failed converting object '{0}' of type {1} into type {2}", rawValue, rawValue?.GetType(), typeof(T));
                return false;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return IsFixed ? (FixedObjectValue?.ToString() ?? "null") : (_innerLayout?.ToString() ?? base.ToString());
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (IsFixed)
            {
                // Support property-compare
                if (obj is Layout<T> other)
                    return other.IsFixed && object.Equals(FixedObjectValue, other.FixedObjectValue);
                else if (obj is T)
                    return object.Equals(FixedObjectValue, obj);
                else
                    return obj == null && FixedObjectValue == null;
            }
            else
            {
                return ReferenceEquals(this, obj);  // Support LogEventInfo.LayoutCache
            }
        }

        /// <inheritdoc />
        public bool Equals(T other)
        {
            // Support property-compare
            return IsFixed && object.Equals(FixedObjectValue, other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            if (IsFixed)
                return FixedObjectValue?.GetHashCode() ?? typeof(T).GetHashCode();     // Support property-compare
            else
                return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);    // Support LogEventInfo.LayoutCache
        }

        /// <summary>
        /// Converts a given value to a <see cref="Layout{T}" />.
        /// </summary>
        /// <param name="value">Text to be converted.</param>
        public static implicit operator Layout<T>(T value)
        {
            return new Layout<T>(value);
        }

        /// <summary>
        /// Converts a given text to a <see cref="Layout{T}" />.
        /// </summary>
        /// <param name="layout">Text to be converted.</param>
        public static implicit operator Layout<T>([Localizable(false)] string layout)
        {
            return new Layout<T>(layout);
        }

        /// <summary>
        /// Converts a <see cref="Layout{T}" /> its current value
        /// </summary>
        /// <param name="layout">Text to be converted.</param>
        public static implicit operator T(Layout<T> layout) => layout.StaticValue;
    }

    /// <summary>
    /// Provides access to untyped value without knowing underlying generic type
    /// </summary>
    internal interface ITypedLayout
    {
        Layout InnerLayout { get; }
        Type ValueType { get; }
        bool IsFixed { get; }
        object StaticValue { get; }
        object RenderValue(LogEventInfo logEvent, object defaultValue);
        bool TryParseValueFromString(string stringValue, out object parsedValue);
    }
}

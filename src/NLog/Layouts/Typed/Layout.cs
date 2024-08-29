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
    public sealed class Layout<T> : Layout, ILayoutTypeValue<T>, IEquatable<T>
    {
        private readonly T _fixedValue;
        private object _fixedObjectValue;

        ILayoutTypeValue ILayoutTypeValue.InnerLayout => _layoutValue;
        Type ILayoutTypeValue.InnerType => typeof(T);
        bool ILayoutTypeValue<T>.ThreadAgnostic => true;
        bool ILayoutTypeValue<T>.ThreadAgnosticImmutable => false;
        StackTraceUsage ILayoutTypeValue<T>.StackTraceUsage => StackTraceUsage.None;
        LoggingConfiguration ILayoutTypeValue<T>.LoggingConfiguration => LoggingConfiguration;
        void ILayoutTypeValue<T>.InitializeLayout()
        {
            // SONAR: Nothing to initialize
        }
        void ILayoutTypeValue<T>.CloseLayout()
        {
            // SONAR: Nothing to initialize
        }
        bool ILayoutTypeValue<T>.TryRenderValue(LogEventInfo logEvent, StringBuilder stringBuilder, out T value)
        {
            value = _fixedValue;
            return true;
        }
        object ILayoutTypeValue.RenderObjectValue(NLog.LogEventInfo logEvent, System.Text.StringBuilder stringBuilder) => FixedObjectValue;

        private readonly ILayoutTypeValue<T> _layoutValue;

        /// <summary>
        /// Is fixed value?
        /// </summary>
        public bool IsFixed => ReferenceEquals(this, _layoutValue);

        /// <summary>
        /// Fixed value
        /// </summary>
        public T FixedValue => _fixedValue;

        private object FixedObjectValue => (_fixedObjectValue ?? (_fixedObjectValue = _fixedValue));

        private IPropertyTypeConverter ValueTypeConverter => _valueTypeConverter ?? (_valueTypeConverter = ResolveService<IPropertyTypeConverter>());
        IPropertyTypeConverter _valueTypeConverter;

        /// <summary>
        /// Initializes a new instance of the <see cref="Layout{T}" /> class.
        /// </summary>
        /// <param name="layout">Dynamic NLog Layout</param>
        public Layout(Layout layout)
            : this(layout, null, CultureInfo.InvariantCulture)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Layout{T}" /> class.
        /// </summary>
        /// <param name="layout">Dynamic NLog Layout</param>
        /// <param name="parseValueFormat">Format used for parsing string-value into result value type</param>
        /// <param name="parseValueCulture">Culture used for parsing string-value into result value type</param>
        public Layout(Layout layout, string parseValueFormat, CultureInfo parseValueCulture)
        {
            if (PropertyTypeConverter.IsComplexType(typeof(T)))
            {
                throw new NLogConfigurationException($"Layout<{typeof(T).ToString()}> not supported. Immutable value type is recommended");
            }

            if (TryParseFixedValue(layout, parseValueFormat, parseValueCulture, ref _fixedValue))
            {
                _layoutValue = this;
            }
            else
            {
                _layoutValue = new LayoutGenericTypeValue(layout, parseValueFormat, parseValueCulture, this);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Layout{T}" /> class.
        /// </summary>
        /// <param name="value">Fixed value</param>
        public Layout(T value)
        {
            _fixedValue = value;
            _layoutValue = this;
        }

        private Layout(Func<LogEventInfo, T> layoutMethod, LayoutRenderOptions options)
        {
            _layoutValue = new FuncMethodValue(layoutMethod, options);
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

            if (logEvent is null)
                return defaultValue;

            if (logEvent.TryGetCachedLayoutValue(this, out var cachedValue))
            {
                if (cachedValue is null)
                    return defaultValue;
                else
                    return (T)cachedValue;
            }

            if (!IsInitialized)
            {
                Initialize(LoggingConfiguration ?? _layoutValue.LoggingConfiguration);
            }

            if (_layoutValue.TryRenderValue(logEvent, stringBuilder, out var value))
            {
                return value;
            }

            return defaultValue;
        }

        private object RenderObjectValue([CanBeNull] LogEventInfo logEvent, [CanBeNull] StringBuilder stringBuilder)
        {
            if (IsFixed)
                return FixedObjectValue;

            if (logEvent is null)
                return null;

            if (logEvent.TryGetCachedLayoutValue(this, out var cachedValue))
                return cachedValue;

            return _layoutValue.RenderObjectValue(logEvent, stringBuilder);
        }

        /// <summary>
        /// Renders the value and converts the value into string format
        /// </summary>
        /// <remarks>
        /// Only to implement abstract method from <see cref="Layout"/>, and only used when calling <see cref="Layout.Render(LogEventInfo)"/>
        /// </remarks>
        protected override string GetFormattedMessage(LogEventInfo logEvent)
        {
            var value = IsFixed ? FixedObjectValue : RenderObjectValue(logEvent, null);
            var formatProvider = logEvent.FormatProvider ?? LoggingConfiguration?.DefaultCultureInfo;
            return Convert.ToString(value, formatProvider);
        }

        /// <inheritdoc/>
        protected override void InitializeLayout()
        {
            base.InitializeLayout();
            _layoutValue.InitializeLayout();
            ThreadAgnostic = _layoutValue.ThreadAgnostic;
            ThreadAgnosticImmutable = _layoutValue.ThreadAgnosticImmutable;
            StackTraceUsage = _layoutValue.StackTraceUsage;
            _valueTypeConverter = null;
            _fixedObjectValue = null;
        }

        /// <inheritdoc/>
        protected override void CloseLayout()
        {
            _layoutValue.CloseLayout();
            _valueTypeConverter = null;
            _fixedObjectValue = null;
            base.CloseLayout();
        }

        /// <inheritdoc/>
        public override void Precalculate(LogEventInfo logEvent)
        {
            PrecalculateInnerLayout(logEvent, null);
        }

        /// <inheritdoc/>
        internal override void PrecalculateBuilder(LogEventInfo logEvent, StringBuilder target)
        {
            PrecalculateInnerLayout(logEvent, target);
        }

        /// <summary>
        /// Create a typed layout from a lambda method.
        /// </summary>
        /// <param name="layoutMethod">Method that renders the layout.</param>
        /// <param name="options">Whether method is ThreadAgnostic and doesn't depend on context of the logging application thread.</param>
        /// <returns>Instance of typed layout.</returns>
        public static Layout<T> FromMethod(Func<LogEventInfo, T> layoutMethod, LayoutRenderOptions options = LayoutRenderOptions.None)
        {
            Guard.ThrowIfNull(layoutMethod);
            return new Layout<T>(layoutMethod, options);
        }

        private void PrecalculateInnerLayout(LogEventInfo logEvent, StringBuilder target)
        {
            if (IsFixed || (_layoutValue.ThreadAgnostic && !_layoutValue.ThreadAgnosticImmutable))
                return;

            var cachedValue = RenderObjectValue(logEvent, target);
            logEvent.AddCachedLayoutValue(this, cachedValue);
        }

        private sealed class LayoutGenericTypeValue : LayoutTypeValue, ILayoutTypeValue<T>
        {
            private readonly Layout<T> _ownerLayout;

            public override Type InnerType => typeof(T);
            public override IPropertyTypeConverter ValueTypeConverter => _ownerLayout.ValueTypeConverter;

            public LayoutGenericTypeValue(Layout layout, string parseValueFormat, CultureInfo parseValueCulture, Layout<T> ownerLayout)
                :base(layout, typeof(T), parseValueFormat, parseValueCulture, null)
            {
                _ownerLayout = ownerLayout;
            }

            public void InitializeLayout()
            {
                base.InitializeLayout(_ownerLayout);
            }

            public void CloseLayout()
            {
                base.Close();
            }

            public bool TryRenderValue(LogEventInfo logEvent, StringBuilder stringBuilder, out T value)
            {
                var objectValue = RenderObjectValue(logEvent, stringBuilder);
                if (objectValue is T typedValue)
                {
                    value = typedValue;
                    return true;
                }
                value = default(T);
                return false;
            }
        }

        private bool TryParseFixedValue(Layout layout, string parseValueFormat, CultureInfo parseValueCulture, ref T fixedValue)
        {
            if (layout is SimpleLayout simpleLayout && simpleLayout.IsFixedText)
            {
                if (!string.IsNullOrEmpty(simpleLayout.FixedText))
                {
                    try
                    {
                        fixedValue = (T)ParseValueFromObject(simpleLayout.FixedText, parseValueFormat, parseValueCulture);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        var configException = new NLogConfigurationException($"Failed converting into type {typeof(T)}. Value='{simpleLayout.FixedText}'", ex);
                        if (configException.MustBeRethrown())
                            throw configException;
                    }
                }
                else if (typeof(T) == typeof(string))
                {
                    fixedValue = (T)(object)simpleLayout.FixedText;
                    return true;
                }
                else if (Nullable.GetUnderlyingType(typeof(T)) != null)
                {
                    fixedValue = default(T);
                    return true;
                }
            }
            else if (layout is null)
            {
                fixedValue = default(T);
                return true;
            }

            fixedValue = default(T);
            return false;
        }

        private sealed class FuncMethodValue : ILayoutTypeValue<T>
        {
            private readonly Func<LogEventInfo, T> _layoutMethod;

            public bool ThreadAgnostic { get; }

            ILayoutTypeValue ILayoutTypeValue.InnerLayout => this;
            Type ILayoutTypeValue.InnerType => typeof(T);
            LoggingConfiguration ILayoutTypeValue<T>.LoggingConfiguration => null;
            bool ILayoutTypeValue<T>.ThreadAgnosticImmutable => false;
            StackTraceUsage ILayoutTypeValue<T>.StackTraceUsage => StackTraceUsage.None;

            public FuncMethodValue(Func<LogEventInfo, T> layoutMethod, LayoutRenderOptions options)
            {
                _layoutMethod = layoutMethod;
                ThreadAgnostic = options == LayoutRenderOptions.ThreadAgnostic;
            }

            public void InitializeLayout()
            {
                // SONAR: Nothing to initialize
            }

            public void CloseLayout()
            {
                // SONAR: Nothing to close
            }

            public bool TryRenderValue(LogEventInfo logEvent, StringBuilder stringBuilder, out T value)
            {
                value = _layoutMethod(logEvent);
                return true;
            }

            public object RenderObjectValue(LogEventInfo logEvent, StringBuilder stringBuilder)
            {
                return _layoutMethod(logEvent);
            }

            public override string ToString()
            {
                return _layoutMethod.ToString();
            }
        }

        private object ParseValueFromObject(object rawValue, string parseValueFormat, CultureInfo parseValueCulture)
        {
            return ValueTypeConverter.Convert(rawValue, typeof(T), parseValueFormat, parseValueCulture);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return IsFixed ? (FixedObjectValue?.ToString() ?? "null") : _layoutValue.ToString();
        }

        /// <inheritdoc/>
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
                    return ReferenceEquals(obj, FixedObjectValue);
            }
            else
            {
                return ReferenceEquals(this, obj);  // Support LogEventInfo.LayoutCache
            }
        }

        /// <summary>
        /// Implements Equals using <see cref="FixedValue"/>
        /// </summary>
        public bool Equals(T other)
        {
            // Support property-compare
            return IsFixed && object.Equals(FixedObjectValue, other);
        }

        /// <inheritdoc/>
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
            if (object.Equals(value, default(T)) && !typeof(T).IsValueType()) return null;

            return new Layout<T>(value);
        }

        /// <summary>
        /// Converts a given text to a <see cref="Layout{T}" />.
        /// </summary>
        /// <param name="layout">Text to be converted.</param>
        public static implicit operator Layout<T>([Localizable(false)] string layout)
        {
            if (layout is null) return null;

            return new Layout<T>(layout);
        }

        /// <summary>
        /// Implements the operator == using <see cref="FixedValue"/>
        /// </summary>
        public static bool operator ==(Layout<T> left, T right)
        {
            return left?.Equals(right) == true || (left is null && object.Equals(right, default(T)));
        }

        /// <summary>
        /// Implements the operator != using <see cref="FixedValue"/>
        /// </summary>
        public static bool operator !=(Layout<T> left, T right)
        {
            return left?.Equals(right) != true && !(left is null && object.Equals(right, default(T)));
        }
    }

    internal interface ILayoutTypeValue
    {
        Type InnerType { get; }
        ILayoutTypeValue InnerLayout { get; }
        object RenderObjectValue(LogEventInfo logEvent, StringBuilder stringBuilder);
    }

    internal interface ILayoutTypeValue<T> : ILayoutTypeValue
    {
        LoggingConfiguration LoggingConfiguration { get; }
        bool ThreadAgnostic { get; }
        bool ThreadAgnosticImmutable { get; }
        StackTraceUsage StackTraceUsage { get; }
        void InitializeLayout();
        void CloseLayout();
        bool TryRenderValue(LogEventInfo logEvent, StringBuilder stringBuilder, out T value);
    }

    internal class LayoutTypeValue : ILayoutTypeValue
    {
        private readonly Layout _innerLayout;
        private readonly Type _valueType;
        private readonly CultureInfo _parseValueCulture;
        private readonly string _parseValueFormat;
        private string _previousStringValue;
        private object _previousValue;

        public LoggingConfiguration LoggingConfiguration => _innerLayout.LoggingConfiguration;
        public bool ThreadAgnostic => _innerLayout.ThreadAgnostic;
        public bool ThreadAgnosticImmutable => _innerLayout.ThreadAgnosticImmutable;
        public StackTraceUsage StackTraceUsage => _innerLayout.StackTraceUsage;
        public virtual IPropertyTypeConverter ValueTypeConverter { get; }
        public ILayoutTypeValue InnerLayout => this;
        public virtual Type InnerType => null;

        public LayoutTypeValue(Layout layout, Type valueType, string parseValueFormat, CultureInfo parseValueCulture, IPropertyTypeConverter valueTypeConverter)
        {
            _innerLayout = layout;
            _valueType = valueType;
            _parseValueFormat = parseValueFormat;
            _parseValueCulture = parseValueCulture;
            ValueTypeConverter = valueTypeConverter;
        }

        public object TryParseFixedValue()
        {
            if (_innerLayout is SimpleLayout simpleLayout && simpleLayout.IsFixedText)
            {
                if (TryParseValueFromString(simpleLayout.FixedText, _parseValueFormat, _parseValueCulture, out object objectValue))
                {
                    return objectValue;
                }
            }
            return null;
        }

        protected void InitializeLayout(Layout ownerLayout)
        {
            _innerLayout.Initialize(ownerLayout.LoggingConfiguration ?? _innerLayout.LoggingConfiguration);
            _previousStringValue = null;
            _previousValue = null;
        }

        protected void Close()
        {
            _innerLayout.Close();
            _previousStringValue = null;
            _previousValue = null;
        }

        public object RenderObjectValue(LogEventInfo logEvent, StringBuilder stringBuilder)
        {
            if (_innerLayout.TryGetRawValue(logEvent, out var rawValue))
            {
                if (rawValue is string rawStringValue)
                {
                    if (string.IsNullOrEmpty(rawStringValue))
                    {
                        TryParseValueFromString(rawStringValue, _parseValueFormat, _parseValueCulture, out var objectValue);
                        return objectValue;
                    }
                }
                else
                {
                    if (rawValue is null)
                    {
                        return null;
                    }

                    TryParseValueFromObject(rawValue, _parseValueFormat, _parseValueCulture, out var objectValue);
                    return objectValue;
                }
            }

            var previousStringValue = _previousStringValue;
            var previousValue = _previousValue;

            var stringValue = RenderStringValue(logEvent, stringBuilder, previousStringValue);
            if (previousStringValue != null && previousStringValue == stringValue)
            {
                return previousValue;
            }

            if (TryParseValueFromString(stringValue, _parseValueFormat, _parseValueCulture, out var parsedValue))
            {
                if (string.IsNullOrEmpty(previousStringValue) || stringValue?.Length < 3)
                {
                    // Only cache initial value to avoid constantly changing values like CorrelationId (Guid) or DateTime.UtcNow
                    _previousValue = parsedValue;
                    _previousStringValue = stringValue;
                }
                return parsedValue;
            }

            return null;
        }

        private string RenderStringValue(LogEventInfo logEvent, StringBuilder stringBuilder, string previousStringValue)
        {
            if (_innerLayout is SimpleLayout simpleLayout && simpleLayout.IsSimpleStringText)
            {
                return simpleLayout.Render(logEvent, false);
            }

            if (stringBuilder?.Length == 0)
            {
                _innerLayout.Render(logEvent, stringBuilder);
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

        bool TryParseValueFromObject(object rawValue, string parseValueFormat, CultureInfo parseValueCulture, out object parsedValue)
        {
            try
            {
                parsedValue = ParseValueFromObject(rawValue, parseValueFormat, parseValueCulture);
                return true;
            }
            catch (Exception ex)
            {
                parsedValue = null;
                InternalLogger.Warn(ex, "Failed converting object '{0}' of type {1} into type {2}", rawValue, rawValue?.GetType(), _valueType);
                return false;
            }
        }

        private object ParseValueFromObject(object rawValue, string parseValueFormat, CultureInfo parseValueCulture)
        {
            return ValueTypeConverter.Convert(rawValue, _valueType, parseValueFormat, parseValueCulture);
        }

        private bool TryParseValueFromString(string stringValue, string parseValueFormat, CultureInfo parseValueCulture, out object parsedValue)
        {
            if (string.IsNullOrEmpty(stringValue))
            {
                parsedValue = _valueType == typeof(string) ? stringValue : null;
                return true;
            }

            return TryParseValueFromObject(stringValue, parseValueFormat, parseValueCulture, out parsedValue);
        }

        public override string ToString()
        {
            return _innerLayout.ToString();
        }
    }
}

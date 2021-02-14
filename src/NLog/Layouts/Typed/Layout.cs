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
using System.Collections.Generic;
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
    /// Layout with a simple value (e.g. int) or a layout which results in a simple value (e.g. ${counter})
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Layout<T> : Layout, IRenderable<T>, IInternalLoggerContext, IEquatable<Layout<T>>
    {
        // ReSharper disable StaticMemberInGenericType - this is safe, static ctor is called for every generic type
        [NotNull] private static readonly Type Type;
        [NotNull] private static readonly string TypeNamed;
        // ReSharper restore StaticMemberInGenericType

        [CanBeNull] private readonly Layout _layout;
        private readonly T _fixedValue;

        /// <inheritdoc />
        public Layout(T value)
        {
            _fixedValue = value;
            IsFixed = true;
            _layout = null;
        }

        /// <inheritdoc />
        public Layout(Layout layout)
        {
            IsFixed = TryGetFixedValue(layout, out _fixedValue);
            _layout = layout;
        }

        static Layout()
        {
            var type = typeof(T);
            Type = Nullable.GetUnderlyingType(type) ?? type;
            TypeNamed = type.Name;
        }

        /// <summary>
        /// Is fixed value?
        /// </summary>
        public bool IsFixed { get; }

        #region Overrides of TypedLayout<T>

        private static string ValueToString(T value, CultureInfo cultureInfo)
        {
            if (value is IConvertible convertible)
            {
                return convertible.ToString(cultureInfo);
            }

            return value?.ToString();
        }

        private bool TryParse(string text, out T value, T defaultValue)
        {
            return TryConvertTo(text, out value, defaultValue);
        }

        private object _cacheKey;
        private T _cacheValue;
        private bool _cacheResult;

        private bool TryGetCacheValue(object rawValue, out T resultValue, out bool resultValid)
        {
            var cacheKey = _cacheKey;
            var cacheValue = _cacheValue;
            var cacheValid = _cacheResult;
            if (!ReferenceEquals(cacheKey, null) && (ReferenceEquals(cacheKey, rawValue) || cacheKey.Equals(rawValue)))
            {
                resultValue = cacheValue;
                resultValid = cacheValid;
                return true;
            }
            resultValue = default(T);
            resultValid = false;
            return false;
        }

        private void UpdateCache(object raw, T value, bool result)
        {
            _cacheKey = raw;
            _cacheValue = value;
            _cacheResult = result;
        }

        private bool TryConvertTo(object raw, out T value, T defaultValue)
        {
            if (raw == null)
            {
                value = defaultValue;
                return false;
            }

            if (TryGetCacheValue(raw, out var resultValue, out var resultValid))
            {
                value = resultValue;
                return resultValid;
            }

            var cultureInfo = CultureInfo.CurrentCulture;
            try
            {
                var converter = ResolveService<IPropertyTypeConverter>() ?? PropertyTypeConverter.Instance;
                var convertedValue = converter.Convert(raw, Type, null, cultureInfo);
                if (convertedValue is T goodValue)
                {
                    value = goodValue;
                    UpdateCache(raw, value, true);
                    return true;
                }
            }
            catch (Exception e)
            {
                InternalLogger.Debug(e, "Conversion to type {0} failed. Value: '{1}'", Type, raw.ToString().Left(1000));
                if (e.MustBeRethrown(this))
                {
                    throw;
                }
            }

            value = defaultValue;
            UpdateCache(raw, value, false);
            return false;
        }

        #endregion

        #region Conversion

        /// <summary>
        /// Converts a given text to a <see cref="Layout" />.
        /// </summary>
        /// <param name="value">Text to be converted.</param>
        /// <returns><see cref="SimpleLayout" /> object represented by the text.</returns>
        public static implicit operator Layout<T>(T value)
        {
            return new Layout<T>(value);
        }

        /// <summary>
        /// Converts a given text to a <see cref="Layout" />.
        /// </summary>
        /// <param name="layout">Text to be converted.</param>
        /// <returns><see cref="SimpleLayout" /> object represented by the text.</returns>
        public static implicit operator Layout<T>([Localizable(false)] string layout)
        {
            return new Layout<T>(layout);
        }

        #endregion

        /// <inheritdoc />
        protected override void InitializeLayout()
        {
            _layout?.Initialize(LoggingConfiguration);
            base.InitializeLayout();
            ThreadAgnostic = _layout?.ThreadAgnostic ?? true;
            MutableUnsafe = _layout?.MutableUnsafe ?? false;
        }

        internal override void PrecalculateBuilder(LogEventInfo logEvent, StringBuilder target)
        {
            _layout?.PrecalculateBuilder(logEvent, target);
        }

        /// <inheritdoc />
        protected override string GetFormattedMessage(LogEventInfo logEvent)
        {
            if (IsFixed)
            {
                if (_fixedValue == null)
                {
                    return null;
                }

                var text = ValueToString(_fixedValue, LoggingConfiguration?.DefaultCultureInfo);
                if (text != null)
                {
                    return text;
                }
            }

            return _layout?.Render(logEvent);
        }

        /// <summary>
        /// Render to value
        /// </summary>
        /// <returns></returns>
        T IRenderable<T>.RenderToValue(LogEventInfo logEvent, T defaultValue)
        {
            return RenderToValueInternal(logEvent, null, defaultValue);
        }

        /// <inheritdoc cref="IRawValue" />
        internal override bool TryGetRawValue(LogEventInfo logEvent, out object rawValue)
        {
            var success = TryGetRawValueIntern(logEvent, out var value, default(T));
            rawValue = value;
            return success;
        }

        /// <summary>
        /// Render to value
        /// </summary>
        /// <param name="logEvent"></param>
        /// <param name="reusableBuilder">if null, default layout render will be used</param>
        /// <param name="defaultValue">Default value if parse failed</param>
        /// <returns></returns>
        internal T RenderToValueInternal(LogEventInfo logEvent, [CanBeNull] StringBuilder reusableBuilder, T defaultValue = default(T))
        {
            if (TryGetRawValueIntern(logEvent, out var rawValue, defaultValue))
            {
                return rawValue;
            }

            var text = reusableBuilder != null ? RenderAllocateBuilder(logEvent, reusableBuilder) : _layout?.Render(logEvent);
            if (TryParse(text, out var parsedValue, defaultValue))
            {
                return parsedValue;
            }

            InternalLogger.Warn("Parse {0} to {1} failed", text, TypeNamed);
            return defaultValue;
        }

        private bool TryGetRawValueIntern(LogEventInfo logEvent, out T rawValue, T defaultValue)
        {
            if (IsFixed)
            {
                rawValue = _fixedValue;
                return true;
            }

            if (_layout == null)
            {
                rawValue = defaultValue;
                return true;
            }

            if (_layout.TryGetRawValue(logEvent, out var raw))
            {
                var success = TryConvertRawToValue(raw, defaultValue, out var value);
                if (success)
                {
                    rawValue = value;
                    return true;
                }

                InternalLogger.Warn("rawvalue isn't a {0}. Value: '{1}'", TypeNamed, raw?.ToString().Left(1000));
            }

            rawValue = defaultValue;
            return false;
        }


        private bool TryConvertRawToValue(object raw, T defaultValue, out T value)
        {
            if (raw is T tValue)
            {
                value = tValue;
                return true;
            }

            if (TryConvertTo(raw, out value, defaultValue))
            {
                return true;
            }

            value = defaultValue;

            return false;
        }

        /// <summary>
        /// Pre compile if is fixed text
        /// </summary>
        /// <returns>Is fixed value</returns>
        private bool TryGetFixedValue(Layout layout, out T value)
        {
            if (layout is SimpleLayout simpleLayout && simpleLayout.IsFixedText)
            {
                var success = TryParse(simpleLayout.FixedText, out value, default(T));
                if (!success)
                {
                    InternalLogger.Warn("layout with text '{0}' isn't an {1}", simpleLayout.FixedText, TypeNamed);
                }

                return true;
            }

            value = default(T);

            return false;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Describe();
        }

        private string Describe()
        {
            if (IsFixed)
            {
                return $"Typed Layout with fixed value: {_layout}, Value: {_fixedValue}";
            }

            return $"Typed Layout with dynamic value: {_layout}";
        }
        string IInternalLoggerContext.Name => Describe();

        LogFactory IInternalLoggerContext.LogFactory => base.LoggingConfiguration?.LogFactory;

        #region Equality members

        /// <summary>
        /// Equals another layout?
        /// </summary>
        public bool Equals(Layout<T> other)
        {
            if (other == null)
            {
                return false;
            }

            return IsFixed == other.IsFixed && Equals(_layout, other._layout) && Equals(_fixedValue, other._fixedValue);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((Layout<T>)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (_layout != null ? _layout.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(_fixedValue);
                hashCode = (hashCode * 397) ^ IsFixed.GetHashCode();
                return hashCode;
            }
        }

        #endregion
    }
}
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
using System.Reflection;
using System.Text;
using System.Threading;
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
    public class Layout<T> : Layout, IRenderable<T>
    {
        private static Type _type;
        private static readonly string _typeNamed;
        private readonly Layout _layout;
        private readonly T _value;
        private readonly bool _fixedValue;

        /// <inheritdoc />
        public Layout(T value)
        {
            _value = value;
            _fixedValue = true;
            _layout = null;
        }

        /// <inheritdoc />
        public Layout(Layout layout)
        {
            _fixedValue = TryGetFixedValue(layout, out _value);
            _layout = layout;
        }

        static Layout()
        {
            var type = typeof(T);
            _type = Nullable.GetUnderlyingType(type) ?? type;
            _typeNamed = type.Name;
        }

        /// <summary>
        /// Is fixed value?
        /// </summary>
        public bool IsFixed => _fixedValue;

        #region Overrides of TypedLayout<T>

        private static string ValueToString(T value, CultureInfo cultureInfo)
        {
            if (value is IConvertible convertible)
            {
                return convertible.ToString(cultureInfo);
            }

            return value?.ToString();
        }

        private static bool TryParse(string text, out T value)
        {
            return TryConvertTo(text, out value);
        }

        private static bool TryConvertTo(object raw, out T value)
        {
            if (_type == null || raw == null)
            {
                value = default(T);
                return false;
            }

            // We don't use DI here because of this will be called before DI setup and performance reasons
            var cultureInfo = CultureInfo.CurrentCulture;
            try
            {
                var convertedValue = ValueConverter.Instance.Convert(raw, _type, null, cultureInfo);
                if (convertedValue is T goodValue)
                {
                    value = goodValue;
                    return true;
                }
            }
            catch (Exception e)
            {
                InternalLogger.Debug(e, "Conversion to type {0} failed", _type);
                if (e.MustBeRethrown())
                {
                    throw;
                }
            }

            value = default(T);
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
        protected override string GetFormattedMessage(LogEventInfo logEvent)
        {
            if (_fixedValue)
            {
                if (_value == null)
                {
                    return null;
                }

                var text = ValueToString(_value, LoggingConfiguration.DefaultCultureInfo);
                if (text != null)
                {
                    return text;
                }
            }

            return _layout.Render(logEvent);
        }



        /// <summary>
        /// Render to value
        /// </summary>
        /// <returns></returns>
        T IRenderable<T>.RenderToValue(LogEventInfo logEvent)
        {
            return RenderToValueInternal(logEvent, null);
        }

        /// <inheritdoc cref="IRawValue" />
        internal override bool TryGetRawValue(LogEventInfo logEvent, out object rawValue)
        {
            var success = TryGetRawValueIntern(logEvent, out var value);
            rawValue = value;
            return success;
        }
        
        /// <summary>
        /// Render to value
        /// </summary>
        /// <param name="logEvent"></param>
        /// <param name="reusableBuilder">if null, default layout render will be used</param>
        /// <returns></returns>
        internal T RenderToValueInternal(LogEventInfo logEvent, [CanBeNull] StringBuilder reusableBuilder)
        {
            if (TryGetRawValueIntern(logEvent, out var rawValue))
            {
                return rawValue;
            }

            var text = reusableBuilder != null ? RenderAllocateBuilder(logEvent, reusableBuilder) : _layout.Render(logEvent);
            if (TryParse(text, out var parsedValue))
            {
                return parsedValue;
            }

            InternalLogger.Warn("Parse {0} to {1} failed", text, _typeNamed);
            return default(T);
        }

        private bool TryGetRawValueIntern(LogEventInfo logEvent, out T rawValue)
        {
            if (_fixedValue)
            {
                rawValue = _value;
                return true;
            }

            if (_layout == null)
            {
                rawValue = default(T);
                return true;
            }

            if (_layout.TryGetRawValue(logEvent, out var raw))
            {
                var success = TryConvertRawToValue(raw, out var value);
                rawValue = value;
                if (!success)
                {
                    InternalLogger.Warn("rawvalue isn't a {0} ", _typeNamed);
                }

                return success;
            }

            rawValue = default(T);
            return false;
        }


        private static bool TryConvertRawToValue(object raw, out T value)
        {
            if (raw == null)
            {
                value = default(T);
                return true;
            }

            if (raw is T i)
            {
                value = i;
                return true;
            }

            if (TryConvertTo(raw, out value))
            {
                return true;
            }

            value = default(T);

            return false;
        }

        private static bool TryGetFixedValue(Layout layout, out T value)
        {
            if (layout != null && layout is SimpleLayout simpleLayout && simpleLayout.IsFixedText)
            {
                var success = TryParse(simpleLayout.FixedText, out value);
                if (!success)
                {
                    InternalLogger.Warn("layout with text '{0}' isn't an {1}", simpleLayout.FixedText, _typeNamed);
                }

                return success;
            }

            value = default(T);

            return false;
        }

    }
}
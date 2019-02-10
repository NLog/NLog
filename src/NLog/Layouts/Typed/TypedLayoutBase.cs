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

using System;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using NLog.Common;
using NLog.Internal;

namespace NLog.Layouts
{
    /// <summary>
    /// Layout rendering of <typeparam name="T"></typeparam>
    /// </summary>
    public abstract class TypedLayoutBase<T> : Layout, IToValue<T>
    {
        private readonly Layout _layout;
        private readonly T _value;
        private readonly bool _fixedValue;

        /// <summary>
        /// Name of the typed, for logging
        /// </summary>
        protected abstract string TypedName { get; }

        /// <summary>
        /// Layout with fixed value
        /// </summary>
        /// <param name="value"></param>
        protected TypedLayoutBase(T value)
        {
            _value = value;
            _fixedValue = true;
            _layout = null;
        }

        /// <summary>
        /// Layout with template
        /// </summary>
        /// <param name="layout"></param>
        protected TypedLayoutBase(Layout layout)
        {
            _fixedValue = TryGetFixedValue(layout, out _value);
            _layout = layout;
        }

        /// <summary>
        /// Is fixed?
        /// </summary>
        public bool IsFixed => _fixedValue;

        #region Implementation of IRawValue

        /// <inheritdoc cref="IRawValue" />
        internal override bool TryGetRawValue(LogEventInfo logEvent, out object rawValue)
        {
            if (_fixedValue)
            {
                rawValue = _value;
                return true;
            }

            if (_layout == null)
            {
                rawValue = null;
                return true;
            }

            if (_layout.TryGetRawValue(logEvent, out var raw))
            {
                var success = TryConvertRawToValue(raw, out var i);
                rawValue = i;
                return success;
            }

            rawValue = null;
            return false;
        }

        #endregion

        /// <summary>
        /// Render to value
        /// </summary>
        /// <returns></returns>
        T IToValue<T>.ToValue(LogEventInfo logEvent)
        {
            if (_fixedValue)
            {
                return _value;
            }

            if (_layout == null)
            {
                return default(T);
            }

            if (_layout.TryGetRawValue(logEvent, out var raw))
            {
                if (TryConvertRawToValue(raw, out var value))
                {
                    return value;
                }

                InternalLogger.Warn("rawvalue isn't a {0} ", TypedName);
            }

            var text = _layout.Render(logEvent);
            if (TryParse(text, out var parsedValue))
            {
                return parsedValue;
            }

            InternalLogger.Warn("Parse {0} to {1} failed", text, TypedName);
            return default(T);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        protected abstract string ValueToString([NotNull] T value, CultureInfo cultureInfo);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected abstract bool TryParse(string text, out T value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="raw"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected abstract bool TryConvertTo(object raw, out T value);

        private bool TryConvertRawToValue(object raw, out T value)
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

        private bool TryGetFixedValue(Layout layout, out T value)
        {
            if (layout != null && layout is SimpleLayout simpleLayout && simpleLayout.IsFixedText)
            {
                if (!TryParse(simpleLayout.FixedText, out value))
                {
                    InternalLogger.Warn("layout with text '{0}' isn't an {1}", simpleLayout.FixedText, TypedName);
                }

                return true;
            }

            value = default(T);

            return false;
        }


        #region Overrides of Layout

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



        #endregion
    }
}
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
using System.ComponentModel;
using System.Globalization;

namespace NLog.Layouts
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [CLSCompliant(false)]
    public class PrimitiveLayout<T> : TypedLayout<T>
        where T : IConvertible
    {
        /// <inheritdoc />
        public PrimitiveLayout(T value) : base(value)
        {
        }

        /// <inheritdoc />
        public PrimitiveLayout(Layout layout) : base(layout)
        {
        }

        #region Overrides of TypedLayout<T>

        /// <inheritdoc />
        protected override string TypedName => typeof(T).Name; //todo cache?

        /// <inheritdoc />
        protected override string ValueToString(T value, CultureInfo cultureInfo)
        {
            return value?.ToString(cultureInfo);
        }

        /// <inheritdoc />
        protected override bool TryParse(string text, out T value)
        {
            return TryConvertTo(text, out value);
        }

        /// <inheritdoc />
        protected override bool TryConvertTo(object raw, out T value)
        {
            try
            {
                value = (T)Convert.ChangeType(raw, typeof(T));
                return true;
            }
            catch (Exception)
            {
                // todo log
                value = default;
                return false;
            }
        }

        #endregion

        #region Conversion

        /// <summary>
        /// Converts a given text to a <see cref="Layout" />.
        /// </summary>
        /// <param name="value">Text to be converted.</param>
        /// <returns><see cref="SimpleLayout" /> object represented by the text.</returns>
        public static implicit operator PrimitiveLayout<T>(T value)
        {
            return new PrimitiveLayout<T>(value);
        }

        /// <summary>
        /// Converts a given text to a <see cref="Layout" />.
        /// </summary>
        /// <param name="layout">Text to be converted.</param>
        /// <returns><see cref="SimpleLayout" /> object represented by the text.</returns>
        public static implicit operator PrimitiveLayout<T>([Localizable(false)] string layout)
        {
            return new PrimitiveLayout<T>(layout);
        }

        #endregion
    }
}
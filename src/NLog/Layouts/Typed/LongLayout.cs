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
using System.Linq;
using NLog.Common;

namespace NLog.Layouts
{
    /// <summary>
    /// Layout rendering to Long
    /// </summary>
    public class LongLayout : TypedLayout<long?>
    {
        /// <inheritdoc />
        public LongLayout(Layout layout) : base(layout)
        {

        }

        /// <inheritdoc />
        public LongLayout(long value) : base(value)
        {
        }

        #region Overrides of GenericLayout<long?>

        /// <inheritdoc />
        protected override string TypedName => "long";

        /// <inheritdoc />
        protected override string ValueToString(long? value, CultureInfo cultureInfo)
        {
            return value?.ToString(cultureInfo);
        }

        /// <inheritdoc />
        protected override bool TryParse(string text, out long? value)
        {
            var success = long.TryParse(text, out var value1);
            value = value1;
            return success;
        }

        /// <inheritdoc />
        protected override bool TryConvertTo(object raw, out long? value)
        {
            if (raw is IConvertible c)
            {
                value = Convert.ToInt64(c);

                return true;
            }

            value = null;
            return false;



        }

        #endregion

        #region Conversion

        /// <summary>
        /// Converts a given text to a <see cref="Layout" />.
        /// </summary>
        /// <param name="value">Text to be converted.</param>
        /// <returns><see cref="SimpleLayout" /> object represented by the text.</returns>
        public static implicit operator LongLayout(long value)
        {
            return new LongLayout(value);
        }

        /// <summary>
        /// Converts a given text to a <see cref="Layout" />.
        /// </summary>
        /// <param name="layout">Text to be converted.</param>
        /// <returns><see cref="SimpleLayout" /> object represented by the text.</returns>
        public static implicit operator LongLayout([Localizable(false)] string layout)
        {
            return new LongLayout(layout);
        }

        #endregion
    }
}
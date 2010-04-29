// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using System.Text;
using System.IO;
using NLog.Internal;
using System.ComponentModel;
using NLog.Config;

namespace NLog.LayoutRenderers
{
    /// <summary>
    /// Decodes text "encrypted" with ROT-13.
    /// </summary>
    /// <remarks>
    /// See <a href="http://en.wikipedia.org/wiki/ROT13">http://en.wikipedia.org/wiki/ROT13</a>.
    /// </remarks>
    [LayoutRenderer("rot13")]
    public class Rot13LayoutRenderer: LayoutRenderer
    {
        private Layout _inner;

        /// <summary>
        /// Returns the estimated number of characters that are needed to
        /// hold the rendered value for the specified logging event.
        /// </summary>
        /// <param name="logEvent">Logging event information.</param>
        /// <returns>The number of characters.</returns>
        /// <remarks>
        /// If the exact number is not known or
        /// expensive to calculate this function should return a rough estimate
        /// that's big enough in most cases, but not too big, in order to conserve memory.
        /// </remarks>
        protected internal override int GetEstimatedBufferSize(LogEventInfo logEvent)
        {
            return 30;
        }

        /// <summary>
        /// The text to be decoded.
        /// </summary>
        [DefaultParameter]
        public Layout Text
        {
            get { return _inner; }
            set { _inner = value; }
        }
        /// <summary>
        /// Renders the inner message, decrypts it with ROT-13 and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected internal override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            string msg = _inner.GetFormattedMessage(logEvent);
            builder.Append(ApplyPadding(DecodeRot13(msg)));
        }


        /// <summary>
        /// Determines whether stack trace information should be gathered
        /// during log event processing. By default it calls <see cref="NLog.Layout.NeedsStackTrace"/> on
        /// <see cref="TargetWithLayout.CompiledLayout"/>.
        /// </summary>
        /// <returns>
        /// 0 - don't include stack trace<br/>1 - include stack trace without source file information<br/>2 - include full stack trace
        /// </returns>
        protected internal override int NeedsStackTrace()
        {
            return Math.Max(base.NeedsStackTrace(), _inner.NeedsStackTrace());
        }

        /// <summary>
        /// Encodes/Decodes ROT-13-encoded string.
        /// </summary>
        /// <param name="s">The string to be encoded/decoded</param>
        /// <returns>Encoded/Decoded text</returns>
        public static string DecodeRot13(string s)
        {
            char[] chars = s.ToCharArray();
            for (int i = 0; i < chars.Length; ++i)
            {
                chars[i] = DecodeRot13Char(chars[i]);
            }

            return new string(chars);
        }

        private static char DecodeRot13Char(char c)
        {
            if (c >= 'A' && c <= 'M')
                return (char)('N' + (c - 'A'));
            if (c >= 'a' && c <= 'm')
                return (char)('n' + (c - 'a'));
            if (c >= 'N' && c <= 'Z')
                return (char)('A' + (c - 'N'));
            if (c >= 'n' && c <= 'z')
                return (char)('a' + (c - 'n'));

            return c;
        }
    }
}

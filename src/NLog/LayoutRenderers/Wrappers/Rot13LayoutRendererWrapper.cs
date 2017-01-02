// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.LayoutRenderers.Wrappers
{
    using NLog.Config;
    using NLog.Layouts;

    /// <summary>
    /// Decodes text "encrypted" with ROT-13.
    /// </summary>
    /// <remarks>
    /// See <a href="http://en.wikipedia.org/wiki/ROT13">http://en.wikipedia.org/wiki/ROT13</a>.
    /// </remarks>
    [LayoutRenderer("rot13")]
    [AppDomainFixedOutput]
    [ThreadAgnostic]
    public sealed class Rot13LayoutRendererWrapper : WrapperLayoutRendererBuilderBase
    {
        /// <summary>
        /// Gets or sets the layout to be wrapped.
        /// </summary>
        /// <value>The layout to be wrapped.</value>
        /// <remarks>This variable is for backwards compatibility</remarks>
        /// <docgen category='Transformation Options' order='10' />
        public Layout Text
        {
            get { return this.Inner; }
            set { this.Inner = value; }
        }

        /// <summary>
        /// Encodes/Decodes ROT-13-encoded string.
        /// </summary>
        /// <param name="encodedValue">The string to be encoded/decoded.</param>
        /// <returns>Encoded/Decoded text.</returns>
        public static string DecodeRot13(string encodedValue)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(encodedValue.Length);
            sb.Append(encodedValue);
            DecodeRot13(sb);
            return sb.ToString();
        }

        /// <summary>
        /// Encodes/Decodes ROT-13-encoded string.
        /// </summary>
        /// <param name="encodedValue">The string to be encoded/decoded.</param>
        internal static void DecodeRot13(System.Text.StringBuilder encodedValue)
        {
            if (encodedValue == null)
            {
                return;
            }

            for (int i = 0; i < encodedValue.Length; ++i)
            {
                encodedValue[i] = DecodeRot13Char(encodedValue[i]);
            }
        }

        /// <summary>
        /// Post-processes the rendered message. 
        /// </summary>
        /// <param name="target">Output to be transform.</param>
        protected override void TransformFormattedMesssage(System.Text.StringBuilder target)
        {
            DecodeRot13(target);
        }

        private static char DecodeRot13Char(char c)
        {
            if (c >= 'A' && c <= 'M')
            {
                return (char)('N' + (c - 'A'));
            }

            if (c >= 'a' && c <= 'm')
            {
                return (char)('n' + (c - 'a'));
            }

            if (c >= 'N' && c <= 'Z')
            {
                return (char)('A' + (c - 'N'));
            }

            if (c >= 'n' && c <= 'z')
            {
                return (char)('a' + (c - 'n'));
            }

            return c;
        }
    }
}

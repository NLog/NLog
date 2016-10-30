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
    using NLog.Internal;

    /// <summary>
    /// Encodes the result of another layout output for use with URLs.
    /// </summary>
    [LayoutRenderer("url-encode")]
    [ThreadAgnostic]
    public sealed class UrlEncodeLayoutRendererWrapper : WrapperLayoutRendererBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UrlEncodeLayoutRendererWrapper" /> class.
        /// </summary>
        public UrlEncodeLayoutRendererWrapper()
        {
            this.SpaceAsPlus = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether spaces should be translated to '+' or '%20'.
        /// </summary>
        /// <value>A value of <c>true</c> if space should be translated to '+'; otherwise, <c>false</c>.</value>
        /// <docgen category='Transformation Options' order='10' />
        public bool SpaceAsPlus { get; set; }

        /// <summary>
        /// Gets or sets a value whether escaping be done according to Rfc3986 (Supports Internationalized Resource Identifiers - IRIs)
        /// </summary>
        /// <value>A value of <c>true</c> if Rfc3986; otherwise, <c>false</c> for legacy Rfc2396.</value>
        /// <docgen category='Transformation Options' order='10' />
        public bool EscapeDataRfc3986 { get; set; }

        /// <summary>
        /// Gets or sets a value whether escaping be done according to the old NLog style (Very non-standard)
        /// </summary>
        /// <value>A value of <c>true</c> if legacy encoding; otherwise, <c>false</c> for standard UTF8 encoding.</value>
        /// <docgen category='Transformation Options' order='10' />
        public bool EscapeDataNLogLegacy { get; set; }

        /// <summary>
        /// Transforms the output of another layout.
        /// </summary>
        /// <param name="text">Output to be transform.</param>
        /// <returns>Transformed text.</returns>
        protected override string Transform(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                UrlHelper.EscapeEncodingFlag encodingFlags = UrlHelper.GetUriStringEncodingFlags(EscapeDataNLogLegacy, SpaceAsPlus, EscapeDataRfc3986);
                System.Text.StringBuilder sb = new System.Text.StringBuilder(text.Length + 20);
                UrlHelper.EscapeDataEncode(text, sb, encodingFlags);
                return sb.ToString();
            }
            return string.Empty;
        }
    }
}

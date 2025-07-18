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

namespace NLog.LayoutRenderers.Wrappers
{
    using System;
    using System.ComponentModel;
    using System.Text;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// Encodes the result of another layout output for use with URLs.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/Url-Encode-Layout-Renderer">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/Url-Encode-Layout-Renderer">Documentation on NLog Wiki</seealso>
    [LayoutRenderer("url-encode")]
    [AppDomainFixedOutput]
    [ThreadAgnostic]
    public sealed class UrlEncodeLayoutRendererWrapper : WrapperLayoutRendererBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether spaces should be translated to '+' or '%20'.
        /// </summary>
        /// <remarks>Default: <see langword="true"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        public bool SpaceAsPlus { get; set; } = true;

        /// <summary>
        /// Gets or sets a value whether escaping be done according to Rfc3986 (Supports Internationalized Resource Identifiers - IRIs)
        /// </summary>
        /// <remarks>Default: <see langword="false"/></remarks>
        /// <docgen category='Layout Options' order='50' />
        public bool EscapeDataRfc3986 { get; set; }

        /// <summary>
        /// Gets or sets a value whether escaping be done according to the old NLog style (Very non-standard)
        /// </summary>
        /// <remarks>Default: <see langword="false"/></remarks>
        /// <docgen category='Layout Options' order='100' />
        [Obsolete("Instead use default Rfc2396 or EscapeDataRfc3986. Marked obsolete with NLog v5.3")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool EscapeDataNLogLegacy { get; set; }

        /// <inheritdoc/>
        protected override string Transform(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
#pragma warning disable CS0618 // Type or member is obsolete
                bool escapeDataNLogLegacy = EscapeDataNLogLegacy;
#pragma warning restore CS0618 // Type or member is obsolete
                UrlHelper.EscapeEncodingOptions encodingOptions = UrlHelper.GetUriStringEncodingFlags(escapeDataNLogLegacy, SpaceAsPlus, EscapeDataRfc3986);
                System.Text.StringBuilder sb = new System.Text.StringBuilder(text.Length + 20);
                UrlHelper.EscapeDataEncode(text, sb, encodingOptions);
                return sb.ToString();
            }
            return string.Empty;
        }

        /// <inheritdoc/>
        protected override void RenderInnerAndTransform(LogEventInfo logEvent, StringBuilder builder, int orgLength)
        {
            Inner.Render(logEvent, builder);
            if (builder.Length > orgLength)
            {
                var text = builder.ToString(orgLength, builder.Length - orgLength);
                builder.Length = orgLength;

#pragma warning disable CS0618 // Type or member is obsolete
                bool escapeDataNLogLegacy = EscapeDataNLogLegacy;
#pragma warning restore CS0618 // Type or member is obsolete
                UrlHelper.EscapeEncodingOptions encodingOptions = UrlHelper.GetUriStringEncodingFlags(escapeDataNLogLegacy, SpaceAsPlus, EscapeDataRfc3986);
                UrlHelper.EscapeDataEncode(text, builder, encodingOptions);
            }
        }
    }
}

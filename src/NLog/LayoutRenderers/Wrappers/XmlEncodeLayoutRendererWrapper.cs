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
    using System.Text;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Layouts;

    /// <summary>
    /// Converts the result of another layout output to be XML-compliant.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/Xml-Encode-Layout-Renderer">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/Xml-Encode-Layout-Renderer">Documentation on NLog Wiki</seealso>
    [LayoutRenderer("xml-encode")]
    [AmbientProperty(nameof(XmlEncode))]
    [AppDomainFixedOutput]
    [ThreadAgnostic]
    public sealed class XmlEncodeLayoutRendererWrapper : WrapperLayoutRendererBase
    {
        /// <summary>
        /// Gets or sets whether output should be encoded with XML-string escaping.
        /// </summary>
        /// <remarks>Default: <see langword="true"/></remarks>
        /// <docgen category="Layout Options" order="10"/>
        public bool XmlEncode { get; set; } = true;

        /// <summary>
        /// Gets or sets whether output should be wrapped using CDATA section instead of XML-string escaping
        /// </summary>
        /// <remarks>Default: <see langword="false"/></remarks>
        public bool CDataEncode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to transform newlines (\r\n) into (&#13;&#10;)
        /// </summary>
        /// <remarks>Default: <see langword="false"/></remarks>
        /// <docgen category="Layout Options" order="10"/>
        public bool XmlEncodeNewlines { get; set; }

        private INoAllocationStringValueRenderer? _stringValueRenderer;

        /// <inheritdoc/>
        protected override void InitializeLayoutRenderer()
        {
            base.InitializeLayoutRenderer();
            _stringValueRenderer = ValueTypeLayoutInfo.ResolveStringValueMethod(Inner);
        }

        /// <inheritdoc/>
        protected override void RenderInnerAndTransform(LogEventInfo logEvent, StringBuilder builder, int orgLength)
        {
            if (_stringValueRenderer != null)
            {
                var stringValue = _stringValueRenderer.GetFormattedStringNoAllocation(logEvent);
                if (stringValue != null)
                {
                    if (CDataEncode)
                    {
                        XmlHelper.EscapeCData(stringValue, builder);
                    }
                    else if (XmlEncode)
                    {
                        XmlHelper.EscapeXmlWhenNeeded(stringValue, XmlEncodeNewlines, builder);
                    }
                    else
                    {
                        builder.Append(stringValue);
                    }
                    return;
                }
            }

            if (CDataEncode)
            {
                builder.Append("<![CDATA[");
                orgLength = builder.Length;
            }

            Inner?.Render(logEvent, builder);

            if (CDataEncode)
            {
                XmlHelper.EscapeCDataWhenNeeded(builder, orgLength);
                builder.Append("]]>");
            }
            else if (XmlEncode)
            {
                XmlHelper.EscapeXmlWhenNeeded(builder, orgLength, XmlEncodeNewlines);
            }
        }

        /// <inheritdoc/>
        protected override string Transform(string text)
        {
            if (CDataEncode)
            {
                return XmlHelper.EscapeCData(text);
            }

            if (XmlEncode)
            {
                return XmlHelper.EscapeXmlWhenNeeded(text, XmlEncodeNewlines);
            }

            return text;
        }
    }
}

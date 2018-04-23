// 
// Copyright (c) 2004-2018 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.ComponentModel;
    using System.Text;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// Converts the result of another layout output to be XML-compliant.
    /// </summary>
    [LayoutRenderer("xml-encode")]
    [AmbientProperty("XmlEncode")]
    [ThreadAgnostic]
    [ThreadSafe]
    public sealed class XmlEncodeLayoutRendererWrapper : WrapperLayoutRendererBuilderBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlEncodeLayoutRendererWrapper" /> class.
        /// </summary>
        public XmlEncodeLayoutRendererWrapper()
        {
            XmlEncode = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to apply XML encoding.
        /// </summary>
        /// <docgen category="Transformation Options" order="10"/>
        [DefaultValue(true)]
        public bool XmlEncode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to tranform newlines (\r\n) into (&#13;&#10;)
        /// </summary>
        [DefaultValue(false)]
        public bool XmlEncodeNewlines { get; set; }

        /// <summary>
        /// Post-processes the rendered message. 
        /// </summary>
        /// <param name="target">The text to be post-processed.</param>
        protected override void TransformFormattedMesssage(StringBuilder target)
        {
            if (!XmlEncode)
                return;

            for (int i = 0; i < target.Length; ++i)
            {
                switch (target[i])
                {
                    case '<':
                    case '>':
                    case '&':
                    case '\'':
                    case '"':
                        {
                            string escapeString = target.ToString();
                            target.Length = 0;
                            XmlHelper.EscapeXmlString(escapeString, XmlEncodeNewlines, target);
                            return;
                        }
                    case '\r':
                    case '\n':
                        {
                            if (XmlEncodeNewlines)
                            {
                                string escapeString = target.ToString();
                                target.Length = 0;
                                XmlHelper.EscapeXmlString(escapeString, true, target);
                                return;
                            }
                        } break;
                }
            }
        }
    }
}

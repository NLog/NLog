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
    using System;
    using System.ComponentModel;
    using System.Text;
    using NLog.Config;

    /// <summary>
    /// Escapes output of another layout using JSON rules.
    /// </summary>
    [LayoutRenderer("json-encode")]
    [AmbientProperty("JsonEncode")]
    [ThreadAgnostic]
    public sealed class JsonEncodeLayoutRendererWrapper : WrapperLayoutRendererBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonEncodeLayoutRendererWrapper" /> class.
        /// </summary>
        public JsonEncodeLayoutRendererWrapper()
        {
            this.JsonEncode = true;
            this.EscapeUnicode = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to apply JSON encoding.
        /// </summary>
        /// <docgen category="Transformation Options" order="10"/>
        [DefaultValue(true)]
        public bool JsonEncode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to escape non-ascii characters
        /// </summary>
        /// <docgen category="Transformation Options" order="10"/>
        [DefaultValue(true)]
        public bool EscapeUnicode { get; set; }

        /// <summary>
        /// Post-processes the rendered message. 
        /// </summary>
        /// <param name="text">The text to be post-processed.</param>
        /// <returns>JSON-encoded string.</returns>
        protected override string Transform(string text)
        {
            return this.JsonEncode ? Targets.DefaultJsonSerializer.EscapeString(text, this.EscapeUnicode) : text;
        }
    }
}

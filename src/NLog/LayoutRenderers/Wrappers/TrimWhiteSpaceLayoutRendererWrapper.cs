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
    using System.ComponentModel;
    using NLog.Config;

    /// <summary>
    /// Trims the whitespace from the result of another layout renderer.
    /// </summary>
    [LayoutRenderer("trim-whitespace")]
    [AmbientProperty("TrimWhiteSpace")]
    [ThreadAgnostic]
    public sealed class TrimWhiteSpaceLayoutRendererWrapper : WrapperLayoutRendererBuilderBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TrimWhiteSpaceLayoutRendererWrapper" /> class.
        /// </summary>
        public TrimWhiteSpaceLayoutRendererWrapper()
        {
            this.TrimWhiteSpace = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether lower case conversion should be applied.
        /// </summary>
        /// <value>A value of <c>true</c> if lower case conversion should be applied; otherwise, <c>false</c>.</value>
        /// <docgen category='Transformation Options' order='10' />
        [DefaultValue(true)]
        public bool TrimWhiteSpace { get; set; }

        /// <summary>
        /// Removes white-spaces from both sides of the provided target
        /// </summary>
        /// <param name="target">Output to be transform.</param>
        protected override void TransformFormattedMesssage(System.Text.StringBuilder target)
        {
            if (target == null || target.Length == 0)
                return;

            if (this.TrimWhiteSpace)
            {
                TrimRight(target);  // Fast
                if (target.Length > 0)
                    TrimLeft(target);   // Slower
            }
        }

        private void TrimRight(System.Text.StringBuilder sb)
        {
            int i = sb.Length - 1;
            for (; i >= 0; i--)
                if (!char.IsWhiteSpace(sb[i]))
                    break;

            if (i < sb.Length - 1)
                sb.Length = i + 1;
        }

        private void TrimLeft(System.Text.StringBuilder sb)
        {
            int i = 0;
            for (; i < sb.Length; i++)
                if (!char.IsWhiteSpace(sb[i]))
                    break;

            if (i > 0)
                sb.Remove(0, i);
        }
    }
}

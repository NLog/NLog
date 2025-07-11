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
    using System.Text;
    using NLog.Config;

    /// <summary>
    /// Replaces newline characters from the result of another layout renderer with spaces.
    /// </summary>
    [LayoutRenderer("wrapline")]
    [AmbientProperty(nameof(WrapLine))]
    [AppDomainFixedOutput]
    [ThreadAgnostic]
    public sealed class WrapLineLayoutRendererWrapper : WrapperLayoutRendererBase
    {
        /// <summary>
        /// Gets or sets the line length for wrapping. Only positive values are allowed.
        /// </summary>
        /// <remarks>Default: <see langword="80"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        public int WrapLine { get; set; } = 80;

        /// <inheritdoc/>
        protected override string Transform(string text)
        {
            if (WrapLine <= 0)
            {
                return text;
            }

            var chunkLength = WrapLine;

            if (text.Length <= chunkLength) return text;

            // preallocate correct number of chars
            var result = new StringBuilder(text.Length + (text.Length / chunkLength) * Environment.NewLine.Length);

            // based on : https://stackoverflow.com/questions/36788754/how-can-i-limit-the-length-of-a-line-in-nlog/36789394
            // and : https://stackoverflow.com/questions/1450774/splitting-a-string-into-chunks-of-a-certain-size/8944374#8944374
            for (int pos = 0; pos < text.Length; pos += chunkLength)
            {
                if (chunkLength + pos > text.Length)
                {
                    chunkLength = text.Length - pos;
                }

                result.Append(text.Substring(pos, chunkLength));

                if (chunkLength + pos < text.Length)
                {
                    result.AppendLine();
                }
            }

            return result.ToString();
        }
    }
}

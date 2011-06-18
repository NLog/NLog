// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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

    /// <summary>
    /// Filters characters not allowed in the file names by replacing them with safe character.
    /// </summary>
    [LayoutRenderer("filesystem-normalize")]
    [AmbientProperty("FSNormalize")]
    public sealed class FileSystemNormalizeLayoutRendererWrapper : WrapperLayoutRendererBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemNormalizeLayoutRendererWrapper" /> class.
        /// </summary>
        public FileSystemNormalizeLayoutRendererWrapper()
        {
            this.FSNormalize = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to modify the output of this renderer so it can be used as a part of file path
        /// (illegal characters are replaced with '_').
        /// </summary>
        /// <docgen category='Advanced Options' order='10' />
        [DefaultValue(true)]
        public bool FSNormalize { get; set; }

        /// <summary>
        /// Post-processes the rendered message. 
        /// </summary>
        /// <param name="text">The text to be post-processed.</param>
        /// <returns>Padded and trimmed string.</returns>
        protected override string Transform(string text)
        {
            if (this.FSNormalize)
            {
                var builder = new StringBuilder(text);
                for (int i = 0; i < builder.Length; i++)
                {
                    char c = builder[i];
                    if (!IsSafeCharacter(c))
                    {
                        builder[i] = '_';
                    }
                }

                return builder.ToString();
            }

            return text;
        }

        private static bool IsSafeCharacter(char c)
        {
            if (char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '.' || c == ' ')
            {
                return true;
            }

            return false;
        }
    }
}

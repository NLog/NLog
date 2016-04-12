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
    using NLog.Config;

    /// <summary>
    /// Applies padding to another layout output.
    /// </summary>
    [LayoutRenderer("pad")]
    [AmbientProperty("Padding")]
    [AmbientProperty("PadCharacter")]
    [AmbientProperty("FixedLength")]
    [AmbientProperty("AlignmentOnTruncation")]
    [ThreadAgnostic]
    public sealed class PaddingLayoutRendererWrapper : WrapperLayoutRendererBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaddingLayoutRendererWrapper" /> class.
        /// </summary>
        public PaddingLayoutRendererWrapper()
        {
            this.PadCharacter = ' ';
        }

        /// <summary>
        /// Gets or sets the number of characters to pad the output to. 
        /// </summary>
        /// <remarks>
        /// Positive padding values cause left padding, negative values 
        /// cause right padding to the desired width.
        /// </remarks>
        /// <docgen category='Transformation Options' order='10' />
        public int Padding { get; set; }

        /// <summary>
        /// Gets or sets the padding character.
        /// </summary>
        /// <docgen category='Transformation Options' order='10' />
        [DefaultValue(' ')]
        public char PadCharacter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to trim the 
        /// rendered text to the absolute value of the padding length.
        /// </summary>
        /// <docgen category='Transformation Options' order='10' />
        [DefaultValue(false)]
        public bool FixedLength { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a value that has
        /// been truncated (when <see cref="FixedLength" /> is true)
        /// will be left-aligned (characters removed from the right)
        /// or right-aligned (characters removed from the left). The
        /// default is left alignment.
        /// </summary>
        [DefaultValue(PaddingHorizontalAlignment.Left)]
        public PaddingHorizontalAlignment AlignmentOnTruncation { get; set; }

        /// <summary>
        /// Transforms the output of another layout.
        /// </summary>
        /// <param name="text">Output to be transform.</param>
        /// <returns>Transformed text.</returns>
        protected override string Transform(string text)
        {
            string s = text ?? string.Empty;

            if (this.Padding != 0)
            {
                if (this.Padding > 0)
                {
                    s = s.PadLeft(this.Padding, this.PadCharacter);
                }
                else
                {
                    s = s.PadRight(-this.Padding, this.PadCharacter);
                }

                int absolutePadding = this.Padding;
                if (absolutePadding < 0)
                {
                    absolutePadding = -absolutePadding;
                }

                if (this.FixedLength && s.Length > absolutePadding)
                {
                    if (this.AlignmentOnTruncation == PaddingHorizontalAlignment.Right)
                    {
                        s = s.Substring(s.Length - absolutePadding);
                    }
                    else
                    {
                        //left
                        s = s.Substring(0, absolutePadding);
                    }
                }
            }

            return s;
        }
    }
}

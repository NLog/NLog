// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    /// Applies padding to another layout output.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/Pad-Layout-Renderer">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/Pad-Layout-Renderer">Documentation on NLog Wiki</seealso>
    [LayoutRenderer("pad")]
    [AmbientProperty(nameof(Padding))]
    [AmbientProperty(nameof(PadCharacter))]
    [AmbientProperty(nameof(FixedLength))]
    [AmbientProperty(nameof(AlignmentOnTruncation))]
    [AppDomainFixedOutput]
    [ThreadAgnostic]
    public sealed class PaddingLayoutRendererWrapper : WrapperLayoutRendererBase
    {
        /// <summary>
        /// Gets or sets the number of characters to pad the output to. 
        /// </summary>
        /// <remarks>
        /// Positive padding values cause left padding, negative values 
        /// cause right padding to the desired width.
        /// </remarks>
        /// <docgen category='Layout Options' order='10' />
        public int Padding { get; set; }

        /// <summary>
        /// Gets or sets the padding character.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public char PadCharacter { get; set; } = ' ';

        /// <summary>
        /// Gets or sets a value indicating whether to trim the 
        /// rendered text to the absolute value of the padding length.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public bool FixedLength { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a value that has
        /// been truncated (when <see cref="FixedLength" /> is true)
        /// will be left-aligned (characters removed from the right)
        /// or right-aligned (characters removed from the left). The
        /// default is left alignment.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public PaddingHorizontalAlignment AlignmentOnTruncation { get; set; } = PaddingHorizontalAlignment.Left;

        /// <inheritdoc/>
        protected override void RenderInnerAndTransform(LogEventInfo logEvent, StringBuilder builder, int orgLength)
        {
            Inner.Render(logEvent, builder);
            if (Padding != 0)
            {
                int absolutePadding = Padding;
                if (absolutePadding < 0)
                {
                    absolutePadding = -absolutePadding;
                }

                var deltaLength = AppendPadding(builder, orgLength, absolutePadding);

                if (FixedLength && deltaLength > absolutePadding)
                {
                    if (AlignmentOnTruncation == PaddingHorizontalAlignment.Left)
                    {
                        // Keep left side
                        builder.Length = orgLength + absolutePadding;
                    }
                    else
                    {
                        builder.Remove(orgLength, deltaLength - absolutePadding);
                    }
                }
            }
        }

        private int AppendPadding(StringBuilder builder, int orgLength, int absolutePadding)
        {
            int deltaLength = builder.Length - orgLength;

            if (Padding > 0)
            {
                // Pad Left
                if (deltaLength < 10 || deltaLength >= absolutePadding)
                {
                    for (int i = deltaLength; i < absolutePadding; ++i)
                    {
                        builder.Append(PadCharacter);
                        for (int j = builder.Length - 1; j > orgLength; --j)
                        {
                            builder[j] = builder[j - 1];
                        }
                        builder[orgLength] = PadCharacter;
                        ++deltaLength;
                    }
                }
                else
                {
                    var innerResult = builder.ToString(orgLength, deltaLength);
                    builder.Length = orgLength;
                    for (int i = deltaLength; i < absolutePadding; ++i)
                    {
                        builder.Append(PadCharacter);
                        ++deltaLength;
                    }
                    builder.Append(innerResult);
                }
            }
            else
            {
                // Pad Right
                for (int i = deltaLength; i < absolutePadding; ++i)
                {
                    builder.Append(PadCharacter);
                    ++deltaLength;
                }
            }

            return deltaLength;
        }

        /// <inheritdoc/>
        protected override string Transform(string text)
        {
            throw new NotSupportedException();
        }
    }
}

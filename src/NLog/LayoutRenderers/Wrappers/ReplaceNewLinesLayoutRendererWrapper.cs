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
    using System.Linq;
    using System.Text;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// Replaces newline characters from the result of another layout renderer with spaces.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/Replace-NewLines-Layout-Renderer">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/Replace-NewLines-Layout-Renderer">Documentation on NLog Wiki</seealso>
    [LayoutRenderer("replace-newlines")]
    [AmbientProperty(nameof(ReplaceNewLines))]
    [AppDomainFixedOutput]
    [ThreadAgnostic]
    public sealed class ReplaceNewLinesLayoutRendererWrapper : WrapperLayoutRendererBase
    {
        /// <summary>
        /// Gets or sets a value indicating the string that should be used to replace newlines.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public string Replacement
        {
            get => _replacement;
            set
            {
                _replacement = value;
            }
        }
        private string _replacement = " ";

        /// <summary>
        /// Gets or sets a value indicating the string that should be used to replace newlines.
        /// </summary>
        /// <docgen category='Layout Options' order='50' />
        public string ReplaceNewLines { get => Replacement; set => Replacement = value; }

        private static readonly char[] LineEndCharacters = new char[] { '\u000D', '\u000A', '\u0085', '\u2028', '\u000C', '\u2029' };
        /// <inheritdoc/>
        protected override void RenderInnerAndTransform(LogEventInfo logEvent, StringBuilder builder, int orgLength)
        {
            Inner?.Render(logEvent, builder);
            if (builder.Length > orgLength)
            {
                var containsNewLines = builder.IndexOfAny(LineEndCharacters, orgLength) >= 0;
                if (containsNewLines)
                {
                    string str = builder.ToString(orgLength, builder.Length - orgLength);
                    // If replacement is longer than single character then reserve some extra capacity in StringBuilder to avoid some resize operations.
                    var sb = new StringBuilder(str.Length + (Replacement.Length <= 1 ? 0 : 10 * Replacement.Length));
                    for (int i = 0; i < str.Length; i++)
                    {
                        char currentChar = str[i];
                        if (LineEndCharacters.Contains(currentChar))
                        {
                            if (i < str.Length - 1 && currentChar == '\u000D' && str[i + 1] == '\u000A')
                            {
                                i++;
                                sb.Append(Replacement);
                            }
                            else
                            {
                                sb.Append(Replacement);
                            }
                        }
                        else
                        {
                            sb.Append(currentChar);
                        }
                    }
                    builder.Length = orgLength;
                    builder.Append(sb.ToString());
                }
            }
        }

        /// <inheritdoc/>
        protected override string Transform(string text)
        {
            throw new NotSupportedException();
        }
    }
}

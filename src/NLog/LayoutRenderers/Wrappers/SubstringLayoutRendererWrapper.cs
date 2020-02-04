// 
// Copyright (c) 2004-2020 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    /// Substring the result
    /// </summary>
    /// <example>
    /// ${substring:${level}:start=2:length=2}
    /// ${substring:${level}:start=-2:length=2}
    /// ${substring:Inner=${level}:start=2:length=2}
    /// </example>
    [LayoutRenderer("substring")]
    [AppDomainFixedOutput]
    [ThreadAgnostic]
    [ThreadSafe]
    public sealed class SubstringLayoutRendererWrapper : WrapperLayoutRendererBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UppercaseLayoutRendererWrapper" /> class.
        /// </summary>
        public SubstringLayoutRendererWrapper()
        {
            Start = 0;
        }

        /// <summary>
        /// Gets or sets the start index. 
        /// </summary>
        /// <value>Index</value>
        /// <docgen category='Transformation Options' order='10' />
        [DefaultValue(0)]
        public int Start { get; set; }

        /// <summary>
        /// Gets or sets the length in characters. If <c>null</c>, then the whole string
        /// </summary>
        /// <value>Index</value>
        /// <docgen category='Transformation Options' order='10' />
        [DefaultValue(null)]
        public int? Length { get; set; }


        /// <inheritdoc/>
        protected override void RenderInnerAndTransform(LogEventInfo logEvent, StringBuilder builder, int orgLength)
        {
            if (Length == 0)
            {
                return;
            }

            Inner.RenderAppendBuilder(logEvent, builder);
            var renderedLength = builder.Length - orgLength;

            if (renderedLength > 0)
            {
                var start = CalcStart(renderedLength);
                var length = CalcLength(renderedLength, start);

                var substring = builder.ToString(orgLength + start, length);
                builder.Length = orgLength;
                builder.Append(substring);
            }
        }

        /// <inheritdoc/>
        protected override string Transform(string text)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Calculate start position
        /// </summary>
        /// <returns>0 or positive number</returns>
        private int CalcStart(int textLength)
        {
            var start = Start;
            if (start > textLength)
            {
                start = textLength;
            }
            if (start < 0)
            {
                start = (textLength + start);
                if (start < 0)
                    start = 0;
            }
            return start;
        }

        /// <summary>
        /// Calculate needed length
        /// </summary>
        /// <returns>0 or positive number</returns>
        private int CalcLength(int textLength, int start)
        {
            var length = textLength - start;

            if (Length.HasValue && textLength > Length.Value + start)
            {
                length = Length.Value;
            }
            if (length < 0)
            {
                length = 0;
            }
            return length;
        }
    }
}
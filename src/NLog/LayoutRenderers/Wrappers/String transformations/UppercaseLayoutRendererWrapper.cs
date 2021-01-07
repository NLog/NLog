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
    using System.Globalization;
    using System.Text;
    using NLog.Config;

    /// <summary>
    /// Converts the result of another layout output to upper case.
    /// </summary>
    /// <example>
    /// ${uppercase:${level}} //[DefaultParameter]
    /// ${uppercase:Inner=${level}} 
    /// ${level:uppercase} // [AmbientProperty]
    /// </example>
    [LayoutRenderer("uppercase")]
    [AmbientProperty("Uppercase")]
    [AppDomainFixedOutput]
    [ThreadAgnostic]
    [ThreadSafe]
    public sealed class UppercaseLayoutRendererWrapper : WrapperLayoutRendererBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UppercaseLayoutRendererWrapper" /> class.
        /// </summary>
        public UppercaseLayoutRendererWrapper()
        {
            Culture = CultureInfo.InvariantCulture;
            Uppercase = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether upper case conversion should be applied.
        /// </summary>
        /// <value>A value of <c>true</c> if upper case conversion should be applied otherwise, <c>false</c>.</value>
        /// <docgen category='Transformation Options' order='10' />
        [DefaultValue(true)]
        public bool Uppercase { get; set; }

        /// <summary>
        /// Gets or sets the culture used for rendering. 
        /// </summary>
        /// <docgen category='Transformation Options' order='10' />
        public CultureInfo Culture { get; set; }

        /// <inheritdoc/>
        protected override void RenderInnerAndTransform(LogEventInfo logEvent, StringBuilder builder, int orgLength)
        {
            Inner.RenderAppendBuilder(logEvent, builder);
            if (Uppercase && builder.Length > orgLength)
            {
                TransformToUpperCase(builder, orgLength);
            }
        }

        /// <inheritdoc/>
        protected override string Transform(string text)
        {
            throw new NotSupportedException();
        }

        private void TransformToUpperCase(StringBuilder target, int startPos)
        {
            CultureInfo culture = Culture;

#if NETSTANDARD1_3 || NETSTANDARD1_5
            string stringToUpper = null;
            if (culture != null && culture != CultureInfo.InvariantCulture)
            {
                stringToUpper = target.ToString(startPos, target.Length - startPos);
                stringToUpper = culture.TextInfo.ToUpper(stringToUpper);
            }
#endif

            for (int i = startPos; i < target.Length; ++i)
            {
#if NETSTANDARD1_3 || NETSTANDARD1_5
                if (stringToUpper != null)
                    target[i] = stringToUpper[i];    //no char.ToUpper with culture
                else
                    target[i] = char.ToUpperInvariant(target[i]);
#else
                target[i] = char.ToUpper(target[i], culture);
#endif
            }
        }
    }
}

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
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/Uppercase-Layout-Renderer">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/Uppercase-Layout-Renderer">Documentation on NLog Wiki</seealso>
    [LayoutRenderer("uppercase")]
    [AmbientProperty(nameof(Uppercase))]
    [AmbientProperty(nameof(ToUpper))]
    [AppDomainFixedOutput]
    [ThreadAgnostic]
    public sealed class UppercaseLayoutRendererWrapper : WrapperLayoutRendererBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether upper case conversion should be applied.
        /// </summary>
        /// <value>A value of <see langword="true"/> if upper case conversion should be applied otherwise, <see langword="false"/>.</value>
        /// <docgen category='Layout Options' order='10' />
        public bool Uppercase { get; set; } = true;

        /// <summary>
        /// Same as <see cref="Uppercase"/>-property, so it can be used as ambient property.
        /// </summary>
        /// <example>
        /// ${level:toupper}
        /// </example>
        /// <docgen category="Layout Options" order="10"/>
        public bool ToUpper { get => Uppercase; set => Uppercase = value; }

        /// <summary>
        /// Gets or sets the culture used for rendering.
        /// </summary>
        /// <docgen category='Layout Options' order='100' />
        public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

        /// <inheritdoc/>
        protected override void RenderInnerAndTransform(LogEventInfo logEvent, StringBuilder builder, int orgLength)
        {
            Inner.Render(logEvent, builder);
            if (Uppercase && builder.Length > orgLength)
            {
                TransformToUpperCase(builder, logEvent, orgLength);
            }
        }

        /// <inheritdoc/>
        protected override string Transform(string text)
        {
            throw new NotSupportedException();
        }

        private void TransformToUpperCase(StringBuilder target, LogEventInfo logEvent, int startPos)
        {
            CultureInfo culture = GetCulture(logEvent, Culture);
            for (int i = startPos; i < target.Length; ++i)
            {
                target[i] = char.ToUpper(target[i], culture);
            }
        }
    }
}

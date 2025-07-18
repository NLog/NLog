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
    using NLog.Conditions;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Layouts;

    /// <summary>
    /// Only outputs the inner layout when the specified condition has been met.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/When-Layout-Renderer">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/When-Layout-Renderer">Documentation on NLog Wiki</seealso>
    [LayoutRenderer("when")]
    [AmbientProperty(nameof(When))]
    [ThreadAgnostic]
    public sealed class WhenLayoutRendererWrapper : WrapperLayoutRendererBase, IRawValue
    {
        /// <summary>
        /// Gets or sets the condition that must be met for the <see cref="WrapperLayoutRendererBase.Inner"/> layout to be printed.
        /// </summary>
        /// <remarks><b>[Required]</b> Default: <see langword="null"/></remarks>
        /// <docgen category="Condition Options" order="10"/>
        public ConditionExpression? When { get; set; }

        /// <summary>
        /// If <see cref="When"/> is not met, print this layout.
        /// </summary>
        /// <remarks>Default: <see cref="Layout.Empty"/></remarks>
        /// <docgen category="Condition Options" order="10"/>
        public Layout Else { get; set; } = Layout.Empty;

        /// <inheritdoc/>
        protected override void InitializeLayoutRenderer()
        {
            if (When is null)
                throw new NLogConfigurationException("When-LayoutRenderer When-property must be assigned.");

            base.InitializeLayoutRenderer();
        }

        /// <inheritdoc/>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            int orgLength = builder.Length;
            try
            {
                if (ShouldRenderInner(logEvent))
                {
                    Inner?.Render(logEvent, builder);
                }
                else
                {
                    Else?.Render(logEvent, builder);
                }
            }
            catch
            {
                builder.Length = orgLength; // Rewind/Truncate on exception
                throw;
            }
        }

        /// <inheritdoc/>
        protected override string Transform(string text)
        {
            throw new NotSupportedException();
        }

        private bool ShouldRenderInner(LogEventInfo logEvent)
        {
            return When is null || true.Equals(When.Evaluate(logEvent));
        }

        bool IRawValue.TryGetRawValue(LogEventInfo logEvent, out object? value)
        {
            if (ShouldRenderInner(logEvent))
            {
                return TryGetRawValueFromLayout(logEvent, Inner, out value);
            }

            return TryGetRawValueFromLayout(logEvent, Else, out value);
        }

        private static bool TryGetRawValueFromLayout(LogEventInfo logEvent, Layout layout, out object? value)
        {
            if (layout is null)
            {
                value = null;
                return false;
            }

            return layout.TryGetRawValue(logEvent, out value);
        }
    }
}

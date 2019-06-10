// 
// Copyright (c) 2004-2019 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using NLog.Internal;
    using NLog.Layouts;

    /// <summary>
    /// Outputs alternative layout when the inner layout produces empty result.
    /// </summary>
    [LayoutRenderer("whenEmpty")]
    [AmbientProperty("WhenEmpty")]
    [ThreadAgnostic]
    [ThreadSafe]
    public sealed class WhenEmptyLayoutRendererWrapper : WrapperLayoutRendererBuilderBase, IRawValue, IStringValueRenderer
    {
        private bool _skipStringValueRenderer;

        /// <summary>
        /// Gets or sets the layout to be rendered when original layout produced empty result.
        /// </summary>
        /// <docgen category="Transformation Options" order="10"/>
        [RequiredParameter]
        public Layout WhenEmpty { get; set; }

        /// <inheritdoc/>
        protected override void InitializeLayoutRenderer()
        {
            base.InitializeLayoutRenderer();
            WhenEmpty?.Initialize(LoggingConfiguration);
            _skipStringValueRenderer = !TryGetStringValue(out _, out _);
        }

        /// <inheritdoc/>
        protected override void RenderInnerAndTransform(LogEventInfo logEvent, StringBuilder builder, int orgLength)
        {
            Inner.RenderAppendBuilder(logEvent, builder);
            if (builder.Length > orgLength)
                return;

            // render WhenEmpty when the inner layout was empty
            WhenEmpty.RenderAppendBuilder(logEvent, builder);
        }

        string IStringValueRenderer.GetFormattedString(LogEventInfo logEvent)
        {
            if (_skipStringValueRenderer)
            {
                return null;
            }

            if (TryGetStringValue(out var innerLayout, out var whenEmptyLayout))
            {
                var innerValue = innerLayout.Render(logEvent);
                if (!string.IsNullOrEmpty(innerValue))
                {
                    return innerValue;
                }

                // render WhenEmpty when the inner layout was empty
                return whenEmptyLayout.Render(logEvent);
            }

            _skipStringValueRenderer = true;
            return null;
        }

        private bool TryGetStringValue(out SimpleLayout innerLayout, out SimpleLayout whenEmptyLayout)
        {
            whenEmptyLayout = WhenEmpty as SimpleLayout;
            innerLayout = Inner as SimpleLayout;

            return IsStringLayout(innerLayout) && IsStringLayout(whenEmptyLayout);
        }

        private static bool IsStringLayout(SimpleLayout innerLayout)
        {
            return innerLayout != null && (innerLayout.IsFixedText || innerLayout.IsSimpleStringText);
        }

        bool IRawValue.TryGetRawValue(LogEventInfo logEvent, out object value)
        {
            if (Inner.TryGetRawValue(logEvent, out var innerValue))
            {
                if (innerValue != null && !innerValue.Equals(string.Empty))
                {
                    value = innerValue;
                    return true;
                }
            }
            else
            {
                var innerResult = Inner.Render(logEvent); // Beware this can be very expensive call
                if (!string.IsNullOrEmpty(innerResult))
                {
                    value = null;
                    return false;
                }
            }

            // render WhenEmpty when the inner layout was empty
            return WhenEmpty.TryGetRawValue(logEvent, out value);
        }

        /// <inheritdoc/>
        [Obsolete("Inherit from WrapperLayoutRendererBase and override RenderInnerAndTransform() instead. Marked obsolete in NLog 4.6")]
        protected override void TransformFormattedMesssage(StringBuilder target)
        {
        }
    }
}

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
    using System.Text;
    using NLog.Config;
    using NLog.Layouts;

    /// <summary>
    /// Outputs alternative layout when the inner layout produces empty result.
    /// </summary>
    [LayoutRenderer("whenEmpty")]
    [AmbientProperty("WhenEmpty")]
    [ThreadAgnostic]
    public sealed class WhenEmptyLayoutRendererWrapper : WrapperLayoutRendererBuilderBase
    {
        /// <summary>
        /// Gets or sets the layout to be rendered when original layout produced empty result.
        /// </summary>
        /// <docgen category="Transformation Options" order="10"/>
        [RequiredParameter]
        public Layout WhenEmpty { get; set; }

        /// <summary>
        /// Transforms the output of another layout.
        /// </summary>
        /// <param name="target">Output to be transform.</param>
        protected override void TransformFormattedMesssage(StringBuilder target)
        {
        }

        /// <summary>
        /// Renders the inner layout contents.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <param name="target">Initially empty <see cref="StringBuilder"/> for the result</param>
        protected override void RenderFormattedMessage(LogEventInfo logEvent, StringBuilder target)
        {
            base.RenderFormattedMessage(logEvent, target);
            if (target.Length > 0)
                return;

            // render WhenEmpty when the inner layout was empty
            this.WhenEmpty.RenderAppendBuilder(logEvent, target);
        }
    }
}

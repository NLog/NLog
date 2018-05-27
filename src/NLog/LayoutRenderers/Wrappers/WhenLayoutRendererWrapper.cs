// 
// Copyright (c) 2004-2018 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using NLog.Conditions;
    using NLog.Config;
    using NLog.Layouts;

    /// <summary>
    /// Only outputs the inner layout when the specified condition has been met.
    /// </summary>
    [LayoutRenderer("when")]
    [AmbientProperty("When")]
    [ThreadAgnostic]
    [ThreadSafe]
    public sealed class WhenLayoutRendererWrapper : WrapperLayoutRendererBuilderBase
    {
        /// <summary>
        /// Gets or sets the condition that must be met for the <see cref="WrapperLayoutRendererBase.Inner"/> layout to be printed.
        /// </summary>
        /// <docgen category="Transformation Options" order="10"/>
        [RequiredParameter]
        public ConditionExpression When { get; set; }

        /// <summary>
        /// If <see cref="When"/> is not met, print this layout.
        /// </summary>
        /// <docgen category="Transformation Options" order="10"/>
        public Layout Else { get; set; }

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
        /// <param name="builder"><see cref="StringBuilder"/> for the result</param>
        /// <param name="logEvent">The log event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            int orgLength = builder.Length;
            try
            {
                if (When == null || true.Equals(When.Evaluate(logEvent)))
                {
                    base.RenderFormattedMessage(logEvent, builder);
                }
                else if (Else != null)
                {
                    Else.RenderAppendBuilder(logEvent, builder);
                }
            }
            catch
            {
                builder.Length = orgLength; // Rewind/Truncate on exception
                throw;
            }
        }
    }
}

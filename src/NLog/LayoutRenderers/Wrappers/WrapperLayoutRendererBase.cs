// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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

using System.Text;
using NLog.Config;
using NLog.Internal;
using NLog.Layouts;

namespace NLog.LayoutRenderers.Wrappers
{
    /// <summary>
    /// Decodes text "encrypted" with ROT-13.
    /// </summary>
    /// <remarks>
    /// See <a href="http://en.wikipedia.org/wiki/ROT13">http://en.wikipedia.org/wiki/ROT13</a>.
    /// </remarks>
    public abstract class WrapperLayoutRendererBase : LayoutRenderer
    {
        /// <summary>
        /// Gets or sets the wrapped layout.
        /// </summary>
        [DefaultParameter]
        public Layout Inner { get; set; }

        /// <summary>
        /// Initializes the layout renderer.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            if (this.Inner != null)
            {
                this.Inner.Initialize();
            }
        }

        /// <summary>
        /// Closes the layout renderer.
        /// </summary>
        public override void Close()
        {
            if (this.Inner != null && this.Inner.IsInitialized)
            {
                this.Inner.Close();
            }

            base.Close();
        }

        /// <summary>
        /// Returns the estimated number of characters that are needed to
        /// hold the rendered value for the specified logging event.
        /// </summary>
        /// <param name="logEvent">Logging event information.</param>
        /// <returns>The number of characters.</returns>
        /// <remarks>
        /// If the exact number is not known or
        /// expensive to calculate this function should return a rough estimate
        /// that's big enough in most cases, but not too big, in order to conserve memory.
        /// </remarks>
        protected internal override int GetEstimatedBufferSize(LogEventInfo logEvent)
        {
            return 30;
        }

        /// <summary>
        /// Renders the inner message, processes it and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected internal override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            string msg = this.Inner.GetFormattedMessage(logEvent);
            builder.Append(this.Transform(msg));
        }

        /// <summary>
        /// Gets or sets a value indicating whether stack trace information should be gathered
        /// during log event processing. By default it calls <see cref="Layout.GetStackTraceUsage"/> on
        /// <see cref="Layout"/>.
        /// </summary>
        /// <returns>A <see cref="StackTraceUsage" /> value.</returns>
        protected internal override StackTraceUsage GetStackTraceUsage()
        {
            return StackTraceUsageUtils.Max(base.GetStackTraceUsage(), this.Inner.GetStackTraceUsage());
        }

        /// <summary>
        /// Gets or sets a value indicating whether the value produced by the layout renderer
        /// is fixed per current app-domain.
        /// </summary>
        /// <returns>
        /// The boolean value of <c>true</c> makes the value
        /// of the layout renderer be precalculated and inserted as a literal
        /// in the resulting layout string.
        /// </returns>
        protected internal override bool IsAppDomainFixed()
        {
            return this.Inner.IsAppDomainFixed();
        }

        /// <summary>
        /// Transforms the output of another layout.
        /// </summary>
        /// <param name="text">Output to be transform.</param>
        /// <returns>Transformed text.</returns>
        protected abstract string Transform(string text);
    }
}

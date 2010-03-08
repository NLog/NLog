// 
// Copyright (c) 2004-2010 Jaroslaw Kowalski <jaak@jkowalski.net>
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

using System;
using System.Text;
using NLog.Config;
using NLog.Internal;

namespace NLog.LayoutRenderers
{
    /// <summary>
    /// Render environmental information related to logging events.
    /// </summary>
    public abstract class LayoutRenderer : INLogConfigurationItem, ISupportsInitialize
    {
        public int GetBufferSize(LogEventInfo logEvent)
        {
            return this.GetEstimatedBufferSize(logEvent);
        }

        /// <summary>
        /// Initializes the layout renderer.
        /// </summary>
        public virtual void Initialize()
        {
        }

        /// <summary>
        /// Closes the layout renderer.
        /// </summary>
        public virtual void Close()
        {
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var lra = (LayoutRendererAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(LayoutRendererAttribute));
            if (lra != null)
            {
                return "Layout Renderer: ${" + lra.Name + "}";
            }

            return this.GetType().Name;
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
        protected abstract int GetEstimatedBufferSize(LogEventInfo logEvent);

        /// <summary>
        /// Determines whether stack trace information should be gathered
        /// during log event processing.
        /// </summary>
        /// <returns>A <see cref="StackTraceUsage" /> value that determines stack trace handling.</returns>
        public virtual StackTraceUsage GetStackTraceUsage()
        {
            return StackTraceUsage.None;
        }

        /// <summary>
        /// Determines whether the layout renderer is volatile.
        /// </summary>
        /// <returns>A boolean indicating whether the layout renderer is volatile.</returns>
        /// <remarks>
        /// Volatile layout renderers are dependent on information not contained 
        /// in <see cref="LogEventInfo"/> (such as thread-specific data, MDC data, NDC data).
        /// </remarks>
        public virtual bool IsVolatile()
        {
            return true;
        }

        /// <summary>
        /// Renders the specified environmental information and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected abstract void Append(StringBuilder builder, LogEventInfo logEvent);

        /// <summary>
        /// Determines whether the value produced by the layout renderer
        /// is fixed per current app-domain.
        /// </summary>
        /// <returns>The boolean value of <c>true</c> makes the value
        /// of the layout renderer be precalculated and inserted as a literal
        /// in the resulting layout string.</returns>
        protected virtual bool IsAppDomainFixed()
        {
            return false;
        }

        public bool CanBeConvertedToLiteral()
        {
            return this.IsAppDomainFixed();
        }

        public int GetEstimatedBufferSize2(LogEventInfo logEvent)
        {
            return this.GetEstimatedBufferSize(logEvent);
        }

        public void Render(StringBuilder builder, LogEventInfo logEvent)
        {
            this.Append(builder, logEvent);
        }
    }
}

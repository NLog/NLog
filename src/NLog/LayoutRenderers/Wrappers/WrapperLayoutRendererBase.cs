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

using System;
using System.Text;
using System.IO;
using NLog.Internal;
using System.ComponentModel;
using NLog.Config;
using NLog.Layouts;

namespace NLog.LayoutRenderers.Wrappers
{
    /// <summary>
    /// Decodes text "encrypted" with ROT-13.
    /// </summary>
    /// <remarks>
    /// See <a href="http://en.wikipedia.org/wiki/ROT13">http://en.wikipedia.org/wiki/ROT13</a>.
    /// </remarks>
    public abstract class WrapperLayoutRendererBase: LayoutRenderer
    {
        private Layout _inner;

        /// <summary>
        /// Wrapped layout
        /// </summary>
        [DefaultParameter]
        public Layout Inner
        {
            get { return _inner; }
            set { _inner = value; }
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
            string msg = Inner.GetFormattedMessage(logEvent);
            builder.Append(Transform(msg));
        }

        /// <summary>
        /// Transforms a layout output.
        /// </summary>
        /// <param name="text">Layout output to be transform</param>
        /// <returns>Transformed text</returns>
        protected abstract string Transform(string text);

        /// <summary>
        /// Determines whether stack trace information should be gathered
        /// during log event processing. By default it calls <see cref="Layout.GetStackTraceUsage"/> on
        /// <see cref="Layout"/>.
        /// </summary>
        /// <returns>A <see cref="StackTraceUsage" /> value.</returns>
        protected internal override StackTraceUsage GetStackTraceUsage()
        {
            return StackTraceUsageUtils.Max(base.GetStackTraceUsage(), Inner.GetStackTraceUsage());
        }

        public override void Initialize()
        {
            base.Initialize();
            if (Inner != null)
            {
                Inner.Initialize();
            }
        }

        public override void Close()
        {
            if (Inner != null && Inner.IsInitialized)
            {
                Inner.Close();
            }
            base.Close();
        }

        protected internal override bool IsAppDomainFixed()
        {
            return Inner.IsAppDomainFixed();
        }
    }
}

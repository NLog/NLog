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

using System;

namespace NLog.LayoutRenderers.Wrappers
{
    using System.Text;
    using NLog.Common;
    using NLog.Config;
    using NLog.Layouts;

    /// <summary>
    /// Base class for <see cref="LayoutRenderer"/>s which wrapping other <see cref="LayoutRenderer"/>s. 
    /// 
    /// This has the <see cref="Inner"/> property (which is default) and can be used to wrap.
    /// </summary>
    /// <example>
    /// ${uppercase:${level}} //[DefaultParameter]
    /// ${uppercase:Inner=${level}} 
    /// </example>
    public abstract class WrapperLayoutRendererBase : LayoutRenderer
    {
        /// <summary>
        /// Gets or sets the wrapped layout.
        /// 
        /// [DefaultParameter] so Inner: is not required if it's the first
        /// </summary>
        /// <docgen category='Transformation Options' order='10' />
        [DefaultParameter]
        public Layout Inner { get; set; }

        /// <inheritdoc/>
        protected override void InitializeLayoutRenderer()
        {
            base.InitializeLayoutRenderer();
            Inner?.Initialize(LoggingConfiguration);
        }

        /// <summary>
        /// Renders the inner message, processes it and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (Inner == null)
            {
                InternalLogger.Warn("{0} has no configured Inner-Layout, so skipping", this);
                return;
            }

            int orgLength = builder.Length;
            try
            {
                RenderInnerAndTransform(logEvent, builder, orgLength);
            }
            catch
            {
                builder.Length = orgLength; // Rewind/Truncate on exception
                throw;
            }
        }

        /// <summary>
        /// Appends the rendered output from <see cref="Inner"/>-layout and transforms the added output (when necessary)
        /// </summary>
        /// <param name="logEvent">Logging event.</param>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="orgLength">Start position for any necessary transformation of <see cref="StringBuilder"/>.</param>
        protected virtual void RenderInnerAndTransform(LogEventInfo logEvent, StringBuilder builder, int orgLength)
        {
            string msg = RenderInner(logEvent);
            builder.Append(Transform(logEvent, msg));
        }

        /// <summary>
        /// Transforms the output of another layout.
        /// </summary>
        /// <param name="logEvent">Logging event.</param>
        /// <param name="text">Output to be transform.</param>
        /// <returns>Transformed text.</returns>
        protected virtual string Transform(LogEventInfo logEvent, string text)
        {
            return Transform(text);
        }

        /// <summary>
        /// Transforms the output of another layout.
        /// </summary>
        /// <param name="text">Output to be transform.</param>
        /// <returns>Transformed text.</returns>
        protected abstract string Transform(string text);

        /// <summary>
        /// Renders the inner layout contents.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <returns>Contents of inner layout.</returns>
        protected virtual string RenderInner(LogEventInfo logEvent)
        {
            return Inner?.Render(logEvent) ?? string.Empty;
        }
    }
}

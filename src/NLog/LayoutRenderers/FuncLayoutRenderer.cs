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

namespace NLog.LayoutRenderers
{
    using System;
    using System.Text;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// A layout renderer which could have different behavior per instance by using a <see cref="Func{TResult}"/>.
    /// </summary>
    public class FuncLayoutRenderer : LayoutRenderer
    {
        /// <summary>
        /// Create a new.
        /// </summary>
        /// <param name="layoutRendererName">Name without ${}.</param>
        /// <param name="renderMethod">Method that renders the layout.</param>
        public FuncLayoutRenderer(string layoutRendererName, Func<LogEventInfo, LoggingConfiguration, object> renderMethod)
        {
            RenderMethod = renderMethod;
            LayoutRendererName = layoutRendererName;
        }

        /// <summary>
        /// Name used in config without ${}. E.g. "test" could be used as "${test}".
        /// </summary>
        public string LayoutRendererName { get; set; }

        /// <summary>
        /// Method that renders the layout. 
        /// </summary>
        public Func<LogEventInfo, LoggingConfiguration, object> RenderMethod { get; private set; }

        /// <inheritdoc />
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            var value = GetValue(logEvent);
            if (value != null)
            {
                builder.Append(value);
            }
        }

        private object GetValue(LogEventInfo logEvent)
        {
            var renderMethod = RenderMethod?.Invoke(logEvent, LoggingConfiguration);
            return renderMethod;
        }
    }
}
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

using System;
using System.Text;
using NLog.Config;

namespace NLog.LayoutRenderers
{
    /// <summary>
    /// A layout renderer which could have different behavriour per instance.
    /// </summary>
    public class AdhocLayoutRenderer : LayoutRenderer, IAdhocLayoutRenderer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="layoutRendererName"></param>
        /// <param name="renderFunc"></param>
        public AdhocLayoutRenderer( string layoutRendererName, Func<LogEventInfo, LoggingConfiguration, object> renderFunc)
        {
            RenderFunc = renderFunc;
            LayoutRendererName = layoutRendererName;
        }

        /// <summary>
        /// Name used in config. E.g. "test" could be used as "${test}".
        /// </summary>
        public string LayoutRendererName { get; set; }

        /// <summary>
        /// Method to render 
        /// </summary>
        public Func<LogEventInfo, LoggingConfiguration, object> RenderFunc { get; private set; }

        #region Overrides of LayoutRenderer

        /// <summary>
        /// Renders the specified environmental information and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (RenderFunc != null)
            {
                builder.Append(RenderFunc(logEvent, LoggingConfiguration));
            }

        }

        #endregion
    }
}
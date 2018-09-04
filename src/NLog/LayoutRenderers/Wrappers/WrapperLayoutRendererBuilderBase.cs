// 
// Copyright (c) 2004-2017 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.LayoutRenderers.Wrappers
{
    /// <summary>
    /// Base class for <see cref="LayoutRenderer"/>s which wrapping other <see cref="LayoutRenderer"/>s. 
    /// 
    /// This expects the transformation to work on a <see cref="StringBuilder"/>
    /// </summary>
    public abstract class WrapperLayoutRendererBuilderBase : WrapperLayoutRendererBase
    {
        /// <summary>
        /// Render to local target using Inner Layout, and then transform before final append
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="logEvent"></param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            using (var localTarget = new Internal.AppendBuilderCreator(builder, true))
            {
                RenderFormattedMessage(logEvent, localTarget.Builder);
                TransformFormattedMesssage(logEvent, localTarget.Builder);
            }
        }

        /// <summary>
        /// Transforms the output of another layout.
        /// </summary>
        /// <param name="logEvent"></param>
        /// <param name="target">Output to be transform.</param>
        protected virtual void TransformFormattedMesssage(LogEventInfo logEvent, StringBuilder target)
        {
            TransformFormattedMesssage(target);
        }

        /// <summary>
        /// Transforms the output of another layout.
        /// </summary>
        /// <param name="target">Output to be transform.</param>
        protected abstract void TransformFormattedMesssage(StringBuilder target);

        /// <summary>
        /// Renders the inner layout contents.
        /// </summary>
        /// <param name="logEvent"></param>
        /// <param name="target"><see cref="StringBuilder"/> for the result</param>
        protected virtual void RenderFormattedMessage(LogEventInfo logEvent, StringBuilder target)
        {
            Inner.RenderAppendBuilder(logEvent, target);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        protected sealed override string Transform(string text)
        {
            throw new System.NotSupportedException("Use TransformFormattedMesssage");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logEvent"></param>
        /// <returns></returns>
        protected sealed override string RenderInner(LogEventInfo logEvent)
        {
            throw new System.NotSupportedException("Use RenderFormattedMessage");
        }
    }
}

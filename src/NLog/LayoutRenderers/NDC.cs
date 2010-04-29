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

using NLog.LayoutRenderers;

namespace NLog.LayoutRenderers
{
    /// <summary>
    /// Nested Diagnostic Context item. Provided for compatibility with log4net.
    /// </summary>
    [LayoutRenderer("ndc")]
    public class NDCLayoutRenderer: LayoutRenderer
    {
        private int _topFrames = -1;
        private int _bottomFrames = -1;
        private string _separator = " ";

        /// <summary>
        /// The number of top stack frames to be rendered.
        /// </summary>
        public int TopFrames
        {
            get { return _topFrames; }
            set { _topFrames = value; }
        }

        /// <summary>
        /// The number of bottom stack frames to be rendered.
        /// </summary>
        public int BottomFrames
        {
            get { return _bottomFrames; }
            set { _bottomFrames = value; }
        }

        /// <summary>
        /// The separator to be used for concatenating NDC output.
        /// </summary>
        public string Separator
        {
            get { return _separator; }
            set { _separator = value; }
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
            return 0;
        }

        /// <summary>
        /// Renders the specified NDC item and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected internal override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            string msg;

            if (TopFrames !=  - 1)
            {
                msg = NDC.GetTopMessages(TopFrames, Separator);
            }
            else if (BottomFrames !=  - 1)
            {
                msg = NDC.GetBottomMessages(BottomFrames, Separator);
            }
            else
            {
                msg = NDC.GetAllMessages(Separator);
            }
            builder.Append(ApplyPadding(msg));
        }
    }
}

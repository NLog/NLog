// 
// Copyright (c) 2004,2005 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
// 
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
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

#if !NETCF
using System;
using System.Text;
using System.Diagnostics;
using System.Reflection;

namespace NLog.LayoutRenderers
{
    /// <summary>
    /// Stack trace renderer.
    /// </summary>
    [LayoutRenderer("stacktrace")]
    public class StackTraceLayoutRenderer: LayoutRenderer
    {
        private string _format = "flat";
        private int _topFrames = 3;

        /// <summary>
        /// The output format of the stack trace.
        /// </summary>
        /// <remarks>
        /// Allowed values are <c>raw</c>, <c>flat</c> and <c>detailedflat</c>.
        /// </remarks>
        [System.ComponentModel.DefaultValue("flat")]
        public string Format
        {
            get { return _format; }
            set { _format = value; }
        }

        /// <summary>
        /// The number of top stack frames to be rendered.
        /// </summary>
        [System.ComponentModel.DefaultValue(3)]
        public int TopFrames
        {
            get { return _topFrames; }
            set { _topFrames = value; }
        }


        /// <summary>
        /// Returns the estimated number of characters that are needed to
        /// hold the rendered value for the specified logging event.
        /// </summary>
        /// <param name="ev">Logging event information.</param>
        /// <returns>The number of characters.</returns>
        /// <remarks>
        /// If the exact number is not known or
        /// expensive to calculate this function should return a rough estimate
        /// that's big enough in most cases, but not too big, in order to conserve memory.
        /// </remarks>
        protected internal override int GetEstimatedBufferSize(LogEventInfo ev)
        {
            return 200;
        }

        /// <summary>
        /// Checks whether the stack trace is requested.
        /// </summary>
        /// <returns>2 when the source file information is requested, 1 otherwise.</returns>
        protected internal override int NeedsStackTrace()
        {
            return 2;
        }

        /// <summary>
        /// Renders the call site and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="ev">Logging event.</param>
        protected internal override void Append(StringBuilder builder, LogEventInfo ev)
        {
            bool first = true;
            int startingFrame = ev.UserStackFrameNumber + TopFrames - 1;
            if (startingFrame >= ev.StackTrace.FrameCount)
                startingFrame = ev.StackTrace.FrameCount - 1;

            switch (Format)
            {
                case "raw":
                    for (int i = startingFrame; i >= ev.UserStackFrameNumber; --i)
                    {
                        StackFrame f = ev.StackTrace.GetFrame(i);
                        builder.Append(f.ToString());
                    }
                    break;

                case "flat":
                    for (int i = startingFrame; i >= ev.UserStackFrameNumber; --i)
                    {
                        StackFrame f = ev.StackTrace.GetFrame(i);
                        if (!first)
                            builder.Append(" => ");

                        builder.Append(f.GetMethod().DeclaringType.Name);
                        builder.Append(".");
                        builder.Append(f.GetMethod().Name);
                        first = false;
                    }
                    break;

                case "detailedflat":
                    for (int i = startingFrame; i >= ev.UserStackFrameNumber; --i)
                    {
                        StackFrame f = ev.StackTrace.GetFrame(i);
                        if (!first)
                            builder.Append(" => ");

                        builder.Append("[");
                        builder.Append(f.GetMethod());
                        builder.Append("]");
                        first = false;
                    }
                    break;
            }
        }
    }
}

#endif

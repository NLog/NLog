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

#if !NETCF
using System;
using System.Text;
using System.Diagnostics;
using System.Reflection;

using NLog.Config;

namespace NLog.LayoutRenderers
{
    /// <summary>
    /// Format of the ${stacktrace} layout renderer output.
    /// </summary>
    public enum StackTraceFormat
    {
        /// <summary>
        /// Raw format (multiline - as returned by StackFrame.ToString() method)
        /// </summary>
        Raw,

        /// <summary>
        /// Flat format (class and method names displayed in a single line)
        /// </summary>
        Flat,

        /// <summary>
        /// Detailed flat format (method signatures displayed in a single line)
        /// </summary>
        DetailedFlat,
    }

    /// <summary>
    /// Stack trace renderer.
    /// </summary>
    [LayoutRenderer("stacktrace",UsingLogEventInfo=true,IgnoresPadding=true)]
    [NotSupportedRuntime(Framework=RuntimeFramework.DotNetCompactFramework)]
    public class StackTraceLayoutRenderer: LayoutRenderer
    {
        private StackTraceFormat _format = StackTraceFormat.Flat;
        private int _topFrames = 3;
        private string _separator = " => ";

        /// <summary>
        /// The output format of the stack trace.
        /// </summary>
        [System.ComponentModel.DefaultValue("Flat")]
        public StackTraceFormat Format
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
        /// Stack frame separator string.
        /// </summary>
        [System.ComponentModel.DefaultValue(" => ")]
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
        /// <param name="logEvent">Logging event.</param>
        protected internal override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            bool first = true;
            int startingFrame = logEvent.UserStackFrameNumber + TopFrames - 1;
            if (startingFrame >= logEvent.StackTrace.FrameCount)
                startingFrame = logEvent.StackTrace.FrameCount - 1;

            switch (Format)
            {
                case StackTraceFormat.Raw:
                    for (int i = startingFrame; i >= logEvent.UserStackFrameNumber; --i)
                    {
                        StackFrame f = logEvent.StackTrace.GetFrame(i);
                        builder.Append(f.ToString());
                    }
                    break;

                case StackTraceFormat.Flat:
                    for (int i = startingFrame; i >= logEvent.UserStackFrameNumber; --i)
                    {
                        StackFrame f = logEvent.StackTrace.GetFrame(i);
                        if (!first)
                            builder.Append(_separator);

                        builder.Append(f.GetMethod().DeclaringType.Name);
                        builder.Append(".");
                        builder.Append(f.GetMethod().Name);
                        first = false;
                    }
                    break;

                case StackTraceFormat.DetailedFlat:
                    for (int i = startingFrame; i >= logEvent.UserStackFrameNumber; --i)
                    {
                        StackFrame f = logEvent.StackTrace.GetFrame(i);
                        if (!first)
                            builder.Append(_separator);

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

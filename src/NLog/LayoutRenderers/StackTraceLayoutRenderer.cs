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

namespace NLog.LayoutRenderers
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Text;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// Stack trace renderer.
    /// </summary>
    [LayoutRenderer("stacktrace")]
    [ThreadAgnostic]
    [ThreadSafe]
    public class StackTraceLayoutRenderer : LayoutRenderer, IUsesStackTrace
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StackTraceLayoutRenderer" /> class.
        /// </summary>
        public StackTraceLayoutRenderer()
        {
            Separator = " => ";
            TopFrames = 3;
            Format = StackTraceFormat.Flat;
        }

        /// <summary>
        /// Gets or sets the output format of the stack trace.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue("Flat")]
        public StackTraceFormat Format { get; set; }

        /// <summary>
        /// Gets or sets the number of top stack frames to be rendered.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(3)]
        public int TopFrames { get; set; }

        /// <summary>
        /// Gets or sets the number of frames to skip.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(0)]
        public int SkipFrames { get; set; }

        /// <summary>
        /// Gets or sets the stack frame separator string.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(" => ")]
        public string Separator { get; set; }

        /// <summary>
        /// Gets the level of stack trace information required by the implementing class.
        /// </summary>
        /// <value></value>
        StackTraceUsage IUsesStackTrace.StackTraceUsage => (Format == StackTraceFormat.Raw) ? StackTraceUsage.Max : StackTraceUsage.WithoutSource;

        /// <summary>
        /// Renders the call site and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (logEvent.StackTrace == null)
                return;

            int startingFrame = logEvent.UserStackFrameNumber + TopFrames - 1;
            if (startingFrame >= logEvent.StackTrace.GetFrameCount())
            {
                startingFrame = logEvent.StackTrace.GetFrameCount() - 1;
            }

            int endingFrame = logEvent.UserStackFrameNumber + SkipFrames;
            switch (Format)
            {
                case StackTraceFormat.Raw:
                    AppendRaw(builder, logEvent, startingFrame, endingFrame);
                    break;

                case StackTraceFormat.Flat:
                    AppendFlat(builder, logEvent, startingFrame, endingFrame);
                    break;

                case StackTraceFormat.DetailedFlat:
                    AppendDetailedFlat(builder, logEvent, startingFrame, endingFrame);
                    break;
            }
        }

        private static void AppendRaw(StringBuilder builder, LogEventInfo logEvent, int startingFrame, int endingFrame)
        {
            for (int i = startingFrame; i >= endingFrame; --i)
            {
                StackFrame f = logEvent.StackTrace.GetFrame(i);
                builder.Append(f.ToString());
            }
        }

        private void AppendFlat(StringBuilder builder, LogEventInfo logEvent, int startingFrame, int endingFrame)
        {
            bool first = true;
            for (int i = startingFrame; i >= endingFrame; --i)
            {
                StackFrame f = logEvent.StackTrace.GetFrame(i);
                if (!first)
                {
                    builder.Append(Separator);
                }

                var method = f.GetMethod();
                if (method == null)
                {
                    continue;   // Net Native can have StackFrames without managed methods
                }

                var type = method.DeclaringType;
                if (type != null)
                {
                    builder.Append(type.Name);
                }
                else
                {
                    builder.Append("<no type>");
                }

                builder.Append(".");
                builder.Append(method.Name);
                first = false;
            }
        }

        private void AppendDetailedFlat(StringBuilder builder, LogEventInfo logEvent, int startingFrame, int endingFrame)
        {
            bool first = true;
            for (int i = startingFrame; i >= endingFrame; --i)
            {
                StackFrame f = logEvent.StackTrace.GetFrame(i);
                var method = f.GetMethod();
                if (method == null)
                {
                    continue;   // Net Native can have StackFrames without managed methods
                }

                if (!first)
                {
                    builder.Append(Separator);
                }
                builder.Append("[");
                builder.Append(method);
                builder.Append("]");
                first = false;
            }
        }
    }
}

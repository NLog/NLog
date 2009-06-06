// 
// Copyright (c) 2004-2009 Jaroslaw Kowalski <jaak@jkowalski.net>
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

#if !NET_CF
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace NLog.LayoutRenderers
{
    /// <summary>
    /// The call site (class name, method name and source information).
    /// </summary>
    [LayoutRenderer("callsite")]
    public class CallSiteLayoutRenderer : LayoutRenderer
    {
        /// <summary>
        /// Initializes a new instance of the CallSiteLayoutRenderer class.
        /// </summary>
        public CallSiteLayoutRenderer()
        {
            this.ClassName = true;
            this.MethodName = true;
            this.FileName = false;
            this.IncludeSourcePath = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to render the class name.
        /// </summary>
        [DefaultValue(true)]
        public bool ClassName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to render the method name.
        /// </summary>
        [DefaultValue(true)]
        public bool MethodName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to render the source file name and line number.
        /// </summary>
        [DefaultValue(false)]
        public bool FileName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include source file path.
        /// </summary>
        [DefaultValue(true)]
        public bool IncludeSourcePath { get; set; }

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
        protected internal override StackTraceUsage GetStackTraceUsage()
        {
            return this.FileName ? StackTraceUsage.WithSource : StackTraceUsage.WithoutSource;
        }

        /// <summary>
        /// Determines whether the layout renderer is volatile.
        /// </summary>
        /// <returns>
        /// A boolean indicating whether the layout renderer is volatile.
        /// </returns>
        /// <remarks>
        /// Volatile layout renderers are dependent on information not contained
        /// in <see cref="LogEventInfo"/> (such as thread-specific data, MDC data, NDC data).
        /// </remarks>
        protected internal override bool IsVolatile()
        {
            return false;
        }

        /// <summary>
        /// Renders the call site and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected internal override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            StackFrame frame = logEvent.UserStackFrame;
            if (frame != null)
            {
                MethodBase method = frame.GetMethod();
                if (this.ClassName)
                {
                    builder.Append(method.DeclaringType.FullName);
                }

                if (this.MethodName)
                {
                    if (this.ClassName)
                    {
                        builder.Append(".");
                    }

                    builder.Append(method.Name);
                }

                if (this.FileName)
                {
                    string fileName = frame.GetFileName();
                    if (fileName != null)
                    {
                        builder.Append("(");
                        if (this.IncludeSourcePath)
                        {
                            builder.Append(fileName);
                        }
                        else
                        {
                            builder.Append(Path.GetFileName(fileName));
                        }

                        builder.Append(":");
                        builder.Append(frame.GetFileLineNumber());
                        builder.Append(")");
                    }
                }
            }
        }
    }
}

#endif

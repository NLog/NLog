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
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Text;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// The call site (class name, method name and source information).
    /// </summary>
    [LayoutRenderer("callsite")]
    [ThreadAgnostic]
    [ThreadSafe]
    public class CallSiteLayoutRenderer : LayoutRenderer, IUsesStackTrace
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CallSiteLayoutRenderer" /> class.
        /// </summary>
        public CallSiteLayoutRenderer()
        {
            ClassName = true;
            MethodName = true;
            IncludeNamespace = true;
            FileName = false;
            IncludeSourcePath = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to render the class name.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(true)]
        public bool ClassName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to render the include the namespace with <see cref="ClassName"/>.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(true)]
        public bool IncludeNamespace { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to render the method name.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(true)]
        public bool MethodName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the method name will be cleaned up if it is detected as an anonymous delegate.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(true)]
        public bool CleanNamesOfAnonymousDelegates { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the method and class names will be cleaned up if it is detected as an async continuation
        /// (everything after an await-statement inside of an async method).
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(true)]
        public bool CleanNamesOfAsyncContinuations { get; set; } = true;

        /// <summary>
        /// Gets or sets the number of frames to skip.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(0)]
        public int SkipFrames { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to render the source file name and line number.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(false)]
        public bool FileName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include source file path.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(true)]
        public bool IncludeSourcePath { get; set; }

        /// <summary>
        /// Logger should capture StackTrace, if it was not provided manually
        /// </summary>
        [DefaultValue(true)]
        public bool CaptureStackTrace { get; set; } = true;

        /// <summary>
        /// Gets the level of stack trace information required by the implementing class.
        /// </summary>
        StackTraceUsage IUsesStackTrace.StackTraceUsage
        {
            get
            {
                return StackTraceUsageUtils.GetStackTraceUsage(
                    FileName,
                    SkipFrames,
                    CaptureStackTrace) | ((ClassName || IncludeNamespace) ? StackTraceUsage.WithCallSiteClassName : StackTraceUsage.None);
            }
        }

        /// <summary>
        /// Renders the call site and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (logEvent.CallSiteInformation != null)
            {
                if (ClassName || MethodName)
                {
                    var method = logEvent.CallSiteInformation.GetCallerStackFrameMethod(SkipFrames);
                    if (ClassName)
                    {
                        string className = logEvent.CallSiteInformation.GetCallerClassName(method, IncludeNamespace, CleanNamesOfAsyncContinuations, CleanNamesOfAnonymousDelegates);
                        if (string.IsNullOrEmpty(className))
                            className = "<no type>";
                        builder.Append(className);
                    }
                    if (MethodName)
                    {
                        string methodName = logEvent.CallSiteInformation.GetCallerMethodName(method, false, CleanNamesOfAsyncContinuations, CleanNamesOfAnonymousDelegates);
                        if (string.IsNullOrEmpty(methodName))
                            methodName = "<no method>";

                        if (ClassName)
                        {
                            builder.Append(".");
                        }
                        builder.Append(methodName);
                    }
                }

                if (FileName)
                {
                    string fileName = logEvent.CallSiteInformation.GetCallerFilePath(SkipFrames);
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        int lineNumber = logEvent.CallSiteInformation.GetCallerLineNumber(SkipFrames);
                        AppendFileName(builder, fileName, lineNumber);
                    }
                }
            }
        }

        private void AppendFileName(StringBuilder builder, string fileName, int lineNumber)
        {
            builder.Append("(");
            if (IncludeSourcePath)
            {
                builder.Append(fileName);
            }
            else
            {
                builder.Append(Path.GetFileName(fileName));
            }

            builder.Append(":");
            builder.AppendInvariant(lineNumber);
            builder.Append(")");
        }
    }
}
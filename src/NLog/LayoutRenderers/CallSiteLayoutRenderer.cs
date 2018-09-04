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

using System;

namespace NLog.LayoutRenderers
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using Config;
    using Internal;

    /// <summary>
    /// The call site (class name, method name and source information).
    /// </summary>
    [LayoutRenderer("callsite")]
    [ThreadAgnostic]
    public class CallSiteLayoutRenderer : LayoutRenderer, IUsesStackTrace
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CallSiteLayoutRenderer" /> class.
        /// </summary>
        public CallSiteLayoutRenderer()
        {
            ClassName = true;
            MethodName = true;
            CleanNamesOfAnonymousDelegates = false;
            IncludeNamespace = true;
#if !SILVERLIGHT
            FileName = false;
            IncludeSourcePath = true;
#endif
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
        [DefaultValue(false)]
        public bool CleanNamesOfAnonymousDelegates { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the method and class names will be cleaned up if it is detected as an async continuation
        /// (everything after an await-statement inside of an async method).
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(false)]
        public bool CleanNamesOfAsyncContinuations { get; set; }

        /// <summary>
        /// Gets or sets the number of frames to skip.
        /// </summary>
        [DefaultValue(0)]
        public int SkipFrames { get; set; }

#if !SILVERLIGHT
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
#endif

        /// <summary>
        /// Gets the level of stack trace information required by the implementing class.
        /// </summary>
        StackTraceUsage IUsesStackTrace.StackTraceUsage
        {
            get
            {
#if !SILVERLIGHT
                if (FileName)
                {
                    return StackTraceUsage.Max;
                }
#endif

                return StackTraceUsage.WithoutSource;
            }
        }

        /// <summary>
        /// Renders the call site and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            StackFrame frame = logEvent.StackTrace?.GetFrame(logEvent.UserStackFrameNumber + SkipFrames);
            if (frame != null)
            {
                MethodBase method = frame.GetMethod();
                if (ClassName)
                {
                    AppendClassName(builder, method);
                }

                if (MethodName)
                {
                    AppendMethodName(builder, method);
                }

#if !SILVERLIGHT
                if (FileName)
                {
                    AppendFileName(builder, frame);
                }
#endif
            }
        }

        private void AppendClassName(StringBuilder builder, MethodBase method)
        {
            var type = method.DeclaringType;
            if (type != null)
            {

                if (CleanNamesOfAsyncContinuations && method.Name == "MoveNext" && type.DeclaringType != null && type.Name.StartsWith("<"))
                {
                    // NLog.UnitTests.LayoutRenderers.CallSiteTests+<CleanNamesOfAsyncContinuations>d_3'1
                    int endIndex = type.Name.IndexOf('>', 1);
                    if (endIndex > 1)
                    {
                        type = type.DeclaringType;
                    }
                }
                string className = IncludeNamespace ? type.FullName : type.Name;

                if (CleanNamesOfAnonymousDelegates && className != null)
                {
                    // NLog.UnitTests.LayoutRenderers.CallSiteTests+<>c__DisplayClassa
                    int index = className.IndexOf("+<>", StringComparison.Ordinal);
                    if (index >= 0)
                    {
                        className = className.Substring(0, index);
                    }
                }

                builder.Append(className);
            }
            else
            {
                builder.Append("<no type>");
            }
        }

        private void AppendMethodName(StringBuilder builder, MethodBase method)
        {
            if (ClassName)
            {
                builder.Append(".");
            }

            if (method != null)
            {
                string methodName = method.Name;

                var type = method.DeclaringType;
                if (CleanNamesOfAsyncContinuations && method.Name == "MoveNext" && type?.DeclaringType != null && type.Name.StartsWith("<"))
                {
                    // NLog.UnitTests.LayoutRenderers.CallSiteTests+<CleanNamesOfAsyncContinuations>d_3'1.MoveNext
                    int endIndex = type.Name.IndexOf('>', 1);
                    if (endIndex > 1)
                    {
                        methodName = type.Name.Substring(1, endIndex - 1);
                    }
                }

                // Clean up the function name if it is an anonymous delegate
                // <.ctor>b__0
                // <Main>b__2
                if (CleanNamesOfAnonymousDelegates && (methodName.StartsWith("<") && methodName.Contains("__") && methodName.Contains(">")))
                {
                    int startIndex = methodName.IndexOf('<') + 1;
                    int endIndex = methodName.IndexOf('>');

                    methodName = methodName.Substring(startIndex, endIndex - startIndex);
                }

                builder.Append(methodName);
            }
            else
            {
                builder.Append("<no method>");
            }
        }

#if !SILVERLIGHT
        private void AppendFileName(StringBuilder builder, StackFrame frame)
        {
            string fileName = frame.GetFileName();
            if (fileName != null)
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
                builder.Append(frame.GetFileLineNumber());
                builder.Append(")");
            }
        }
#endif

    }
}
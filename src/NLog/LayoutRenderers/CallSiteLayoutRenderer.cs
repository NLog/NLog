// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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
    using System.IO;
    using System.Reflection;
    using System.Text;
    using NLog.Config;
    using NLog.Internal;

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
            this.ClassName = true;
            this.MethodName = true;
            this.CleanNamesOfAnonymousDelegates = false;
#if !SILVERLIGHT
            this.FileName = false;
            this.IncludeSourcePath = true;
#endif
        }

        /// <summary>
        /// Gets or sets a value indicating whether to render the class name.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(true)]
        public bool ClassName { get; set; }

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
                if (this.FileName)
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
            if (this.ClassName)
            {
                AppendFormatedCalssName(builder, logEvent);
            }
            if (this.MethodName)
            {
                AppendFormatedMethodName(builder, logEvent);
            }
#if !SILVERLIGHT
            if (this.FileName)
            {
                AppendFormatedFileName(builder, logEvent);
            }
#endif
        }


        private void AppendFormatedCalssName(StringBuilder builder, LogEventInfo logEvent)
        {
            string className = SkipFrames == 0 ?
                logEvent.CallerClassName : logEvent.CallerClassNameFromCallStack(SkipFrames);
            if (className == null)
            {
                className = "<no type>";
            }
            else if (this.CleanNamesOfAnonymousDelegates && className.Contains("+<>"))
            {
                // NLog.UnitTests.LayoutRenderers.CallSiteTests+<>c__DisplayClassa
                int index = className.IndexOf("+<>");
                className = className.Substring(0, index);
            }
            builder.Append(className);
        }

        private void AppendFormatedMethodName(StringBuilder builder, LogEventInfo logEvent)
        {
            if (this.ClassName) builder.Append(".");
            string methodName = SkipFrames == 0 ?
                logEvent.CallerMethodName : logEvent.CallerMethodNameFromCallStack(SkipFrames);
            if (methodName == null)
            {
                methodName = "<no method>";
            }
            else if (this.CleanNamesOfAnonymousDelegates &&
                (methodName.Contains("__") == true && methodName.StartsWith("<") == true && methodName.Contains(">") == true))
            {
                // Clean up the function name if it is an anonymous delegate
                // <.ctor>b__0
                // <Main>b__2
                int startIndex = methodName.IndexOf('<') + 1;
                int endIndex = methodName.IndexOf('>');
                methodName = methodName.Substring(startIndex, endIndex - startIndex);
            }
            builder.Append(methodName);
        }

        private void AppendFormatedFileName(StringBuilder builder, LogEventInfo logEvent)
        {
            string fileName = SkipFrames == 0 ?
                logEvent.CallerSourceFilePath : logEvent.CallerSourceFilePathFromCallStack(SkipFrames);
            int lineNumber = SkipFrames == 0 ?
                logEvent.CallerLineNumber : logEvent.CallerLineNumberFromCallStack(SkipFrames);
            if (fileName != null)
            {
                if (!this.IncludeSourcePath)
                {
                    fileName = Path.GetFileName(fileName);
                }
                builder.Append("(");
                builder.Append(fileName);
                builder.Append(":");
                builder.Append(lineNumber);
                builder.Append(")");
            }
        }
    }
}
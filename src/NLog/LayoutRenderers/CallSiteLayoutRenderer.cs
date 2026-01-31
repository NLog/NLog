//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/Callsite-Layout-Renderer">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/Callsite-Layout-Renderer">Documentation on NLog Wiki</seealso>
    [LayoutRenderer("callsite")]
    [ThreadAgnostic]
    public class CallSiteLayoutRenderer : LayoutRenderer, IUsesStackTrace
    {
        /// <summary>
        /// Gets or sets a value indicating whether to render the class name.
        /// </summary>
        /// <remarks>Default: <see langword="true"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        public bool ClassName { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to render the include the namespace with <see cref="ClassName"/>.
        /// </summary>
        /// <remarks>Default: <see langword="true"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        public bool IncludeNamespace { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to render the method name.
        /// </summary>
        /// <remarks>Default: <see langword="true"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        public bool MethodName { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the method name will be cleaned up if it is detected as an anonymous delegate.
        /// </summary>
        /// <remarks>Default: <see langword="true"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        [Obsolete("Should always be enabled. Marked obsolete with v6.1")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool CleanNamesOfAnonymousDelegates { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the method and class names will be cleaned up if it is detected as an async continuation
        /// (everything after an await-statement inside of an async method).
        /// </summary>
        /// <remarks>Default: <see langword="true"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        [Obsolete("Should always be enabled. Marked obsolete with v6.1")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool CleanNamesOfAsyncContinuations { get; set; } = true;

        /// <summary>
        /// Gets or sets the number of frames to skip.
        /// </summary>
        /// <remarks>Default: <see langword="0"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        public int SkipFrames { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to render the source file name and line number.
        /// </summary>
        /// <remarks>Default: <see langword="false"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        public bool FileName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include source file path.
        /// </summary>
        /// <remarks>Default: <see langword="true"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        public bool IncludeSourcePath { get; set; } = true;

        /// <summary>
        /// Logger should capture StackTrace, if it was not provided manually
        /// </summary>
        /// <remarks>Default: <see langword="true"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        public bool CaptureStackTrace { get; set; } = true;

        /// <inheritdoc/>
        StackTraceUsage IUsesStackTrace.StackTraceUsage
        {
            get
            {
                return StackTraceUsageUtils.GetStackTraceUsage(
                    FileName,
                    SkipFrames,
                    CaptureStackTrace) | (ClassName ? StackTraceUsage.WithCallSiteClassName : StackTraceUsage.None);
            }
        }

        /// <inheritdoc/>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            var logEventCallSize = logEvent.CallSiteInformation;
            if (logEventCallSize is null)
            {
                AppendExceptionCallSite(builder, logEvent);
                return;
            }

            if (ClassName || MethodName)
            {
                var method = logEventCallSize.GetCallerStackFrameMethod(SkipFrames);

                if (ClassName)
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    var className = logEventCallSize.GetCallerClassName(method, IncludeNamespace, CleanNamesOfAsyncContinuations, CleanNamesOfAnonymousDelegates);
#pragma warning restore CS0618 // Type or member is obsolete
                    builder.Append(string.IsNullOrEmpty(className) ? "<no type>" : className);
                }

                if (MethodName)
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    var methodName = logEventCallSize.GetCallerMethodName(method, false, CleanNamesOfAsyncContinuations, CleanNamesOfAnonymousDelegates);
#pragma warning restore CS0618 // Type or member is obsolete
                    if (ClassName)
                    {
                        builder.Append('.');
                    }
                    builder.Append(string.IsNullOrEmpty(methodName) ? "<no method>" : methodName);
                }
            }

            if (FileName)
            {
                string fileName = logEventCallSize.GetCallerFilePath(SkipFrames);
                if (!string.IsNullOrEmpty(fileName))
                {
                    int lineNumber = logEventCallSize.GetCallerLineNumber(SkipFrames);
                    AppendFileName(builder, fileName, lineNumber);
                }
            }
        }

        private void AppendFileName(StringBuilder builder, string fileName, int lineNumber)
        {
            builder.Append('(');
            if (IncludeSourcePath)
            {
                builder.Append(fileName);
            }
            else
            {
                builder.Append(Path.GetFileName(fileName));
            }

            builder.Append(':');
            builder.AppendInvariant(lineNumber);
            builder.Append(')');
        }

        [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming - Allow callsite logic", "IL2026")]
        private void AppendExceptionCallSite(StringBuilder builder, LogEventInfo logEvent)
        {
            var targetSite = logEvent?.Exception?.TargetSite;
            if (targetSite != null)
            {
                if (ClassName)
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    var className = StackTraceUsageUtils.GetStackFrameMethodClassName(targetSite, true, CleanNamesOfAsyncContinuations, CleanNamesOfAnonymousDelegates);
#pragma warning restore CS0618 // Type or member is obsolete
                    builder.Append(className);
                }

                if (MethodName)
                {
                    if (ClassName)
                    {
                        builder.Append('.');
                    }
#pragma warning disable CS0618 // Type or member is obsolete
                    var methodName = StackTraceUsageUtils.GetStackFrameMethodName(targetSite, false, CleanNamesOfAsyncContinuations, CleanNamesOfAnonymousDelegates);
#pragma warning restore CS0618 // Type or member is obsolete
                    builder.Append(methodName);
                }
            }
            else
            {
                if (ClassName || FileName)
                {
                    var exceptionSource = logEvent?.Exception?.Source;
                    builder.Append(exceptionSource);
                }
            }
        }
    }
}

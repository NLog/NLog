// 
// Copyright (c) 2004-2019 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.Internal
{
    using System;
    using System.Diagnostics;
    using System.Reflection;

    internal class CallSiteInformation
    {
        /// <summary>
        /// Sets the stack trace for the event info.
        /// </summary>
        /// <param name="stackTrace">The stack trace.</param>
        /// <param name="userStackFrame">Index of the first user stack frame within the stack trace.</param>
        /// <param name="userStackFrameLegacy">Index of the first user stack frame within the stack trace.</param>
        public void SetStackTrace(StackTrace stackTrace, int userStackFrame, int? userStackFrameLegacy)
        {
            StackTrace = stackTrace;
            UserStackFrameNumber = userStackFrame;
            UserStackFrameNumberLegacy = userStackFrameLegacy != userStackFrame ? userStackFrameLegacy : null;
        }

        /// <summary>
        /// Sets the details retrieved from the Caller Information Attributes
        /// </summary>
        /// <param name="callerClassName"></param>
        /// <param name="callerMemberName"></param>
        /// <param name="callerFilePath"></param>
        /// <param name="callerLineNumber"></param>
        public void SetCallerInfo(string callerClassName, string callerMemberName, string callerFilePath, int callerLineNumber)
        {
            CallerClassName = callerClassName;
            CallerMemberName = callerMemberName;
            CallerFilePath = callerFilePath;
            CallerLineNumber = callerLineNumber;
        }

        /// <summary>
        /// Gets the stack frame of the method that did the logging.
        /// </summary>
        public StackFrame UserStackFrame => StackTrace?.GetFrame(UserStackFrameNumberLegacy ?? UserStackFrameNumber);

        /// <summary>
        /// Gets the number index of the stack frame that represents the user
        /// code (not the NLog code).
        /// </summary>
        public int UserStackFrameNumber { get; private set; }

        /// <summary>
        /// Legacy attempt to skip async MoveNext, but caused source file line number to be lost
        /// </summary>
        public int? UserStackFrameNumberLegacy { get; private set; }

        /// <summary>
        /// Gets the entire stack trace.
        /// </summary>
        public StackTrace StackTrace { get; private set; }

        public MethodBase GetCallerStackFrameMethod(int skipFrames)
        {
            StackFrame frame = StackTrace?.GetFrame(UserStackFrameNumber + skipFrames);
            return frame?.GetMethod();
        }

        public string GetCallerClassName(MethodBase method, bool includeNameSpace, bool cleanAsyncMoveNext, bool cleanAnonymousDelegates)
        {
            if (!string.IsNullOrEmpty(CallerClassName))
            {
                if (includeNameSpace)
                {
                    return CallerClassName;
                }
                else
                {
                    int lastDot = CallerClassName.LastIndexOf('.');
                    if (lastDot < 0 || lastDot >= CallerClassName.Length - 1)
                        return CallerClassName;
                    else
                        return CallerClassName.Substring(lastDot + 1);
                }
            }

            method = method ?? GetCallerStackFrameMethod(0);
            if (method == null)
                return string.Empty;

            cleanAsyncMoveNext = cleanAsyncMoveNext || UserStackFrameNumberLegacy.HasValue;
            cleanAnonymousDelegates = cleanAnonymousDelegates || UserStackFrameNumberLegacy.HasValue;
            return StackTraceUsageUtils.GetStackFrameMethodClassName(method, includeNameSpace, cleanAsyncMoveNext, cleanAnonymousDelegates) ?? string.Empty;
        }

        public string GetCallerMemberName(MethodBase method, bool includeMethodInfo, bool cleanAsyncMoveNext, bool cleanAnonymousDelegates)
        {
            if (!string.IsNullOrEmpty(CallerMemberName))
                return CallerMemberName;

            method = method ?? GetCallerStackFrameMethod(0);
            if (method == null)
                return string.Empty;

            cleanAsyncMoveNext = cleanAsyncMoveNext || UserStackFrameNumberLegacy.HasValue;
            cleanAnonymousDelegates = cleanAnonymousDelegates || UserStackFrameNumberLegacy.HasValue;
            return StackTraceUsageUtils.GetStackFrameMethodName(method, includeMethodInfo, cleanAsyncMoveNext, cleanAnonymousDelegates) ?? string.Empty;
        }

        public string GetCallerFilePath(int skipFrames)
        {
            if (!string.IsNullOrEmpty(CallerFilePath))
                return CallerFilePath;

#if !SILVERLIGHT
            StackFrame frame = StackTrace?.GetFrame(UserStackFrameNumber + skipFrames);
            return frame?.GetFileName() ?? string.Empty;
#else
            return string.Empty;
#endif
        }

        public int GetCallerLineNumber(int skipFrames)
        {
            if (CallerLineNumber.HasValue)
                return CallerLineNumber.Value;

            StackFrame frame = StackTrace?.GetFrame(UserStackFrameNumber + skipFrames);
            return frame?.GetFileLineNumber() ?? 0;
        }

        public string CallerClassName { get; private set; }
        public string CallerMemberName { get; private set; }
        public string CallerFilePath { get; private set; }
        public int? CallerLineNumber { get; private set; }
    }
}

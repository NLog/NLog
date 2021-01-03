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
        /// <param name="loggerType">Type of the logger or logger wrapper. This is still Logger if it's a subclass of Logger.</param>
        public void SetStackTrace(StackTrace stackTrace, int? userStackFrame = null, Type loggerType = null)
        {
            StackTrace = stackTrace;
            if (!userStackFrame.HasValue && stackTrace != null)
            {
                var stackFrames = stackTrace.GetFrames();
                var firstUserFrame = loggerType != null ? FindCallingMethodOnStackTrace(stackFrames, loggerType) : 0;
                var firstLegacyUserFrame = firstUserFrame.HasValue ? SkipToUserStackFrameLegacy(stackFrames, firstUserFrame.Value) : firstUserFrame;
                UserStackFrameNumber = firstUserFrame ?? 0;
                UserStackFrameNumberLegacy = firstLegacyUserFrame != firstUserFrame ? firstLegacyUserFrame : null;
            }
            else
            {
                UserStackFrameNumber = userStackFrame ?? 0;
                UserStackFrameNumberLegacy = null;
            }
        }

        /// <summary>
        /// Sets the details retrieved from the Caller Information Attributes
        /// </summary>
        /// <param name="callerClassName"></param>
        /// <param name="callerMethodName"></param>
        /// <param name="callerFilePath"></param>
        /// <param name="callerLineNumber"></param>
        public void SetCallerInfo(string callerClassName, string callerMethodName, string callerFilePath, int callerLineNumber)
        {
            CallerClassName = callerClassName;
            CallerMethodName = callerMethodName;
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

        public string GetCallerMethodName(MethodBase method, bool includeMethodInfo, bool cleanAsyncMoveNext, bool cleanAnonymousDelegates)
        {
            if (!string.IsNullOrEmpty(CallerMethodName))
                return CallerMethodName;

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

            StackFrame frame = StackTrace?.GetFrame(UserStackFrameNumber + skipFrames);
            return frame?.GetFileName() ?? string.Empty;
        }

        public int GetCallerLineNumber(int skipFrames)
        {
            if (CallerLineNumber.HasValue)
                return CallerLineNumber.Value;

            StackFrame frame = StackTrace?.GetFrame(UserStackFrameNumber + skipFrames);
            return frame?.GetFileLineNumber() ?? 0;
        }

        public string CallerClassName { get; internal set; }
        public string CallerMethodName { get; private set; }
        public string CallerFilePath { get; private set; }
        public int? CallerLineNumber { get; private set; }

        /// <summary>
        ///  Finds first user stack frame in a stack trace
        /// </summary>
        /// <param name="stackFrames">The stack trace of the logging method invocation</param>
        /// <param name="loggerType">Type of the logger or logger wrapper. This is still Logger if it's a subclass of Logger.</param>
        /// <returns>Index of the first user stack frame or 0 if all stack frames are non-user</returns>
        private static int? FindCallingMethodOnStackTrace(StackFrame[] stackFrames, Type loggerType)
        {
            if (stackFrames == null || stackFrames.Length == 0)
                return null;

            int? firstStackFrameAfterLogger = null;
            int? firstUserStackFrame = null;
            for (int i = 0; i < stackFrames.Length; ++i)
            {
                var stackFrame = stackFrames[i];
                if (SkipAssembly(stackFrame))
                    continue;

                if (!firstUserStackFrame.HasValue)
                    firstUserStackFrame = i;

                if (IsLoggerType(stackFrame, loggerType))
                {
                    firstStackFrameAfterLogger = null;
                    continue;
                }

                if (!firstStackFrameAfterLogger.HasValue)
                    firstStackFrameAfterLogger = i;
            }

            return firstStackFrameAfterLogger ?? firstUserStackFrame;
        }

        /// <summary>
        /// This is only done for legacy reason, as the correct method-name and line-number should be extracted from the MoveNext-StackFrame
        /// </summary>
        /// <param name="stackFrames">The stack trace of the logging method invocation</param>
        /// <param name="firstUserStackFrame">Starting point for skipping async MoveNext-frames</param>
        private static int SkipToUserStackFrameLegacy(StackFrame[] stackFrames, int firstUserStackFrame)
        {
#if !NET35 && !NET40
            for (int i = firstUserStackFrame; i < stackFrames.Length; ++i)
            {
                var stackFrame = stackFrames[i];
                if (SkipAssembly(stackFrame))
                    continue;

                if (stackFrame.GetMethod()?.Name == "MoveNext" && stackFrames.Length > i)
                {
                    var nextStackFrame = stackFrames[i + 1];
                    var declaringType = nextStackFrame.GetMethod()?.DeclaringType;
                    if (declaringType?.Namespace == "System.Runtime.CompilerServices" || declaringType == typeof(System.Threading.ExecutionContext))
                    {
                        //async, search further
                        continue;
                    }
                }

                return i;
            }
#endif
            return firstUserStackFrame;
        }

        /// <summary>
        /// Assembly to skip?
        /// </summary>
        /// <param name="frame">Find assembly via this frame. </param>
        /// <returns><c>true</c>, we should skip.</returns>
        private static bool SkipAssembly(StackFrame frame)
        {
            var assembly = StackTraceUsageUtils.LookupAssemblyFromStackFrame(frame);
            return assembly == null || LogManager.IsHiddenAssembly(assembly);
        }

        /// <summary>
        /// Is this the type of the logger?
        /// </summary>
        /// <param name="frame">get type of this logger in this frame.</param>
        /// <param name="loggerType">Type of the logger.</param>
        /// <returns></returns>
        private static bool IsLoggerType(StackFrame frame, Type loggerType)
        {
            var method = frame.GetMethod();
            Type declaringType = method?.DeclaringType;
            var isLoggerType = declaringType != null && (loggerType == declaringType || declaringType.IsSubclassOf(loggerType) || loggerType.IsAssignableFrom(declaringType));
            return isLoggerType;
        }
    }
}

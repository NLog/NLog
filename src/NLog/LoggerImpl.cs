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

#if !NETSTANDARD1_0 || NETSTANDARD1_5
#define CaptureCallSiteInfo
#endif

namespace NLog
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using JetBrains.Annotations;
    using NLog.Common;
    using NLog.Config;
    using NLog.Filters;
    using NLog.Internal;

    /// <summary>
    /// Implementation of logging engine.
    /// </summary>
    internal static class LoggerImpl
    {
        private const int StackTraceSkipMethods = 0;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", Justification = "Using 'NLog' in message.")]
        internal static void Write([NotNull] Type loggerType, [NotNull] TargetWithFilterChain targetsForLevel, LogEventInfo logEvent, LogFactory logFactory)
        {
            logEvent.SetMessageFormatter(logFactory.ActiveMessageFormatter, targetsForLevel.NextInChain == null ? logFactory.SingleTargetMessageFormatter : null);

#if CaptureCallSiteInfo
            StackTraceUsage stu = targetsForLevel.GetStackTraceUsage();
            if (stu != StackTraceUsage.None)
            {
                bool attemptCallSiteOptimization = targetsForLevel.TryCallSiteClassNameOptimization(stu, logEvent);
                if (attemptCallSiteOptimization && targetsForLevel.TryLookupCallSiteClassName(logEvent, out string callSiteClassName))
                {
                    logEvent.CallSiteInformation.CallerClassName = callSiteClassName;
                }
                else if (attemptCallSiteOptimization || targetsForLevel.MustCaptureStackTrace(stu, logEvent))
                {
                    CaptureCallSiteInfo(logFactory, loggerType, logEvent, stu);

                    if (attemptCallSiteOptimization)
                    {
                        targetsForLevel.TryRememberCallSiteClassName(logEvent);
                    }
                }
            }
#endif

            AsyncContinuation exceptionHandler = (ex) => { };
            if (logFactory.ThrowExceptions)
            {
                int originalThreadId = AsyncHelpers.GetManagedThreadId();
                exceptionHandler = ex =>
                {
                    if (ex != null && AsyncHelpers.GetManagedThreadId() == originalThreadId)
                    {
                        throw new NLogRuntimeException("Exception occurred in NLog", ex);
                    }
                };
            }

            IList<Filter> prevFilterChain = null;
            FilterResult prevFilterResult = FilterResult.Neutral;
            for (var t = targetsForLevel; t != null; t = t.NextInChain)
            {
                FilterResult result = ReferenceEquals(prevFilterChain, t.FilterChain) ?
                    prevFilterResult : GetFilterResult(t.FilterChain, logEvent, t.DefaultResult);
                if (!WriteToTargetWithFilterChain(t.Target, result, logEvent, exceptionHandler))
                {
                    break;
                }

                prevFilterResult = result;  // Cache the result, and reuse it for the next target, if it comes from the same logging-rule
                prevFilterChain = t.FilterChain;
            }
        }

#if CaptureCallSiteInfo
        private static void CaptureCallSiteInfo(LogFactory logFactory, Type loggerType, LogEventInfo logEvent, StackTraceUsage stackTraceUsage)
        {
            try
            {
#if SILVERLIGHT
                var stackTrace = new StackTrace();
#else
                bool includeSource = (stackTraceUsage & StackTraceUsage.WithFileNameAndLineNumber) != 0;
#if NETSTANDARD1_5
                var stackTrace = (StackTrace)Activator.CreateInstance(typeof(StackTrace), new object[] { includeSource });
#else
                var stackTrace = new StackTrace(StackTraceSkipMethods, includeSource);
#endif
#endif
                var stackFrames = stackTrace.GetFrames();
                int? firstUserFrame = FindCallingMethodOnStackTrace(stackFrames, loggerType);
                int? firstLegacyUserFrame = firstUserFrame.HasValue ? SkipToUserStackFrameLegacy(stackFrames, firstUserFrame.Value) : (int?)null;
                logEvent.GetCallSiteInformationInternal().SetStackTrace(stackTrace, firstUserFrame ?? 0, firstLegacyUserFrame);
            }
            catch (Exception ex)
            {
                if (logFactory.ThrowExceptions || ex.MustBeRethrownImmediately())
                    throw;

                InternalLogger.Error(ex, "Failed to capture CallSite for Logger {0}. Platform might not support ${{callsite}}", logEvent.LoggerName);
            }
        }
#endif

        /// <summary>
        ///  Finds first user stack frame in a stack trace
        /// </summary>
        /// <param name="stackFrames">The stack trace of the logging method invocation</param>
        /// <param name="loggerType">Type of the logger or logger wrapper. This is still Logger if it's a subclass of Logger.</param>
        /// <returns>Index of the first user stack frame or 0 if all stack frames are non-user</returns>
        internal static int? FindCallingMethodOnStackTrace(StackFrame[] stackFrames, [NotNull] Type loggerType)
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
        internal static int SkipToUserStackFrameLegacy(StackFrame[] stackFrames, int firstUserStackFrame)
        {
#if NET4_5
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

        private static bool WriteToTargetWithFilterChain(Targets.Target target, FilterResult result, LogEventInfo logEvent, AsyncContinuation onException)
        {
            if ((result == FilterResult.Ignore) || (result == FilterResult.IgnoreFinal))
            {
                if (InternalLogger.IsDebugEnabled)
                {
                    InternalLogger.Debug("{0}.{1} Rejecting message because of a filter.", logEvent.LoggerName, logEvent.Level);
                }

                if (result == FilterResult.IgnoreFinal)
                {
                    return false;
                }

                return true;
            }

            target.WriteAsyncLogEvent(logEvent.WithContinuation(onException));
            if (result == FilterResult.LogFinal)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the filter result.
        /// </summary>
        /// <param name="filterChain">The filter chain.</param>
        /// <param name="logEvent">The log event.</param>
        /// <param name="defaultFilterResult">default result if there are no filters, or none of the filters decides.</param>
        /// <returns>The result of the filter.</returns>
        private static FilterResult GetFilterResult(IList<Filter> filterChain, LogEventInfo logEvent, FilterResult defaultFilterResult)
        {
            if (filterChain == null || filterChain.Count == 0) 
                return FilterResult.Neutral;

            try
            {
                //Memory profiling pointed out that using a foreach-loop was allocating
                //an Enumerator. Switching to a for-loop avoids the memory allocation.
                for (int i = 0; i < filterChain.Count; i++)
                {
                    Filter f = filterChain[i];
                    var result = f.GetFilterResult(logEvent);
                    if (result != FilterResult.Neutral)
                    {
                        return result;
                    }
                }

                return defaultFilterResult;
            }
            catch (Exception exception)
            {
                InternalLogger.Warn(exception, "Exception during filter evaluation. Message will be ignore.");

                if (exception.MustBeRethrown())
                {
                    throw;
                }

                return FilterResult.Ignore;
            }
        }
    }
}

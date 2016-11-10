// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

using JetBrains.Annotations;

namespace NLog
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using JetBrains.Annotations;
    using NLog.Common;
    using NLog.Config;
    using NLog.Filters;
    using NLog.Internal;
    using NLog.Targets;

    /// <summary>
    /// Implementation of logging engine.
    /// </summary>
    internal static class LoggerImpl
    {
        private const int StackTraceSkipMethods = 0;
        private static readonly Assembly nlogAssembly = typeof(LoggerImpl).Assembly;
        private static readonly Assembly mscorlibAssembly = typeof(string).Assembly;
        private static readonly Assembly systemAssembly = typeof(Debug).Assembly;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", Justification = "Using 'NLog' in message.")]
        internal static void Write([NotNull] Type loggerType, TargetWithFilterChain targets, LogEventInfo logEvent, LogFactory factory)
        {
            if (targets == null)
            {
                return;
            }

            StackTraceUsage stu = targets.GetStackTraceUsage();

            if (stu != StackTraceUsage.None && !logEvent.HasStackTrace)
            {
                StackTrace stackTrace;
#if !SILVERLIGHT
                stackTrace = new StackTrace(StackTraceSkipMethods, stu == StackTraceUsage.WithSource);
#else
                stackTrace = new StackTrace();
#endif

                int firstUserFrame = FindCallingMethodOnStackTrace(stackTrace, loggerType);

                logEvent.SetStackTrace(stackTrace, firstUserFrame);
            }

            int originalThreadId = Thread.CurrentThread.ManagedThreadId;
            AsyncContinuation exceptionHandler = ex =>
                {
                    if (ex != null)
                    {
                        if (factory.ThrowExceptions && Thread.CurrentThread.ManagedThreadId == originalThreadId)
                        {
                            throw new NLogRuntimeException("Exception occurred in NLog", ex);
                        }
                    }
                };

            for (var t = targets; t != null; t = t.NextInChain)
            {
                if (!WriteToTargetWithFilterChain(t, logEvent, exceptionHandler))
                {
                    break;
                }
            }
        }

        /// <summary>
        ///  Finds first user stack frame in a stack trace
        /// </summary>
        /// <param name="stackTrace">The stack trace of the logging method invocation</param>
        /// <param name="loggerType">Type of the logger or logger wrapper. This is still Logger if it's a subclass of Logger.</param>
        /// <returns>Index of the first user stack frame or 0 if all stack frames are non-user</returns>
        internal static int FindCallingMethodOnStackTrace([NotNull] StackTrace stackTrace, [NotNull] Type loggerType)
        {
            var stackFrames = stackTrace.GetFrames();
            if (stackFrames == null)
                return 0;

            //create StackFrameWithIndex so the index is know after filtering
            var allStackFrames = stackFrames.Select((f, i) => new StackFrameWithIndex(i, f)).ToList();
            //filter on assemblies
            var filteredStackframes = allStackFrames.Where(p => !SkipAssembly(p.StackFrame)).ToList();
            //find until logger type
            var intermediate = filteredStackframes.SkipWhile(p => !IsLoggerType(p.StackFrame, loggerType));
            //skip the logger type
            var stackframesAfterLogger = intermediate.SkipWhile(p => IsLoggerType(p.StackFrame, loggerType)).ToList();

            //get first call after logger (or skip if is moveNext)
            var candidateStackFrames = stackframesAfterLogger;
            if (!candidateStackFrames.Any())
            {
                //If some calls got inlined, we can't find LoggerType on the stack. Fallback to the filteredStackframes
                candidateStackFrames = filteredStackframes;
            }

            return FindIndexOfCallingMethod(allStackFrames, candidateStackFrames);
        }

        /// <summary>
        /// Get the index which correspondens to the calling method.
        /// 
        /// This is most of the time the first index after <paramref name="candidateStackFrames"/>.
        /// </summary>
        /// <param name="allStackFrames">all the frames of the stacktrace</param>
        /// <param name="candidateStackFrames">frames which all hiddenAssemblies are removed</param>
        /// <returns>index on stacktrace</returns>
        private static int FindIndexOfCallingMethod(List<StackFrameWithIndex> allStackFrames, List<StackFrameWithIndex> candidateStackFrames)
        {
            var stackFrameWithIndex = candidateStackFrames.FirstOrDefault();
            var last = stackFrameWithIndex;

            if (last != null)
            {
#if ASYNC_SUPPORTED

                //movenext and then AsyncTaskMethodBuilder (method start)? this is a generated MoveNext by async.
                if (last.StackFrame.GetMethod().Name == "MoveNext")
                {

                    if (allStackFrames.Count > last.StackFrameIndex)
                    {
                        var next = allStackFrames[last.StackFrameIndex + 1];
                        var declaringType = next.StackFrame.GetMethod().DeclaringType;
                        if (declaringType == typeof(System.Runtime.CompilerServices.AsyncTaskMethodBuilder) || 
                            declaringType == typeof(System.Runtime.CompilerServices.AsyncTaskMethodBuilder<>))
                        {
                            //async, search futher
                            candidateStackFrames = candidateStackFrames.Skip(1).ToList();
                            return FindIndexOfCallingMethod(allStackFrames, candidateStackFrames);
                        }
                    }
                }
#endif

                return last.StackFrameIndex;
            }
            return 0;
        }

        /// <summary>
        /// Assembly to skip?
        /// </summary>
        /// <param name="frame">Find assembly via this frame. </param>
        /// <returns><c>true</c>, we should skip.</returns>
        private static bool SkipAssembly(StackFrame frame)
        {
            var method = frame.GetMethod();
            var assembly = method.DeclaringType != null ? method.DeclaringType.Assembly : method.Module.Assembly;
            // skip stack frame if the method declaring type assembly is from hidden assemblies list
            var skipAssembly = SkipAssembly(assembly);
            return skipAssembly;
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
            Type declaringType = method.DeclaringType;
            var isLoggerType = declaringType != null && loggerType == declaringType;
            return isLoggerType;
        }

        private static bool SkipAssembly(Assembly assembly)
        {
            if (assembly == nlogAssembly)
            {
                return true;
            }

            if (assembly == mscorlibAssembly)
            {
                return true;
            }

            if (assembly == systemAssembly)
            {
                return true;
            }

            if (LogManager.IsHiddenAssembly(assembly))
            {
                return true;
            }

            return false;
        }

        private static bool WriteToTargetWithFilterChain(TargetWithFilterChain targetListHead, LogEventInfo logEvent, AsyncContinuation onException)
        {
            Target target = targetListHead.Target;
            FilterResult result = GetFilterResult(targetListHead.FilterChain, logEvent);

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
        /// <returns>The result of the filter.</returns>
        private static FilterResult GetFilterResult(IList<Filter> filterChain, LogEventInfo logEvent)
        {
            FilterResult result = FilterResult.Neutral;

            if (filterChain == null || filterChain.Count == 0)
                return result;

            try
            {
                //Memory profiling pointed out that using a foreach-loop was allocating
                //an Enumerator. Switching to a for-loop avoids the memory allocation.
                for (int i = 0; i < filterChain.Count; i++)
                {
                    Filter f = filterChain[i];
                    result = f.GetFilterResult(logEvent);
                    if (result != FilterResult.Neutral)
                    {
                        break;
                    }
                }

                return result;
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

        /// <summary>
        /// Stackframe with correspending index on the stracktrace
        /// </summary>
        private class StackFrameWithIndex
        {
            /// <summary>
            /// Index of <see cref="StackFrame"/> on the stack.
            /// </summary>
            public int StackFrameIndex { get; private set; }

            /// <summary>
            /// A stackframe
            /// </summary>
            public StackFrame StackFrame { get; private set; }

            /// <summary>
            /// New item
            /// </summary>
            /// <param name="stackFrameIndex">Index of <paramref name="stackFrame"/> on the stack.</param>
            /// <param name="stackFrame">A stackframe</param>
            public StackFrameWithIndex(int stackFrameIndex, StackFrame stackFrame)
            {
                StackFrameIndex = stackFrameIndex;
                StackFrame = stackFrame;
            }
        }
    }
}

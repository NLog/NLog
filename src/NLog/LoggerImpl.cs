// 
// Copyright (c) 2004-2010 Jaroslaw Kowalski <jaak@jkowalski.net>
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

namespace NLog
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading;
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
        private static Assembly nlogAssembly = typeof(LoggerImpl).Assembly;

        internal static void Write(Type loggerType, TargetWithFilterChain targets, LogEventInfo logEvent, LogFactory factory)
        {
            if (targets == null)
            {
                return;
            }

#if !NET_CF
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
#endif

            int originalThreadId = Thread.CurrentThread.ManagedThreadId;

            WriteToTargetWithFilterChain(targets, logEvent, ex =>
                {
                    if (factory.ThrowExceptions && Thread.CurrentThread.ManagedThreadId == originalThreadId)
                    {
                        throw new NLogRuntimeException("Exception occured in NLog", ex);
                    }
                });
        }

#if !NET_CF
        private static int FindCallingMethodOnStackTrace(StackTrace stackTrace, Type loggerType)
        {
            int firstUserFrame = 0;
            for (int i = 0; i < stackTrace.FrameCount; ++i)
            {
                var frame = stackTrace.GetFrame(i);
                MethodBase mb = frame.GetMethod();
                Assembly methodAssembly = null;

                if (mb.DeclaringType != null)
                {
                    methodAssembly = mb.DeclaringType.Assembly;
                }

                if (methodAssembly == nlogAssembly || mb.DeclaringType == loggerType)
                {
                    firstUserFrame = i + 1;
                }
                else
                {
                    if (firstUserFrame != 0)
                    {
                        break;
                    }
                }
            }

            return firstUserFrame;
        }
#endif

        private static void WriteToTargetWithFilterChain(TargetWithFilterChain targetListHead, LogEventInfo logEvent, AsyncContinuation onException)
        {
            if (targetListHead == null)
            {
                return;
            }

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
                    return;
                }

                // move to next target
                WriteToTargetWithFilterChain(targetListHead.NextInChain, logEvent, onException);
                return;
            }

            target.WriteLogEvent(logEvent,
                AsyncHelpers.OneTimeOnly(
                ex =>
                    {
                        if (ex == null)
                        {
                            // success
                            if (result == FilterResult.LogFinal)
                            {
                                return;
                            }
                        }
                        else
                        {
                            // intentionally not returning here
                            // onException will throw or not, depending on ThrowExceptions setting
                            // and/or whether we are still on the original thread
                            // if it does not throw, we just proceed to the next target
                            InternalLogger.Error("Target exception: {0}", ex);
                            onException(ex);
                        }

                        // write to the next target
                        WriteToTargetWithFilterChain(targetListHead.NextInChain, logEvent, onException);
                    }));
        }

        /// <summary>
        /// Gets the filter result.
        /// </summary>
        /// <param name="filterChain">The filter chain.</param>
        /// <param name="logEvent">The log event.</param>
        /// <returns></returns>
        private static FilterResult GetFilterResult(ICollection<Filter> filterChain, LogEventInfo logEvent)
        {
            var result = FilterResult.Neutral;

            try
            {
                foreach (Filter f in filterChain)
                {
                    result = f.GetFilterResult(logEvent);
                    if (result != FilterResult.Neutral)
                    {
                        break;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                InternalLogger.Warn("Exception during filter evaluation: {0}", ex);
                return FilterResult.Ignore;
            }
        }
    }
}

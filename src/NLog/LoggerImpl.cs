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

namespace NLog
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
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

        internal static void Write(Type loggerType, TargetWithFilterChain targetsForLevel, LogEventInfo logEvent, LogFactory logFactory)
        {
            logEvent.SetMessageFormatter(logFactory.ActiveMessageFormatter, targetsForLevel.NextInChain is null ? logFactory.SingleTargetMessageFormatter : null);

            if (targetsForLevel.StackTraceUsage != StackTraceUsage.None)
            {
                CaptureCallSiteInfo(loggerType, targetsForLevel, logEvent, logFactory);
            }

            AsyncContinuation exceptionHandler = SingleCallContinuation.Completed;
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

            IList<Filter> prevFilterChain = ArrayHelper.Empty<Filter>();
            FilterResult prevFilterResult = FilterResult.Neutral;
            for (var t = targetsForLevel; t != null; t = t.NextInChain)
            {
                var currentFilterChain = t.FilterChain;
                FilterResult result = ReferenceEquals(prevFilterChain, currentFilterChain) ?
                    prevFilterResult : GetFilterResult(currentFilterChain, logEvent, t.FilterDefaultAction);

                if (result != FilterResult.Ignore && result != FilterResult.IgnoreFinal)
                {
                    t.Target.WriteAsyncLogEvent(logEvent.WithContinuation(exceptionHandler));
                    if (result == FilterResult.LogFinal)
                        break;
                }
                else
                {
                    InternalLogger.Debug("{0} [{1}] Rejecting message because of a filter.", logEvent.LoggerName, logEvent.Level);
                    if (result == FilterResult.IgnoreFinal)
                        break;
                }

                prevFilterResult = result;  // Cache the result, and reuse it for the next target, if it comes from the same logging-rule
                prevFilterChain = currentFilterChain;
            }
        }

        private static void CaptureCallSiteInfo(Type loggerType, TargetWithFilterChain targetsForLevel, LogEventInfo logEvent, LogFactory logFactory)
        {
            var stu = targetsForLevel.StackTraceUsage;
            bool attemptCallSiteOptimization = TryCallSiteClassNameOptimization(stu, logEvent);
            if (attemptCallSiteOptimization && targetsForLevel.TryLookupCallSiteClassName(logEvent, out var callSiteClassName))
            {
                logEvent.GetCallSiteInformationInternal().CallerClassName = callSiteClassName;
            }
            else if (attemptCallSiteOptimization || MustCaptureStackTrace(stu, logEvent))
            {
                try
                {
                    bool includeSource = (stu & StackTraceUsage.WithFileNameAndLineNumber) != 0;
                    var stackTrace = new StackTrace(StackTraceSkipMethods, includeSource);
                    logEvent.GetCallSiteInformationInternal().SetStackTrace(stackTrace, null, loggerType);
                }
                catch (Exception ex)
                {
#if DEBUG
                    if (ex.MustBeRethrownImmediately())
                        throw;
#endif

                    if (logFactory.ThrowExceptions || LogManager.ThrowExceptions)
                        throw;

                    InternalLogger.Warn(ex, "{0} Failed to capture CallSite. Platform might not support ${{callsite}}", logEvent.LoggerName);
                }

                if (attemptCallSiteOptimization)
                {
                    targetsForLevel.TryRememberCallSiteClassName(logEvent);
                }
            }
        }

        internal static bool TryCallSiteClassNameOptimization(StackTraceUsage stackTraceUsage, LogEventInfo logEvent)
        {
            if ((stackTraceUsage & (StackTraceUsage.WithCallSiteClassName | StackTraceUsage.WithStackTrace)) != StackTraceUsage.WithCallSiteClassName)
                return false;

            if (string.IsNullOrEmpty(logEvent.CallSiteInformation?.CallerFilePath))
                return false;

            if (logEvent.HasStackTrace)
                return false;

            return true;
        }

        internal static bool MustCaptureStackTrace(StackTraceUsage stackTraceUsage, LogEventInfo logEvent)
        {
            if (logEvent.HasStackTrace)
                return false;

            if ((stackTraceUsage & StackTraceUsage.WithStackTrace) != StackTraceUsage.None)
                return true;

            if ((stackTraceUsage & StackTraceUsage.WithCallSite) != StackTraceUsage.None && string.IsNullOrEmpty(logEvent.CallSiteInformation?.CallerMethodName) && string.IsNullOrEmpty(logEvent.CallSiteInformation?.CallerFilePath))
                return true;    // We don't have enough CallSiteInformation

            return false;
        }

        /// <summary>
        /// Gets the filter result.
        /// </summary>
        /// <param name="filterChain">The filter chain.</param>
        /// <param name="logEvent">The log event.</param>
        /// <param name="filterDefaultAction">default result if there are no filters, or none of the filters decides.</param>
        /// <returns>The result of the filter.</returns>
        private static FilterResult GetFilterResult(IList<Filter> filterChain, LogEventInfo logEvent, FilterResult filterDefaultAction)
        {
            if (filterChain.Count == 0)
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

                return filterDefaultAction;
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

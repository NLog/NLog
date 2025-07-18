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

namespace NLog.Internal
{
    using System;
    using System.Collections.Generic;
    using NLog.Common;
    using NLog.Config;
    using NLog.Filters;
    using NLog.Targets;

    /// <summary>
    /// Represents target with a chain of filters which determine
    /// whether logging should happen.
    /// </summary>
    internal sealed class TargetWithFilterChain : ITargetWithFilterChain
    {
        internal static readonly TargetWithFilterChain[] NoTargetsByLevel = CreateLoggerConfiguration();

        private static TargetWithFilterChain[] CreateLoggerConfiguration() => new TargetWithFilterChain[LogLevel.MaxLevel.Ordinal + 2];    // +2 to include LogLevel.Off

        private MruCache<CallSiteKey, string>? _callSiteClassNameCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetWithFilterChain" /> class.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="filterChain">The filter chain.</param>
        /// <param name="filterDefaultAction">Default action if none of the filters match.</param>
        public TargetWithFilterChain(Target target, IList<Filter> filterChain, FilterResult filterDefaultAction)
        {
            Target = target;
            FilterChain = filterChain;
            FilterDefaultAction = filterDefaultAction;
        }

        /// <summary>
        /// Gets the target.
        /// </summary>
        /// <value>The target.</value>
        public Target Target { get; }

        /// <summary>
        /// Gets the filter chain.
        /// </summary>
        /// <value>The filter chain.</value>
        public IList<Filter> FilterChain { get; }

        /// <summary>
        /// Gets or sets the next <see cref="TargetWithFilterChain"/> item in the chain.
        /// </summary>
        /// <value>The next item in the chain.</value>
        /// <example>This is for example the 'target2' logger in writeTo='target1,target2'  </example>
        public TargetWithFilterChain? NextInChain { get; set; }

        /// <summary>
        /// Gets the stack trace usage.
        /// </summary>
        /// <returns>A <see cref="StackTraceUsage" /> value that determines stack trace handling.</returns>
        public StackTraceUsage StackTraceUsage { get; private set; }

        /// <summary>
        /// Default action if none of the filters match.
        /// </summary>
        public FilterResult FilterDefaultAction { get; }

        internal StackTraceUsage PrecalculateStackTraceUsage()
        {
            var stackTraceUsage = StackTraceUsage.None;

            // find all objects which may need stack trace
            // and determine maximum
            if (Target != null)
            {
                stackTraceUsage = Target.StackTraceUsage;
            }

            //recurse into chain if not max
            if (NextInChain != null && (stackTraceUsage & StackTraceUsage.Max) != StackTraceUsage.Max)
            {
                var stackTraceUsageForChain = NextInChain.PrecalculateStackTraceUsage();
                stackTraceUsage |= stackTraceUsageForChain;
            }

            StackTraceUsage = stackTraceUsage;
            return stackTraceUsage;
        }

        static internal TargetWithFilterChain[] BuildLoggerConfiguration(string loggerName, LoggingRule[] loggingRules, LogLevel globalLogLevel)
        {
            TargetWithFilterChain[] targetsByLevel = TargetWithFilterChain.CreateLoggerConfiguration();
            TargetWithFilterChain[] lastTargetsByLevel = TargetWithFilterChain.CreateLoggerConfiguration();
            bool[] suppressedLevels = new bool[LogLevel.MaxLevel.Ordinal + 1];

            bool targetsFound = GetTargetsByLevelForLogger(loggerName, loggingRules, globalLogLevel, targetsByLevel, lastTargetsByLevel, suppressedLevels);
            return targetsFound ? targetsByLevel : TargetWithFilterChain.NoTargetsByLevel;
        }

        static private bool GetTargetsByLevelForLogger(string name, LoggingRule[] loggingRules, LogLevel globalLogLevel, TargetWithFilterChain[] targetsByLevel, TargetWithFilterChain[] lastTargetsByLevel, bool[] suppressedLevels)
        {
            IList<KeyValuePair<FilterResult?, IList<Filter>>>? finalMinLevelWithFilters = null;
            bool targetsFound = false;
            foreach (LoggingRule rule in loggingRules)
            {
                if (!rule.NameMatches(name))
                {
                    continue;
                }

                if (LoggingRuleHasFinalMinLevelFilters(rule))
                {
                    CollectFinalMinLevelFiltersFromRule(rule, ref finalMinLevelWithFilters);
                }

                targetsFound = AddTargetsFromLoggingRule(rule, name, globalLogLevel, targetsByLevel, lastTargetsByLevel, suppressedLevels) || targetsFound;

#pragma warning disable CS0618 // Type or member is obsolete
                if (rule.ChildRules.Count != 0)
                {
                    // Recursively analyze the child rules.
                    targetsFound = GetTargetsByLevelForLogger(name, rule.GetChildRulesThreadSafe(), globalLogLevel, targetsByLevel, lastTargetsByLevel, suppressedLevels) || targetsFound;
                }
#pragma warning restore CS0618 // Type or member is obsolete
            }

            for (int i = 0; i <= LogLevel.MaxLevel.Ordinal; ++i)
            {
                TargetWithFilterChain tfc = targetsByLevel[i];
                if (tfc is null)
                    continue;

                if (finalMinLevelWithFilters?.Count > 0)
                {
                    var finalMinLevelFilters = finalMinLevelWithFilters[i];
                    if (finalMinLevelFilters.Value?.Count > 0 && finalMinLevelFilters.Key.HasValue)
                    {
                        targetsByLevel[i] = tfc = AppendFinalMinLevelFilters(tfc, finalMinLevelFilters.Value, finalMinLevelFilters.Key.Value);
                    }
                }

                tfc.PrecalculateStackTraceUsage();
            }

            return targetsFound;
        }

        private static bool LoggingRuleHasFinalMinLevelFilters(LoggingRule rule)
        {
            return rule.FinalMinLevel != LogLevel.Off && rule.Filters.Count != 0 && rule.Targets.Count == 0;
        }

        private static void CollectFinalMinLevelFiltersFromRule(LoggingRule rule, ref IList<KeyValuePair<FilterResult?, IList<Filter>>>? finalMinLevelWithFilters)
        {
            var finalMinLevel = rule.FinalMinLevel;
            if (finalMinLevel is null)
                return;

            finalMinLevelWithFilters = finalMinLevelWithFilters ?? new KeyValuePair<FilterResult?, IList<Filter>>[LogLevel.MaxLevel.Ordinal + 1];
            for (int i = 0; i <= LogLevel.MaxLevel.Ordinal; ++i)
            {
                if (i < finalMinLevel.Ordinal)
                    continue;

                var newFilterResult = finalMinLevelWithFilters[i].Key ?? rule.FilterDefaultAction;
                if (newFilterResult != rule.FilterDefaultAction)
                    continue;

                var newFilterChain = finalMinLevelWithFilters[i].Value?.Count > 0 ? System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Concat(finalMinLevelWithFilters[i].Value, rule.Filters)) : rule.Filters;
                finalMinLevelWithFilters[i] = new KeyValuePair<FilterResult?, IList<Filter>>(newFilterResult, newFilterChain);
            }
        }

        private static TargetWithFilterChain AppendFinalMinLevelFilters(TargetWithFilterChain targetsByLevel, IList<Filter> finalMinLevelFilters, FilterResult finalMinLevelDefaultResult)
        {
            if (targetsByLevel.FilterChain?.Count > 0 && targetsByLevel.FilterDefaultAction != finalMinLevelDefaultResult)
                return targetsByLevel;

            var newFilterChain = targetsByLevel.FilterChain?.Count > 0 ? System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Concat(finalMinLevelFilters, targetsByLevel.FilterChain)) : finalMinLevelFilters;
            var newTargetsByLevel = new TargetWithFilterChain(targetsByLevel.Target, newFilterChain, finalMinLevelDefaultResult);

            var nextInChain = targetsByLevel.NextInChain is null ? null : AppendFinalMinLevelFilters(targetsByLevel.NextInChain, finalMinLevelFilters, finalMinLevelDefaultResult);
            newTargetsByLevel.NextInChain = nextInChain ?? targetsByLevel.NextInChain;
            return newTargetsByLevel;
        }

        private static bool AddTargetsFromLoggingRule(LoggingRule rule, string loggerName, LogLevel globalLogLevel, TargetWithFilterChain[] targetsByLevel, TargetWithFilterChain[] lastTargetsByLevel, bool[] suppressedLevels)
        {
            bool targetsFound = false;
            bool duplicateTargetsFound = false;

            var finalMinLevel = rule.FinalMinLevel;
            var ruleLogLevels = rule.LogLevels;

            for (int i = 0; i <= LogLevel.MaxLevel.Ordinal; ++i)
            {
                if (SuppressLogLevel(rule, ruleLogLevels, finalMinLevel, globalLogLevel, i, ref suppressedLevels[i]))
                {
                    continue;
                }

                foreach (Target target in rule.GetTargetsThreadSafe())
                {
                    targetsFound = true;

                    var awf = CreateTargetChainFromLoggingRule(rule, target, targetsByLevel[i]);
                    if (awf is null)
                    {
                        if (!duplicateTargetsFound)
                        {
                            InternalLogger.Warn("Logger: {0} configured with duplicate output to target: {1}. LoggingRule with NamePattern='{2}' and Level={3} has been skipped.", loggerName, target, rule.LoggerNamePattern, LogLevel.FromOrdinal(i));
                        }
                        duplicateTargetsFound = true;
                        continue;
                    }

                    if (lastTargetsByLevel[i] != null)
                    {
                        lastTargetsByLevel[i].NextInChain = awf;
                    }
                    else
                    {
                        targetsByLevel[i] = awf;
                    }

                    lastTargetsByLevel[i] = awf;
                }
            }

            return targetsFound;
        }

        private static bool SuppressLogLevel(LoggingRule rule, bool[] ruleLogLevels, LogLevel? finalMinLevel, LogLevel globalLogLevel, int logLevelOrdinal, ref bool suppressedLevels)
        {
            if (logLevelOrdinal < globalLogLevel.Ordinal)
            {
                return true;
            }

            if (finalMinLevel is null)
            {
                if (suppressedLevels)
                {
                    return true;
                }
            }
            else
            {
                suppressedLevels = finalMinLevel.Ordinal > logLevelOrdinal;
            }

            if (!ruleLogLevels[logLevelOrdinal])
            {
                return true;
            }

            if (rule.Final)
            {
                suppressedLevels = true;
            }

            return false;
        }

        private static TargetWithFilterChain? CreateTargetChainFromLoggingRule(LoggingRule rule, Target target, TargetWithFilterChain existingTargets)
        {
            var filterChain = rule.Filters.Count == 0 ? ArrayHelper.Empty<NLog.Filters.Filter>() : rule.Filters;
            var newTarget = new TargetWithFilterChain(target, filterChain, rule.FilterDefaultAction);

            if (existingTargets != null && newTarget.FilterChain.Count == 0)
            {
                for (TargetWithFilterChain? afc = existingTargets; afc != null; afc = afc.NextInChain)
                {
                    if (ReferenceEquals(target, afc.Target) && afc.FilterChain.Count == 0)
                    {
                        return null;    // Duplicate Target
                    }
                }
            }

            return newTarget;
        }

        internal bool TryCallSiteClassNameOptimization(StackTraceUsage stackTraceUsage, LogEventInfo logEvent)
        {
            if ((stackTraceUsage & (StackTraceUsage.WithCallSiteClassName | StackTraceUsage.WithStackTrace)) != StackTraceUsage.WithCallSiteClassName)
                return false;

            if (string.IsNullOrEmpty(logEvent.CallSiteInformation?.CallerFilePath))
                return false;

            if (logEvent.HasStackTrace)
                return false;

            return true;
        }

        internal bool MustCaptureStackTrace(StackTraceUsage stackTraceUsage, LogEventInfo logEvent)
        {
            if (logEvent.HasStackTrace)
                return false;

            if ((stackTraceUsage & StackTraceUsage.WithStackTrace) != StackTraceUsage.None)
                return true;

            if ((stackTraceUsage & StackTraceUsage.WithCallSite) != StackTraceUsage.None && string.IsNullOrEmpty(logEvent.CallSiteInformation?.CallerMethodName) && string.IsNullOrEmpty(logEvent.CallSiteInformation?.CallerFilePath))
                return true;    // We don't have enough CallSiteInformation

            return false;
        }

        internal bool TryRememberCallSiteClassName(LogEventInfo logEvent)
        {
            if (string.IsNullOrEmpty(logEvent.CallSiteInformation?.CallerFilePath))
                return false;

            var className = logEvent.CallSiteInformation?.GetCallerClassName(null, true, true, true);
            if (string.IsNullOrEmpty(className))
                return false;

            if (_callSiteClassNameCache is null)
                return false;

            string internClassName = logEvent.LoggerName == className ?
                logEvent.LoggerName :
                string.Intern(className);   // Single string-reference for all logging-locations for the same class

            CallSiteKey callSiteKey = new CallSiteKey(logEvent.CallerMemberName, logEvent.CallerFilePath, logEvent.CallerLineNumber);
            return _callSiteClassNameCache.TryAddValue(callSiteKey, internClassName);
        }

        internal bool TryLookupCallSiteClassName(LogEventInfo logEvent, out string? callSiteClassName)
        {
            callSiteClassName = logEvent.CallSiteInformation?.CallerClassName;
            if (!string.IsNullOrEmpty(callSiteClassName))
                return true;

            if (_callSiteClassNameCache is null)
            {
                System.Threading.Interlocked.CompareExchange(ref _callSiteClassNameCache, new MruCache<CallSiteKey, string>(1000), null);
            }

            CallSiteKey callSiteKey = new CallSiteKey(logEvent.CallerMemberName, logEvent.CallerFilePath, logEvent.CallerLineNumber);
            return _callSiteClassNameCache.TryGetValue(callSiteKey, out callSiteClassName);
        }

        public void WriteToLoggerTargets(Type loggerType, LogEventInfo logEvent, LogFactory logFactory)
        {
            LoggerImpl.Write(loggerType, this, logEvent, logFactory);
        }

        private
#if !NETFRAMEWORK
        readonly
#endif
            struct CallSiteKey : IEquatable<CallSiteKey>
        {
            public CallSiteKey(string? methodName, string? fileSourceName, int fileSourceLineNumber)
            {
                MethodName = methodName ?? string.Empty;
                FileSourceName = fileSourceName ?? string.Empty;
                FileSourceLineNumber = fileSourceLineNumber;
            }

            public readonly string MethodName;
            public readonly string FileSourceName;
            public readonly int FileSourceLineNumber;

            public override int GetHashCode()
            {
                return MethodName.GetHashCode() ^ FileSourceName.GetHashCode() ^ FileSourceLineNumber;
            }

            public override bool Equals(object obj)
            {
                return obj is CallSiteKey key && Equals(key);
            }

            public bool Equals(CallSiteKey other)
            {
                return FileSourceLineNumber == other.FileSourceLineNumber
                    && string.Equals(FileSourceName, other.FileSourceName, StringComparison.Ordinal)
                    && string.Equals(MethodName, other.MethodName, StringComparison.Ordinal);
            }
        }
    }
}

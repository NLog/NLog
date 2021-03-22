﻿// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
using System.Collections.Generic;
using NLog.Config;
using NLog.Filters;
using NLog.Internal;
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace NLog
{
    /// <summary>
    /// Extension methods to setup NLog <see cref="LoggingConfiguration"/>
    /// </summary>
    public static class SetupLoadConfigurationExtensions
    {
        /// <summary>
        /// Configures the global time-source used for all logevents
        /// </summary>
        /// <remarks>
        /// Available by default: <see cref="Time.AccurateLocalTimeSource"/>, <see cref="Time.AccurateUtcTimeSource"/>, <see cref="Time.FastLocalTimeSource"/>, <see cref="Time.FastUtcTimeSource"/>
        /// </remarks>
        public static ISetupLoadConfigurationBuilder SetTimeSource(this ISetupLoadConfigurationBuilder configBuilder, NLog.Time.TimeSource timeSource)
        {
            NLog.Time.TimeSource.Current = timeSource;
            return configBuilder;
        }

        /// <summary>
        /// Updates the dictionary <see cref="GlobalDiagnosticsContext"/> ${gdc:item=} with the name-value-pair
        /// </summary>
        public static ISetupLoadConfigurationBuilder SetGlobalContextProperty(this ISetupLoadConfigurationBuilder configBuilder, string name, string value)
        {
            GlobalDiagnosticsContext.Set(name, value);
            return configBuilder;
        }

        /// <summary>
        /// Defines <see cref="LoggingRule" /> for redirecting output from matching <see cref="Logger"/> to wanted targets.
        /// </summary>
        /// <param name="configBuilder">Fluent interface parameter.</param>
        /// <param name="loggerNamePattern">Logger name pattern to check which <see cref="Logger"/> names matches this rule</param>
        /// <param name="ruleName">Rule identifier to allow rule lookup</param>
        public static ISetupConfigurationLoggingRuleBuilder ForLogger(this ISetupLoadConfigurationBuilder configBuilder, string loggerNamePattern = "*", string ruleName = null)
        {
            var ruleBuilder = new SetupConfigurationLoggingRuleBuilder(configBuilder.LogFactory, configBuilder.Configuration, loggerNamePattern, ruleName);
            ruleBuilder.LoggingRule.EnableLoggingForLevels(LogLevel.MinLevel, LogLevel.MaxLevel);
            return ruleBuilder;
        }

        /// <summary>
        /// Defines <see cref="LoggingRule" /> for redirecting output from matching <see cref="Logger"/> to wanted targets.
        /// </summary>
        /// <param name="configBuilder">Fluent interface parameter.</param>
        /// <param name="targetName">Override the name for the target created</param>
        public static ISetupConfigurationTargetBuilder ForTarget(this ISetupLoadConfigurationBuilder configBuilder, string targetName = null)
        {
            var ruleBuilder = new SetupConfigurationTargetBuilder(configBuilder.LogFactory, configBuilder.Configuration, targetName);
            return ruleBuilder;
        }

        /// <summary>
        /// Apply fast filtering based on <see cref="LogLevel"/>. Include LogEvents with same or worse severity as <paramref name="minLevel"/>.
        /// </summary>
        /// <param name="configBuilder">Fluent interface parameter.</param>
        /// <param name="minLevel">Minimum level that this rule matches</param>
        public static ISetupConfigurationLoggingRuleBuilder FilterMinLevel(this ISetupConfigurationLoggingRuleBuilder configBuilder, LogLevel minLevel)
        {
            if (minLevel == null)
                throw new ArgumentNullException(nameof(minLevel));

            configBuilder.LoggingRule.DisableLoggingForLevels(LogLevel.MinLevel, LogLevel.MaxLevel);
            configBuilder.LoggingRule.EnableLoggingForLevels(minLevel, LogLevel.MaxLevel);
            return configBuilder;
        }

        /// <summary>
        /// Apply fast filtering based on <see cref="LogLevel"/>. Include LogEvents with same or less severity as <paramref name="maxLevel"/>.
        /// </summary>
        /// <param name="configBuilder">Fluent interface parameter.</param>
        /// <param name="maxLevel">Maximum level that this rule matches</param>
        public static ISetupConfigurationLoggingRuleBuilder FilterMaxLevel(this ISetupConfigurationLoggingRuleBuilder configBuilder, LogLevel maxLevel)
        {
            if (maxLevel == null)
                throw new ArgumentNullException(nameof(maxLevel));

            configBuilder.LoggingRule.DisableLoggingForLevels(LogLevel.MinLevel, LogLevel.MaxLevel);
            configBuilder.LoggingRule.EnableLoggingForLevels(LogLevel.MinLevel, maxLevel);
            return configBuilder;
        }

        /// <summary>
        /// Apply fast filtering based on <see cref="LogLevel"/>. Include LogEvents with severity that equals <paramref name="logLevel"/>.
        /// </summary>
        /// <param name="configBuilder">Fluent interface parameter.</param>
        /// <param name="logLevel">Single loglevel that this rule matches</param>
        public static ISetupConfigurationLoggingRuleBuilder FilterLevel(this ISetupConfigurationLoggingRuleBuilder configBuilder, LogLevel logLevel)
        {
            if (logLevel == null)
                throw new ArgumentNullException(nameof(logLevel));

            if (configBuilder.LoggingRule.IsLoggingEnabledForLevel(logLevel))
            {
                configBuilder.LoggingRule.DisableLoggingForLevels(LogLevel.MinLevel, LogLevel.MaxLevel);
            }
            configBuilder.LoggingRule.EnableLoggingForLevel(logLevel);
            return configBuilder;
        }

        /// <summary>
        /// Apply fast filtering based on <see cref="LogLevel"/>. Include LogEvents with severity between <paramref name="minLevel"/> and <paramref name="maxLevel"/>.
        /// </summary>
        /// <param name="configBuilder">Fluent interface parameter.</param>
        /// <param name="minLevel">Minimum level that this rule matches</param>
        /// <param name="maxLevel">Maximum level that this rule matches</param>
        public static ISetupConfigurationLoggingRuleBuilder FilterLevels(this ISetupConfigurationLoggingRuleBuilder configBuilder, LogLevel minLevel, LogLevel maxLevel)
        {
            configBuilder.LoggingRule.DisableLoggingForLevels(LogLevel.MinLevel, LogLevel.MaxLevel);
            configBuilder.LoggingRule.EnableLoggingForLevels(minLevel ?? LogLevel.MinLevel, maxLevel ?? LogLevel.MaxLevel);
            return configBuilder;
        }

        /// <summary>
        /// Apply dynamic filtering logic for advanced control of when to redirect output to target.
        /// </summary>
        /// <param name="configBuilder">Fluent interface parameter.</param>
        /// <param name="filter">Filter for controlling whether to write</param>
        /// <param name="defaultFilterResult">Default action if none of the filters match</param>
        public static ISetupConfigurationLoggingRuleBuilder FilterDynamic(this ISetupConfigurationLoggingRuleBuilder configBuilder, Filter filter, FilterResult? defaultFilterResult = null)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            configBuilder.LoggingRule.Filters.Add(filter);
            if (defaultFilterResult.HasValue)
                configBuilder.LoggingRule.DefaultFilterResult = defaultFilterResult.Value;
            return configBuilder;
        }

        /// <summary>
        /// Apply dynamic filtering logic for advanced control of when to redirect output to target.
        /// </summary>
        /// <param name="configBuilder">Fluent interface parameter.</param>
        /// <param name="filterMethod">Delegate for controlling whether to write</param>
        /// <param name="defaultFilterResult">Default action if none of the filters match</param>
        public static ISetupConfigurationLoggingRuleBuilder FilterDynamic(this ISetupConfigurationLoggingRuleBuilder configBuilder, Func<LogEventInfo, FilterResult> filterMethod, FilterResult? defaultFilterResult = null)
        {
            if (filterMethod == null)
                throw new ArgumentNullException(nameof(filterMethod));

            return configBuilder.FilterDynamic(new WhenMethodFilter(filterMethod), defaultFilterResult);
        }

        /// <summary>
        /// Configure <see cref="LoggingRule.Final"/>, so LogEvents matching this LoggingRule will not flow down to the following rules.
        /// </summary>
        public static ISetupConfigurationLoggingRuleBuilder FinalRule(this ISetupConfigurationLoggingRuleBuilder configBuilder, bool final = true)
        {
            configBuilder.LoggingRule.Final = final;
            return configBuilder;
        }

        /// <summary>
        /// Move the <see cref="LoggingRule" /> to the top, to match before any of the existing <see cref="LoggingConfiguration.LoggingRules"/>
        /// </summary>
        public static ISetupConfigurationLoggingRuleBuilder TopRule(this ISetupConfigurationLoggingRuleBuilder configBuilder, bool insertFirst = true)
        {
            var loggingRule = configBuilder.LoggingRule;
            if (configBuilder.Configuration.LoggingRules.Contains(loggingRule))
            {
                if (!insertFirst)
                    return configBuilder;

                configBuilder.Configuration.LoggingRules.Remove(loggingRule);
            }

            if (insertFirst)
                configBuilder.Configuration.LoggingRules.Insert(0, loggingRule);
            else
                configBuilder.Configuration.LoggingRules.Add(loggingRule);
            return configBuilder;
        }

        /// <summary>
        /// Redirect output from matching <see cref="Logger"/> to the provided <paramref name="target"/>
        /// </summary>
        /// <param name="configBuilder">Fluent interface parameter.</param>
        /// <param name="target">Target that should be written to.</param>
        /// <returns>Fluent interface for configuring targets for the new LoggingRule.</returns>
        public static ISetupConfigurationTargetBuilder WriteTo(this ISetupConfigurationTargetBuilder configBuilder, Target target)
        {
            if (target != null)
            {
                if (string.IsNullOrEmpty(target.Name))
                    target.Name = EnsureUniqueTargetName(configBuilder.Configuration, target);
                configBuilder.Targets.Add(target);
                configBuilder.Configuration.AddTarget(target);
            }

            return configBuilder;
        }

        /// <summary>
        /// Redirect output from matching <see cref="Logger"/> to the provided <paramref name="targets"/>
        /// </summary>
        /// <param name="configBuilder">Fluent interface parameter.</param>
        /// <param name="targets">Target-collection that should be written to.</param>
        /// <returns>Fluent interface for configuring targets for the new LoggingRule.</returns>
        public static ISetupConfigurationTargetBuilder WriteTo(this ISetupConfigurationTargetBuilder configBuilder, params Target[] targets)
        {
            if (targets?.Length > 0)
            {
                for (int i = 0; i < targets.Length; ++i)
                {
                    configBuilder.WriteTo(targets[i]);
                }
            }

            return configBuilder;
        }

        /// <summary>
        /// Redirect output from matching <see cref="Logger"/> to the provided <paramref name="targetBuilder"/>
        /// </summary>
        /// <param name="configBuilder">Fluent interface parameter.</param>
        /// <param name="targetBuilder">Target-collection that should be written to.</param>
        /// <returns>Fluent interface for configuring targets for the new LoggingRule.</returns>
        public static ISetupConfigurationTargetBuilder WriteTo(this ISetupConfigurationTargetBuilder configBuilder, ISetupConfigurationTargetBuilder targetBuilder)
        {
            if (ReferenceEquals(configBuilder, targetBuilder))
                throw new ArgumentException("ConfigBuilder and TargetBuilder cannot be the same object", nameof(targetBuilder));

            if (targetBuilder.Targets?.Count > 0)
            {
                for (int i = 0; i < targetBuilder.Targets.Count; ++i)
                {
                    configBuilder.WriteTo(targetBuilder.Targets[i]);
                }
            }

            return configBuilder;
        }

        /// <summary>
        /// Discard output from any matching <see cref="Logger"/>, so the output will not reach <see cref="LoggingConfiguration.LoggingRules"/> added after this.
        /// </summary>
        /// <param name="configBuilder">Fluent interface parameter.</param>
        /// <param name="maxLevel">Maximum level that this rule matches</param>
        public static void WriteToNil(this ISetupConfigurationLoggingRuleBuilder configBuilder, LogLevel maxLevel = null)
        {
            var logginRule = configBuilder.FilterMaxLevel(maxLevel ?? LogLevel.MaxLevel).FinalRule().LoggingRule;
            if (!configBuilder.Configuration.LoggingRules.Contains(logginRule))
            {
                configBuilder.Configuration.LoggingRules.Add(logginRule);
            }
        }

        /// <summary>
        /// Returns first target registered
        /// </summary>
        public static Target FirstTarget(this ISetupConfigurationTargetBuilder configBuilder)
        {
            return System.Linq.Enumerable.First(configBuilder.Targets);
        }

        /// <summary>
        /// Returns first target registered with the specified type
        /// </summary>
        /// <typeparam name="T">Type of target</typeparam>
        public static T FirstTarget<T>(this ISetupConfigurationTargetBuilder configBuilder) where T : Target
        {
            var target = System.Linq.Enumerable.First(configBuilder.Targets);

            for (int i = 0; i < configBuilder.Targets.Count; ++i)
            {
                foreach (var unwrappedTarget in YieldAllTargets(configBuilder.Targets[i]))
                {
                    if (unwrappedTarget is T typedTarget)
                        return typedTarget;
                }
            }

            throw new InvalidCastException($"Unable to cast object of type '{target.GetType()}' to type '{typeof(T)}'");
        }

        internal static IEnumerable<Target> YieldAllTargets(Target target)
        {
            yield return target;

            if (target is WrapperTargetBase wrapperTarget)
            {
                foreach (var unwrappedTarget in YieldAllTargets(wrapperTarget.WrappedTarget))
                    yield return unwrappedTarget;
            }
            else if (target is CompoundTargetBase compoundTarget)
            {
                foreach (var nestedTarget in compoundTarget.Targets)
                {
                    foreach (var unwrappedTarget in YieldAllTargets(nestedTarget))
                        yield return unwrappedTarget;
                }
            }
        }

        /// <summary>
        /// Write to <see cref="NLog.Targets.MethodCallTarget"/>
        /// </summary>
        /// <param name="configBuilder">Fluent interface parameter.</param>
        /// <param name="logEventAction">Method to call on logevent</param>
        /// <param name="layouts">Layouts to render object[]-args before calling <paramref name="logEventAction"/></param>
        public static ISetupConfigurationTargetBuilder WriteToMethodCall(this ISetupConfigurationTargetBuilder configBuilder, Action<LogEventInfo, object[]> logEventAction, Layout[] layouts = null)
        {
            if (logEventAction == null)
                throw new ArgumentNullException(nameof(logEventAction));

            var methodTarget = new MethodCallTarget(string.Empty, logEventAction);
            if (layouts?.Length > 0)
            {
                foreach (var layout in layouts)
                    methodTarget.Parameters.Add(new MethodCallParameter(layout));
            }
            
            return configBuilder.WriteTo(methodTarget);
        }

#if !NETSTANDARD1_3
        /// <summary>
        /// Write to <see cref="NLog.Targets.ConsoleTarget"/> 
        /// </summary>
        /// <param name="configBuilder">Fluent interface parameter.</param>
        /// <param name="layout">Override the default Layout for output</param>
        /// <param name="encoding">Override the default Encoding for output (Ex. UTF8)</param>
        /// <param name="stderr">Write to stderr instead of standard output (stdout)</param>
        /// <param name="detectConsoleAvailable">Skip overhead from writing to console, when not available (Ex. running as Windows Service)</param>
        /// <param name="writeBuffered">Enable batch writing of logevents, instead of Console.WriteLine for each logevent (Requires <see cref="WithAsync"/>)</param>
        public static ISetupConfigurationTargetBuilder WriteToConsole(this ISetupConfigurationTargetBuilder configBuilder, Layout layout = null, System.Text.Encoding encoding = null, bool stderr = false, bool detectConsoleAvailable = false, bool writeBuffered = false)
        {
            var consoleTarget = new ConsoleTarget();
            if (layout != null)
                consoleTarget.Layout = layout;
            if (encoding != null)
                consoleTarget.Encoding = encoding;
            consoleTarget.StdErr = stderr;
            consoleTarget.DetectConsoleAvailable = detectConsoleAvailable;
            consoleTarget.WriteBuffer = writeBuffered;
            return configBuilder.WriteTo(consoleTarget);
        }
#endif

        /// <summary>
        /// Write to <see cref="NLog.Targets.FileTarget"/> 
        /// </summary>
        /// <param name="configBuilder">Fluent interface parameter.</param>
        /// <param name="fileName"></param>
        /// <param name="layout">Override the default Layout for output</param>
        /// <param name="encoding">Override the default Encoding for output (Default = UTF8)</param>
        /// <param name="lineEnding">Override the default line ending characters (Ex. <see cref="LineEndingMode.LF"/> without CR)</param>
        /// <param name="keepFileOpen">Keep log file open instead of opening and closing it on each logging event</param>
        /// <param name="concurrentWrites">Activate multi-process synchronization using global mutex on the operating system</param>
        /// <param name="archiveAboveSize">Size in bytes where log files will be automatically archived.</param>
        /// <param name="maxArchiveFiles">Maximum number of archive files that should be kept.</param>
        /// <param name="maxArchiveDays">Maximum days of archive files that should be kept.</param>
        public static ISetupConfigurationTargetBuilder WriteToFile(this ISetupConfigurationTargetBuilder configBuilder, Layout fileName, Layout layout = null, System.Text.Encoding encoding = null, LineEndingMode lineEnding = null, bool keepFileOpen = false, bool concurrentWrites = false, long archiveAboveSize = 0, int maxArchiveFiles = 0, int maxArchiveDays = 0)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));

            var fileTarget = new FileTarget();
            fileTarget.FileName = fileName;
            if (layout != null)
                fileTarget.Layout = layout;
            if (encoding != null)
                fileTarget.Encoding = encoding;
            if (lineEnding != null)
                fileTarget.LineEnding = lineEnding;
            fileTarget.KeepFileOpen = keepFileOpen;
            fileTarget.ConcurrentWrites = concurrentWrites;
            fileTarget.ArchiveAboveSize = archiveAboveSize;
            fileTarget.MaxArchiveFiles = maxArchiveFiles;
            fileTarget.MaxArchiveDays = maxArchiveDays;
            return configBuilder.WriteTo(fileTarget);
        }

        /// <summary>
        /// Applies target wrapper for existing <see cref="LoggingRule.Targets"/>
        /// </summary>
        /// <param name="configBuilder">Fluent interface parameter.</param>
        /// <param name="wrapperFactory">Factory method for creating target-wrapper</param>
        public static ISetupConfigurationTargetBuilder WithWrapper(this ISetupConfigurationTargetBuilder configBuilder, Func<Target, Target> wrapperFactory)
        {
            if (wrapperFactory == null)
                throw new ArgumentNullException(nameof(wrapperFactory));

            var targets = configBuilder.Targets;

            if (targets?.Count > 0)
            {
                for (int i = 0; i < targets.Count; ++i)
                {
                    var target = targets[i];
                    var targetWrapper = wrapperFactory(target);
                    if (targetWrapper == null || ReferenceEquals(targetWrapper, target))
                        continue;

                    if (string.IsNullOrEmpty(targetWrapper.Name))
                        targetWrapper.Name = EnsureUniqueTargetName(configBuilder.Configuration, targetWrapper, target.Name);

                    targets[i] = targetWrapper;
                    configBuilder.Configuration.AddTarget(targetWrapper);
                }
            }
            else
            {
                throw new ArgumentException("Must call WriteTo(...) before applying target wrapper");
            }

            return configBuilder;
        }

        /// <summary>
        /// Applies <see cref="NLog.Targets.Wrappers.AsyncTargetWrapper"/> for existing <see cref="LoggingRule.Targets"/> for asynchronous background writing
        /// </summary>
        /// <param name="configBuilder">Fluent interface parameter.</param>
        /// <param name="overflowAction">Action to take when queue overflows</param>
        /// <param name="queueLimit">Queue size limit for pending logevents</param>
        /// <param name="batchSize">Batch size when writing on the background thread</param>
        public static ISetupConfigurationTargetBuilder WithAsync(this ISetupConfigurationTargetBuilder configBuilder, AsyncTargetWrapperOverflowAction overflowAction = AsyncTargetWrapperOverflowAction.Discard, int queueLimit = 10000, int batchSize = 200)
        {
            return configBuilder.WithWrapper(t =>
            {
                if (t is AsyncTargetWrapper)
                    return null;
#if !NET35
                if (t is AsyncTaskTarget)
                    return null;
#endif
                var asyncWrapper = new AsyncTargetWrapper() { WrappedTarget = t };
                asyncWrapper.OverflowAction = overflowAction;
                asyncWrapper.QueueLimit = queueLimit;
                asyncWrapper.BatchSize = batchSize;
                return asyncWrapper;
            });
        }

        /// <summary>
        /// Applies <see cref="NLog.Targets.Wrappers.BufferingTargetWrapper"/> for existing <see cref="LoggingRule.Targets"/> for throttled writing
        /// </summary>
        /// <param name="configBuilder">Fluent interface parameter.</param>
        /// <param name="bufferSize">Buffer size limit for pending logevents</param>
        /// <param name="flushTimeout">Timeout for when the buffer will flush automatically using background thread</param>
        /// <param name="slidingTimeout">Restart timeout when when logevent is written</param>
        /// <param name="overflowAction">Action to take when buffer overflows</param>
        public static ISetupConfigurationTargetBuilder WithBuffering(this ISetupConfigurationTargetBuilder configBuilder, int? bufferSize = null, TimeSpan? flushTimeout = null, bool? slidingTimeout = null, BufferingTargetWrapperOverflowAction? overflowAction = null)
        {
            return configBuilder.WithWrapper(t =>
            {
                var targetWrapper = new BufferingTargetWrapper() { WrappedTarget = t };
                if (bufferSize.HasValue)
                    targetWrapper.BufferSize = bufferSize.Value;
                if (flushTimeout.HasValue)
                    targetWrapper.FlushTimeout = (int)flushTimeout.Value.TotalMilliseconds;
                if (slidingTimeout.HasValue)
                    targetWrapper.SlidingTimeout = slidingTimeout.Value;
                if (overflowAction.HasValue)
                    targetWrapper.OverflowAction = overflowAction.Value;
                return targetWrapper;
            });
        }

        /// <summary>
        /// Applies <see cref="NLog.Targets.Wrappers.AutoFlushTargetWrapper"/> for existing <see cref="LoggingRule.Targets"/> for flushing after conditional event
        /// </summary>
        /// <param name="configBuilder">Fluent interface parameter.</param>
        /// <param name="conditionMethod">Method delegate that controls whether logevent should force flush.</param>
        /// <param name="flushOnConditionOnly">Only flush when <paramref name="conditionMethod"/> triggers (Ignore config-reload and config-shutdown)</param>
        public static ISetupConfigurationTargetBuilder WithAutoFlush(this ISetupConfigurationTargetBuilder configBuilder, Func<LogEventInfo, bool> conditionMethod, bool? flushOnConditionOnly = null)
        {
            return configBuilder.WithWrapper(t =>
            {
                var targetWrapper = new AutoFlushTargetWrapper() { WrappedTarget = t };
                var methodInfo = conditionMethod.GetDelegateInfo();
                ReflectionHelpers.LateBoundMethod lateBound = (target, args) => conditionMethod((LogEventInfo)args[0]);
                var conditionExpression = new Conditions.ConditionMethodExpression(methodInfo.Name, methodInfo, lateBound, ArrayHelper.Empty<Conditions.ConditionExpression>());
                targetWrapper.Condition = conditionExpression;
                if (flushOnConditionOnly.HasValue)
                    targetWrapper.FlushOnConditionOnly = flushOnConditionOnly.Value;
                return targetWrapper;
            });
        }

        /// <summary>
        /// Applies <see cref="NLog.Targets.Wrappers.RetryingTargetWrapper"/> for existing <see cref="LoggingRule.Targets"/> for retrying after failure
        /// </summary>
        /// <param name="configBuilder">Fluent interface parameter.</param>
        /// <param name="retryCount">Number of retries that should be attempted on the wrapped target in case of a failure.</param>
        /// <param name="retryDelay">Time to wait between retries</param>
        public static ISetupConfigurationTargetBuilder WithRetry(this ISetupConfigurationTargetBuilder configBuilder, int? retryCount = null, TimeSpan? retryDelay = null)
        {
            return configBuilder.WithWrapper(t =>
            {
                var targetWrapper = new RetryingTargetWrapper() { WrappedTarget = t };
                if (retryCount.HasValue)
                    targetWrapper.RetryCount = retryCount.Value;
                if (retryDelay.HasValue)
                    targetWrapper.RetryDelayMilliseconds = (int)retryDelay.Value.TotalMilliseconds;
                return targetWrapper;
            });
        }

        /// <summary>
        /// Applies <see cref="NLog.Targets.Wrappers.FallbackGroupTarget"/> for existing <see cref="LoggingRule.Targets"/> to fallback on failure.
        /// </summary>
        /// <param name="configBuilder">Fluent interface parameter.</param>
        /// <param name="fallbackTarget">Target to use for fallback</param>
        /// <param name="returnToFirstOnSuccess">Whether to return to the first target after any successful write</param>
        public static ISetupConfigurationTargetBuilder WithFallback(this ISetupConfigurationTargetBuilder configBuilder, Target fallbackTarget, bool returnToFirstOnSuccess = true)
        {
            if (fallbackTarget == null)
                throw new ArgumentNullException(nameof(fallbackTarget));

            if (string.IsNullOrEmpty(fallbackTarget.Name))
                fallbackTarget.Name = EnsureUniqueTargetName(configBuilder.Configuration, fallbackTarget, "_Fallback");

            return configBuilder.WithWrapper(t =>
            {
                var targetWrapper = new FallbackGroupTarget();
                targetWrapper.ReturnToFirstOnSuccess = returnToFirstOnSuccess;
                targetWrapper.Targets.Add(t);
                targetWrapper.Targets.Add(fallbackTarget);
                configBuilder.Configuration.AddTarget(fallbackTarget);
                return targetWrapper;
            });
        }   

        private static string EnsureUniqueTargetName(LoggingConfiguration configuration, Target target, string suffix = "")
        {
            var allTargets = configuration.AllTargets;
            var targetName = target.Name;
            if (string.IsNullOrEmpty(targetName))
            {
                targetName = GenerateTargetName(target.GetType());
            }
            if (!string.IsNullOrEmpty(suffix))
            {
                targetName = string.Concat(targetName, "_", suffix);
            }

            int targetIndex = 0;
            string newTargetName = targetName;
            while (!IsTargetNameUnique(allTargets, target, newTargetName))
            {
                newTargetName = string.Concat(targetName, "_", (++targetIndex).ToString());
            }

            return newTargetName;
        }

        private static bool IsTargetNameUnique(IList<Target> allTargets, Target target, string targetName)
        {
            for (int i = 0; i < allTargets.Count; ++i)
            {
                var otherTarget = allTargets[i];
                if (ReferenceEquals(target, otherTarget))
                    return true;

                if (string.CompareOrdinal(otherTarget.Name, targetName) == 0)
                    return false;
            }

            return true;
        }

        private static string GenerateTargetName(Type targetType)
        {
            var targetName = targetType.GetFirstCustomAttribute<TargetAttribute>()?.Name ?? string.Empty;
            if (string.IsNullOrEmpty(targetName))
                targetName = targetType.ToString();

            if (targetName.EndsWith("TargetWrapper", StringComparison.Ordinal))
                targetName = targetName.Substring(0, targetName.Length - 13);

            if (targetName.EndsWith("Wrapper", StringComparison.Ordinal))
                targetName = targetName.Substring(0, targetName.Length - 7);

            if (targetName.EndsWith("GroupTarget", StringComparison.Ordinal))
                targetName = targetName.Substring(0, targetName.Length - 12);

            if (targetName.EndsWith("Group", StringComparison.Ordinal))
                targetName = targetName.Substring(0, targetName.Length - 5);

            if (targetName.EndsWith("Target", StringComparison.Ordinal))
                targetName = targetName.Substring(0, targetName.Length - 6);

            if (string.IsNullOrEmpty(targetName))
                targetName = "Unknown";

            return targetName;
        }
    }
}

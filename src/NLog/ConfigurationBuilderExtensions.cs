// 
// Copyright (c) 2004-2018 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.IO;
    using System.Reflection;
    using NLog.Common;
    using NLog.Config;
    using NLog.Layouts;
    using NLog.Targets;
    using NLog.Targets.Wrappers;
    using NLog.Time;

    /// <summary>
    /// Extension methods to help building <see cref="LoggingConfiguration" /> using fluent API
    /// </summary>
    public static class ConfigurationBuilderExtensions
    {
        /// <summary>
        /// Configures a new <see cref="LoggingRule" /> instance with pre-configured <see cref="LogLevel"/> filter.
        /// </summary>
        public static ConfigurationBuilder FilterLevel(this ConfigurationBuilder configurationBuilder, LogLevel logLevel, Action<ConfigurationBuilderFilterRule> buildAction, bool final = false)
        {
            return configurationBuilder.FilterRule(cb =>
            {
                cb.FilterLogger("*").FilterLevel(logLevel);
                if (final)
                    cb.FinalRule();
                buildAction.Invoke(cb);
            });
        }

        /// <summary>
        /// Configures a new <see cref="LoggingRule" /> instance with pre-configured minimum <see cref="LogLevel"/> filter.
        /// </summary>
        public static ConfigurationBuilder FilterMinLevel(this ConfigurationBuilder configurationBuilder, LogLevel logLevel, Action<ConfigurationBuilderFilterRule> buildAction, bool final = false)
        {
            return configurationBuilder.FilterRule(cb =>
            {
                cb.FilterLogger("*").FilterLevels(logLevel, LogLevel.MaxLevel);
                if (final)
                    cb.FinalRule();
                buildAction.Invoke(cb);
            });
        }

        /// <summary>
        /// Configures a new <see cref="LoggingRule" /> instance that captures all matching LogEvents (Black Hole), so they will not reach following LoggingRules
        /// </summary>
        /// <param name="configurationBuilder">ConfigurationBuilder</param>
        /// <param name="maxLevel">Captures all LogEvents with this LogLevel (or below)</param>
        /// <param name="loggerName">Captures all LogEvents matching Logger-name wilcard</param>
        /// <returns></returns>
        public static ConfigurationBuilder FilterIntoVoid(this ConfigurationBuilder configurationBuilder, LogLevel maxLevel, string loggerName = "*")
        {
            return configurationBuilder.FilterRule(cb =>
            {
                cb.FilterLogger(loggerName).FilterLevels(LogLevel.MinLevel, maxLevel).FinalRule();
            });
        }

        /// <summary>
        /// Updates the dictionary <see cref="GlobalDiagnosticsContext"/> ${gdc:item=} with the name-value-pair
        /// </summary>
        public static ConfigurationBuilder AddGlobalContextValue(this ConfigurationBuilder configurationBuilder, string name, string value)
        {
            GlobalDiagnosticsContext.Set(name, value);
            return configurationBuilder;
        }

        /// <summary>
        /// Enables the <see cref="InternalLogger.LogFile"/> 
        /// </summary>
        public static ConfigurationBuilder UseInternalLoggerFile(this ConfigurationBuilder configurationBuilder, LogLevel logLevel, string logFilePath)
        {
            InternalLogger.LogLevel = logLevel;
            InternalLogger.LogFile = logFilePath;
            return configurationBuilder;
        }

        /// <summary>
        /// Enables the <see cref="InternalLogger.LogWriter"/> 
        /// </summary>
        public static ConfigurationBuilder UseInternalLoggerWriter(this ConfigurationBuilder configurationBuilder, LogLevel logLevel, TextWriter textWriter)
        {
            InternalLogger.LogLevel = logLevel;
            InternalLogger.LogWriter = textWriter;
            return configurationBuilder;
        }

        /// <summary>
        /// Configures the <see cref="TimeSource.Current"/>. For better time precision use <see cref="FastUtcTimeSource"/>
        /// </summary>
        public static ConfigurationBuilder UseTimeSource(this ConfigurationBuilder configurationBuilder, TimeSource timeSource)
        {
            TimeSource.Current = timeSource ?? new FastUtcTimeSource();
            return configurationBuilder;
        }

        /// <summary>
        /// Register NLog configuration type instead of depending on the scanning of assemblies
        /// </summary>
        public static ConfigurationBuilder RegisterConfigType(this ConfigurationBuilder configurationBuilder, Type type, Func<Target> factory = null)
        {
#if NETSTANDARD1_0
            Assembly typeAssembly = type.GetTypeInfo().Assembly;
#elif !SILVERLIGHT && !__IOS__ && !__ANDROID__ && !MONO && !NETSTANDARD1_0
            Assembly typeAssembly = type.Assembly;
#else
            Assembly typeAssembly = null;
#endif
            if (configurationBuilder.VerifyUniqueInstance(typeAssembly))
            {
                ConfigurationItemFactory.Default.RegisterItemsFromAssembly(typeAssembly);
            }

            ConfigurationItemFactory.Default.RegisterType(type, string.Empty);

            if (factory != null)
            {
                var defaultFactory = ConfigurationItemFactory.Default.CreateInstance;
                ConfigurationItemFactory.Default.CreateInstance = targetType =>
                {
                    if (targetType == type)
                        return factory();
                    return defaultFactory(targetType);
                };
            }
            return configurationBuilder;
        }

        /// <summary>
        /// Read NLog <see cref="LoggingConfiguration"/> from XML-file
        /// </summary>
        public static LoggingConfiguration ReadXmlConfig(this ConfigurationBuilder configurationBuilder, string configFile = null)
        {
            if (!string.IsNullOrEmpty(configFile))
            {
                configFile = configurationBuilder.LogFactory.GetConfigFile(configFile);
                if (File.Exists(configFile))
                {
                    var xmlConfig = new XmlLoggingConfiguration(configFile, configurationBuilder.LogFactory);
                    return configurationBuilder.LoadConfiguration(xmlConfig);
                }
            }
            if (configurationBuilder.LogFactory.TryLoadFromFilePaths(out var loggingConfiguration))
            {
                return configurationBuilder.LoadConfiguration(loggingConfiguration);
            }
            return configurationBuilder.LoggingConfiguration;
        }

        private static SimpleLayout GenerateDefaultLayout() => new SimpleLayout("${longdate}|${level:uppercase=true}|${logger}|${message:withException=true:exceptionSeparator=|}");

        /// <summary>
        /// Write to <see cref="FileTarget"/>
        /// </summary>
        public static ConfigurationBuilderTargets WriteToFile(this ConfigurationBuilderTargets targetsBuilder, Layout fileName, Layout logEventLayout = null, bool keepFileOpen = true, bool concurrentWrites = false, bool? autoFlush = null, TimeSpan? openFileFlushTimeout = null, System.Text.Encoding encoding = null, int? maxArchiveFiles = null, int? archiveAboveSize = null)
        {
            var fileTarget = new FileTarget();
            fileTarget.FileName = fileName;
            fileTarget.KeepFileOpen = keepFileOpen;
            fileTarget.ConcurrentWrites = concurrentWrites;
            if (encoding != null && encoding.WebName != System.Text.Encoding.UTF8.WebName)
            {
                fileTarget.Encoding = encoding;
                if (encoding.GetPreamble().Length > 0)
                    fileTarget.WriteBom = true;
            }
            else
            {
                fileTarget.Encoding = System.Text.Encoding.UTF8;
            }
            if (!openFileFlushTimeout.HasValue || openFileFlushTimeout.Value > TimeSpan.FromMilliseconds(250))
            {
                if (autoFlush.HasValue)
                    fileTarget.AutoFlush = autoFlush.Value;
                if (openFileFlushTimeout.HasValue)
                    fileTarget.OpenFileFlushTimeout = Math.Max(1, (int)openFileFlushTimeout.Value.TotalSeconds);
            }
            if (maxArchiveFiles.HasValue)
                fileTarget.MaxArchiveFiles = maxArchiveFiles.Value;
            if (archiveAboveSize.HasValue)
                fileTarget.ArchiveAboveSize = archiveAboveSize.Value;
            if (logEventLayout != null)
                fileTarget.Layout = logEventLayout;
            else
                fileTarget.Layout = GenerateDefaultLayout();
            return targetsBuilder.WriteToTarget(fileTarget);
        }

#if !NETSTANDARD1_3
        /// <summary>
        /// Write to <see cref="ConsoleTarget"/>
        /// </summary>
        public static ConfigurationBuilderTargets WriteToConsole(this ConfigurationBuilderTargets targetsBuilder, Layout logEventLayout = null, bool? stdError = null)
        {
#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
            var consoleTarget = new ColoredConsoleTarget();
            consoleTarget.UseDefaultRowHighlightingRules = false;
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule("level == LogLevel.Trace", ConsoleOutputColor.DarkGray, ConsoleOutputColor.NoChange));
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule("level == LogLevel.Debug", ConsoleOutputColor.Gray, ConsoleOutputColor.NoChange));
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule("level == LogLevel.Info", ConsoleOutputColor.White, ConsoleOutputColor.NoChange));
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule("level == LogLevel.Warn", ConsoleOutputColor.Yellow, ConsoleOutputColor.NoChange));
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule("level == LogLevel.Error", ConsoleOutputColor.Red, ConsoleOutputColor.NoChange));
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule("level == LogLevel.Fatal", ConsoleOutputColor.White, ConsoleOutputColor.Red));
            if (stdError.HasValue)
                consoleTarget.ErrorStream = stdError.Value;
#else
            var consoleTarget = new ConsoleTarget();
            if (stdError.HasValue)
                consoleTarget.Error = stdError.Value;
#endif
            if (logEventLayout != null)
                consoleTarget.Layout = logEventLayout;
            else
                consoleTarget.Layout = GenerateDefaultLayout();
            return targetsBuilder.WriteToTarget(consoleTarget);
        }
#endif

#if !SILVERLIGHT && !NETSTANDARD1_3
        /// <summary>
        /// Write to System.Diagnostic <see cref="TraceTarget"/>
        /// </summary>
        public static ConfigurationBuilderTargets WriteToTrace(this ConfigurationBuilderTargets targetsBuilder, Layout logEventLayout = null, bool rawWrite = true)
        {
            var traceTarget = new TraceTarget();
            traceTarget.RawWrite = rawWrite;
            if (logEventLayout != null)
                traceTarget.Layout = logEventLayout;
            else
                traceTarget.Layout = GenerateDefaultLayout();
            return targetsBuilder.WriteToTarget(traceTarget);
        }
#endif

        /// <summary>
        /// Write to <see cref="OutputDebugStringTarget"/>
        /// </summary>
        public static ConfigurationBuilderTargets WriteToDebug(this ConfigurationBuilderTargets targetsBuilder, Layout logEventLayout = null)
        {
            var debugTarget = new OutputDebugStringTarget();
            if (logEventLayout != null)
                debugTarget.Layout = logEventLayout;
            else
                debugTarget.Layout = GenerateDefaultLayout();
            return targetsBuilder.WriteToTarget(debugTarget);
        }

        /// <summary>
        /// Write to <see cref="MethodCallTarget"/>
        /// </summary>
        public static ConfigurationBuilderTargets WriteToMethod(this ConfigurationBuilderTargets targetsBuilder, Action<LogEventInfo, object[]> logEventAction, params Layout[] layouts)
        {
            var methodTarget = new MethodCallTarget(string.Empty, logEventAction);
            if (layouts?.Length > 0)
            {
                foreach (var layout in layouts)
                    methodTarget.Parameters.Add(new MethodCallParameter(layout));
            }
            return targetsBuilder.WriteToTarget(methodTarget);
        }

        /// <summary>
        /// Async writing to target. See also <see cref="AsyncTargetWrapper"/>
        /// </summary>
        public static ConfigurationBuilderTargets WriteToWithAsync(this ConfigurationBuilderTargets targetsBuilder, Action<ConfigurationBuilderTargets> buildAction, AsyncTargetWrapperOverflowAction? overflowAction = null, int? queueLimit = null, int? batchSize = null, int? timeToSleepBetweenBatches = null)
        {
            return targetsBuilder.WriteToWrapper(() =>
            {
                var wrapper = new AsyncTargetWrapper();
                if (overflowAction.HasValue)
                    wrapper.OverflowAction = overflowAction.Value;
                if (queueLimit.HasValue)
                    wrapper.QueueLimit = queueLimit.Value;
                if (batchSize.HasValue)
                    wrapper.BatchSize = batchSize.Value;
                if (timeToSleepBetweenBatches.HasValue)
                    wrapper.TimeToSleepBetweenBatches = timeToSleepBetweenBatches.Value;
                return wrapper;
            }, buildAction);
        }

        /// <summary>
        /// Buffering before writing to target. See also <see cref="BufferingTargetWrapper"/>
        /// </summary>
        public static ConfigurationBuilderTargets WriteToWithBuffering(this ConfigurationBuilderTargets targetsBuilder, Action<ConfigurationBuilderTargets> buildAction, int? bufferSize = null, int? flushTimeout = null, bool? slidingTimeout = null, BufferingTargetWrapperOverflowAction? overflowAction = null)
        {
            return targetsBuilder.WriteToWrapper(() =>
            {
                var wrapper = new BufferingTargetWrapper();
                if (bufferSize.HasValue)
                    wrapper.BufferSize = bufferSize.Value;
                if (flushTimeout.HasValue)
                    wrapper.FlushTimeout = flushTimeout.Value;
                if (slidingTimeout.HasValue)
                    wrapper.SlidingTimeout = slidingTimeout.Value;
                if (overflowAction.HasValue)
                    wrapper.OverflowAction = overflowAction.Value;
                return wrapper;
            }, buildAction);
        }

        /// <summary>
        /// Retry when target write fails. See also <see cref="RetryingTargetWrapper"/>
        /// </summary>
        public static ConfigurationBuilderTargets WriteToWithRetry(this ConfigurationBuilderTargets targetsBuilder, Action<ConfigurationBuilderTargets> buildAction, int? retryCount = null, int? retryDelayMilliseconds = null)
        {
            return targetsBuilder.WriteToWrapper(() =>
            {
                var wrapper = new RetryingTargetWrapper();
                if (retryCount.HasValue)
                    wrapper.RetryCount = retryCount.Value;
                if (retryDelayMilliseconds.HasValue)
                    wrapper.RetryDelayMilliseconds = retryDelayMilliseconds.Value;
                return wrapper;
            }, buildAction);
        }

        /// <summary>
        /// Fallback when target write fails. See also <see cref="FallbackGroupTarget"/>
        /// </summary>
        public static ConfigurationBuilderTargets WriteToWithFallback(this ConfigurationBuilderTargets targetsBuilder, Action<ConfigurationBuilderTargets> buildAction, bool? returnToFirstOnSuccess = null)
        {
            return targetsBuilder.WriteToCompound(() =>
            {
                var compound = new FallbackGroupTarget();
                if (returnToFirstOnSuccess.HasValue)
                    compound.ReturnToFirstOnSuccess = returnToFirstOnSuccess.Value;
                return compound;
            }, buildAction);
        }

        /// <summary>
        /// Apply extra Target handling using provided specified <see cref="WrapperTargetBase"/>
        /// </summary>
        internal static ConfigurationBuilderTargets WriteToWrapper<T>(this ConfigurationBuilderTargets targetsBuilder, Func<T> wrapperTargetFactory, Action<ConfigurationBuilderTargets> buildAction) where T : WrapperTargetBase
        {
            var localTargetBuilder = new ConfigurationBuilderRuleTargets(targetsBuilder.ConfigurationBuilder);
            buildAction(localTargetBuilder);
            var targets = localTargetBuilder.Build();
            foreach (var target in targets)
            {
                var wrapperTarget = wrapperTargetFactory();
                var targetName = wrapperTarget.Name;
                if (string.IsNullOrEmpty(targetName))
                {
                    targetName = targetsBuilder.ConfigurationBuilder.GenerateTargetName(wrapperTarget.GetType());
                    targetName = string.Concat(targetName, "_", target.Name);
                    targetName = targetsBuilder.ConfigurationBuilder.EnsureUniqueTargetName(targetName);
                    wrapperTarget.Name = targetName;
                }
                wrapperTarget.WrappedTarget = target;
                targetsBuilder.WriteToTarget(wrapperTarget);
            }

            return targetsBuilder;
        }

        /// <summary>
        /// Apply extra Target handling using provided specified <see cref="CompoundTargetBase"/>
        /// </summary>
        internal static ConfigurationBuilderTargets WriteToCompound<T>(this ConfigurationBuilderTargets targetsBuilder, Func<T> compoundTargetFactory, Action<ConfigurationBuilderTargets> buildAction) where T : CompoundTargetBase
        {
            var localTargetBuilder = new ConfigurationBuilderRuleTargets(targetsBuilder.ConfigurationBuilder);
            buildAction(localTargetBuilder);
            var targets = localTargetBuilder.Build();
            if (targets.Count > 0)
            {
                var compoundTarget = compoundTargetFactory();
                var targetName = compoundTarget.Name;
                if (string.IsNullOrEmpty(targetName))
                {
                    targetName = targetsBuilder.ConfigurationBuilder.GenerateTargetName(compoundTarget.GetType());
                    targetName = string.Concat(targetName, "_", targets[0].Name);
                    targetName = targetsBuilder.ConfigurationBuilder.EnsureUniqueTargetName(targetName);
                    compoundTarget.Name = targetName;
                }

                foreach (var target in targets)
                    compoundTarget.Targets.Add(target);
                targetsBuilder.WriteToTarget(compoundTarget);
            }
            return targetsBuilder;
        }
    }

}

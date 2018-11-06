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
    using System.Collections.Generic;
    using System.Reflection;
    using NLog.Common;
    using NLog.Config;
    using NLog.Filters;
    using NLog.Internal;
    using NLog.Targets;

    /// <summary>
    /// Controls <see cref="LoggingConfiguration"/> configuration
    /// </summary>
    public sealed class ConfigurationBuilder
    {
        private readonly List<LoggingRule> _loggingRules = new List<LoggingRule>();
        private LoggingConfiguration _loggingConfiguration;

        private readonly HashSet<string> _targetNames = new HashSet<string>();
        struct UniqueReference : IEquatable<UniqueReference>
        {
            private readonly object _uniqueInstance;

            public UniqueReference(object uniqueInstance)
            {
                _uniqueInstance = uniqueInstance;
            }

            public bool Equals(UniqueReference other)
            {
                return ReferenceEquals(_uniqueInstance, other._uniqueInstance);
            }

            public override bool Equals(object obj)
            {
                return obj is UniqueReference && Equals((UniqueReference)obj);
            }

            public override int GetHashCode()
            {
                return _uniqueInstance.GetHashCode();
            }
        }
        private readonly HashSet<UniqueReference> _uniqueInstances = new HashSet<UniqueReference>();

        /// <summary>
        /// Gets the <see cref="NLog.LogFactory" /> instance used for building configuration
        /// </summary>
        public LogFactory LogFactory { get; }

        internal LoggingConfiguration LoggingConfiguration
        {
            get { return _loggingConfiguration; }
            set { _loggingConfiguration = value; if (value != null && value.LoggingRules.Count > 0) _loggingRules.Clear(); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationBuilder"/> class.
        /// </summary>
        internal ConfigurationBuilder(LogFactory logFactory, LoggingConfiguration loggingConfiguration)
        {
            if (loggingConfiguration == null && !InternalLogger.HasActiveLoggers())
            {
                InternalLogger.LogLevel = LogLevel.Off;
            }
            LogFactory = logFactory;
            _loggingConfiguration = loggingConfiguration;
        }

        /// <summary>
        /// Configures a new <see cref="LoggingRule" /> instance, with one or more <see cref="Target"/> instances
        /// </summary>
        public ConfigurationBuilder FilterRule(Action<ConfigurationBuilderFilterRule> buildAction)
        {
            var loggingRule = new LoggingRule();
            loggingRule.LoggerNamePattern = "*";
            var ruleBuilder = new ConfigurationBuilderFilterRule(this, loggingRule);
            buildAction(ruleBuilder);
            loggingRule = ruleBuilder.Build();
            if (loggingRule != null)
            {
                bool levelsConfigured = false;
                for (int i = LogLevel.MinLevel.Ordinal; i <= LogLevel.MaxLevel.Ordinal; ++i)
                {
                    if (loggingRule.IsLoggingEnabledForLevel(LogLevel.FromOrdinal(i)))
                    {
                        levelsConfigured = true;
                        break;
                    }
                }
                if (!levelsConfigured)
                    loggingRule.EnableLoggingForLevels(LogLevel.MinLevel, LogLevel.MaxLevel);
                _loggingRules.Add(loggingRule);
            }
            return this;
        }

        /// <summary>
        /// Loads <see cref="LoggingConfiguration"/> into ConfigurationBuilder
        /// </summary>
        public LoggingConfiguration LoadConfiguration(LoggingConfiguration loggingConfiguration)
        {
            if (loggingConfiguration?.LoggingRules?.Count > 0)
            {
                // Valid configuration loaded, lets clear the existing one
                _loggingConfiguration = loggingConfiguration;
                _loggingRules.Clear();
            }
            return _loggingConfiguration;
        }

        internal LoggingConfiguration Build()
        {
            if (_loggingRules.Count == 0)
                return _loggingConfiguration;

            LoggingConfiguration loggingConfiguration = new LoggingConfiguration(LogFactory);
            foreach (var loggingRule in _loggingRules)
            {
                foreach (var target in loggingRule.Targets)
                    loggingConfiguration.AddTarget(target);
                loggingConfiguration.LoggingRules.Add(loggingRule);
            }
            return loggingConfiguration;
        }

        internal string GenerateTargetName(Type targetType)
        {
            var targetName = targetType.GetCustomAttribute<TargetAttribute>()?.Name ?? string.Empty;
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

        internal string EnsureUniqueTargetName(string targetName)
        {
            var newTargetName = targetName;
            int targetIndex = 0;
            while (_targetNames.Contains(newTargetName))
            {
                newTargetName = string.Concat(targetName, "_", (++targetIndex).ToString());
            }
            return newTargetName;
        }

        internal bool VerifyUniqueInstance(object newInstance)
        {
            if (newInstance == null)
                return false;

            if (_uniqueInstances.Contains(new UniqueReference(newInstance)))
                return false;

            _uniqueInstances.Add(new UniqueReference(newInstance));
            return true;
        }
    }

    /// <summary>
    /// Controls <see cref="Target"/> configuration
    /// </summary>
    public abstract class ConfigurationBuilderTargets
    {
        internal ConfigurationBuilder ConfigurationBuilder { get; }
        private readonly List<Target> _targets = new List<Target>();

#pragma warning disable S3442 // "abstract" classes should not have "public" constructors
        internal ConfigurationBuilderTargets(ConfigurationBuilder configBuilder)
#pragma warning restore S3442 // "abstract" classes should not have "public" constructors
        {
            ConfigurationBuilder = configBuilder;
        }

        /// <summary>
        /// Write LogEvents to the specified <see cref="Target"/>
        /// </summary>
        public virtual ConfigurationBuilderTargets WriteToTarget(Target target)
        {
            if (ConfigurationBuilder.VerifyUniqueInstance(target))
            {
                if (target is TargetWithLayout targetWithLayout)
                {
                    if (!ConfigurationBuilder.VerifyUniqueInstance(targetWithLayout.Layout))
                        throw new NLogConfigurationException($"NLog Target {target} is not allowed to share Layout with others");
                }
            }
                
            if (string.IsNullOrEmpty(target.Name))
            {
                var targetName = ConfigurationBuilder.GenerateTargetName(target.GetType());
                target.Name = ConfigurationBuilder.EnsureUniqueTargetName(targetName);
            }

            _targets.Add(target);
            return this;
        }

        internal List<Target> Build()
        {
            return _targets;
        }
    }

    internal class ConfigurationBuilderRuleTargets : ConfigurationBuilderTargets
    {
        public ConfigurationBuilderRuleTargets(ConfigurationBuilder configBuilder)
            :base(configBuilder)
        {
        }
    }

    /// <summary>
    /// Controls <see cref="LoggingRule"/> configuration
    /// </summary>
    public sealed class ConfigurationBuilderFilterRule : ConfigurationBuilderTargets
    {
        private readonly LoggingRule _loggingRule;
        private readonly ConfigurationBuilderTargets _targetBuilder;

        internal ConfigurationBuilderFilterRule(ConfigurationBuilder configBuilder, LoggingRule loggingRule)
            :base(configBuilder)
        {
            _loggingRule = loggingRule;
            _targetBuilder = new ConfigurationBuilderRuleTargets(configBuilder);
        }

        internal new LoggingRule Build()
        {
            var targets = _targetBuilder.Build();
            foreach (var target in targets)
                _loggingRule.Targets.Add(target);
            return _loggingRule;
        }

        /// <summary>
        /// Configure <see cref="LoggingRule.Levels"/> to react to specified <see cref="LogLevel"/> (only)
        /// </summary>
        public ConfigurationBuilderFilterRule FilterLevel(LogLevel logLevel)
        {
            _loggingRule.SetLoggingLevels(logLevel, logLevel);
            return this;
        }

        /// <summary>
        /// Configure <see cref="LoggingRule.Levels"/> to react to specified <see cref="LogLevel"/> (and above)
        /// </summary>
        public ConfigurationBuilderFilterRule FilterMinLevel(LogLevel logLevel)
        {
            _loggingRule.SetLoggingLevels(logLevel, LogLevel.MaxLevel);
            return this;
        }

        /// <summary>
        /// Configure <see cref="LoggingRule.Levels"/> to react to specified <see cref="LogLevel"/> interval
        /// </summary>
        public ConfigurationBuilderFilterRule FilterLevels(LogLevel minLevel, LogLevel maxLevel)
        {
            _loggingRule.SetLoggingLevels(minLevel, maxLevel);
            return this;
        }

        /// <summary>
        /// Configure <see cref="LoggingRule.LoggerNamePattern"/> to specified logger-wildcard
        /// </summary>
        public ConfigurationBuilderFilterRule FilterLogger(string loggerNamePattern)
        {
            _loggingRule.LoggerNamePattern = loggerNamePattern;
            return this;
        }

        /// <summary>
        /// Configure <see cref="LoggingRule.Filters"/> to  specified logger-wildcard
        /// </summary>
        public ConfigurationBuilderFilterRule FilterMethod(Func<LogEventInfo, bool> applyFilter, FilterResult filterResult = FilterResult.Log)
        {
            _loggingRule.Filters.Add(new WhenMethodFilter(applyFilter) { Action = filterResult });
            return this;
        }

        /// <summary>
        /// Configure <see cref="LoggingRule.DefaultFilterResult"/>, default action if all filters won't match
        /// </summary>
        public ConfigurationBuilderFilterRule FilterMethodDefaultResult(FilterResult filterResult)
        {
            _loggingRule.DefaultFilterResult = filterResult;
            return this;
        }

        /// <summary>
        /// Configure <see cref="LoggingRule.Final"/>, so LogEvents matching this LoggingRule will not match other rules
        /// </summary>
        public ConfigurationBuilderFilterRule FinalRule()
        {
            _loggingRule.Final = true;
            return this;
        }

        /// <inheritdoc/>
        public override ConfigurationBuilderTargets WriteToTarget(Target target)
        {
            _targetBuilder.WriteToTarget(target);
            return _targetBuilder;
        }
    }

}

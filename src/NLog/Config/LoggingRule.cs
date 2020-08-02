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

namespace NLog.Config
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using NLog.Filters;
    using NLog.Targets;

    /// <summary>
    /// Represents a logging rule. An equivalent of &lt;logger /&gt; configuration element.
    /// </summary>
    [NLogConfigurationItem]
    public class LoggingRule
    {
        private ILoggingRuleLevelFilter _logLevelFilter = LoggingRuleLevelFilter.Off;
        private LoggerNameMatcher _loggerNameMatcher = LoggerNameMatcher.Create(null);

        /// <summary>
        /// Create an empty <see cref="LoggingRule" />.
        /// </summary>
        public LoggingRule()
            :this(null)
        {
        }

        /// <summary>
        /// Create an empty <see cref="LoggingRule" />.
        /// </summary>
        public LoggingRule(string ruleName)
        {
            RuleName = ruleName;
            Filters = new List<Filter>();
            ChildRules = new List<LoggingRule>();
            Targets = new List<Target>();
        }

        /// <summary>
        /// Create a new <see cref="LoggingRule" /> with a <paramref name="minLevel"/> and  <paramref name="maxLevel"/> which writes to <paramref name="target"/>.
        /// </summary>
        /// <param name="loggerNamePattern">Logger name pattern used for <see cref="LoggerNamePattern"/>. It may include one or more '*' or '?' wildcards at any position.</param>
        /// <param name="minLevel">Minimum log level needed to trigger this rule.</param>
        /// <param name="maxLevel">Maximum log level needed to trigger this rule.</param>
        /// <param name="target">Target to be written to when the rule matches.</param>
        public LoggingRule(string loggerNamePattern, LogLevel minLevel, LogLevel maxLevel, Target target)
            : this()
        {
            LoggerNamePattern = loggerNamePattern;
            Targets.Add(target);
            EnableLoggingForLevels(minLevel, maxLevel);
        }

        /// <summary>
        /// Create a new <see cref="LoggingRule" /> with a <paramref name="minLevel"/> which writes to <paramref name="target"/>.
        /// </summary>
        /// <param name="loggerNamePattern">Logger name pattern used for <see cref="LoggerNamePattern"/>. It may include one or more '*' or '?' wildcards at any position.</param>
        /// <param name="minLevel">Minimum log level needed to trigger this rule.</param>
        /// <param name="target">Target to be written to when the rule matches.</param>
        public LoggingRule(string loggerNamePattern, LogLevel minLevel, Target target)
            : this()
        {
            LoggerNamePattern = loggerNamePattern;
            Targets.Add(target);
            EnableLoggingForLevels(minLevel, LogLevel.MaxLevel);
        }

        /// <summary>
        /// Create a (disabled) <see cref="LoggingRule" />. You should call <see cref="EnableLoggingForLevel"/> or see cref="EnableLoggingForLevels"/> to enable logging.
        /// </summary>
        /// <param name="loggerNamePattern">Logger name pattern used for <see cref="LoggerNamePattern"/>. It may include one or more '*' or '?' wildcards at any position.</param>
        /// <param name="target">Target to be written to when the rule matches.</param>
        public LoggingRule(string loggerNamePattern, Target target)
            : this()
        {
            LoggerNamePattern = loggerNamePattern;
            Targets.Add(target);
        }

        /// <summary>
        /// Rule identifier to allow rule lookup
        /// </summary>
        public string RuleName { get; set; }

        /// <summary>
        /// Gets a collection of targets that should be written to when this rule matches.
        /// </summary>
        public IList<Target> Targets { get; }

        /// <summary>
        /// Gets a collection of child rules to be evaluated when this rule matches.
        /// </summary>
        public IList<LoggingRule> ChildRules { get; }

        internal List<LoggingRule> GetChildRulesThreadSafe() { lock (ChildRules) return ChildRules.ToList(); }
        internal List<Target> GetTargetsThreadSafe() { lock (Targets) return Targets.ToList(); }
        internal bool RemoveTargetThreadSafe(Target target) { lock (Targets) return Targets.Remove(target); }

        /// <summary>
        /// Gets a collection of filters to be checked before writing to targets.
        /// </summary>
        public IList<Filter> Filters { get; }

        /// <summary>
        /// Gets or sets a value indicating whether to quit processing any further rule when this one matches.
        /// </summary>
        public bool Final { get; set; }

        /// <summary>
        /// Gets or sets logger name pattern.
        /// </summary>
        /// <remarks>
        /// Logger name pattern used by <see cref="NameMatches(string)"/> to check if a logger name matches this rule.
        /// It may include one or more '*' or '?' wildcards at any position.
        /// <list type="bullet">
        /// <item>'*' means zero or more occurrences of any character</item>
        /// <item>'?' means exactly one occurrence of any character</item>
        /// </list>
        /// </remarks>
        public string LoggerNamePattern
        {
            get => _loggerNameMatcher.Pattern;

            set
            {
                _loggerNameMatcher = LoggerNameMatcher.Create(value);
            }
        }

        /// <summary>
        /// Gets the collection of log levels enabled by this rule.
        /// </summary>
        [NLogConfigurationIgnoreProperty]
        public ReadOnlyCollection<LogLevel> Levels
        {
            get
            {
                var enabledLogLevels = new List<LogLevel>();
                var currentLogLevels = _logLevelFilter.LogLevels;
                for (int i = LogLevel.MinLevel.Ordinal; i <= LogLevel.MaxLevel.Ordinal; ++i)
                {
                    if (currentLogLevels[i])
                    {
                        enabledLogLevels.Add(LogLevel.FromOrdinal(i));
                    }
                }

                return enabledLogLevels.AsReadOnly();
            }
        }

        /// <summary>
        /// Default action if none of the filters match
        /// </summary>
        public FilterResult DefaultFilterResult { get; set; } = FilterResult.Ignore;

        /// <summary>
        /// Enables logging for a particular level.
        /// </summary>
        /// <param name="level">Level to be enabled.</param>
        public void EnableLoggingForLevel(LogLevel level)
        {
            if (level == LogLevel.Off)
            {
                return;
            }

            _logLevelFilter = _logLevelFilter.GetSimpleFilterForUpdate().SetLoggingLevels(level, level, true);
        }

        /// <summary>
        /// Enables logging for a particular levels between (included) <paramref name="minLevel"/> and <paramref name="maxLevel"/>.
        /// </summary>
        /// <param name="minLevel">Minimum log level needed to trigger this rule.</param>
        /// <param name="maxLevel">Maximum log level needed to trigger this rule.</param>
        public void EnableLoggingForLevels(LogLevel minLevel, LogLevel maxLevel)
        {
            _logLevelFilter = _logLevelFilter.GetSimpleFilterForUpdate().SetLoggingLevels(minLevel, maxLevel, true);
        }

        internal void EnableLoggingForLevels(NLog.Layouts.SimpleLayout simpleLayout)
        {
            _logLevelFilter = new DynamicLogLevelFilter(this, simpleLayout);
        }

        internal void EnableLoggingForRange(Layouts.SimpleLayout minLevel, Layouts.SimpleLayout maxLevel)
        {
            _logLevelFilter = new DynamicRangeLevelFilter(this, minLevel, maxLevel);
        }

        /// <summary>
        /// Disables logging for a particular level.
        /// </summary>
        /// <param name="level">Level to be disabled.</param>
        public void DisableLoggingForLevel(LogLevel level)
        {
            if (level == LogLevel.Off)
            {
                return;
            }

            _logLevelFilter = _logLevelFilter.GetSimpleFilterForUpdate().SetLoggingLevels(level, level, false);
        }

        /// <summary>
        /// Disables logging for particular levels between (included) <paramref name="minLevel"/> and <paramref name="maxLevel"/>.
        /// </summary>
        /// <param name="minLevel">Minimum log level to be disables.</param>
        /// <param name="maxLevel">Maximum log level to de disabled.</param>
        public void DisableLoggingForLevels(LogLevel minLevel, LogLevel maxLevel)
        {
            _logLevelFilter = _logLevelFilter.GetSimpleFilterForUpdate().SetLoggingLevels(minLevel, maxLevel, false);
        }

        /// <summary>
        /// Enables logging the levels between (included) <paramref name="minLevel"/> and <paramref name="maxLevel"/>. All the other levels will be disabled.
        /// </summary>
        /// <param name="minLevel">>Minimum log level needed to trigger this rule.</param>
        /// <param name="maxLevel">Maximum log level needed to trigger this rule.</param>
        public void SetLoggingLevels(LogLevel minLevel, LogLevel maxLevel)
        {
            _logLevelFilter = _logLevelFilter.GetSimpleFilterForUpdate().SetLoggingLevels(LogLevel.MinLevel, LogLevel.MaxLevel, false).SetLoggingLevels(minLevel, maxLevel, true);
        }

        /// <summary>
        /// Returns a string representation of <see cref="LoggingRule"/>. Used for debugging.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(_loggerNameMatcher.ToString());
            sb.Append(" levels: [ ");

            var currentLogLevels = _logLevelFilter.LogLevels;
            for (int i = 0; i < currentLogLevels.Length; ++i)
            {
                if (currentLogLevels[i])
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture, "{0} ", LogLevel.FromOrdinal(i).ToString());
                }
            }

            sb.Append("] appendTo: [ ");
            foreach (Target app in GetTargetsThreadSafe())
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0} ", app.Name);
            }

            sb.Append("]");
            return sb.ToString();
        }

        /// <summary>
        /// Checks whether te particular log level is enabled for this rule.
        /// </summary>
        /// <param name="level">Level to be checked.</param>
        /// <returns>A value of <see langword="true"/> when the log level is enabled, <see langword="false" /> otherwise.</returns>
        public bool IsLoggingEnabledForLevel(LogLevel level)
        {
            if (level == LogLevel.Off)
            {
                return false;
            }

            var currentLogLevels = _logLevelFilter.LogLevels;
            return currentLogLevels[level.Ordinal];
        }

        /// <summary>
        /// Checks whether given name matches the <see cref="LoggerNamePattern"/>.
        /// </summary>
        /// <param name="loggerName">String to be matched.</param>
        /// <returns>A value of <see langword="true"/> when the name matches, <see langword="false" /> otherwise.</returns>
        public bool NameMatches(string loggerName)
        {
            return _loggerNameMatcher.NameMatches(loggerName);
        }
    }
}
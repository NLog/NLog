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

namespace NLog.Config
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using NLog.Filters;
    using NLog.Targets;

    /// <summary>
    /// Represents a logging rule. An equivalent of &lt;logger /&gt; configuration element.
    /// </summary>
    [NLogConfigurationItem]
    public class LoggingRule
    {
        private readonly bool[] _logLevels = new bool[LogLevel.MaxLevel.Ordinal + 1];

        private string _loggerNamePattern;
        private LoggerNameMatcher _loggerNameMatcher = new LoggerNameMatcher.None();

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
        /// <param name="loggerNamePattern">Logger name pattern. It may include one or more '*' or '?' wildcards at any position.</param>
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
        /// <param name="loggerNamePattern">Logger name pattern. It may include one or more '*' or '?' wildcards at any position.</param>
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
        /// <param name="loggerNamePattern">Logger name pattern. It may include one or more '*' or '?' wildcards at any position.</param>
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
        public string RuleName { get; }

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
        /// Logger name pattern. 
        /// It may include one or more '*' or '?' wildcards at any position.
        ///  - '*' means zero or more occurrecnces of any character
        ///  - '?' means exactly one occurrence of any character
        /// </remarks>
        public string LoggerNamePattern
        {
            get => _loggerNamePattern;

            set
            {
                _loggerNamePattern = value;
                if(_loggerNamePattern == null)
                {
                    _loggerNameMatcher = new LoggerNameMatcher.None();
                    return;
                }

                int starPos1 = _loggerNamePattern.IndexOf('*');
                int starPos2 = _loggerNamePattern.IndexOf('*', starPos1 + 1);
                int questionPos = _loggerNamePattern.IndexOf('?');
                if (starPos1 < 0 && questionPos < 0)
                {
                    _loggerNameMatcher = new LoggerNameMatcher.Equals(_loggerNamePattern);
                    return;
                }
                if(_loggerNamePattern == "*")
                {
                    _loggerNameMatcher = new LoggerNameMatcher.All();
                    return;
                }
                if(questionPos < 0)
                {
                    if(starPos1 == 0 && starPos2 == _loggerNamePattern.Length-1)
                    {
                        _loggerNameMatcher = new LoggerNameMatcher.Contains(_loggerNamePattern.Substring(1, _loggerNamePattern.Length - 2));
                        return;
                    }
                    if(starPos2<0)
                    {
                        if(starPos1 == 0)
                        {
                            _loggerNameMatcher = new LoggerNameMatcher.EndsWith(_loggerNamePattern.Substring(1));
                            return;
                        }
                        if (starPos1 == _loggerNamePattern.Length - 1)
                        {
                            _loggerNameMatcher = new LoggerNameMatcher.StartsWith(_loggerNamePattern.Substring(0, _loggerNamePattern.Length - 1));
                            return;
                        }
                    }
                }
                _loggerNameMatcher = new LoggerNameMatcher.MultiplePattern(_loggerNamePattern);
            }
        }

        /// <summary>
        /// Gets the collection of log levels enabled by this rule.
        /// </summary>
        public ReadOnlyCollection<LogLevel> Levels
        {
            get
            {
                var levels = new List<LogLevel>();

                for (int i = LogLevel.MinLevel.Ordinal; i <= LogLevel.MaxLevel.Ordinal; ++i)
                {
                    if (_logLevels[i])
                    {
                        levels.Add(LogLevel.FromOrdinal(i));
                    }
                }

                return levels.AsReadOnly();
            }
        }
        /// <summary>
        /// Default action if all filters won't match
        /// </summary>
        public FilterResult DefaultFilterResult { get; set; } = FilterResult.Neutral;

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

            _logLevels[level.Ordinal] = true;
        }

        /// <summary>
        /// Enables logging for a particular levels between (included) <paramref name="minLevel"/> and <paramref name="maxLevel"/>.
        /// </summary>
        /// <param name="minLevel">Minimum log level needed to trigger this rule.</param>
        /// <param name="maxLevel">Maximum log level needed to trigger this rule.</param>
        public void EnableLoggingForLevels(LogLevel minLevel, LogLevel maxLevel)
        {
            for (int i = minLevel.Ordinal; i <= maxLevel.Ordinal; ++i)
            {
                EnableLoggingForLevel(LogLevel.FromOrdinal(i));
            }
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

            _logLevels[level.Ordinal] = false;
        }

        /// <summary>
        /// Disables logging for particular levels between (included) <paramref name="minLevel"/> and <paramref name="maxLevel"/>.
        /// </summary>
        /// <param name="minLevel">Minimum log level to be disables.</param>
        /// <param name="maxLevel">Maximum log level to de disabled.</param>
        public void DisableLoggingForLevels(LogLevel minLevel, LogLevel maxLevel)
        {
            for (int i = minLevel.Ordinal; i <= maxLevel.Ordinal; i++)
            {
                DisableLoggingForLevel(LogLevel.FromOrdinal(i));
            }
        }

        /// <summary>
        /// Enables logging the levels between (included) <paramref name="minLevel"/> and <paramref name="maxLevel"/>. All the other levels will be disabled.
        /// </summary>
        /// <param name="minLevel">>Minimum log level needed to trigger this rule.</param>
        /// <param name="maxLevel">Maximum log level needed to trigger this rule.</param>
        public void SetLoggingLevels(LogLevel minLevel, LogLevel maxLevel)
        {
            DisableLoggingForLevels(LogLevel.MinLevel, LogLevel.MaxLevel);
            EnableLoggingForLevels(minLevel, maxLevel);
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
            for (int i = 0; i < _logLevels.Length; ++i)
            {
                if (_logLevels[i])
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

            return _logLevels[level.Ordinal];
        }

        /// <summary>
        /// Checks whether given name matches the logger name pattern.
        /// </summary>
        /// <param name="loggerName">String to be matched.</param>
        /// <returns>A value of <see langword="true"/> when the name matches, <see langword="false" /> otherwise.</returns>
        public bool NameMatches(string loggerName)
        {
            return _loggerNameMatcher.NameMatches(loggerName);
        }

        abstract class LoggerNameMatcher
        {
            protected readonly string _pattern;
            private readonly string _toString;
            public LoggerNameMatcher(string pattern)
            {
                _pattern = pattern;
                _toString = "logNamePattern: (" + pattern + ":" + GetType().Name + ")";
            }
            public override string ToString()
            {
                return _toString;
            }
            public abstract bool NameMatches(string loggerName);
            internal class None : LoggerNameMatcher
            {
                public None() : base(null)
                {

                }
                public override bool NameMatches(string loggerName)
                {
                    return false;
                }
            }
            internal class All : LoggerNameMatcher
            {
                public All() : base(null) { }
                public override bool NameMatches(string loggerName)
                {
                    return true;
                }
            }
            internal new class Equals : LoggerNameMatcher
            {
                public Equals(string pattern) : base(pattern) { }
                public override bool NameMatches(string loggerName)
                {
                    return loggerName.Equals(_pattern, StringComparison.Ordinal);
                }
            }
            internal class StartsWith : LoggerNameMatcher
            {
                public StartsWith(string pattern) : base(pattern) { }
                public override bool NameMatches(string loggerName)
                {
                    return loggerName.StartsWith(_pattern, StringComparison.Ordinal);
                }
            }
            internal class EndsWith : LoggerNameMatcher
            {
                public EndsWith(string pattern) : base(pattern) { }
                public override bool NameMatches(string loggerName)
                {
                    return loggerName.EndsWith(_pattern, StringComparison.Ordinal);
                }
            }
            internal class Contains : LoggerNameMatcher
            {
                public Contains(string pattern) : base(pattern) { }
                public override bool NameMatches(string loggerName)
                {
                    return loggerName.IndexOf(_pattern, StringComparison.Ordinal) >= 0;
                }
            }
            internal class MultiplePattern : LoggerNameMatcher
            {
                private readonly Regex _regex;
                private static string getRegex(string wildcardsPattern)
                {
                    return 
                        '^' +
                        Regex.Escape(wildcardsPattern)
                            .Replace("\\*", ".*")
                            .Replace("\\?", ".")
                        + '$';
                }
                public MultiplePattern(string pattern) : base(getRegex(pattern))
                {
                    _regex = new Regex(_pattern, RegexOptions.CultureInvariant);
                }
                public override bool NameMatches(string loggerName)
                {
                    return _regex.IsMatch(loggerName);
                }
            }
            internal class RegexPattern : LoggerNameMatcher
            {
                private readonly Regex _regex;
                public RegexPattern(string pattern) : base(pattern)
                {
                    _regex = new Regex(_pattern, RegexOptions.CultureInvariant);
                }
                public override bool NameMatches(string loggerName)
                {
                    return _regex.IsMatch(loggerName);
                }
            }
        }
    }
}

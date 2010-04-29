// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Globalization;

using NLog;
using NLog.Targets;
using NLog.Filters;

namespace NLog.Config
{
    /// <summary>
    /// Represents a logging rule. An equivalent of &lt;logger /&gt; configuration element.
    /// </summary>
    public class LoggingRule
    {
        internal enum MatchMode
        {
            All, None, Equals, StartsWith, EndsWith, Contains, 
        } 

        private string _loggerNamePattern;
        private MatchMode _loggerNameMatchMode;
        private string _loggerNameMatchArgument;

        private bool[]_logLevels = new bool[LogLevel.MaxLevel.Ordinal + 1];
        private TargetCollection _targets = new TargetCollection();
        private FilterCollection _filters = new FilterCollection();
        private LoggingRuleCollection _childRules = new LoggingRuleCollection();
        private bool _final = false;

        /// <summary>
        /// Returns a string representation of <see cref="LoggingRule"/>. Used for debugging.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(CultureInfo.InvariantCulture, "logNamePattern: ({0}:{1})", _loggerNameMatchArgument, _loggerNameMatchMode);
            sb.Append(" levels: [ ");
            for (int i = 0; i < _logLevels.Length; ++i)
            {
                if (_logLevels[0])
                    sb.AppendFormat(CultureInfo.InvariantCulture, "{0} ", LogLevel.FromOrdinal(i).ToString());
            }
            sb.Append("] appendTo: [ ");
            foreach (Target app in _targets)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0} ", app.Name);
            }
            sb.Append("]");
            return sb.ToString();
        }

        /// <summary>
        /// A collection of targets that should be written to when this rule matches.
        /// </summary>
        public TargetCollection Targets
        {
            get { return _targets; }
        }

        /// <summary>
        /// A collection of child rules to be evaluated when this rule matches.
        /// </summary>
        public LoggingRuleCollection ChildRules
        {
            get { return _childRules; }
        }

        /// <summary>
        /// A collection of filters to be checked before writing to targets.
        /// </summary>
        public FilterCollection Filters
        {
            get { return _filters; }
        }

        /// <summary>
        /// Quit processing any further rule when this one matches.
        /// </summary>
        public bool Final
        {
            get { return _final; }
            set { _final = value; }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="LoggingRule"/>.
        /// </summary>
        public LoggingRule(){}

        /// <summary>
        /// Initializes a new instance of <see cref="LoggingRule"/> by
        /// setting the logger name pattern, minimum logging level and 
        /// the target to be written to when logger name and log level match.
        /// </summary>
        /// <param name="loggerNamePattern">Logger name pattern. It may include the '*' wildcard at the beginning, at the end or at both ends.</param>
        /// <param name="minLevel">Minimum log level needed to trigger this rule.</param>
        /// <param name="target">Target to be written to when the rule matches.</param>
        public LoggingRule(string loggerNamePattern, LogLevel minLevel, Target target)
        {
            LoggerNamePattern = loggerNamePattern;
            _targets.Add(target);
            for (int i = (int)minLevel.Ordinal; i <= (int)LogLevel.MaxLevel.Ordinal; ++i)
            {
                EnableLoggingForLevel(LogLevel.FromOrdinal(i));
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="LoggingRule"/> by
        /// setting the logger name pattern and 
        /// the target to be written to when logger name matches.
        /// </summary>
        /// <param name="loggerNamePattern">Logger name pattern. It may include the '*' wildcard at the beginning, at the end or at both ends.</param>
        /// <param name="target">Target to be written to when the rule matches.</param>
        /// <remarks>By default no logging levels are defined. You should call <see cref="EnableLoggingForLevel" /> and <see cref="DisableLoggingForLevel"/> to set them.</remarks>
        public LoggingRule(string loggerNamePattern, Target target)
        {
            LoggerNamePattern = loggerNamePattern;
            _targets.Add(target);
        }

        /// <summary>
        /// Enables logging for a particular level.
        /// </summary>
        /// <param name="level">Level to be enabled.</param>
        public void EnableLoggingForLevel(LogLevel level)
        {
            _logLevels[level.Ordinal] = true;
        }

        /// <summary>
        /// Disables logging for a particular level.
        /// </summary>
        /// <param name="level">Level to be disabled.</param>
        public void DisableLoggingForLevel(LogLevel level)
        {
            _logLevels[level.Ordinal] = false;
        }

        /// <summary>
        /// Checks whether te particular log level is enabled for this rule.
        /// </summary>
        /// <param name="level">Level to be checked</param>
        /// <returns><see langword="true"/> when the log level is enabled, <see langword="false" /> otherwise.</returns>
        public bool IsLoggingEnabledForLevel(LogLevel level)
        {
            return _logLevels[level.Ordinal];
        }

        /// <summary>
        /// Logger name pattern.
        /// </summary>
        /// <remarks>
        /// Logger name pattern. It may include the '*' wildcard at the beginning, at the end or at both ends but not anywhere else.
        /// </remarks>
        public string LoggerNamePattern
        {
            get { return _loggerNamePattern; }
            set
            {
                _loggerNamePattern = value;
                int firstPos = _loggerNamePattern.IndexOf('*');
                int lastPos = _loggerNamePattern.LastIndexOf('*');

                if (firstPos < 0)
                {
                    _loggerNameMatchMode = MatchMode.Equals;
                    _loggerNameMatchArgument = value;
                    return ;
                }

                if (firstPos == lastPos)
                {
                    string before = LoggerNamePattern.Substring(0, firstPos);
                    string after = LoggerNamePattern.Substring(firstPos + 1);

                    if (before.Length > 0)
                    {
                        _loggerNameMatchMode = MatchMode.StartsWith;
                        _loggerNameMatchArgument = before;
                        return ;
                    }

                    if (after.Length > 0)
                    {
                        _loggerNameMatchMode = MatchMode.EndsWith;
                        _loggerNameMatchArgument = after;
                        return ;
                    }
                    return ;
                }

                // *text*
                if (firstPos == 0 && lastPos == LoggerNamePattern.Length - 1)
                {
                    string text = LoggerNamePattern.Substring(1, LoggerNamePattern.Length - 2);
                    _loggerNameMatchMode = MatchMode.Contains;
                    _loggerNameMatchArgument = text;
                    return ;
                }

                _loggerNameMatchMode = MatchMode.None;
                _loggerNameMatchArgument = String.Empty;
            }
        }

        /// <summary>
        /// Checks whether given name matches the logger name pattern.
        /// </summary>
        /// <param name="loggerName">String to be matched</param>
        /// <returns><see langword="true"/> when the name matches, <see langword="false" /> otherwise.</returns>
        public bool NameMatches(string loggerName)
        {
            switch (_loggerNameMatchMode)
            {
                case MatchMode.All:
                    return true;

                default:
                case MatchMode.None:
                    return false;

                case MatchMode.Equals:
                    return String.CompareOrdinal(loggerName, _loggerNameMatchArgument) == 0;

                case MatchMode.StartsWith:
                    return loggerName.StartsWith(_loggerNameMatchArgument);

                case MatchMode.EndsWith:
                    return loggerName.EndsWith(_loggerNameMatchArgument);

                case MatchMode.Contains:
                    return loggerName.IndexOf(_loggerNameMatchArgument) >= 0;
            }
        }
    }
}

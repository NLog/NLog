// 
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

namespace NLog.Config
{
    using System;
    using System.Collections.Generic;
    using NLog.Common;
    using NLog.Internal;
    using NLog.Layouts;

    /// <summary>
    /// Dynamic filtering with a positive list of enabled levels
    /// </summary>
    internal sealed class DynamicLogLevelFilter : ILoggingRuleLevelFilter
    {
        private readonly LoggingRule _loggingRule;
        private readonly SimpleLayout _levelFilter;
        private readonly SimpleLayout _finalMinLevelFilter;
        private KeyValuePair<string, bool[]> _activeFilter;

        public bool[] LogLevels => GenerateLogLevels();

        public LogLevel FinalMinLevel => GenerateFinalMinLevel();

        public DynamicLogLevelFilter(LoggingRule loggingRule, SimpleLayout levelFilter, SimpleLayout finalMinLevelFilter)
        {
            _loggingRule = loggingRule;
            _levelFilter = levelFilter;
            _finalMinLevelFilter = finalMinLevelFilter;
            _activeFilter = new KeyValuePair<string, bool[]>(string.Empty, LoggingRuleLevelFilter.Off.LogLevels);
        }

        public LoggingRuleLevelFilter GetSimpleFilterForUpdate()
        {
            return new LoggingRuleLevelFilter(LogLevels, FinalMinLevel);
        }

        private bool[] GenerateLogLevels()
        {
            var levelFilter = _levelFilter.Render(LogEventInfo.CreateNullEvent());
            if (string.IsNullOrEmpty(levelFilter))
                return LoggingRuleLevelFilter.Off.LogLevels;

            var activeFilter = _activeFilter;
            if (activeFilter.Key != levelFilter)
            {
                bool[] logLevels;
                if (levelFilter.IndexOf(',') >= 0)
                {
                    logLevels = ParseLevels(levelFilter);
                }
                else
                {
                    logLevels = ParseSingleLevel(levelFilter);
                }

                if (ReferenceEquals(logLevels, LoggingRuleLevelFilter.Off.LogLevels))
                    return logLevels;

                _activeFilter = activeFilter = new KeyValuePair<string, bool[]>(levelFilter, logLevels);
            }

            return activeFilter.Value;
        }

        private LogLevel GenerateFinalMinLevel()
        {
            var levelFilter = _finalMinLevelFilter?.Render(LogEventInfo.CreateNullEvent());
            return ParseLogLevel(levelFilter, null);
        }

        private bool[] ParseSingleLevel(string levelFilter)
        {
            var logLevel = ParseLogLevel(levelFilter, null);
            if (logLevel is null || logLevel == LogLevel.Off)
                return LoggingRuleLevelFilter.Off.LogLevels;

            bool[] logLevels = new bool[LogLevel.MaxLevel.Ordinal + 1];
            logLevels[logLevel.Ordinal] = true;
            return logLevels;
        }

        private LogLevel ParseLogLevel(string logLevel, LogLevel levelIfEmpty)
        {
            try
            {
                if (string.IsNullOrEmpty(logLevel))
                    return levelIfEmpty;

                return LogLevel.FromString(logLevel.Trim());
            }
            catch (ArgumentException ex)
            {
                InternalLogger.Warn(ex, "Logging rule {0} with pattern '{1}' has invalid loglevel: {2}", _loggingRule.RuleName, _loggingRule.LoggerNamePattern, logLevel);
                return null;
            }
        }

        private bool[] ParseLevels(string levelFilter)
        {
            var levels = levelFilter.SplitAndTrimTokens(',');
            if (levels.Length == 0)
                return LoggingRuleLevelFilter.Off.LogLevels;
            if (levels.Length == 1)
                return ParseSingleLevel(levels[0]);

            bool[] logLevels = new bool[LogLevel.MaxLevel.Ordinal + 1];
            foreach (var level in levels)
            {
                try
                {
                    var logLevel = LogLevel.FromString(level);
                    if (logLevel == LogLevel.Off)
                        continue;

                    logLevels[logLevel.Ordinal] = true;
                }
                catch (ArgumentException ex)
                {
                    InternalLogger.Warn(ex, "Logging rule {0} with pattern '{1}' has invalid loglevel: {2}", _loggingRule.RuleName, _loggingRule.LoggerNamePattern, levelFilter);
                }
            }

            return logLevels;
        }
    }
}

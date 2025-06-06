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

namespace NLog.Config
{
    using System;
    using System.Collections.Generic;
    using NLog.Common;
    using NLog.Layouts;

    /// <summary>
    /// Dynamic filtering with a minlevel and maxlevel range
    /// </summary>
    internal sealed class DynamicRangeLevelFilter : ILoggingRuleLevelFilter
    {
        private readonly LoggingRule _loggingRule;
        private readonly SimpleLayout? _minLevel;
        private readonly SimpleLayout? _maxLevel;
        private readonly SimpleLayout? _finalMinLevelFilter;
        private KeyValuePair<MinMaxLevels, bool[]> _activeFilter;

        public bool[] LogLevels => GenerateLogLevels();

        public LogLevel? FinalMinLevel => GenerateFinalMinLevel();

        public DynamicRangeLevelFilter(LoggingRule loggingRule, SimpleLayout? minLevel, SimpleLayout? maxLevel, SimpleLayout? finalMinLevelFilter)
        {
            _loggingRule = loggingRule;
            _minLevel = minLevel;
            _maxLevel = maxLevel;
            _finalMinLevelFilter = finalMinLevelFilter;
            _activeFilter = new KeyValuePair<MinMaxLevels, bool[]>(new MinMaxLevels(string.Empty, string.Empty), LoggingRuleLevelFilter.Off.LogLevels);
        }

        public LoggingRuleLevelFilter GetSimpleFilterForUpdate()
        {
            return new LoggingRuleLevelFilter(LogLevels, FinalMinLevel);
        }

        private bool[] GenerateLogLevels()
        {
            var minLevelFilter = _minLevel?.Render(LogEventInfo.CreateNullEvent())?.Trim() ?? string.Empty;
            var maxLevelFilter = _maxLevel?.Render(LogEventInfo.CreateNullEvent())?.Trim() ?? string.Empty;

            var activeFilter = _activeFilter;
            if (!activeFilter.Key.Equals(new MinMaxLevels(minLevelFilter, maxLevelFilter)))
            {
                bool[] logLevels = ParseLevelRange(minLevelFilter, maxLevelFilter);
                _activeFilter = activeFilter = new KeyValuePair<MinMaxLevels, bool[]>(new MinMaxLevels(minLevelFilter, maxLevelFilter), logLevels);
            }
            return activeFilter.Value;
        }

        private LogLevel? GenerateFinalMinLevel()
        {
            var levelFilter = _finalMinLevelFilter?.Render(LogEventInfo.CreateNullEvent())?.Trim() ?? string.Empty;
            return ParseLogLevel(levelFilter, null);
        }

        private bool[] ParseLevelRange(string minLevelFilter, string maxLevelFilter)
        {
            if (string.IsNullOrEmpty(minLevelFilter) && string.IsNullOrEmpty(maxLevelFilter))
                return LoggingRuleLevelFilter.Off.LogLevels;

            var minLevel = ParseLogLevel(minLevelFilter, LogLevel.MinLevel);
            var maxLevel = ParseLogLevel(maxLevelFilter, LogLevel.MaxLevel);

            bool[] logLevels = new bool[LogLevel.MaxLevel.Ordinal + 1];
            if (minLevel != null && maxLevel != null)
            {
                for (int i = minLevel.Ordinal; i <= logLevels.Length - 1 && i <= maxLevel.Ordinal; ++i)
                {
                    logLevels[i] = true;
                }
            }
            return logLevels;
        }

        private LogLevel? ParseLogLevel(string logLevel, LogLevel? levelIfEmpty)
        {
            try
            {
                if (string.IsNullOrEmpty(logLevel))
                    return levelIfEmpty;

                return LogLevel.FromString(logLevel);
            }
            catch (ArgumentException ex)
            {
                InternalLogger.Warn(ex, "Logging rule {0} with pattern '{1}' has invalid loglevel: {2}", _loggingRule.RuleName, _loggingRule.LoggerNamePattern, logLevel);
                return null;
            }
        }

        private
#if !NETFRAMEWORK
        readonly
#endif
        struct MinMaxLevels : IEquatable<MinMaxLevels>
        {
            private readonly string _minLevel;
            private readonly string _maxLevel;

            public MinMaxLevels(string minLevel, string maxLevel)
            {
                _minLevel = minLevel;
                _maxLevel = maxLevel;
            }

            public bool Equals(MinMaxLevels other)
            {
                return _minLevel == other._minLevel && _maxLevel == other._maxLevel;
            }
        }
    }
}

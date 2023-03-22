using System;
using NLog.Common;
using NLog.Internal;
using NLog.Layouts;

namespace NLog.Config
{
    internal class FinalMinLogLevelLayoutFilter : ILoggingRuleLevelFilter
    {
        private readonly LoggingRule _loggingRule;
        private readonly SimpleLayout _levelFilter;
        private LogFilter _activeFilter;

        public bool[] LogLevels => GenerateLogFilter().LogLevels;
        public LogLevel FinalMinLevel => GenerateLogFilter().FinalMinLevel;

        public FinalMinLogLevelLayoutFilter(LoggingRule loggingRule, SimpleLayout levelFilter)
        {
            _loggingRule = loggingRule;
            _levelFilter = levelFilter;
            _activeFilter = CreateDefaultLogFilter();
        }

        public LoggingRuleLevelFilter GetSimpleFilterForUpdate()
        {
            return new LoggingRuleLevelFilter(FinalMinLevel);
        }

        private LogFilter GenerateLogFilter()
        {
            string levelFilter = _levelFilter.Render(LogEventInfo.CreateNullEvent());
            if (string.IsNullOrEmpty(levelFilter))
                return CreateDefaultLogFilter();
            LogFilter activeFilter = _activeFilter;
            if (activeFilter.LevelFilter != levelFilter)
            {
                LogLevel finalMinLevel = ParseLevel(levelFilter);
                bool[] logLevels = FinalMinLevelCalculator.GetLogLevels(finalMinLevel);
                if (activeFilter.FinalMinLevel.Ordinal == finalMinLevel.Ordinal
                    && ReferenceEquals(logLevels, LoggingRuleLevelFilter.Off.LogLevels))
                    return activeFilter;
                _activeFilter = new LogFilter(levelFilter, finalMinLevel, logLevels);
            }

            return activeFilter;
        }

        private LogLevel ParseLevel(string levelFilter)
        {
            try
            {
                if (StringHelpers.IsNullOrWhiteSpace(levelFilter))
                    return LogLevel.Off;
                return LogLevel.FromString(levelFilter.Trim());
            }
            catch (ArgumentException ex)
            {
                InternalLogger.Warn(ex, "Logging rule {0} with filter `{1}` has invalid level filter: {2}",
                    _loggingRule.RuleName, _loggingRule.LoggerNamePattern, levelFilter);
                return LogLevel.Off;
            }
        }

        private static LogFilter CreateDefaultLogFilter()
        {
            return new LogFilter(string.Empty, LogLevel.Off, LoggingRuleLevelFilter.Off.LogLevels);
        }

        private readonly struct LogFilter
        {
            public string LevelFilter { get; }
            public bool[] LogLevels { get; }
            public LogLevel FinalMinLevel { get; }

            public LogFilter(string levelFilter, LogLevel finalMinLevel, bool[] logLevels)
            {
                LevelFilter = levelFilter;
                FinalMinLevel = finalMinLevel;
                LogLevels = logLevels;
            }
        }
    }
}
namespace NLog.Config
{
    internal class FinalMinLogLevelFilter : ILoggingRuleLevelFilter
    {
        public LogLevel FinalMinLevel { get; }

        public bool[] LogLevels { get; }

        public FinalMinLogLevelFilter(LogLevel finalMinLevel)
        {
            FinalMinLevel = finalMinLevel;
            LogLevels = FinalMinLevelCalculator.GetLogLevels(finalMinLevel);
        }

        public LoggingRuleLevelFilter GetSimpleFilterForUpdate()
        {
            return new LoggingRuleLevelFilter(FinalMinLevel);
        }
    }
}
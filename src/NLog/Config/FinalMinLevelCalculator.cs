namespace NLog.Config
{
    internal static class FinalMinLevelCalculator
    {
        public static bool[] GetLogLevels(LogLevel finalMinLevel)
        {
            var logLevels = new bool[LogLevel.MaxLevel.Ordinal + 1];
            for (int i = 0; i < logLevels.Length; i++)
                logLevels[i] = i >= finalMinLevel.Ordinal;
            return logLevels;
        }
    }
}
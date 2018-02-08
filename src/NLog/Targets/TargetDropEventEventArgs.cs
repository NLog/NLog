using System;

namespace NLog.Targets
{
    /// <summary>
    /// Arguments for <see cref="Target.LogEventDropped"/> events.
    /// </summary>
    public class TargetDropEventEventArgs : EventArgs
    {
        /// <inheritdoc />
        public TargetDropEventEventArgs(LogEventInfo logEventInfo) => DroppedLogEventInfo = logEventInfo;

        /// <summary>
        /// Instance of <see cref="LogEventInfo"/> that was dropped by <see cref="Target"/>
        /// </summary>
        public LogEventInfo DroppedLogEventInfo { get; }
    }
}

using System;
using NLog.Common;

namespace NLog.Targets.Wrappers
{
    /// <summary>
    /// Contains LogEvent that was dropped from AsyncRequestQueue because of reaching max capacity
    /// </summary>
    internal class LogEventDroppedEventArgs : EventArgs
    {
        public LogEventDroppedEventArgs(AsyncLogEventInfo eventInfo) => AsyncLogEventInfo = eventInfo;

        /// <summary>
        /// Dropped AsyncLogEventInfo
        /// </summary>
        public AsyncLogEventInfo AsyncLogEventInfo { get; }
    }
}
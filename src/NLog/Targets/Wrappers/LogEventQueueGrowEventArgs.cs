namespace NLog.Targets.Wrappers
{
    using System;

    /// <summary> 
    /// Raises by  <see cref="AsyncRequestQueue"/> when 
    /// <see cref="AsyncRequestQueue.OnOverflow"/> setted to <see cref="AsyncTargetWrapperOverflowAction.Grow"/>
    /// and current queue size bigger than requested.
    /// </summary>
    public class LogEventQueueGrowEventArgs : EventArgs
    {
        /// <summary>
        /// Contains <see cref="AsyncRequestQueue"/> required and current queue size.
        /// </summary>
        /// <param name="requestLimit">Required queue size</param>
        /// <param name="requestCount">Current queue size</param>
        public LogEventQueueGrowEventArgs(int requestLimit, int requestCount)
        {
            RequestLimit = requestLimit;
            RequestCount = requestCount;
        }

        /// <summary>
        /// Required queue size
        /// </summary>
        public int RequestLimit { get; }

        /// <summary>
        /// Current queue size
        /// </summary>
        public int RequestCount { get; }
    }
}
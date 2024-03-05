namespace NLog.Internal.Timers
{
    /// <summary>
    /// The type of timer to be used for scheduling. 
    /// </summary>
    public enum TimerType
    {
        /// <summary>
        /// Timer based on a thread pool thread.
        /// </summary>
        Default = 0,
        /// <summary>
        /// Timer based on a dedicated thread.
        /// </summary>
        DedicatedThread = 1,
    }
}

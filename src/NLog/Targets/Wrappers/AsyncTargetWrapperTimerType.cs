namespace NLog.Targets.Wrappers
{
    /// <summary>
    /// The type of timer to be used for scheduling. 
    /// </summary>
    public enum AsyncTargetWrapperTimerType
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

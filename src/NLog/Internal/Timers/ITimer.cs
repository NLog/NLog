using System;

namespace NLog.Internal.Timers
{
    /// <summary>
    /// Provides a mechanism for executing a method at specified intervals.
    /// </summary>
    internal interface ITimer : IDisposable
    {
        /// <summary>
        /// Set the next trigger time
        /// </summary>
        /// <param name="dueTime">Time, in milliseconds, after which the timer will be triggered</param>
        void NextTrigger(int dueTime);

        /// <summary>
        /// Disposes the ITimer, and waits for it to leave the Timer-callback-method
        /// </summary>
        /// <param name="timeout">Timeout to wait (TimeSpan.Zero means dispose without waiting)</param>
        void WaitForDispose(TimeSpan timeout);
    }
}

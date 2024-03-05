using System;
using System.Threading;
using NLog.Common;

namespace NLog.Internal.Timers
{
    /// <summary>
    /// Provides a mechanism for executing a method on a thread pool thread at specified intervals.
    /// Wrapper over System.Threading.Timer class.
    /// </summary>
    internal class DefaultTimer : ITimer
    {
        private readonly Action _handle;
        private readonly Timer _timer;

        /// <summary>
        /// Initializes a new instance of the NLog.Internal.Timers.DefaultTimer class.
        /// </summary>
        /// <param name="handle">A System.Action delegate representing a method to be executed.</param>
        public DefaultTimer(Action handle)
        {
            this._handle = handle;
            this._timer = new Timer(Handle, null, Timeout.Infinite, Timeout.Infinite);
        }

        private void Handle(object _)
        {
            this._handle.Invoke();
        }

        /// <inheritdoc />
        public void NextTrigger(int dueTime)
        {
            this._timer.Change(dueTime, Timeout.Infinite);
        }

        /// <inheritdoc />
        public void WaitForDispose(TimeSpan waitTime)
        {
            this._timer.WaitForDispose(waitTime);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this._timer.Dispose();
        }
    }
}

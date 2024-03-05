#if NETSTANDARD1_4_OR_GREATER || NET40_OR_GREATER
using System;
using System.Collections.Concurrent;
using System.Threading;
using NLog.Common;

namespace NLog.Internal.Timers
{
    /// <summary>
    /// Provides a mechanism for executing a method on a dedicated thread at specified intervals.
    /// </summary>
    internal class DedicatedThreadTimer : ITimer
    {
        private readonly Action _handle;
        private readonly Thread _worker;
        private readonly ConcurrentQueue<int> _schedules;
        private readonly WaitHandle[] _waits;
        private const string ThreadName = "Nlog internal timer";

        /// <summary>
        /// Initializes a new instance of the NLog.Internal.Timers.DedicatedThreadTimer class.
        /// </summary>
        /// <param name="handle">A System.Action delegate representing a method to be executed.</param>
        public DedicatedThreadTimer(Action handle)
        {
            this._handle = handle;
            this._schedules = new ConcurrentQueue<int>();
            this._waits = new WaitHandle[]
            {
                new AutoResetEvent(false), // cancel
                new AutoResetEvent(false)  // change
            };
            this._worker = new Thread(Handle)
            {
                Name = ThreadName
            };
            this._worker.Start();
        }

        /// <inheritdoc />
        public void NextTrigger(int dueTime)
        {
            if (dueTime < 0)
            {
                InternalLogger.Warn("{0}: Incorrect time value, it will be set to zero.", this);
                dueTime = 0;
            }

            if (!this._worker.IsAlive)
                throw new ObjectDisposedException("Cannot access a disposed timer.");

            this._schedules.Enqueue(dueTime);
            ((AutoResetEvent)this._waits[1]).Set();
        }

        private void Handle()
        {
            try
            {
                int waitResult = WaitHandle.WaitAny(this._waits);

                if (waitResult == 0) // cancel
                    return;

                bool instant = false;
                int? dueTime = null;

                while (true)
                {
                    while (this._schedules.TryDequeue(out var schedule))
                    {
                        switch (schedule)
                        {
                            case 0:
                                instant = true;
                                dueTime = null;
                                break;
                            default:
                                dueTime = schedule;
                                break;
                        }
                    }

                    if (instant)
                    {
                        instant = false;
                        DoWork();
                        continue;
                    }

                    if (!dueTime.HasValue)
                    {
                        waitResult = WaitHandle.WaitAny(this._waits);

                        if (waitResult == 0) // cancel
                        {
                            break;
                        }

                        continue;
                    }

                    if (dueTime.Value <= int.MinValue)
                        InternalLogger.Info("Timer exit");

                    waitResult = WaitHandle.WaitAny(this._waits, dueTime.Value);

                    if (waitResult == 0) // cancel
                    {
                        break;
                    }

                    if (waitResult == 1) // change
                    {
                        continue;
                    }

                    dueTime = null;
                    DoWork();
                }
            }
            catch (ObjectDisposedException)
            {
                InternalLogger.Debug("{0}: One of the timer handlers was disposed.", this);
            }
            finally
            {
                InternalLogger.Debug("{0}: The timer thread is complete.", this);
            }
        }

        private void DoWork()
        {
            try
            {
                this._handle.Invoke();
            }
            catch (Exception exception)
            {
#if DEBUG
                if (exception.MustBeRethrownImmediately())
                {
                    throw;  // Throwing exceptions here will crash the entire application (.NET 2.0 behavior)
                }
#endif
                InternalLogger.Error(exception, "{0}: Error in timer handle.", this);
            }
        }

        /// <inheritdoc />
        public void WaitForDispose(TimeSpan _)
        {
            Dispose();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            ((AutoResetEvent)this._waits[1]).Set();
            this._waits[0].Dispose();
            this._waits[1].Dispose();
            this._schedules.Enqueue(-1);
        }
    }
}

#endif
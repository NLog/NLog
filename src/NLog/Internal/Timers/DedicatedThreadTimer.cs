// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

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
//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.Targets.Dispatchers
{
    using System;
    using System.Threading;
    using NLog.Common;
    using NLog.Internal;

    /// <summary>
    /// <see cref="IDispatcher"/> that invokes the callback on its own dedicated background thread,
    /// so it keeps working when the ThreadPool is starved.
    /// </summary>
    /// <remarks>
    /// Callbacks are serialized on the single thread, and always run outside the internal lock,
    /// so <see cref="Change"/> never waits for a callback to complete.
    /// </remarks>
    internal sealed class DedicatedThreadDispatcher : IDispatcher
    {
        private readonly object _lockObject = new object();
        private readonly string _threadName;
        private Action? _callback;
        private Thread? _thread;
        private bool _armed;
        private int _dueAtTicks;
        private int _period;
        private bool _stopped;

        public DedicatedThreadDispatcher(string threadName)
        {
            _threadName = string.IsNullOrEmpty(threadName) ? nameof(DedicatedThreadDispatcher) : threadName;
        }

        /// <inheritdoc/>
        public bool Set(Action callback)
        {
            if (callback is null)
                throw new ArgumentNullException(nameof(callback));

            lock (_lockObject)
            {
                if (_callback != null || _stopped)
                    throw new InvalidOperationException("The dispatcher has already been set.");

                _callback = callback;
                _thread = new Thread(RunLoop) { IsBackground = true, Name = _threadName };

                // Long-lived thread must not capture the caller's AsyncLocal / ScopeContext state
                if (ExecutionContext.IsFlowSuppressed())
                {
                    _thread.Start();
                }
                else
                {
                    using (ExecutionContext.SuppressFlow())
                    {
                        _thread.Start();
                    }
                }

                return true;
            }
        }

        /// <inheritdoc/>
        public bool Change(int dueTime, int period)
        {
            lock (_lockObject)
            {
                if (_callback is null)
                    throw new InvalidOperationException("The dispatcher has not been initialized. Call the Set method to initialize the dispatcher");
                if (_stopped)
                    return false;

                if (dueTime < 0)
                {
                    _armed = false;
                }
                else
                {
                    _armed = true;
                    _dueAtTicks = unchecked(Environment.TickCount + dueTime);
                    _period = period;
                }

                Monitor.Pulse(_lockObject);
                return true;
            }
        }

        /// <inheritdoc/>
        public bool Stop(TimeSpan timeout)
        {
            Thread? thread;
            lock (_lockObject)
            {
                _stopped = true;
                _armed = false;
                thread = _thread;
                Monitor.Pulse(_lockObject);
            }

            if (thread is null || ReferenceEquals(thread, Thread.CurrentThread))
                return true;    // Never set, or Stop called from inside the callback

            return thread.Join(timeout);
        }

        // The loop revolves around the lockObject and its locked state. State changes restart the loop so that
        // no subsequent locks go unnoticed.
        private void RunLoop()
        {
            lock (_lockObject)
            {
                while (!_stopped)
                {
                    if (!_armed)
                    {
                        Monitor.Wait(_lockObject);
                        continue;
                    }

                    int remainingMs = unchecked(_dueAtTicks - Environment.TickCount);
                    if (remainingMs > 0)
                    {
                        Monitor.Wait(_lockObject, remainingMs);
                        continue;
                    }

                    if (_period > 0)
                        _dueAtTicks = unchecked(Environment.TickCount + _period);
                    else
                        _armed = false;

                    var callback = _callback;
                    Monitor.Exit(_lockObject);
                    try
                    {
                        callback?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        if (ex.MustBeRethrownImmediately())
                            throw;

                        InternalLogger.Error(ex, "{0}: Error in dispatcher callback.", _threadName);
                    }
                    finally
                    {
                        Monitor.Enter(_lockObject);
                    }
                }
            }
        }
    }
}

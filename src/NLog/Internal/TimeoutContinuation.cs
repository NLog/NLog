// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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

namespace NLog.Internal
{
    using System;
    using System.Threading;
    using NLog.Common;

    /// <summary>
    /// Wraps <see cref="AsyncContinuation"/> with a timeout.
    /// </summary>
    internal class TimeoutContinuation : IDisposable
    {
        private AsyncContinuation asyncContinuation;
        private Timer timeoutTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeoutContinuation"/> class.
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        /// <param name="timeout">The timeout.</param>
        public TimeoutContinuation(AsyncContinuation asyncContinuation, TimeSpan timeout)
        {
            this.asyncContinuation = asyncContinuation;
            this.timeoutTimer = new Timer(this.TimerElapsed, null, timeout, TimeSpan.FromMilliseconds(-1));
        }

        /// <summary>
        /// Continuation function which implements the timeout logic.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void Function(Exception exception)
        {
            try
            {
                this.StopTimer();

                var cont = Interlocked.Exchange(ref this.asyncContinuation, null);
                if (cont != null)
                {
                    cont(exception);
                }
            }
            catch (Exception ex)
            {
                if (ex.MustBeRethrown())
                {
                    throw;
                }

                ReportExceptionInHandler(ex);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.StopTimer();
            GC.SuppressFinalize(this);
        }

        private static void ReportExceptionInHandler(Exception exception)
        {
            InternalLogger.Error("Exception in asynchronous handler {0}", exception);
        }

        private void StopTimer()
        {
            lock (this)
            {
                if (this.timeoutTimer != null)
                {
                    this.timeoutTimer.Dispose();
                    this.timeoutTimer = null;
                }
            }
        }

        private void TimerElapsed(object state)
        {
            this.Function(new TimeoutException("Timeout."));
        }
    }
}
// 
// Copyright (c) 2004,2005 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
// 
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
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

using System;
using System.Xml;
using System.IO;
using System.Threading;
using System.Collections;
using System.Collections.Specialized;

using NLog;
using NLog.Config;

using NLog.Internal;

namespace NLog.Targets
{
    /// <summary>
    /// Base class for targets that support asynchronous operation.
    /// </summary>
    public abstract class AsyncTarget: Target
    {
#if !NETCF
        private bool _async = false;
        private bool _stopLoggingThread = false;
        private Thread _loggingThread = null;
        private AsyncRequestQueue _requestQueue = new AsyncRequestQueue(10000, AsyncRequestQueue.OverflowAction.Discard);

        protected bool LoggingThreadStopRequested
        {
            get { return _stopLoggingThread; }
        }

        protected AsyncRequestQueue RequestQueue
        {
            get { return _requestQueue; }
        }

        protected Thread LoggingThread
        {
            get { return _loggingThread; }
        }

        /// <summary>
        /// Process log requests in a separate thread. (EXPERIMENTAL)
        /// </summary>
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool Async
        {
            get { return _async; }
            set 
            {
                _async = value; 
                if (value)
                    StartLoggingThread();
                else
                    StopLoggingThread();
            }
        }

		/// <summary>
		/// The action to be taken when the background thread request queue count
		/// exceeds the set limit.
		/// </summary>
		/// <remarks>
		/// Can be <c>none</c> - do nothing, add another request to the queue
		/// <c>discard</c> - discard the request, <c>block</c> - block the logging
		/// thread to give the processing thread more time.
		/// </remarks>
		[System.ComponentModel.DefaultValue("discard")]
		public string OverflowAction
		{
			get { return _requestQueue.OverflowActionString; }
			set { _requestQueue.OverflowActionString = value; }
		}

		/// <summary>
		/// The limit on the number of requests in the background thread request queue.
		/// </summary>
		[System.ComponentModel.DefaultValue(10000)]
		public int QueueLimit
		{
			get { return _requestQueue.RequestLimit; }
			set { _requestQueue.RequestLimit = value; }
		}

        protected virtual void StartLoggingThread()
        {
            if (_loggingThread != null)
            {
                // already started.
                return;
            }

            _stopLoggingThread = false;
			_requestQueue.Clear();
            Internal.InternalLogger.Debug("Starting logging thread.");
            _loggingThread = new Thread(new ThreadStart(LoggingThreadProc));
            _loggingThread.IsBackground = true;
            _loggingThread.Start();
        }

        protected virtual void StopLoggingThread()
        {
            if (_loggingThread == null)
                return;

            Flush();
            _stopLoggingThread = true;
            if (!_loggingThread.Join(3000))
            {
                InternalLogger.Warn("Logging thread failed to stop. Aborting.");
                _loggingThread.Abort();
            }
            else
            {
                InternalLogger.Debug("Logging thread stopped.");
            }
            _requestQueue.Clear();
        }
        
        protected internal override void Flush()
        {
            InternalLogger.Debug("Flushing logging thread. Requests in queue: {0}", _requestQueue.RequestCount);
            while (_requestQueue.RequestCount > 0)
            {
                int before = _requestQueue.RequestCount;
                Thread.Sleep(100);
                int after = _requestQueue.RequestCount;
                if (after == before)
                {
                    // some lockup - quit the thread
                    break;
                }
            }
            InternalLogger.Debug("After flush. Requests in queue: {0}", _requestQueue.RequestCount);
        }

        protected abstract void LoggingThreadProc();
#endif
    }
}

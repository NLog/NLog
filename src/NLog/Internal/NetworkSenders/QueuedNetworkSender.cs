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

namespace NLog.Internal.NetworkSenders
{
    using System;
    using System.Collections.Generic;
    using NLog.Common;
    using NLog.Targets;

    /// <summary>
    /// A base class for network senders that can block or send out-of-order
    /// </summary>
    internal abstract class QueuedNetworkSender : NetworkSender
    {
        protected struct NetworkRequestArgs
        {
            public NetworkRequestArgs(byte[] buffer, int offset, int length, AsyncContinuation asyncContinuation)
            {
                AsyncContinuation = asyncContinuation;
                RequestBuffer = buffer;
                RequestBufferOffset = offset;
                RequestBufferLength = length;
            }

            public readonly AsyncContinuation AsyncContinuation;
            public readonly byte[] RequestBuffer;
            public readonly int RequestBufferOffset;
            public readonly int RequestBufferLength;
        }

        private readonly Queue<NetworkRequestArgs> _pendingRequests = new Queue<NetworkRequestArgs>();
        private readonly Queue<NetworkRequestArgs> _activeRequests = new Queue<NetworkRequestArgs>();
        private Exception _pendingError;
        private bool _asyncOperationInProgress;
        private AsyncContinuation _closeContinuation;
        private AsyncContinuation _flushContinuation;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueuedNetworkSender"/> class.
        /// </summary>
        /// <param name="url">URL. Must start with tcp://.</param>
        protected QueuedNetworkSender(string url)
            : base(url)
        {
        }

        public int MaxQueueSize { get; set; }

        public NetworkTargetQueueOverflowAction OnQueueOverflow { get; set; }

        public event EventHandler<NetworkLogEventDroppedEventArgs> LogEventDropped;

        protected override void DoSend(byte[] bytes, int offset, int length, AsyncContinuation asyncContinuation)
        {
            NetworkRequestArgs? eventArgs = new NetworkRequestArgs(bytes, offset, length, asyncContinuation);
            AsyncContinuation failedContinuation = null;

            lock (_pendingRequests)
            {
                if (_pendingError is null)
                {
                    if (_pendingRequests.Count >= MaxQueueSize && MaxQueueSize > 0)
                    {
                        switch (OnQueueOverflow)
                        {
                            case NetworkTargetQueueOverflowAction.Discard:
                                InternalLogger.Debug("NetworkTarget - Discarding single item, because queue is full");
                                OnLogEventDropped(this, NetworkLogEventDroppedEventArgs.MaxQueueOverflow);
                                var dequeued = _pendingRequests.Dequeue();
                                dequeued.AsyncContinuation?.Invoke(null);
                                break;

                            case NetworkTargetQueueOverflowAction.Grow:
                                InternalLogger.Debug("NetworkTarget - Growing the size of queue, because queue is full");
                                MaxQueueSize *= 2;
                                break;

                            case NetworkTargetQueueOverflowAction.Block:
                                while (_pendingRequests.Count >= MaxQueueSize && _pendingError is null)
                                {
                                    InternalLogger.Debug("NetworkTarget - Blocking until ready, because queue is full");
                                    System.Threading.Monitor.Wait(_pendingRequests);
                                    InternalLogger.Trace("NetworkTarget - Entered critical section for queue.");
                                }
                                InternalLogger.Trace("NetworkTarget - Queue Limit ok.");
                                break;
                        }
                    }

                    if (_pendingError is null)
                    {
                        if (!_asyncOperationInProgress)
                        {
                            _asyncOperationInProgress = true;
                        }
                        else
                        {
                            _pendingRequests.Enqueue(eventArgs.Value);
                            eventArgs = null;
                        }
                    }
                    else
                    {
                        failedContinuation = asyncContinuation;
                        eventArgs = null;
                    }
                }
                else
                {
                    failedContinuation = asyncContinuation;
                    eventArgs = null;
                }
            }

            if (eventArgs.HasValue)
            {
                BeginRequest(eventArgs.Value);
            }

            failedContinuation?.Invoke(_pendingError);
        }

        protected override void DoFlush(AsyncContinuation continuation)
        {
            lock (_pendingRequests)
            {
                if (_asyncOperationInProgress || _pendingRequests.Count != 0 || _activeRequests.Count != 0)
                {
                    if (_flushContinuation != null)
                    {
                        var flushChain = _flushContinuation;
                        _flushContinuation = (ex) => { flushChain(ex); continuation(ex); };
                    }
                    else
                    {
                        _flushContinuation = continuation;
                    }
                    return;
                }
            }

            continuation(null);
        }

        protected override void DoClose(AsyncContinuation continuation)
        {
            lock (_pendingRequests)
            {
                if (_asyncOperationInProgress)
                {
                    _closeContinuation = continuation;
                    return;
                }
            }

            continuation(null);
        }

        protected void BeginInitialize()
        {
            lock (_pendingRequests)
            {
                _asyncOperationInProgress = true;
            }
        }

        protected NetworkRequestArgs? EndRequest(AsyncContinuation asyncContinuation, Exception pendingException)
        {
            if (pendingException != null)
            {
                lock (_pendingRequests)
                {
                    _pendingError = pendingException;
                }
            }

            try
            {
                asyncContinuation?.Invoke(pendingException);    // Will attempt to close socket on error
                return DequeueNextItem();
            }
            catch (Exception ex)
            {
#if DEBUG
                if (ex.MustBeRethrownImmediately())
                {
                    throw;  // Throwing exceptions here will crash the entire application
                }
#endif

                if (_pendingError is null)
                    InternalLogger.Error(ex, "NetworkTarget: Error completing network request");
                else
                    InternalLogger.Error(ex, "NetworkTarget: Error completing failed network request");
                return null;
            }
        }

        protected abstract void BeginRequest(NetworkRequestArgs eventArgs);

        private NetworkRequestArgs? DequeueNextItem()
        {
            AsyncContinuation closeContinuation;
            AsyncContinuation flushContinuation;

            lock (_activeRequests)
            {
                if (_pendingError is null)
                {
                    if (_activeRequests.Count != 0)
                    {
                        _asyncOperationInProgress = true;
                        return _activeRequests.Dequeue();
                    }
                }
                else
                {
                    SignalSocketFailedForPendingRequests(_pendingError);
                }

                lock (_pendingRequests)
                {
                    _asyncOperationInProgress = false;

                    if (_pendingRequests.Count == 0)
                    {
                        flushContinuation = _flushContinuation;
                        if (flushContinuation != null)
                        {
                            _flushContinuation = null;
                        }

                        closeContinuation = _closeContinuation;
                        if (closeContinuation != null)
                        {
                            _closeContinuation = null;
                        }
                    }
                    else
                    {
                        try
                        {
                            _asyncOperationInProgress = true;

                            if (_pendingRequests.Count == 1)
                            {
                                return _pendingRequests.Dequeue();
                            }
                            else
                            {
                                int nextBatchSize = Math.Min(_pendingRequests.Count, MaxQueueSize / 2 + 1000);
                                for (int i = 0; i < nextBatchSize; ++i)
                                {
                                    _activeRequests.Enqueue(_pendingRequests.Dequeue());
                                }

                                return _activeRequests.Dequeue();
                            }
                        }
                        finally
                        {
                            if (OnQueueOverflow == NetworkTargetQueueOverflowAction.Block)
                            {
                                System.Threading.Monitor.PulseAll(_pendingRequests);
                            }
                        }
                    }
                }
            }

            flushContinuation?.Invoke(_pendingError);
            closeContinuation?.Invoke(_pendingError);
            return null;
        }

        private void SignalSocketFailedForPendingRequests(Exception pendingException)
        {
            lock (_pendingRequests)
            {
                while (_pendingRequests.Count != 0)
                    _activeRequests.Enqueue(_pendingRequests.Dequeue());

                System.Threading.Monitor.PulseAll(_pendingRequests);
            }

            while (_activeRequests.Count != 0)
            {
                var eventArgs = _activeRequests.Dequeue();
                eventArgs.AsyncContinuation?.Invoke(pendingException);
            }
        }

        private void OnLogEventDropped(object sender, NetworkLogEventDroppedEventArgs logEventDroppedEventArgs)
        {
            LogEventDropped?.Invoke(this, logEventDroppedEventArgs);
        }
    }
}

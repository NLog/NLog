﻿// 
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

namespace NLog.Internal.NetworkSenders
{
    using System;
    using System.Collections.Generic;
    using NLog.Common;

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

        internal int MaxQueueSize { get; set; }

        /// <summary>
        /// Actually sends the given text over the specified protocol.
        /// </summary>
        /// <param name="bytes">The bytes to be sent.</param>
        /// <param name="offset">Offset in buffer.</param>
        /// <param name="length">Number of bytes to send.</param>
        /// <param name="asyncContinuation">The async continuation to be invoked after the buffer has been sent.</param>
        /// <remarks>To be overridden in inheriting classes.</remarks>
        protected override void DoSend(byte[] bytes, int offset, int length, AsyncContinuation asyncContinuation)
        {
            NetworkRequestArgs? eventArgs = new NetworkRequestArgs(bytes, offset, length, asyncContinuation);
            AsyncContinuation failedContinuation = null;

            lock (_pendingRequests)
            {
                if (_pendingError is null)
                {
                    if (MaxQueueSize > 0 && _pendingRequests.Count >= MaxQueueSize)
                    {
                        var dequeued = _pendingRequests.Dequeue();
                        failedContinuation = dequeued.AsyncContinuation;
                    }

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

            if (eventArgs.HasValue)
            {
                BeginRequest(eventArgs.Value);
            }

            failedContinuation?.Invoke(_pendingError);
        }

        /// <summary>
        /// Performs sender-specific flush.
        /// </summary>
        /// <param name="continuation">The continuation.</param>
        protected override void DoFlush(AsyncContinuation continuation)
        {
            lock (_pendingRequests)
            {
                if (_asyncOperationInProgress || _pendingRequests.Count != 0)
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

            asyncContinuation?.Invoke(pendingException);    // Will attempt to close socket on error

            return DequeueNextItem();
        }

        protected abstract void BeginRequest(NetworkRequestArgs eventArgs);

        private NetworkRequestArgs? DequeueNextItem()
        {
            AsyncContinuation closeContinuation;
            AsyncContinuation flushContinuation;

            lock (_pendingRequests)
            {
                _asyncOperationInProgress = false;

                if (_pendingError != null)
                {
                    while (_pendingRequests.Count != 0)
                    {
                        var eventArgs = _pendingRequests.Dequeue();
                        eventArgs.AsyncContinuation?.Invoke(_pendingError);
                    }
                }

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
                    _asyncOperationInProgress = true;
                    return _pendingRequests.Dequeue();
                }
            }

            flushContinuation?.Invoke(_pendingError);
            closeContinuation?.Invoke(_pendingError);
            return null;
        }
    }
}

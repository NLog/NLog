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

namespace NLog.Internal
{
    using System;
    using System.Collections.Generic;
    using NLog.Common;

    /// <summary>
    /// Keeps track of pending operation count, and can notify when pending operation count reaches zero
    /// </summary>
    internal sealed class AsyncOperationCounter
    {
        private int _pendingOperationCounter;
        private readonly LinkedList<AsyncContinuation?> _pendingCompletionList = new LinkedList<AsyncContinuation?>();

        /// <summary>
        /// Mark operation has started
        /// </summary>
        public void BeginOperation()
        {
            System.Threading.Interlocked.Increment(ref _pendingOperationCounter);
        }

        /// <summary>
        /// Mark operation has completed
        /// </summary>
        /// <param name="exception">Exception coming from the completed operation [optional]</param>
        public void CompleteOperation(Exception? exception)
        {
            NotifyCompletion(exception);
        }

        private int NotifyCompletion(Exception? exception)
        {
            int pendingOperations = System.Threading.Interlocked.Decrement(ref _pendingOperationCounter);

            if (_pendingCompletionList.Count > 0)
            {
                lock (_pendingCompletionList)
                {
                    var nodeNext = _pendingCompletionList.First;
                    while (nodeNext != null)
                    {
                        var nodeValue = nodeNext.Value;
                        nodeNext = nodeNext.Next;
                        nodeValue?.Invoke(exception);  // Will modify _pendingCompletionList
                    }
                }
            }

            return pendingOperations;
        }

        /// <summary>
        /// Registers an AsyncContinuation to be called when all pending operations have completed
        /// </summary>
        /// <param name="asyncContinuation">Invoked on completion</param>
        /// <returns>AsyncContinuation operation</returns>
        public AsyncContinuation RegisterCompletionNotification(AsyncContinuation asyncContinuation)
        {
            // We only want to wait for the operations currently in progress (not the future operations)
            int remainingCompletionCounter = System.Threading.Interlocked.Increment(ref _pendingOperationCounter);
            if (remainingCompletionCounter <= 1)
            {
                // No active operations
                if (NotifyCompletion(null) < 0)
                {
                    System.Threading.Interlocked.Exchange(ref _pendingOperationCounter, 0);
                }
                return asyncContinuation;
            }
            else
            {
                lock (_pendingCompletionList)
                {
                    if (NotifyCompletion(null) <= 0)
                    {
                        return asyncContinuation;   // No active operations
                    }

                    var pendingCompletion = new LinkedListNode<AsyncContinuation?>(null);
                    _pendingCompletionList.AddLast(pendingCompletion);
                    remainingCompletionCounter = System.Threading.Interlocked.Increment(ref _pendingOperationCounter);
                    if (remainingCompletionCounter <= 0)
                    {
                        remainingCompletionCounter = 1;
                    }

                    pendingCompletion.Value = (ex) =>
                    {
                        if (System.Threading.Interlocked.Decrement(ref remainingCompletionCounter) == 0)
                        {
                            lock (_pendingCompletionList)
                            {
                                _pendingCompletionList.Remove(pendingCompletion);
                                NotifyCompletion(ex);
                            }

                            asyncContinuation(ex);
                        }
                    };

                    return pendingCompletion.Value;
                }
            }
        }

        /// <summary>
        /// Clear o
        /// </summary>
        public void Clear()
        {
            _pendingCompletionList.Clear();
            System.Threading.Interlocked.Exchange(ref _pendingOperationCounter, 0);
        }
    }
}

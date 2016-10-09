// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

using System;
using System.Threading;
using System.Collections.Generic;
using NLog.Common;

namespace NLog.Internal
{
    /// <summary>
    /// Used to schedule a final <see cref="AsyncContinuation"/> to be executed after having
    /// executed multiple <see cref="AsyncContinuation"/>s (Ex. Flush or Release to pool)
    /// </summary>
    internal class CompleteWhenAllContinuation : PoolFactory.IPoolObject
    {
        public readonly AsyncContinuation Delegate;

        private PoolFactory.ILogEventObjectFactory _owner;
        object PoolFactory.IPoolObject.OwnerPool { get { return _owner; } set { _owner = (PoolFactory.ILogEventObjectFactory)value; } }

        private Counter remainingCounter = null;

        private AsyncContinuation originalContinuation;
        private AsyncContinuation completedContinuation;

        public CompleteWhenAllContinuation(Counter counter)
        {
            this.Delegate = this.Function;
            Init(counter);
        }

        public void Init(Counter counter)
        {
            this.remainingCounter = counter;
        }

        /// <summary>
        /// Registers the final <see cref="AsyncContinuation"/> to be executed, when remaining number of
        /// scheduled continuations has completed
        /// </summary>
        /// <param name="originalContinuation">Continuation to be scheduled</param>
        /// <param name="whenAllDone">The final continuation when all are completed</param>
        /// <returns></returns>
        public AsyncContinuation CreateContinuation(AsyncContinuation originalContinuation, AsyncContinuation whenAllDone)
        {
            if (this.originalContinuation != null && !ReferenceEquals(this.originalContinuation, originalContinuation))
                throw new InvalidOperationException("Cannot change the initial Continuation");

            BeginTargetWrite();

            this.originalContinuation = originalContinuation;
            this.completedContinuation = whenAllDone;

            return this.Delegate;
        }

        /// <summary>
        /// Should be called before using <see cref="CreateContinuation"/> to prevent premature completion
        /// </summary>
        public void BeginTargetWrite()
        {
            this.remainingCounter.Increment();
        }

        /// <summary>
        /// Should be called when done making <see cref="CreateContinuation"/> to allow completion
        /// </summary>
        public void EndTargetWrite(Exception ex)
        {
            int finalResult = this.remainingCounter.Decrement();
            if (finalResult < 0)
                throw new InvalidOperationException("Remaining count has become negative");

            if (finalResult == 0)
            {
                // If no Target wanted the LogEvent, then continuation is never configured
                if (this.completedContinuation != null)
                    this.completedContinuation(ex);
            }

            if (_owner != null)
                _owner.ReleaseCompleteWhenAllContinuation(this);
        }

        /// <summary>
        /// Reset the continuation for reuse
        /// </summary>
        public void Clear()
        {
            this.remainingCounter = null;
            this.originalContinuation = null;
            this.completedContinuation = null;
        }

        private void Function(Exception ex)
        {
            try
            {
                this.originalContinuation(ex);
            }
            finally
            {
                EndTargetWrite(ex);
            }
        }

        internal class Counter
        {
            private int remaining;

            public void Clear()
            {
                this.remaining = 0;
            }

            public int Increment()
            {
                return Interlocked.Increment(ref this.remaining);
            }

            public int Decrement()
            {
                return Interlocked.Decrement(ref this.remaining);
            }
        }
    }
}

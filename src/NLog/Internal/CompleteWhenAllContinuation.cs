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
#if DEBUG
        private readonly List<Tuple<int, int, AsyncContinuation>> _activeChains = new List<Tuple<int, int, AsyncContinuation>>();
#endif
        private PoolFactory.ILogEventObjectFactory _owner;
        object PoolFactory.IPoolObject.Owner { get { return _owner; } set { _owner = (PoolFactory.ILogEventObjectFactory)value; } }

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
        /// 
        /// </summary>
        /// <param name="originalContinuation"></param>
        /// <param name="whenAllDone">The final continuation when all are completed</param>
        /// <param name="chainIndex"></param>
        /// <param name="logEventSeqNo"></param>
        /// <returns></returns>
        public AsyncContinuation StartContinuationChain(AsyncContinuation originalContinuation, AsyncContinuation whenAllDone, int chainIndex, int logEventSeqNo)
        {
            if (this.originalContinuation != null && !ReferenceEquals(this.originalContinuation, originalContinuation))
                throw new InvalidOperationException();

            BeginTargetWrite();

            this.originalContinuation = originalContinuation;
            this.completedContinuation = whenAllDone;

#if DEBUG
            lock (_activeChains)
            {
                _activeChains.Add(new Tuple<int, int, AsyncContinuation>(chainIndex, logEventSeqNo, this.Delegate));
            }

            return (ex) =>
            {
                int chainPosition = -1;

                for (int i = 0; i < _activeChains.Count; ++i)
                {
                    if (_activeChains[i].Item1 == chainIndex)
                    {
                        chainPosition = i;
                        break;
                    }
                }

                if (chainPosition == -1)
                    throw new InvalidOperationException();

                _activeChains[chainPosition] = new Tuple<int, int, AsyncContinuation>(chainIndex, logEventSeqNo, null);
                this.Delegate(ex);
            };
#else
            return this.Delegate;
#endif
        }

        // When sending a LogEvent to a target, then we want to have it like this
        //  - Async-Cont = Release-Handler-Pool -> Exception-Handler
        public AsyncContinuation WithContinuationChain(int chainIndex, AsyncContinuation first)
        {
#if DEBUG
            if (chainIndex == 0)
                throw new InvalidOperationException();

            int chainPosition = -1;
            for (int i = 0; i < _activeChains.Count; ++i)
            {
                if (ReferenceEquals(_activeChains[i].Item3, first) && chainPosition != -1)
                    throw new InvalidOperationException();
                if (_activeChains[i].Item1 == chainIndex)
                    chainPosition = i;
            }
            if (chainPosition == -1)
                throw new InvalidOperationException();

            var oldChain = _activeChains[chainPosition];
            _activeChains[chainPosition] = new Tuple<int, int, AsyncContinuation>(oldChain.Item1, oldChain.Item2, null);
#endif
            return first;
        }

        public void BeginTargetWrite()
        {
            this.remainingCounter.Increment();
        }

        public void EndTargetWrite(Exception ex)
        {
            int finalResult = this.remainingCounter.Decrement();
            if (finalResult < 0)
                throw new InvalidOperationException();

            if (finalResult == 0)
            {
#if DEBUG
                for (int i = 0; i < _activeChains.Count; ++i)
                    if (_activeChains[i].Item3 != null)
                        throw new InvalidOperationException();
#endif
                this.completedContinuation(ex);
            }

            if (_owner != null)
                _owner.ReleaseCompleteWhenAllContinuation(this);
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

        public void Clear()
        {
#if DEBUG
            for (int i = 0; i < _activeChains.Count; ++i)
            {
                if (_activeChains[i].Item3 != null)
                    throw new InvalidOperationException();
            }
            lock(_activeChains)
                _activeChains.Clear();
#endif
            this.remainingCounter = null;
            this.originalContinuation = null;
            this.completedContinuation = null;
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

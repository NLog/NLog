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

using NLog.Internal.Pooling.Pools;

namespace NLog.Internal
{
    using System;
    using System.Threading;
    using NLog.Common;

    using Pooling;

    /// <summary>
    /// Implements a single-call guard around given continuation function.
    /// </summary>
    internal class SingleCallContinuation
    {
        private AsyncContinuation asyncContinuation;

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleCallContinuation"/> class.
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        public SingleCallContinuation(AsyncContinuation asyncContinuation)
        {
            if (asyncContinuation == null)
            {
                throw new ArgumentNullException("asyncContinuation");
            }

            this.asyncContinuation = asyncContinuation;
            this.Delegate = this.Function;
        }

        /// <summary>
        /// Delegate, so that we dont have to allocate one every time we pass .Function as a delegate
        /// </summary>
        public readonly AsyncContinuation Delegate;

        /// <summary>
        /// Continuation function which implements the single-call guard.
        /// </summary>
        /// <param name="exception">The exception.</param>
        private void Function(Exception exception)
        {
            try
            {
                // Exchange async continuation with null, and if we get null back that means some other thread already
                // did the continuation
                var cont = Interlocked.Exchange(ref this.asyncContinuation, null);

                if (cont != null)
                {
                    cont(exception);
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "Exception in asynchronous handler.");

                if (ex.MustBeRethrown())
                {
                    throw;
                }
            }

            finally
            {
                this.PutBack();
            }

        }

        private SingleCallContinuationPool pool;
        internal SingleCallContinuation(SingleCallContinuationPool pool)
        {
            this.pool = pool;
            this.Delegate = this.Function;
        }

        internal void Initialize(SingleCallContinuationPool pool, AsyncContinuation asyncContinuation)
        {
            if (pool == null)
            {
                throw new ArgumentNullException("pool");
            }
            if (asyncContinuation == null)
            {
                throw new ArgumentNullException("asyncContinuation");
            }
            this.pool = pool;
            this.asyncContinuation = asyncContinuation;
        }

        internal void PutBack()
        {
            // Swap with null and if we get a pool back, put it back, otherwise another invocation has happened.
            var localPool = Interlocked.Exchange(ref this.pool, null);
            if (localPool != null)
            {
                localPool.PutBack(this);
            }
        }

        internal void Clear()
        {
            this.asyncContinuation = null;
        }

    }
}
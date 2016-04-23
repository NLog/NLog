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

using NLog.Internal.Pooling;
using NLog.Internal.Pooling.Pools;

namespace NLog.Common
{
    internal class CombinedAsyncContinuation
    {
        private CombinedAsyncContinuationPool pool;

        private AsyncContinuation originalContinuation;

        private AsyncContinuation continueWith;

        public readonly AsyncContinuation Delegate;

        public CombinedAsyncContinuation(CombinedAsyncContinuationPool pool = null)
        {
            this.pool = pool;
            this.Delegate = this.Function;
        }

        public void Init(CombinedAsyncContinuationPool pool, AsyncContinuation first, AsyncContinuation second)
        {
            this.pool = pool;
            this.originalContinuation = first;
            this.continueWith = second;
        }

        private void Function(Exception ex)
        {
            Exception exception = ex;
            try
            {
                var original = Interlocked.Exchange(ref this.originalContinuation, null);
                if (original != null)
                {
                    original(ex);
                }
            }
            catch (Exception continuationException)
            {
                if (exception != null)
                {
                    exception = AsyncHelpers.GetCombinedException(new[] { exception, continuationException });
                }
                else
                {
                    exception = continuationException;
                }
            }
            finally
            {
                try
                {
                    var continuation = Interlocked.Exchange(ref this.continueWith, null);
                    if (continuation != null)
                    {
                        continuation(exception);
                    }
                }
                finally
                {
                    this.PutBack();
                }
            }
        }

        private void PutBack()
        {
            var pool = Interlocked.Exchange(ref this.pool, null);
            if (pool != null)
            {
                pool.PutBack(this);
            }
        }

        public void Clear()
        {
            this.originalContinuation = null;
            this.continueWith = null;
        }
    }
}
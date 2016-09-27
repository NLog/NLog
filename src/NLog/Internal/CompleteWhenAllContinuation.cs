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
using NLog.Common;

namespace NLog.Internal
{
    internal class CompleteWhenAllContinuation : PoolFactory.IPoolObject
    {
        private Counter remaining;
        private AsyncContinuation originalContinuation;
        private AsyncContinuation completedContinuation;

        public readonly AsyncContinuation Delegate;

        private PoolFactory.ILogEventObjectFactory _owner;
        object PoolFactory.IPoolObject.Owner { get { return _owner; } set { _owner = (PoolFactory.ILogEventObjectFactory)value; } }

        public CompleteWhenAllContinuation()
        {
            this.Delegate = this.ContinueAndCheck;
        }

        public void Reset(Counter remaining, AsyncContinuation originalContinuation, AsyncContinuation whenAllDone)
        {
            this.remaining = remaining;
            this.originalContinuation = originalContinuation;
            this.completedContinuation = whenAllDone;
        }

        private void ContinueAndCheck(Exception ex)
        {
            try
            {
                this.originalContinuation(ex);
            }
            catch (Exception e)
            {
                InternalLogger.Error(e, "Exception in asynchronous original handler.");
                if (ex.MustBeRethrown())
                {
                    throw;
                }
            }

            try
            {
                if (0 == remaining.Decrement())
                {
                    try
                    {
                        this.completedContinuation(null);
                    }
                    catch (Exception e)
                    {
                        InternalLogger.Error(e, "Exception in asynchronous completed handler.");
                        if (ex.MustBeRethrown())
                        {
                            throw;
                        }
                    }
                }
            }
            finally
            {
                if (_owner != null)
                    _owner.ReleaseCompleteWhenAllContinuation(this);
            }
        }

        void PoolFactory.IPoolObject.Clear()
        {
            this.remaining = null;
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

            public void Reset(int remaining)
            {
                this.remaining = remaining;
            }

            public int Decrement()
            {
                return Interlocked.Decrement(ref this.remaining);
            }
        }
    }
}

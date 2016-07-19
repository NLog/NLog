﻿// 
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
    /// <summary>
    /// class to handle exceptions.
    /// Used instead of lambda, since lambda capture state and is a class that will be allocated on the heap.
    /// </summary>
    internal class ExceptionHandlerContinuation
    {
        internal ExceptionHandlerPool Pool;

        internal ExceptionHandlerContinuation(ExceptionHandlerPool pool)
        {
            this.Pool = pool;
            this.Delegate = this.Handler;
        }

        public int OriginalThreadId { get; set; }

        public bool ThrowExceptions { get; set; }

        public readonly AsyncContinuation Delegate;

        internal ExceptionHandlerContinuation(int originalThreadId, bool throwExceptions)
        {
            this.OriginalThreadId = originalThreadId;
            this.ThrowExceptions = throwExceptions;
            this.Delegate = this.Handler;
        }

        private void Handler(Exception ex)
        {
            try
            {
                if (ex != null)
                {
                    if (this.ThrowExceptions && Thread.CurrentThread.ManagedThreadId == this.OriginalThreadId)
                    {
                        throw new NLogRuntimeException("Exception occurred in NLog", ex);
                    }
                }
            }
            finally
            {
                this.PutBack();
            }
        }

        private void PutBack()
        {
            var pool = Interlocked.Exchange(ref this.Pool, null);
            if (pool != null)
            {
                pool.PutBack(this);
            }
        }
    }
}

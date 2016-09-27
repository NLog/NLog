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
    /// <summary>
    /// Throws exceptions directly from <see cref="NLog.Targets.Target" /> to thread
    /// calling the <see cref="Logger"/> if <see cref="LogFactory.ThrowExceptions"/> is enabled,
    /// and target is synchronous.
    /// </summary>
    internal class ExceptionHandlerContinuation : PoolFactory.IPoolObject
    {
        public int OriginalThreadId { get; private set; }

        public bool ThrowExceptions { get; private set; }

        public readonly AsyncContinuation Delegate;

        private PoolFactory.ILogEventObjectFactory _owner;
        object PoolFactory.IPoolObject.Owner { get { return _owner; } set { _owner = (PoolFactory.ILogEventObjectFactory)value; } }

        internal ExceptionHandlerContinuation(int originalThreadId, bool throwExceptions)
        {
            this.Delegate = this.Handler;
            Init(originalThreadId, throwExceptions);
        }

        /// <summary>
        /// Captures the exception-handle-state for the thread that called the <see cref="Logger"/>
        /// </summary>
        /// <param name="originalThreadId">ThreadId for the thread calling Logger</param>
        /// <param name="throwExceptions"><see cref="LogFactory.ThrowExceptions"/> policy</param>
        internal void Init(int originalThreadId, bool throwExceptions)
        {
            this.OriginalThreadId = originalThreadId;
            this.ThrowExceptions = throwExceptions;
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
                if (_owner != null)
                    _owner.ReleaseExceptionHandlerContinuation(this);
            }
        }

        void PoolFactory.IPoolObject.Clear()
        {
            // No references to clear
        }
    }
}

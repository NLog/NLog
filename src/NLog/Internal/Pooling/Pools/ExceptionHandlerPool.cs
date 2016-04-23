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

using NLog.Common;

namespace NLog.Internal.Pooling.Pools
{
    /// <summary>
    /// Pool for ExceptionHandler AsyncContinuation
    /// </summary>
    internal class ExceptionHandlerPool : PoolBaseOfT<ExceptionHandlerContinuation>
    {
        /// <summary>
        /// Inializes a new instance of the <see cref="ExceptionHandlerPool"/>
        /// </summary>
        /// <param name="poolSize">the size of the pool.</param>
        /// <param name="preFill">Whether or not to prefill the pool.</param>
        public ExceptionHandlerPool(int poolSize, bool preFill = false)
            : base(poolSize, preFill, null, true)
        {
        }

        /// <summary>
        /// Factory method that creates an instance of the pooled type.
        /// </summary>
        /// <returns>An instance of the pooled type</returns>
        protected override ExceptionHandlerContinuation Factory()
        {
            return new ExceptionHandlerContinuation(this);
        }

        /// <summary>
        /// Special implementation of Get() to save a method call from the calling side.
        /// All parameters will be passed onto the <see cref="ExceptionHandlerContinuation"/> returned.
        /// </summary>
        /// <param name="originalThreadId">The original thread id.</param>
        /// <param name="throwExceptions">Whether or not to throw exceptions.</param>
        /// <returns>A pooled instance of <see cref="ExceptionHandlerContinuation"/></returns>
        public ExceptionHandlerContinuation Get(int originalThreadId, bool throwExceptions)
        {
            var handler = this.Get();
            handler.OriginalThreadId = originalThreadId;
            handler.ThrowExceptions = throwExceptions;
            handler.Pool = this;

            return handler;
        }

        /// <summary>
        /// Clears the item if necessary before putting it back into the pool.
        /// </summary>
        /// <param name="item">The item to clear.</param>
        protected override void Clear(ExceptionHandlerContinuation item)
        {
            item.OriginalThreadId = 0;
            item.ThrowExceptions = false;
        }
    }
}
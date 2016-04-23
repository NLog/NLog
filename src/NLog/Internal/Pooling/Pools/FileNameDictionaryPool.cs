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

using System.Collections.Generic;

using NLog.Common;

namespace NLog.Internal.Pooling.Pools
{
    /// <summary>
    /// Simple pool to hold dictionaries for filename -> async log event info dictionaries
    /// when sorting by filename
    /// </summary>
    internal class FileNameDictionaryPool : PoolBaseOfT<Dictionary<string, List<AsyncLogEventInfo>>>
    {
        private readonly int fileNameCount;
        private readonly AsyncLogEventInfoListPool listPool;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileNameDictionaryPool"/>.
        /// </summary>
        /// <param name="listPool">The list pool to put back the lists contained in the dictionary.</param>
        /// <param name="poolSize">Max size of the pool.</param>
        /// <param name="fileNameCount">Number of initial keys in the dictionaries.</param>
        /// <param name="preFill">Whether or not to prefill pool.</param>
        public FileNameDictionaryPool(AsyncLogEventInfoListPool listPool, int poolSize, int fileNameCount, bool preFill=false)
            :base(poolSize,preFill)
        {
            this.fileNameCount = fileNameCount;
            this.listPool = listPool;
        }

        /// <inheritdoc/>
        protected override Dictionary<string, List<AsyncLogEventInfo>> Factory()
        {
            return new Dictionary<string, List<AsyncLogEventInfo>>(this.fileNameCount);
        }

        /// <inheritdoc/>
        protected override void Clear(Dictionary<string, List<AsyncLogEventInfo>> item)
        {
            foreach (var list in item.Values)
            {
                this.listPool.PutBack(list);
            }
            item.Clear();
        }

        protected override int GetPoolSize(PoolConfiguration configuration)
        {
            return 20;
        }
    }
}
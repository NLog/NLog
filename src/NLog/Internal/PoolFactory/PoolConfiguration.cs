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
using System.Collections.Generic;
using NLog.Common;

namespace NLog.Internal.PoolFactory
{
    /// <summary>
    /// Handles configuration of new pools, and also keep track of active pools and can provide statistics.
    /// 
    /// Keeps a WeakReference to the pools created, so if Logger or Target is no
    /// longer used then the pool is automatically released.
    /// </summary>
    internal class PoolConfiguration
    {
#if NET4_5
        private readonly List<WeakReference<ILogEventObjectFactory>> poolInstances = new List<WeakReference<ILogEventObjectFactory>>();
#else
        private readonly List<WeakReference> poolInstances = new List<WeakReference>();
#endif

        public void ConfigurePool(ref ILogEventObjectFactory pool, string ownerName, PoolSetup poolSetup, bool ownerLogger, int ownerQueueLength)
        {
            if (pool != null && pool.PoolSetup != poolSetup)
            {
                for (int i = poolInstances.Count - 1; i >= 0; --i)
                {
                    ILogEventObjectFactory poolItem;
#if NET4_5
                    if (!poolInstances[i].TryGetTarget(out poolItem))
                        poolInstances.RemoveAt(i);
#else
                    poolItem = poolInstances[i].Target as ILogEventObjectFactory;
                    if (poolItem == null)
                        poolInstances.RemoveAt(i);
#endif
                    if (poolItem == pool)
                    {
                        poolInstances.RemoveAt(i);
                        break;
                    }
                }
                pool = null;
            }
            if (pool == null)
            {
                if (poolSetup != PoolSetup.None)
                {
                    pool = new LogEventPoolFactory(ownerName, poolSetup, ownerLogger, ownerQueueLength);
#if NET4_5
                    poolInstances.Add(new WeakReference<ILogEventObjectFactory>(pool));
#else
                    poolInstances.Add(new WeakReference(pool));
#endif
                }
                else
                {
                    pool = LogEventObjectFactory.Instance;
                }
            }
        }

        public void GetPoolStatistics(System.Text.StringBuilder builder)
        {
            foreach (var poolRef in poolInstances)
            {
                ILogEventObjectFactory poolItem;
#if NET4_5
                if (poolRef.TryGetTarget(out poolItem))
#else
                poolItem = poolRef.Target as ILogEventObjectFactory;
                if (poolItem != null)
#endif
                {
                    if (builder.Length > 0)
                        builder.AppendLine();
                    poolItem.GetPoolsStats(builder);
                }
            }
        }

    }
}

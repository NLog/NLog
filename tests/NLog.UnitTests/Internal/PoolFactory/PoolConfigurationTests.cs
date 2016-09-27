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
using System.Text;
using NLog.Internal.PoolFactory;
using Xunit;

namespace NLog.UnitTests.Internal.PoolFactory
{
    public class PoolConfigurationTests : NLogTestBase
    {
        private List<T> TestPoolTwice<T>(ILogEventObjectFactory pool, Func<ILogEventObjectFactory,T> createObject, Action<ILogEventObjectFactory, T> releaseObject, int objectCount) where T : class
        {
            List<T> createdItems = new List<T>();
            for (int i = 0; i < objectCount; ++i)
            {
                createdItems.Add(createObject(pool));
            }
            for (int i = createdItems.Count -1; i >= 0; --i)
            {
                releaseObject(pool, createdItems[i]);
            }
            for (int i = 0; i < objectCount; ++i)
            {
                T item = createObject(pool);
                if (!ReferenceEquals(createdItems[i], item))
                    createdItems.Add(item);
            }
            return createdItems;
        }

        [Fact]
        public void TestPoolSetupNone()
        {
            PoolConfiguration poolConfig = new PoolConfiguration();

            ILogEventObjectFactory pool = null;
            poolConfig.ConfigurePool(ref pool, "Test", NLog.Common.PoolSetup.None, false, 0);
            Assert.NotNull(pool);
            Assert.Equal(pool.PoolSetup, NLog.Common.PoolSetup.None);
            var createdItems = TestPoolTwice(pool, (p) => p.CreateLogEvent(LogLevel.Off, "", null, "", null, null), (p,obj) => p.ReleaseLogEvent(obj), 2);
            Assert.Equal(4, createdItems.Count);

            StringBuilder sb = new StringBuilder();
            poolConfig.GetPoolStatistics(sb);
            Assert.Equal(0, sb.Length);
        }

        [Fact]
        public void TestPoolSetupActive()
        {
            PoolConfiguration poolConfig = new PoolConfiguration();
            
            ILogEventObjectFactory pool = null;
            poolConfig.ConfigurePool(ref pool, "Test", NLog.Common.PoolSetup.Active, false, 0);

            Assert.NotNull(pool);
            Assert.Equal(pool.PoolSetup, NLog.Common.PoolSetup.Active);
            var createdItems = TestPoolTwice(pool, (p) => p.CreateLogEvent(LogLevel.Off, "", null, "", null, null), (p, obj) => p.ReleaseLogEvent(obj), 2);
            Assert.Equal(2, createdItems.Count);

            StringBuilder sb = new StringBuilder();
            poolConfig.GetPoolStatistics(sb);
            Assert.NotEqual(0, sb.Length);  // Report with Name and numbers
            sb.Clear();

            poolConfig.GetPoolStatistics(sb);
            Assert.NotEqual(0, sb.Length);  // Report without numbers
        }

#if MONO || SILVERLIGHT
        [Fact(Skip="Not working under MONO / Silverlight - Probably the forced GC calls")]
#else
        [Fact]
#endif
        public void TestPoolWeakReference()
        {
            PoolConfiguration poolConfig = new PoolConfiguration();

            ILogEventObjectFactory pool = null;
            poolConfig.ConfigurePool(ref pool, "Test", NLog.Common.PoolSetup.Active, false, 0);

            Assert.NotNull(pool);
            Assert.Equal(pool.PoolSetup, NLog.Common.PoolSetup.Active);
            var createdItems = TestPoolTwice(pool, (p) => p.CreateLogEvent(LogLevel.Off, "", null, "", null, null), (p, obj) => p.ReleaseLogEvent(obj), 2);
            Assert.Equal(2, createdItems.Count);

            createdItems.Clear();
            pool = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.WaitForFullGCComplete();
            GC.Collect();

            StringBuilder sb = new StringBuilder();
            poolConfig.GetPoolStatistics(sb);
            Assert.Equal(0, sb.Length); // Empty Report
        }
    }
}

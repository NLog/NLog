// 
// Copyright (c) 2004-2020 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

using NLog.Internal;
using Xunit;

namespace NLog.UnitTests.Internal
{
    public class MruCacheTests
    {
        [Fact]
        public void SimpleCacheAddAndLookupTest()
        {
            MruCache<int, string> mruCache = new MruCache<int, string>(100);
            for (int i = 0; i < 100; ++i)
                mruCache.TryAddValue(i, i.ToString());

            string value;
            for (int i = 0; i < 100; ++i)
            {
                Assert.True(mruCache.TryGetValue(i, out value));
                Assert.Equal(i.ToString(), value);
            }

            Assert.False(mruCache.TryGetValue(101, out value));
        }

        [Fact]
        public void OverflowCacheAndLookupTest()
        {
            MruCache<int, string> mruCache = new MruCache<int, string>(100);
            for (int i = 0; i < 200; ++i)
                mruCache.TryAddValue(i, i.ToString());

            string value;
            for (int i = 0; i < 100; ++i)
            {
                Assert.False(mruCache.TryGetValue(i, out value));
            }

            for (int i = 140; i < 200; ++i)
            {
                Assert.True(mruCache.TryGetValue(i, out value));
                Assert.Equal(i.ToString(), value);
            }
        }

        [Fact]
        public void OverflowVersionCacheAndLookupTest()
        {
            string value;
            MruCache<int, string> mruCache = new MruCache<int, string>(100);
            for (int i = 0; i < 200; ++i)
            {
                mruCache.TryAddValue(i, i.ToString());
                Assert.True(mruCache.TryGetValue(i, out value));    // No longer a virgin
                Assert.Equal(i.ToString(), value);
            }

            for (int i = 0; i < 90; ++i)
            {
                Assert.False(mruCache.TryGetValue(i, out value));
            }

            for (int i = 140; i < 200; ++i)
            {
                Assert.True(mruCache.TryGetValue(i, out value));
                Assert.Equal(i.ToString(), value);
            }
        }

        [Fact]
        public void OverflowFreshCacheAndLookupTest()
        {
            string value;
            MruCache<int, string> mruCache = new MruCache<int, string>(100);
            for (int i = 0; i < 200; ++i)
            {
                mruCache.TryAddValue(i, i.ToString());
                Assert.True(mruCache.TryGetValue(i, out value));    // No longer a virgin
                Assert.Equal(i.ToString(), value);
            }

            for (int j = 0; j < 2; ++j)
            {
                for (int i = 110; i < 200; ++i)
                {
                    if (!mruCache.TryGetValue(i, out value))
                    {
                        mruCache.TryAddValue(i, i.ToString());
                        Assert.True(mruCache.TryGetValue(i, out value));
                    }
                }
            }

            for (int i = 300; i < 310; ++i)
            {
                mruCache.TryAddValue(i, i.ToString());
            }

            int cacheCount = 0;
            for (int i = 110; i < 200; ++i)
            {
                if (mruCache.TryGetValue(i, out value))
                    ++cacheCount;
            }

            Assert.True(cacheCount > 60);   // See that old cache was not killed
        }

        [Fact]
        public void RecentlyUsedLookupTest()
        {
            string value;

            MruCache<int, string> mruCache = new MruCache<int, string>(100);
            for (int i = 0; i < 200; ++i)
            {
                mruCache.TryAddValue(i, i.ToString());
                for (int j = 0; j < i; j += 10)
                {
                    Assert.True(mruCache.TryGetValue(j, out value));
                    Assert.Equal(j.ToString(), value);
                }
            }

            for (int j = 0; j < 100; j += 10)
            {
                Assert.True(mruCache.TryGetValue(j, out value));
                Assert.Equal(j.ToString(), value);
            }

            for (int i = 170; i < 200; ++i)
            {
                Assert.True(mruCache.TryGetValue(i, out value));
                Assert.Equal(i.ToString(), value);
            }
        }
    }
}

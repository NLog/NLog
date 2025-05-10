//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.UnitTests.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NLog.Internal;
    using Xunit;

    public class SortHelpersTests
    {
        [Fact]
        public void SingleBucketDictionary_NoBucketTest()
        {
            SortHelpers.ReadOnlySingleBucketGroupBy<string, IList<string>> dict = new SortHelpers.ReadOnlySingleBucketGroupBy<string, IList<string>>();
            Assert.Empty(dict);

            Assert.Empty(dict);
            Assert.Equal(0, dict.Count(val => val.Key == "Bucket1"));

            foreach (var _ in dict)
                Assert.False(true);
        }

        [Fact]
        public void SingleBucketDictionary_OneBucketEmptyTest()
        {
            IList<string> bucket = ArrayHelper.Empty<string>();
            SortHelpers.ReadOnlySingleBucketGroupBy<string, IList<string>> dict = new SortHelpers.ReadOnlySingleBucketGroupBy<string, IList<string>>(new KeyValuePair<string, IList<string>>("Bucket1", bucket));
            Assert.Single(dict);

            Assert.Contains(new KeyValuePair<string, IList<string>>("Bucket1", bucket), dict);
            Assert.DoesNotContain(new KeyValuePair<string, IList<string>>("Bucket1", null), dict);
            Assert.DoesNotContain(new KeyValuePair<string, IList<string>>(string.Empty, bucket), dict);

            Assert.Equal(1, dict.Count(val => val.Key == "Bucket1"));

            foreach (var item in dict)
            {
                Assert.Equal("Bucket1", item.Key);
                Assert.Empty(item.Value);
            }
        }

        [Fact]
        public void SingleBucketDictionary_OneBucketOneItem()
        {
            IList<string> bucket = new string[] { "Bucket1Item1" };
            SortHelpers.ReadOnlySingleBucketGroupBy<string, IList<string>> dict = new SortHelpers.ReadOnlySingleBucketGroupBy<string, IList<string>>(new KeyValuePair<string, IList<string>>("Bucket1", bucket));
            Assert.Single(dict);

            Assert.Contains(new KeyValuePair<string, IList<string>>("Bucket1", bucket), dict);
            Assert.DoesNotContain(new KeyValuePair<string, IList<string>>("Bucket1", null), dict);
            Assert.DoesNotContain(new KeyValuePair<string, IList<string>>(string.Empty, bucket), dict);

            Assert.Single(dict);
            Assert.Equal(1, dict.Count(val => val.Key == "Bucket1"));

            foreach (var item in dict)
            {
                Assert.Equal("Bucket1", item.Key);
                Assert.Single(item.Value);
                Assert.Equal("Bucket1Item1", item.Value[0]);
            }
        }

        [Fact]
        public void SingleBucketDictionary_OneBucketTwoItemsTest()
        {
            IList<string> bucket = new string[] { "Bucket1Item1", "Bucket1Item2" };
            SortHelpers.ReadOnlySingleBucketGroupBy<string, IList<string>> dict = new SortHelpers.ReadOnlySingleBucketGroupBy<string, IList<string>>(new KeyValuePair<string, IList<string>>("Bucket1", bucket));
            Assert.Single(dict);

            Assert.Contains(new KeyValuePair<string, IList<string>>("Bucket1", bucket), dict);
            Assert.DoesNotContain(new KeyValuePair<string, IList<string>>("Bucket1", null), dict);
            Assert.DoesNotContain(new KeyValuePair<string, IList<string>>(string.Empty, bucket), dict);

            Assert.Single(dict);
            Assert.Equal(1, dict.Count(val => val.Key == "Bucket1"));

            foreach (var item in dict)
            {
                Assert.Equal("Bucket1", item.Key);
                Assert.Equal(2, item.Value.Count);
                Assert.Equal("Bucket1Item1", item.Value[0]);
                Assert.Equal("Bucket1Item2", item.Value[1]);
            }
        }

        [Fact]
        public void SingleBucketDictionary_TwoBucketEmptyTest()
        {
            IList<string> bucket1 = ArrayHelper.Empty<string>();
            IList<string> bucket2 = ArrayHelper.Empty<string>();
            Dictionary<string, IList<string>> buckets = new Dictionary<string, IList<string>>();
            buckets["Bucket1"] = bucket1;
            buckets["Bucket2"] = bucket2;

            SortHelpers.ReadOnlySingleBucketGroupBy<string, IList<string>> dict = new SortHelpers.ReadOnlySingleBucketGroupBy<string, IList<string>>(buckets);
            Assert.Equal(2, dict.Count);

            Assert.Contains(new KeyValuePair<string, IList<string>>("Bucket1", bucket1), dict);
            Assert.DoesNotContain(new KeyValuePair<string, IList<string>>("Bucket1", null), dict);
            Assert.DoesNotContain(new KeyValuePair<string, IList<string>>(string.Empty, bucket1), dict);

            Assert.Contains(new KeyValuePair<string, IList<string>>("Bucket2", bucket2), dict);
            Assert.DoesNotContain(new KeyValuePair<string, IList<string>>("Bucket2", null), dict);
            Assert.DoesNotContain(new KeyValuePair<string, IList<string>>(string.Empty, bucket2), dict);

            Assert.Equal(2, dict.Count());
            Assert.Equal(1, dict.Count(val => val.Key == "Bucket1"));
            Assert.Equal(1, dict.Count(val => val.Key == "Bucket2"));

            foreach (var item in dict)
            {
                Assert.True(item.Key == "Bucket1" || item.Key == "Bucket2");
                Assert.Empty(item.Value);
            }
        }

        [Fact]
        public void SingleBucketDictionary_TwoBuckettOneItemTest()
        {
            IList<string> bucket1 = new string[] { "Bucket1Item1" };
            IList<string> bucket2 = new string[] { "Bucket1Item1" };
            Dictionary<string, IList<string>> buckets = new Dictionary<string, IList<string>>();
            buckets["Bucket1"] = bucket1;
            buckets["Bucket2"] = bucket2;

            SortHelpers.ReadOnlySingleBucketGroupBy<string, IList<string>> dict = new SortHelpers.ReadOnlySingleBucketGroupBy<string, IList<string>>(buckets);
            Assert.Equal(2, dict.Count);

            Assert.Contains(new KeyValuePair<string, IList<string>>("Bucket1", bucket1), dict);
            Assert.DoesNotContain(new KeyValuePair<string, IList<string>>("Bucket1", null), dict);
            Assert.DoesNotContain(new KeyValuePair<string, IList<string>>(string.Empty, bucket1), dict);

            Assert.Contains(new KeyValuePair<string, IList<string>>("Bucket2", bucket2), dict);
            Assert.DoesNotContain(new KeyValuePair<string, IList<string>>("Bucket2", null), dict);
            Assert.DoesNotContain(new KeyValuePair<string, IList<string>>(string.Empty, bucket2), dict);

            Assert.Equal(2, dict.Count());
            Assert.Equal(1, dict.Count(val => val.Key == "Bucket1"));
            Assert.Equal(1, dict.Count(val => val.Key == "Bucket2"));

            foreach (var item in dict)
            {
                Assert.True(item.Key == "Bucket1" || item.Key == "Bucket2");
                Assert.Single(item.Value);
                Assert.Equal("Bucket1Item1", item.Value[0]);
            }
        }
    }
}

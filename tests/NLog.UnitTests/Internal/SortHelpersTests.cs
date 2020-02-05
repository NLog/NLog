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

using System;
using System.Linq;
using System.Collections.Generic;
using NLog.Internal;
using Xunit;

namespace NLog.UnitTests.Internal
{
    public class SortHelpersTests
    {
        [Fact]
        public void SingleBucketDictionary_NoBucketTest()
        {
            SortHelpers.ReadOnlySingleBucketDictionary<string, IList<string>> dict = new SortHelpers.ReadOnlySingleBucketDictionary<string, IList<string>>();
            Assert.Empty(dict);

            Assert.Empty(dict);
            Assert.Equal(0, dict.Count(val => val.Key == "Bucket1"));

            foreach (var _ in dict)
                Assert.False(true);

            Assert.Equal(0, dict.Keys.Count);
            foreach (var _ in dict.Keys)
                Assert.False(true);

            Assert.Equal(0, dict.Values.Count);
            foreach (var _ in dict.Values)
                Assert.False(true);

            IList<string> bucket;
            Assert.False(dict.TryGetValue("Bucket1", out bucket) || bucket != null);
            Assert.False(dict.TryGetValue(string.Empty, out bucket) || bucket != null);
            Assert.False(dict.TryGetValue(null, out bucket) || bucket != null);

            Assert.Throws<NotSupportedException>(() => dict[string.Empty] = new string[0]);
        }

        [Fact]
        public void SingleBucketDictionary_OneBucketEmptyTest()
        {
            IList<string> bucket = new string[0];
            SortHelpers.ReadOnlySingleBucketDictionary<string, IList<string>> dict = new SortHelpers.ReadOnlySingleBucketDictionary<string, IList<string>>(new KeyValuePair<string, IList<string>>("Bucket1", bucket));
            Assert.Single(dict);

            Assert.Contains(new KeyValuePair<string, IList<string>>("Bucket1", bucket), dict);
            Assert.DoesNotContain(new KeyValuePair<string, IList<string>>("Bucket1", null), dict);
            Assert.DoesNotContain(new KeyValuePair<string, IList<string>>(string.Empty, bucket), dict);

            Assert.True(dict.ContainsKey("Bucket1"));
            Assert.False(dict.ContainsKey(string.Empty));

            KeyValuePair<string, IList<string>>[] copyToResult = new KeyValuePair<string, IList<string>>[10];
            dict.CopyTo(copyToResult, 0);
            Assert.Equal("Bucket1", copyToResult[0].Key);
            Assert.Equal(0, copyToResult[0].Value.Count);

            Assert.Single(dict);
            Assert.Equal(1, dict.Count(val => val.Key == "Bucket1"));

            foreach (var item in dict)
            {
                Assert.Equal("Bucket1", item.Key);
                Assert.Equal(0, item.Value.Count);
            }

            Assert.Equal(1, dict.Keys.Count);
            foreach (var key in dict.Keys)
            {
                Assert.Equal("Bucket1", key);
            }

            Assert.Equal(1, dict.Values.Count);
            foreach (var val in dict.Values)
            {
                Assert.Equal(0, val.Count);
            }

            Assert.Equal(0, dict["Bucket1"].Count);

            Assert.True(dict.TryGetValue("Bucket1", out bucket) && bucket.Count == 0);
            Assert.False(dict.TryGetValue(string.Empty, out bucket) || bucket != null);
            Assert.False(dict.TryGetValue(null, out bucket) || bucket != null);
            Assert.Throws<NotSupportedException>(() => dict[string.Empty] = new string[0]);
        }

        [Fact]
        public void SingleBucketDictionary_OneBucketOneItem()
        {
            IList<string> bucket = new string[] { "Bucket1Item1" };
            SortHelpers.ReadOnlySingleBucketDictionary<string, IList<string>> dict = new SortHelpers.ReadOnlySingleBucketDictionary<string, IList<string>>(new KeyValuePair<string, IList<string>>("Bucket1", bucket));
            Assert.Single(dict);

            Assert.Contains(new KeyValuePair<string, IList<string>>("Bucket1", bucket), dict);
            Assert.DoesNotContain(new KeyValuePair<string, IList<string>>("Bucket1", null), dict);
            Assert.DoesNotContain(new KeyValuePair<string, IList<string>>(string.Empty, bucket), dict);

            Assert.True(dict.ContainsKey("Bucket1"));
            Assert.False(dict.ContainsKey(string.Empty));

            KeyValuePair<string, IList<string>>[] copyToResult = new KeyValuePair<string, IList<string>>[10];
            dict.CopyTo(copyToResult, 0);
            Assert.Equal("Bucket1", copyToResult[0].Key);
            Assert.Equal(1, copyToResult[0].Value.Count);
            Assert.Equal("Bucket1Item1", copyToResult[0].Value[0]);

            Assert.Single(dict);
            Assert.Equal(1, dict.Count(val => val.Key == "Bucket1"));

            foreach (var item in dict)
            {
                Assert.Equal("Bucket1", item.Key);
                Assert.Equal(1, item.Value.Count);
                Assert.Equal("Bucket1Item1", item.Value[0]);
            }

            Assert.Equal(1, dict.Keys.Count);
            foreach (var key in dict.Keys)
            {
                Assert.Equal("Bucket1", key);
            }

            Assert.Equal(1, dict.Values.Count);
            foreach (var val in dict.Values)
            {
                Assert.Equal(1, val.Count);
                Assert.Equal("Bucket1Item1", val[0]);
            }

            Assert.Equal(1, dict["Bucket1"].Count);
            Assert.True(dict.TryGetValue("Bucket1", out bucket) && bucket.Count == 1);
            Assert.False(dict.TryGetValue(string.Empty, out bucket) || bucket != null);
            Assert.False(dict.TryGetValue(null, out bucket) || bucket != null);
            Assert.Throws<NotSupportedException>(() => dict[string.Empty] = new string[0]);
        }

        [Fact]
        public void SingleBucketDictionary_OneBucketTwoItemsTest()
        {
            IList<string> bucket = new string[] { "Bucket1Item1", "Bucket1Item2" };
            SortHelpers.ReadOnlySingleBucketDictionary<string, IList<string>> dict = new SortHelpers.ReadOnlySingleBucketDictionary<string, IList<string>>(new KeyValuePair<string, IList<string>>("Bucket1", bucket));
            Assert.Single(dict);

            Assert.Contains(new KeyValuePair<string, IList<string>>("Bucket1", bucket), dict);
            Assert.DoesNotContain(new KeyValuePair<string, IList<string>>("Bucket1", null), dict);
            Assert.DoesNotContain(new KeyValuePair<string, IList<string>>(string.Empty, bucket), dict);

            Assert.True(dict.ContainsKey("Bucket1"));
            Assert.False(dict.ContainsKey(string.Empty));

            KeyValuePair<string, IList<string>>[] copyToResult = new KeyValuePair<string, IList<string>>[10];
            dict.CopyTo(copyToResult, 0);
            Assert.Equal("Bucket1", copyToResult[0].Key);
            Assert.Equal(2, copyToResult[0].Value.Count);
            Assert.Equal("Bucket1Item1", copyToResult[0].Value[0]);
            Assert.Equal("Bucket1Item2", copyToResult[0].Value[1]);

            Assert.Single(dict);
            Assert.Equal(1, dict.Count(val => val.Key == "Bucket1"));

            foreach (var item in dict)
            {
                Assert.Equal("Bucket1", item.Key);
                Assert.Equal(2, item.Value.Count);
                Assert.Equal("Bucket1Item1", item.Value[0]);
                Assert.Equal("Bucket1Item2", item.Value[1]);
            }

            Assert.Equal(1, dict.Keys.Count);
            foreach (var key in dict.Keys)
            {
                Assert.Equal("Bucket1", key);
            }

            Assert.Equal(1, dict.Values.Count);
            foreach (var val in dict.Values)
            {
                Assert.Equal(2, val.Count);
                Assert.Equal("Bucket1Item1", val[0]);
                Assert.Equal("Bucket1Item2", val[1]);
            }

            Assert.Equal(2, dict["Bucket1"].Count);
            Assert.True(dict.TryGetValue("Bucket1", out bucket) && bucket.Count == 2);
            Assert.False(dict.TryGetValue(string.Empty, out bucket) || bucket != null);
            Assert.False(dict.TryGetValue(null, out bucket) || bucket != null);
            Assert.Throws<NotSupportedException>(() => dict[string.Empty] = new string[0]);
        }

        [Fact]
        public void SingleBucketDictionary_TwoBucketEmptyTest()
        {
            IList<string> bucket1 = new string[0];
            IList<string> bucket2 = new string[0];
            Dictionary<string, IList<string>> buckets = new Dictionary<string, IList<string>>();
            buckets["Bucket1"] = bucket1;
            buckets["Bucket2"] = bucket2;

            SortHelpers.ReadOnlySingleBucketDictionary<string, IList<string>> dict = new SortHelpers.ReadOnlySingleBucketDictionary<string, IList<string>>(buckets);
            Assert.Equal(2, dict.Count);

            Assert.Contains(new KeyValuePair<string, IList<string>>("Bucket1", bucket1), dict);
            Assert.DoesNotContain(new KeyValuePair<string, IList<string>>("Bucket1", null), dict);
            Assert.DoesNotContain(new KeyValuePair<string, IList<string>>(string.Empty, bucket1), dict);

            Assert.Contains(new KeyValuePair<string, IList<string>>("Bucket2", bucket2), dict);
            Assert.DoesNotContain(new KeyValuePair<string, IList<string>>("Bucket2", null), dict);
            Assert.DoesNotContain(new KeyValuePair<string, IList<string>>(string.Empty, bucket2), dict);

            Assert.True(dict.ContainsKey("Bucket1"));
            Assert.True(dict.ContainsKey("Bucket2"));
            Assert.False(dict.ContainsKey(string.Empty));

            KeyValuePair<string, IList<string>>[] copyToResult = new KeyValuePair<string, IList<string>>[10];
            dict.CopyTo(copyToResult, 0);
            Assert.Equal("Bucket1", copyToResult[0].Key);
            Assert.Equal("Bucket2", copyToResult[1].Key);
            Assert.Equal(0, copyToResult[0].Value.Count);
            Assert.Equal(0, copyToResult[1].Value.Count);

            Assert.Equal(2, dict.Count());
            Assert.Equal(1, dict.Count(val => val.Key == "Bucket1"));
            Assert.Equal(1, dict.Count(val => val.Key == "Bucket2"));

            foreach (var item in dict)
            {
                Assert.True(item.Key == "Bucket1" || item.Key == "Bucket2");
                Assert.Equal(0, item.Value.Count);
            }

            Assert.Equal(2, dict.Keys.Count);
            foreach (var key in dict.Keys)
            {
                Assert.True(key == "Bucket1" || key == "Bucket2");
            }

            Assert.Equal(2, dict.Values.Count);
            foreach (var val in dict.Values)
            {
                Assert.Equal(0, val.Count);
            }

            Assert.Equal(0, dict["Bucket1"].Count);
            Assert.Equal(0, dict["Bucket2"].Count);
            Assert.True(dict.TryGetValue("Bucket1", out bucket1) && bucket1.Count == 0);
            Assert.True(dict.TryGetValue("Bucket2", out bucket2) && bucket2.Count == 0);
            Assert.Throws<NotSupportedException>(() => dict[string.Empty] = new string[0]);
        }

        [Fact]
        public void SingleBucketDictionary_TwoBuckettOneItemTest()
        {
            IList<string> bucket1 = new string[] { "Bucket1Item1" };
            IList<string> bucket2 = new string[] { "Bucket1Item1" };
            Dictionary<string, IList<string>> buckets = new Dictionary<string, IList<string>>();
            buckets["Bucket1"] = bucket1;
            buckets["Bucket2"] = bucket2;

            SortHelpers.ReadOnlySingleBucketDictionary<string, IList<string>> dict = new SortHelpers.ReadOnlySingleBucketDictionary<string, IList<string>>(buckets);
            Assert.Equal(2, dict.Count);

            Assert.Contains(new KeyValuePair<string, IList<string>>("Bucket1", bucket1), dict);
            Assert.DoesNotContain(new KeyValuePair<string, IList<string>>("Bucket1", null), dict);
            Assert.DoesNotContain(new KeyValuePair<string, IList<string>>(string.Empty, bucket1), dict);

            Assert.Contains(new KeyValuePair<string, IList<string>>("Bucket2", bucket2), dict);
            Assert.DoesNotContain(new KeyValuePair<string, IList<string>>("Bucket2", null), dict);
            Assert.DoesNotContain(new KeyValuePair<string, IList<string>>(string.Empty, bucket2), dict);

            Assert.True(dict.ContainsKey("Bucket1"));
            Assert.True(dict.ContainsKey("Bucket2"));
            Assert.False(dict.ContainsKey(string.Empty));

            KeyValuePair<string, IList<string>>[] copyToResult = new KeyValuePair<string, IList<string>>[10];
            dict.CopyTo(copyToResult, 0);
            Assert.Equal("Bucket1", copyToResult[0].Key);
            Assert.Equal("Bucket2", copyToResult[1].Key);
            Assert.Equal(1, copyToResult[0].Value.Count);
            Assert.Equal(1, copyToResult[1].Value.Count);

            Assert.Equal(2, dict.Count());
            Assert.Equal(1, dict.Count(val => val.Key == "Bucket1"));
            Assert.Equal(1, dict.Count(val => val.Key == "Bucket2"));

            foreach (var item in dict)
            {
                Assert.True(item.Key == "Bucket1" || item.Key == "Bucket2");
                Assert.Equal(1, item.Value.Count);
                Assert.Equal("Bucket1Item1", item.Value[0]);
            }

            Assert.Equal(2, dict.Keys.Count);
            foreach (var key in dict.Keys)
            {
                Assert.True(key == "Bucket1" || key == "Bucket2");
            }

            Assert.Equal(2, dict.Values.Count);
            foreach (var val in dict.Values)
            {
                Assert.Equal(1, val.Count);
                Assert.Equal("Bucket1Item1", val[0]);
            }

            Assert.Equal(1, dict["Bucket1"].Count);
            Assert.Equal(1, dict["Bucket2"].Count);
            Assert.True(dict.TryGetValue("Bucket1", out bucket1) && bucket1.Count == 1);
            Assert.True(dict.TryGetValue("Bucket2", out bucket2) && bucket2.Count == 1);
            Assert.Throws<NotSupportedException>(() => dict[string.Empty] = new string[0]);
        }
    }
}
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

using System.Globalization;
using NLog.Config;
using UrlHelper = NLog.Internal.UrlHelper;

namespace NLog.UnitTests.Targets
{
    using NLog.Targets;
    using Xunit;
    using Xunit.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Collections;
    using System.Text.RegularExpressions;

    public class DefaultJsonSerializerTests : NLogTestBase
    {
        private DefaultJsonSerializer _serializer;

        public DefaultJsonSerializerTests()
        {
            _serializer = DefaultJsonSerializer.Instance;
        }

        [Fact]
        public void SingleLineString_Test()
        {
            var text = "This is, sort of, surprising the 1. time you see that test-result.";
            var expected = "\"This is, sort of, surprising the 1. time you see that test-result.\"";

            var actual = _serializer.SerializeObject(text);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void MultiLineString_Test()
        {
            var text = "First line followed by Windows line break\r\nNow this is second with UNIX\nand third at last";
            var expected = "\"First line followed by Windows line break\\r\\nNow this is second with UNIX\\nand third at last\"";

            var actual = _serializer.SerializeObject(text);
            Assert.Equal(expected, actual);
        }


        [Fact]
        public void StringWithTabBackSpaceFormfeed_Test()
        {
            var text = "A tab\tis followed by a feed\fand finally cancel last character\b";
            var expected = "\"A tab\\tis followed by a feed\\fand finally cancel last character\\b\"";

            var actual = _serializer.SerializeObject(text);
            Assert.Equal(expected, actual);
        }


        [Fact]
        public void StringWithSlashAndQuotes_Test()
        {
            var text = "This sentence/text is \"normal\", we think.";
            var expected = "\"This sentence\\/text is \\\"normal\\\", we think.\"";

            var actual = _serializer.SerializeObject(text);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SerializeUnicode_test()
        {
            var actual = _serializer.SerializeObject("©");
            Assert.Equal("\"©\"", actual);
        }

        [Fact]
        public void SerializeUnicodeInAnomObject_test()
        {
            var item = new
            {
                text = "©"
            };

            var actual = _serializer.SerializeObject(item);
            Assert.Equal("{\"text\":\"©\"}", actual);
        }

        [Fact]
        public void ReferenceLoopInDictionary_Test()
        {
            var d = new Dictionary<string, object>();
            d.Add("First", 17);
            d.Add("Loop", d);
            var target = new Dictionary<string, object>
            {
                {"Name", "TestObject" },
                {"Assets" , d }
            };

            var expected = "{\"Name\":\"TestObject\",\"Assets\":{\"First\":17}}";
            var actual = _serializer.SerializeObject(target);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ReferenceLoopInList_Test()
        {
            var d = new List<object>();
            d.Add(17);
            d.Add(d);
            d.Add(3.14);
            var target = new List<object>
            {
                {"TestObject" },
                {d }
            };

            var expected = "[\"TestObject\",[17,3.14]]";
            var actual = _serializer.SerializeObject(target);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void InfiniteLoop_Test()
        {
            var d = new TestList();
            var actual = _serializer.SerializeObject(d);

            var cnt = Regex.Matches(actual, "\\[\"alpha\",\"bravo\"\\]").Count;
            Assert.Equal(9, cnt);       // maximum level is 10 => 10 recursion is skipped => count is 10 - 1    
        }

        [Fact]
        public void StringWithMixedControlCharacters_Test()
        {
            var text = "First\\Second\tand" + (char)3 + "for" + (char)0x1f + "with" + (char)0x10 + "but" + (char)0x0d + "and no" + (char)0x20;
            var expected = "\"First\\\\Second\\tand\\u0003for\\u001fwith\\u0010but\\rand no \"";

            var actual = _serializer.SerializeObject(text);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData((short)177, "177")]
        [InlineData((ushort)177, "177")]
        [InlineData((int)177, "177")]
        [InlineData((uint)177, "177")]
        [InlineData((long)32711520331, "32711520331")]
        [InlineData((ulong)32711520331, "32711520331")]
        [InlineData(3.14159265, "3.14159265")]
        [InlineData(2776145.7743, "2776145.7743")]
        [InlineData(double.NaN, "NaN")]
        [InlineData(double.PositiveInfinity, "Infinity")]
        public void SerializeNumber_Test(object o, string expected)
        {
            var actual = _serializer.SerializeObject(o);
            Assert.Equal(expected, actual);

            var sb = new System.Text.StringBuilder();
            _serializer.SerializeObject(o, sb);
            Assert.Equal(expected, sb.ToString());
        }

        [Theory]
        [InlineData((int)177, "177.00")]
        [InlineData((long)32711520331, "32711520331.00")]
        [InlineData(3.14159265, "3.14")]
        [InlineData(2776145.7743, "2776145.77")]
        public void SerializeNumber_format_Test(object o, string expected)
        {
            var actual = _serializer.SerializeObject(o, new JsonSerializeOptions() { Format = "N2" });
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData((int)177, "177")]
        [InlineData((long)32711520331, "32711520331")]
        [InlineData(3.14159265, "3,14159265")]
        [InlineData(2776145.7743, "2776145,7743")]
        public void SerializeNumber_nl_Test(object o, string expected)
        {
            var actual = _serializer.SerializeObject(o, new JsonSerializeOptions() { FormatProvider = new CultureInfo("nl-nl") });
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData((int)177, "177,00")]
        [InlineData((long)32711520331, "32.711.520.331,00")]
        [InlineData(3.14159265, "3,14")]
        [InlineData(2776145.7743, "2.776.145,77")]
        public void SerializeNumber_formatNL_Test(object o, string expected)
        {
            var actual = _serializer.SerializeObject(o, new JsonSerializeOptions() { Format = "N2", FormatProvider = new CultureInfo("nl-nl") });
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SerializeBool_Test()
        {
            var actual = _serializer.SerializeObject(true);
            Assert.Equal("true", actual);
            actual = _serializer.SerializeObject(false);
            Assert.Equal("false", actual);
        }

        [Fact]
        public void SerializeDateTime_Test()
        {
            DateTime utcNow = DateTime.UtcNow;
            utcNow = utcNow.AddTicks(-utcNow.Ticks % TimeSpan.TicksPerSecond);
            var actual = _serializer.SerializeObject(utcNow);
            Assert.Equal("\"" + utcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture) + "\"", actual);
        }

        [Fact]
        public void SerializeDateTime_Test2()
        {
            var val = new DateTime(2016, 12, 31);
            var actual = _serializer.SerializeObject(val);
            Assert.Equal("\"" + "2016-12-31T00:00:00Z" + "\"", actual);
        }

        [Fact]
        public void SerializeDateTime_isoformat_Test()
        {
            var val = new DateTime(2016, 12, 31);
            var actual = _serializer.SerializeObject(val, new JsonSerializeOptions { Format = "O" });
            Assert.Equal("\"2016-12-31T00:00:00.0000000\"", actual);
        }

        [Fact]
        public void SerializeDateTime_format_Test()
        {
            DateTime utcNow = DateTime.UtcNow;
            utcNow = utcNow.AddTicks(-utcNow.Ticks % TimeSpan.TicksPerSecond);
            var actual = _serializer.SerializeObject(utcNow, new JsonSerializeOptions { Format = "dddd d M" });
            Assert.Equal("\"" + utcNow.ToString("dddd d M", System.Globalization.CultureInfo.InvariantCulture) + "\"", actual);
        }

        [Fact]
        public void SerializeDateTime_formatNl_Test()
        {
            DateTime utcNow = DateTime.UtcNow;
            utcNow = utcNow.AddTicks(-utcNow.Ticks % TimeSpan.TicksPerSecond);
            var actual = _serializer.SerializeObject(utcNow, new JsonSerializeOptions { Format = "dddd d M", FormatProvider = new CultureInfo("nl-nl") });
            Assert.Equal("\"" + utcNow.ToString("dddd d M", new CultureInfo("nl-nl")) + "\"", actual);
        }

        [Fact]
        public void SerializeDateTimeOffset_Test()
        {
            var val = new DateTimeOffset(new DateTime(2016, 12, 31, 2, 30, 59), new TimeSpan(4, 30, 0));
            var actual = _serializer.SerializeObject(val);
            Assert.Equal("\"" + "2016-12-31 02:30:59 +04:30" + "\"", actual);
        }

        [Fact]
        public void SerializeTime_Test()
        {
            var actual = _serializer.SerializeObject(new TimeSpan(1, 2, 3, 4));
            Assert.Equal("\"1.02:03:04\"", actual);
        }

        [Fact]
        public void SerializeTime2_Test()
        {
            var actual = _serializer.SerializeObject(new TimeSpan(0, 2, 3, 4));
            Assert.Equal("\"02:03:04\"", actual);
        }

        [Fact]
        public void SerializeTime3_Test()
        {
            var actual = _serializer.SerializeObject(new TimeSpan(0, 0, 2, 3, 4));
            Assert.Equal("\"00:02:03.0040000\"", actual);
        }

        [Fact]
        public void SerializeEmptyDict_Test()
        {
            var actual = _serializer.SerializeObject(new Dictionary<string, int>());
            Assert.Equal("{}", actual);
        }

        [Fact]
        public void SerializeDict_Test()
        {
            var dictionary = new Dictionary<string, object>();
            dictionary.Add("key1", 13);
            dictionary.Add("key 2", 1.3m);
            var actual = _serializer.SerializeObject(dictionary);
            Assert.Equal("{\"key1\":13,\"key 2\":1.3}", actual);
        }

        [Fact]
        public void SerializeNull_Test()
        {
            var actual = _serializer.SerializeObject(null);
            Assert.Equal("null", actual);
        }

        [Fact]
        public void SerializeGuid_Test()
        {
            Guid newGuid = Guid.NewGuid();
            var actual = _serializer.SerializeObject(newGuid);
            Assert.Equal("\"" + newGuid.ToString() + "\"", actual);
        }


        [Fact]
        public void SerializeEnum_Test()
        {
            var val = ExceptionRenderingFormat.Method;
            var actual = _serializer.SerializeObject(val);
            Assert.Equal("\"Method\"", actual);

            var sb = new System.Text.StringBuilder();
            _serializer.SerializeObject(val, sb);
            Assert.Equal("\"Method\"", sb.ToString());
        }

        [Fact]
        public void SerializeEnumInt_Test()
        {
            var val = ExceptionRenderingFormat.Method;
            var actual = _serializer.SerializeObject(val, new JsonSerializeOptions() { EnumAsInteger = true });
            Assert.Equal("4", actual);

            var sb = new System.Text.StringBuilder();
            _serializer.SerializeObject(val, sb, new JsonSerializeOptions() { EnumAsInteger = true });
            Assert.Equal("4", sb.ToString());
        }

        [Fact]
        public void SerializeFlagEnum_Test()
        {
            var val = UrlHelper.EscapeEncodingFlag.LegacyRfc2396 | UrlHelper.EscapeEncodingFlag.LowerCaseHex;
            var actual = _serializer.SerializeObject(val);
            Assert.Equal("\"LegacyRfc2396, LowerCaseHex\"", actual);

            var sb = new System.Text.StringBuilder();
            _serializer.SerializeObject(val, sb);
            Assert.Equal("\"LegacyRfc2396, LowerCaseHex\"", sb.ToString());
        }

        [Fact]
        public void SerializeObject_Test()
        {
            var object1 = new TestObject("object1");
            var object2 = new TestObject("object2");

            object1.Linked = object2;
            var actual = _serializer.SerializeObject(object1);
            Assert.Equal("{\"Name\":\"object1\", \"Linked\":{\"Name\":\"object2\"}}", actual);
        }

        [Fact]
        public void SerializeObject_noQuote_Test()
        {
            var object1 = new TestObject("object1");
            var object2 = new TestObject("object2");

            object1.Linked = object2;
            var actual = _serializer.SerializeObject(object1, new JsonSerializeOptions { QuoteKeys = false });
            Assert.Equal("{Name:\"object1\", Linked:{Name:\"object2\"}}", actual);
        }

        [Fact]
        public void SerializeRecusiveObject_Test()
        {
            var object1 = new TestObject("object1");

            object1.Linked = object1;
            var actual = _serializer.SerializeObject(object1);
            Assert.Equal("{\"Name\":\"object1\"}", actual);
        }

        [Fact]
        public void SerializeListObject_Test()
        {
            var object1 = new TestObject("object1");
            var object2 = new TestObject("object2");
            object1.Linked = object2;

            var list = new[] { object1, object2 };

            var actual = _serializer.SerializeObject(list);
            Assert.Equal("[{\"Name\":\"object1\", \"Linked\":{\"Name\":\"object2\"}},{\"Name\":\"object2\"}]", actual);
        }

        [Fact]
        public void SerializeNoPropsObject_Test()
        {
            var object1 = new NoPropsObject();
            var actual = _serializer.SerializeObject(object1);
            Assert.Equal("\"something\"", actual);
        }

        [Fact]
        public void SerializeObjectWithExceptionAndPrivateSetter_Test()
        {
            var object1 = new ObjectWithExceptionAndPrivateSetter("test name");
            var actual = _serializer.SerializeObject(object1);
            Assert.Equal("{\"Name\":\"test name\"}", actual);
        }

        [Fact]
        public void SingleItemOptimizedHashSetTest()
        {
            var hashSet = default(NLog.Internal.SingleItemOptimizedHashSet<object>);
            Assert.Equal(0, hashSet.Count);
            Assert.Equal(false, hashSet.Contains(new object()));
            foreach (var obj in hashSet)
                throw new Exception("Wrong");
            hashSet.Clear();
            Assert.Equal(0, hashSet.Count);
            hashSet.Add(new object());
            Assert.Equal(1, hashSet.Count);
            hashSet.Add(new object());
            Assert.Equal(2, hashSet.Count);
            foreach (var obj in hashSet)
                Assert.Equal(true, hashSet.Contains(obj));
            object[] objArray = new object[2];
            hashSet.CopyTo(objArray, 0);
            foreach (var obj in objArray)
            {
                Assert.NotNull(obj);
                hashSet.Remove(obj);
            }
            Assert.Equal(0, hashSet.Count);
            hashSet.Clear();
            Assert.Equal(0, hashSet.Count);
        }

        private class TestObject
        {
            /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
            public TestObject(string name)
            {
                Name = name;
            }

            public string Name { get; set; }

            public TestObject Linked { get; set; }
        }

        public class NoPropsObject
        {
            private string something = "something";

            #region Overrides of Object

            /// <summary>Returns a string that represents the current object.</summary>
            /// <returns>A string that represents the current object.</returns>
            public override string ToString()
            {
                return something;
            }

            #endregion
        }

        private class ObjectWithExceptionAndPrivateSetter
        {
            /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
            public ObjectWithExceptionAndPrivateSetter(string name)
            {
                Name = name;
            }

            public string Name { get; }

            public string SetOnly { private get; set; }

            public object Ex
            {
                get
                {
                    throw new Exception("oops");
                }
            }
        }


        private class TestList : IEnumerable<IEnumerable>
        {
            static List<int> _list1 = new List<int> { 17, 3 };
            static List<string> _list2 = new List<string> { "alpha", "bravo" };

            public IEnumerator<IEnumerable> GetEnumerator()
            {
                yield return _list1;
                yield return _list2;
                yield return new TestList();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }
    }
}


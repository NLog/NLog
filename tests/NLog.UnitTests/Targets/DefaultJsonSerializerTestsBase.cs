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
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Text.RegularExpressions;
using NLog.Config;
using NLog.Internal;
using NLog.Targets;
using Xunit;

namespace NLog.UnitTests.Targets
{
    /// <summary>
    /// Base class for the <see cref="DefaultJsonSerializer"/> (<see cref="IJsonConverter"/> and <see cref="IJsonSerializer"/> interfaces)
    /// </summary>
    public abstract class DefaultJsonSerializerTestsBase : NLogTestBase
    {
        protected abstract string SerializeObject(object o);

        [Fact]
        public void SingleLineString_Test()
        {
            var text = "This is, sort of, surprising the 1. time you see that test-result.";
            var expected = "\"This is, sort of, surprising the 1. time you see that test-result.\"";

            var actual = SerializeObject(text);
            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void MultiLineString_Test()
        {
            var text = "First line followed by Windows line break\r\nNow this is second with UNIX\nand third at last";
            var expected = "\"First line followed by Windows line break\\r\\nNow this is second with UNIX\\nand third at last\"";

            var actual = SerializeObject(text);
            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void StringWithTabBackSpaceFormfeed_Test()
        {
            var text = "A tab\tis followed by a feed\fand finally cancel last character\b";
            var expected = "\"A tab\\tis followed by a feed\\fand finally cancel last character\\b\"";

            var actual = SerializeObject(text);
            Assert.Equal(expected, actual);
        }


        [Fact]
        public void StringWithSlashAndQuotes_Test()
        {
            var text = "This sentence/text is \"normal\", we think.";
            var expected = "\"This sentence/text is \\\"normal\\\", we think.\"";

            var actual = SerializeObject(text);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SerializeUnicode_test()
        {
            var actual = SerializeObject("©");
            Assert.Equal("\"©\"", actual);
        }

        [Fact]
        public void SerializeUnicodeInAnomObject_test()
        {
            var item = new
            {
                text = "©"
            };

            var actual = SerializeObject(item);
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
            var actual = SerializeObject(target);

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
            var actual = SerializeObject(target);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void InfiniteLoop_Test()
        {
            var d = new TestList();
            var actual = SerializeObject(d);

            var cnt = Regex.Matches(actual, "\\[\"alpha\",\"bravo\"\\]").Count;
            Assert.Equal(10, cnt);
        }

        [Fact]
        public void StringWithMixedControlCharacters_Test()
        {
            var text = "First\\Second\tand" + (char)3 + "for" + (char)0x1f + "with" + (char)0x10 + "but" + (char)0x0d + "and no" + (char)0x20;
            var expected = "\"First\\\\Second\\tand\\u0003for\\u001fwith\\u0010but\\rand no \"";

            var actual = SerializeObject(text);
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
        [InlineData(0D, "0.0")]
        [InlineData(0F, "0.0")]
        [InlineData(1D, "1.0")]
        [InlineData(1F, "1.0")]
        [InlineData(-1D, "-1.0")]
        [InlineData(-1F, "-1.0")]
        [InlineData(5e30D, "5E+30")]
        [InlineData(5e30F, "5E+30")]
        [InlineData(-5e30D, "-5E+30")]
        [InlineData(-5e30F, "-5E+30")]
        [InlineData(double.NaN, "\"NaN\"")]
        [InlineData(double.PositiveInfinity, "\"Infinity\"")]
        [InlineData(float.NaN, "\"NaN\"")]
        [InlineData(float.PositiveInfinity, "\"Infinity\"")]
        public void SerializeNumber_Test(object o, string expected)
        {
            var actual = SerializeObject(o);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SerializeBool_Test()
        {
            var actual = SerializeObject(true);
            Assert.Equal("true", actual);
            actual = SerializeObject(false);
            Assert.Equal("false", actual);
        }

        [Fact]
        public void SerializeNumberDecimal_Test()
        {
            var actual = SerializeObject(-1M);
            Assert.Equal("-1.0", actual);

            actual = SerializeObject(0M);
            Assert.Equal("0.0", actual);

            actual = SerializeObject(1M);
            Assert.Equal("1.0", actual);

            actual = SerializeObject(2M);
            Assert.Equal("2.0", actual);

            actual = SerializeObject(3M);
            Assert.Equal("3.0", actual);

            actual = SerializeObject(4M);
            Assert.Equal("4.0", actual);

            actual = SerializeObject(5M);
            Assert.Equal("5.0", actual);

            actual = SerializeObject(6M);
            Assert.Equal("6.0", actual);

            actual = SerializeObject(7M);
            Assert.Equal("7.0", actual);

            actual = SerializeObject(8M);
            Assert.Equal("8.0", actual);

            actual = SerializeObject(9M);
            Assert.Equal("9.0", actual);

            actual = SerializeObject(3.14159265M);
            Assert.Equal("3.14159265", actual);
        }

        [Fact]
        public void SerializeDateTime_Test()
        {
            DateTime utcNow = DateTime.UtcNow;
            utcNow = utcNow.AddTicks(-utcNow.Ticks % TimeSpan.TicksPerSecond);
            var actual = SerializeObject(utcNow);
            Assert.Equal("\"" + utcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture) + "\"", actual);
        }

        [Fact]
        public void SerializeDateTime_Test2()
        {
            var culture = System.Threading.Thread.CurrentThread.CurrentCulture;

            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");    // uses "." instead of ":" for time

                var val = new DateTime(2016, 12, 31);
                var actual = SerializeObject(val);
                Assert.Equal("\"" + "2016-12-31T00:00:00Z" + "\"", actual);
            }
            finally
            {
                // Restore
                System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            }
        }

        [Fact]
        public void SerializeDateTimeOffset_Test()
        {
            var culture = System.Threading.Thread.CurrentThread.CurrentCulture;

            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");    // uses "." instead of ":" for time

                var val = new DateTimeOffset(new DateTime(2016, 12, 31, 2, 30, 59), new TimeSpan(4, 30, 0));
                var actual = SerializeObject(val);
                Assert.Equal("\"" + "2016-12-31 02:30:59 +04:30" + "\"", actual);
            }
            finally
            {
                // Restore
                System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            }
        }

        [Fact]
        public void SerializeTime_Test()
        {
            var actual = SerializeObject(new TimeSpan(1, 2, 3, 4));
            Assert.Equal("\"1.02:03:04\"", actual);
        }

        [Fact]
        public void SerializeTime2_Test()
        {
            var actual = SerializeObject(new TimeSpan(0, 2, 3, 4));
            Assert.Equal("\"02:03:04\"", actual);
        }

        [Fact]
        public void SerializeTime3_Test()
        {
            var culture = System.Threading.Thread.CurrentThread.CurrentCulture;

            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");    // uses "." instead of ":" for time

                var actual = SerializeObject(new TimeSpan(0, 0, 2, 3, 4));
                Assert.Equal("\"00:02:03.0040000\"", actual);
            }
            finally
            {
                // Restore
                System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            }
        }

        [Fact]
        public void SerializeEmptyDict_Test()
        {
            var actual = SerializeObject(new Dictionary<string, int>());
            Assert.Equal("{}", actual);
        }

        [Fact]
        public void SerializeDict_Test()
        {
            var dictionary = new Dictionary<string, object>();
            dictionary.Add("key1", 13);
            dictionary.Add("key 2", 1.3m);
            var actual = SerializeObject(dictionary);
            Assert.Equal("{\"key1\":13,\"key 2\":1.3}", actual);
        }

        [Fact]
        public void SerializeTrickyDict_Test()
        {
            IDictionary<object,object> dictionary = new Internal.TrickyTestDictionary();
            dictionary.Add("key1", 13);
            dictionary.Add("key 2", 1.3m);
            var actual = SerializeObject(dictionary);
            Assert.Equal("{\"key1\":13,\"key 2\":1.3}", actual);
        }

        [Fact]
        public void SerializeExpandoDict_Test()
        {
            IDictionary<string, IConvertible> dictionary = new Internal.ExpandoTestDictionary();
            dictionary.Add("key 2", 1.3m);
            dictionary.Add("level", LogLevel.Info);
            var actual = SerializeObject(dictionary);
            Assert.Equal("{\"key 2\":1.3, \"level\":{\"Name\":\"Info\", \"Ordinal\":2}}", actual);
        }

        [Fact]
        public void SerializEmptyExpandoDict_Test()
        {
            IDictionary<string, IConvertible> dictionary = new Internal.ExpandoTestDictionary();
            var actual = SerializeObject(dictionary);
            Assert.Equal("{}", actual);
        }

#if !NET35 && !NET40
        [Fact]
        public void SerializeReadOnlyExpandoDict_Test()
        {
            var dictionary = new Dictionary<string, object>();
            dictionary.Add("key 2", 1.3m);
            dictionary.Add("level", LogLevel.Info);

            var readonlyDictionary = new Internal.ReadOnlyExpandoTestDictionary(dictionary);
            var actual = SerializeObject(readonlyDictionary);
            Assert.Equal("{\"key 2\":1.3, \"level\":{\"Name\":\"Info\", \"Ordinal\":2}}", actual);
        }
#endif

        [Fact]
        public void SerializeIntegerKeyDict_Test()
        {
            var dictionary = new Dictionary<int, string>();
            dictionary.Add(1, "One");
            dictionary.Add(2, "Two");
            var actual = SerializeObject(dictionary);
            Assert.Equal("{\"1\":\"One\",\"2\":\"Two\"}", actual);
        }

        [Fact]
        public void SerializeEnumKeyDict_Test()
        {
            var dictionary = new Dictionary<ExceptionRenderingFormat, int>();
            dictionary.Add(ExceptionRenderingFormat.Method, 4);
            dictionary.Add(ExceptionRenderingFormat.StackTrace, 5);
            var actual = SerializeObject(dictionary);
            Assert.Equal("{\"Method\":4,\"StackTrace\":5}", actual);
        }

        [Fact]
        public void SerializeObjectKeyDict_Test()
        {
            var dictionary = new Dictionary<object, string>();
            dictionary.Add(new { Name = "Hello" }, "World");
            dictionary.Add(new { Name = "Goodbye" }, "Money");
            var actual = SerializeObject(dictionary);
            Assert.Equal("{\"{ Name = Hello }\":\"World\",\"{ Name = Goodbye }\":\"Money\"}", actual);
        }

        [Fact]
        public void SerializeBadStringKeyDict_Test()
        {
            var dictionary = new Dictionary<string, string>();
            dictionary.Add("\t", "Tab");
            dictionary.Add("\n", "Newline");
            var actual = SerializeObject(dictionary);
            Assert.Equal("{\"\\t\":\"Tab\",\"\\n\":\"Newline\"}", actual);
        }

        [Fact]
        public void SerializeNull_Test()
        {
            var actual = SerializeObject(null);
            Assert.Equal("null", actual);
        }

        [Fact]
        public void SerializeGuid_Test()
        {
            Guid newGuid = Guid.NewGuid();
            var actual = SerializeObject(newGuid);
            Assert.Equal("\"" + newGuid.ToString() + "\"", actual);
        }
        
        [Fact]
        public void SerializeEnum_Test()
        {
            var val = ExceptionRenderingFormat.Method;
            var actual = SerializeObject(val);
            Assert.Equal("\"Method\"", actual);
        }

        [Fact]
        public void SerializeFlagEnum_Test()
        {
            var val = UrlHelper.EscapeEncodingOptions.LegacyRfc2396 | UrlHelper.EscapeEncodingOptions.LowerCaseHex;
            var actual = SerializeObject(val);
            Assert.Equal("\"LegacyRfc2396, LowerCaseHex\"", actual);
        }

        [Fact]
        public void SerializeObject_Test()
        {
            var object1 = new TestObject("object1");
            var object2 = new TestObject("object2");

            object1.Linked = object2;
            var actual = SerializeObject(object1);
            Assert.Equal("{\"Name\":\"object1\", \"Linked\":{\"Name\":\"object2\"}}", actual);
        }

        [Fact]
        public void SerializeRecusiveObject_Test()
        {
            var object1 = new TestObject("object1");

            object1.Linked = object1;
            var actual = SerializeObject(object1);
            Assert.Equal("{\"Name\":\"object1\"}", actual);
        }

        [Fact]
        public void SerializeListObject_Test()
        {
            var object1 = new TestObject("object1");
            var object2 = new TestObject("object2");
            object1.Linked = object2;

            var list = new[] { object1, object2 };

            var actual = SerializeObject(list);
            Assert.Equal("[{\"Name\":\"object1\", \"Linked\":{\"Name\":\"object2\"}},{\"Name\":\"object2\"}]", actual);
        }

        [Fact]
        public void SerializeNoPropsObject_Test()
        {
            var object1 = new NoPropsObject();
            var actual = SerializeObject(object1);
            Assert.Equal("\"something\"", actual);
        }

        [Fact]
        public void SerializeObjectWithExceptionAndPrivateSetter_Test()
        {
            var object1 = new ObjectWithExceptionAndPrivateSetter("test name");
            var actual = SerializeObject(object1);
            Assert.Equal("{\"Name\":\"test name\"}", actual);
        }

#if !NET35 && !NET45
        [Fact]
        public void SerializeValueTuple_Test()
        {
            // Could perform reflection on fields, but one have to lookup TupleElementNamesAttribute to get names
            // ValueTuples are for internal usage, better to use AnonymousObject for key/value-pairs
            var object1 = (Name: "test name", Id: 1);
            var actual = SerializeObject(object1);
            Assert.Equal("\"(test name, 1)\"", actual);
        }
#endif

        [Fact]
        public void SerializeAnonymousObject_Test()
        {
            var object1 = new { Id = 123, Name = "test name" };
            var actual = SerializeObject(object1);
            Assert.Equal("{\"Id\":123, \"Name\":\"test name\"}", actual);
        }

#if !NET35 && !NET40

        [Fact]
        public void SerializeExpandoObject_Test()
        {
            dynamic object1 = new ExpandoObject();
            object1.Id = 123;
            object1.Name = "test name";
            var actual = SerializeObject(object1);
            Assert.Equal("{\"Id\":123, \"Name\":\"test name\"}", actual);
        }

        [Fact]
        public void SerializeDynamicObject_Test()
        {
            var object1 = new MyDynamicClass();
            var actual = SerializeObject(object1);
            Assert.Equal("{\"Id\":123, \"Name\":\"test name\"}", actual);
        }

        private class MyDynamicClass : DynamicObject
        {
            private int _id = 123;
            private string _name = "test name";

            public override bool TryGetMember(GetMemberBinder binder,
                out object result)
            {
                if (binder.Name == "Id")
                {
                    result = _id;
                    return true;
                }
                if (binder.Name == "Name")
                {
                    result = _name;
                    return true;
                }
                result = null;
                return false;
            }

            public override bool TrySetMember(SetMemberBinder binder, object value)
            {
                if (binder.Name == "Id")
                {
                    _id = (int)value;
                    return true;
                }
                if (binder.Name == "Name")
                {
                    _name = (string)value;
                    return true;
                }
                return false;
            }

            public override IEnumerable<string> GetDynamicMemberNames()
            {
                return new List<string>() { "Id", "Name" };
            }
        }

#endif

        [Fact]
        public void SingleItemOptimizedHashSetTest()
        {
            var hashSet = default(NLog.Internal.SingleItemOptimizedHashSet<object>);
            Assert.Empty(hashSet);
            Assert.DoesNotContain(new object(), hashSet);
            foreach (var obj in hashSet)
                throw new Exception("Wrong");
            hashSet.Clear();
            Assert.Empty(hashSet);
            hashSet.Add(new object());
            Assert.Single(hashSet);
            hashSet.Add(new object());
            Assert.Equal(2, hashSet.Count);
            foreach (var obj in hashSet)
                Assert.Contains(obj, hashSet);
            object[] objArray = new object[2];
            hashSet.CopyTo(objArray, 0);
            foreach (var obj in objArray)
            {
                Assert.NotNull(obj);
                hashSet.Remove(obj);
            }
            Assert.Empty(hashSet);
            hashSet.Clear();
            Assert.Empty(hashSet);
        }

        protected class TestObject
        {
            /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
            public TestObject(string name)
            {
                Name = name;
            }

            public string Name { get; set; }

            public TestObject Linked { get; set; }
        }

        protected class NoPropsObject
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

            public object Ex => throw new Exception("oops");
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
                return GetEnumerator();
            }

            public override bool Equals(object obj)
            {
                throw new Exception("object.Equals should never be called");
            }

            public override int GetHashCode()
            {
                throw new Exception("GetHashCode should never be called");
            }
        }
    }
}
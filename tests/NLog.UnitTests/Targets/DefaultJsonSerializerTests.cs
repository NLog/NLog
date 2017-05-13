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
            var text = "First\\Second\tand" +(char)3+ "for" + (char)0x1f + "with" + (char)0x10 + "but" + (char)0x0d + "and no" + (char)0x20;
            var expected = "\"First\\\\Second\\tand\\u0003for\\u001fwith\\u0010but\\rand no \"";

            var actual = _serializer.SerializeObject(text);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData((int)177, "177")]
        [InlineData((long)32711520331, "32711520331")]
        [InlineData(3.14159265, "3.14159265")]
        [InlineData(2776145.7743, "2776145.7743")]
        public void SerializeNumber_Test(object o, string expected)
        {
            var actual = _serializer.SerializeObject(o);
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
        public void SerializeGuid_Test()
        {
            Guid newGuid = Guid.NewGuid();
            var actual = _serializer.SerializeObject(newGuid);
            Assert.Equal("\"" + newGuid.ToString() + "\"", actual);
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


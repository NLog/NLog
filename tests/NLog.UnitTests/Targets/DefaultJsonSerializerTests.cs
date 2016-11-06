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
    public class DefaultJsonSerializerTests : NLogTestBase
    {
        private DefaultJsonSerializer _serializer;

        public DefaultJsonSerializerTests()
        {
            _serializer = new DefaultJsonSerializer();
        }

        [Fact]
        public void SingleLineString_Test()
        {
            var text = "This is, sort of, surprising the 1. time you see that test-result.";
            var expected = "\"This is, sort of, surprising the 1. time you see that test-result.\"";

            var actual = _serializer.SerializeValue(text);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void MultiLineString_Test()
        {
            var text = "First line followed by Windows line break\r\nNow this is second with UNIX\nand third at last";
            var expected = "\"First line followed by Windows line break\\r\\nNow this is second with UNIX\\nand third at last\"";

            var actual = _serializer.SerializeValue(text);
            Assert.Equal(expected, actual);
        }


        [Fact]
        public void StringWithTabBackSpaceFormfeed_Test()
        {
            var text = "A tab\tis followed by a feed\fand finally cancel last character\b";
            var expected = "\"A tab\\tis followed by a feed\\fand finally cancel last character\\b\"";

            var actual = _serializer.SerializeValue(text);
            Assert.Equal(expected, actual);
        }


        [Fact]
        public void StringWithSlashAndQuotes_Test()
        {
            var text = "This sentence/text is \"normal\", we think.";
            var expected = "\"This sentence\\/text is \\\"normal\\\", we think.\"";

            var actual = _serializer.SerializeValue(text);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData((int)177, "177")]
        [InlineData((long)32711520331, "32711520331")]
        [InlineData(3.14159265, "3.14159265")]
        [InlineData(2776145.7742, "2776145.7742")]
        public void SerializeNumber_Test(object o, string expected)
        {
            var actual = _serializer.SerializeValue(o);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SerializeBool_Test()
        {
            var actual = _serializer.SerializeValue(true);
            Assert.Equal("true", actual, StringComparer.OrdinalIgnoreCase);
            actual = _serializer.SerializeValue(false);
            Assert.Equal("false", actual, StringComparer.OrdinalIgnoreCase);
        }
    }
}


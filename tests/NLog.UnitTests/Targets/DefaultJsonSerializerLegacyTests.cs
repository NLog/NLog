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
using System.Globalization;
using NLog.Config;
using NLog.Targets;
using Xunit;

namespace NLog.UnitTests.Targets
{
    
    /// <summary>
    /// Test via <see cref="DefaultJsonSerializer"/> / <see cref="IJsonSerializer"/> path
    /// </summary>
    public class DefaultJsonSerializerLegacyTests : DefaultJsonSerializerTestsBase
    {
        private readonly DefaultJsonSerializer _serializer;

        public DefaultJsonSerializerLegacyTests()
        {
            _serializer = new DefaultJsonSerializer(null);
        }

        protected override string SerializeObject(object o)
        {
            return _serializer.SerializeObject(o);
        }

        private string SerializeObjectWithOptions(object o, JsonSerializeOptions options)
        {
            return _serializer.SerializeObject(o, options); //calls IJsonSerializer
        }

        [Theory]
        [InlineData((int)177, "177.00")]
        [InlineData((long)32711520331, "32711520331.00")]
        [InlineData(3.14159265, "3.14")]
        [InlineData(2776145.7743, "2776145.77")]
        public void SerializeNumber_format_Test(object o, string expected)
        {
            var actual = SerializeObjectWithOptions(o, new JsonSerializeOptions() { Format = "N2" });
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData((int)177, "177")]
        [InlineData((long)32711520331, "32711520331")]
        [InlineData(3.14159265, "3,14159265")]
        [InlineData(2776145.7743, "2776145,7743")]
        public void SerializeNumber_nl_Test(object o, string expected)
        {
            var actual = SerializeObjectWithOptions(o, new JsonSerializeOptions() { FormatProvider = new CultureInfo("nl-nl") });
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData((int)177, "177,00")]
        [InlineData((long)32711520331, "32.711.520.331,00")]
        [InlineData(3.14159265, "3,14")]
        [InlineData(2776145.7743, "2.776.145,77")]
        public void SerializeNumber_formatNL_Test(object o, string expected)
        {
            var actual = SerializeObjectWithOptions(o, new JsonSerializeOptions() { Format = "N2", FormatProvider = new CultureInfo("nl-nl") });
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SerializeDateTime_isoformat_Test()
        {
            var val = new DateTime(2016, 12, 31);
            var actual = SerializeObjectWithOptions(val, new JsonSerializeOptions { Format = "O" });
            Assert.Equal("\"2016-12-31T00:00:00.0000000\"", actual);
        }

        [Fact]
        public void SerializeDateTime_format_Test()
        {
            DateTime utcNow = DateTime.UtcNow;
            utcNow = utcNow.AddTicks(-utcNow.Ticks % TimeSpan.TicksPerSecond);
            var actual = SerializeObjectWithOptions(utcNow, new JsonSerializeOptions { Format = "dddd d M" });
            Assert.Equal("\"" + utcNow.ToString("dddd d M", CultureInfo.InvariantCulture) + "\"", actual);
        }

        [Fact]
        public void SerializeDateTime_formatNl_Test()
        {
            DateTime utcNow = DateTime.UtcNow;
            utcNow = utcNow.AddTicks(-utcNow.Ticks % TimeSpan.TicksPerSecond);
            var actual = SerializeObjectWithOptions(utcNow, new JsonSerializeOptions { Format = "dddd d M", FormatProvider = new CultureInfo("nl-nl") });
            Assert.Equal("\"" + utcNow.ToString("dddd d M", new CultureInfo("nl-nl")) + "\"", actual);
        }

        [Fact]
        public void SerializeEnumInt_Test()
        {
            var val = ExceptionRenderingFormat.Method;
            var actual = SerializeObjectWithOptions(val, new JsonSerializeOptions() { EnumAsInteger = true });
            Assert.Equal("4", actual);
        }

        [Fact]
        public void SerializeObject_noQuote_Test()
        {
            var object1 = new TestObject("object1");
            var object2 = new TestObject("object2");

            object1.Linked = object2;
            var actual = SerializeObjectWithOptions(object1, new JsonSerializeOptions { QuoteKeys = false });
            Assert.Equal("{Name:\"object1\", Linked:{Name:\"object2\"}}", actual);
        }
    }
}
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
using System.Collections.Generic;
using System.Text;
using NLog.Internal;
using Xunit;

namespace NLog.UnitTests.Internal
{
    public class StringBuilderExtTests : NLogTestBase
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        [InlineData(9)]
        [InlineData(10)]
        [InlineData(12)]
        [InlineData(123)]
        [InlineData(1234)]
        [InlineData(12345)]
        [InlineData(123456)]
        [InlineData(1234567)]
        [InlineData(12345678)]
        [InlineData(123456789)]
        [InlineData(1234567890)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        void TestAppendInvariant(int input)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilderExt.AppendInvariant(sb, input);
            Assert.Equal(input.ToString(System.Globalization.CultureInfo.InvariantCulture), sb.ToString());

            input = 0 - input;
            sb.Length = 0;
            StringBuilderExt.AppendInvariant(sb, input);
            Assert.Equal(input.ToString(System.Globalization.CultureInfo.InvariantCulture), sb.ToString());
        }

        [Theory]
        [MemberData(nameof(TestAppendXsdDateTimeRoundTripCases))]
        void TestAppendXmlDateTimeRoundTripUndefined(DateTime input)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilderExt.AppendXmlDateTimeRoundTrip(sb, input);
            Assert.Equal(System.Xml.XmlConvert.ToString(input, System.Xml.XmlDateTimeSerializationMode.Utc), sb.ToString());
        }

        [Theory]
        [MemberData(nameof(TestAppendXsdDateTimeRoundTripCases))]
        void TestAppendXmlDateTimeRoundTripLocal(DateTime input)
        {
            input = new DateTime(input.Ticks, DateTimeKind.Local);
            StringBuilder sb = new StringBuilder();
            StringBuilderExt.AppendXmlDateTimeRoundTrip(sb, input);
            Assert.Equal(System.Xml.XmlConvert.ToString(input, System.Xml.XmlDateTimeSerializationMode.Utc), sb.ToString());
        }

        [Theory]
        [MemberData(nameof(TestAppendXsdDateTimeRoundTripCases))]
        void TestAppendXmlDateTimeRoundTripUtc(DateTime input)
        {
            input = new DateTime(input.Ticks, DateTimeKind.Utc);
            StringBuilder sb = new StringBuilder();
            StringBuilderExt.AppendXmlDateTimeRoundTrip(sb, input);
            Assert.Equal(System.Xml.XmlConvert.ToString(input, System.Xml.XmlDateTimeSerializationMode.Utc), sb.ToString());
        }

        public static IEnumerable<object[]> TestAppendXsdDateTimeRoundTripCases()
        {
            yield return new object[] { DateTime.MinValue };
            yield return new object[] { DateTime.MaxValue };
            yield return new object[] { new DateTime(123, 1, 2) };
            yield return new object[] { new DateTime(1234, 10, 20) };
            yield return new object[] { new DateTime(1970, 01, 01, 01, 01, 01) };
            yield return new object[] { new DateTime(1970, 10, 10, 10, 10, 10) };
            yield return new object[] { new DateTime(1970, 01, 01, 01, 01, 01, 1) };
            yield return new object[] { new DateTime(1970, 10, 10, 10, 10, 10, 10) };
            yield return new object[] { new DateTime(1970, 11, 11, 11, 11, 11, 11) };
            yield return new object[] { new DateTime(1970, 10, 10, 10, 10, 10, 100) };
            yield return new object[] { new DateTime(1970, 12, 12, 12, 12, 12, 110) };
            yield return new object[] { new DateTime(1970, 10, 11, 12, 13, 14, 999) };
            yield return new object[] { new DateTime(1970, 11, 12, 13, 14, 15).AddTicks(1) };
            yield return new object[] { new DateTime(1970, 12, 13, 14, 15, 16).AddTicks(10) };
            yield return new object[] { new DateTime(1970, 01, 02, 03, 04, 05).AddTicks(12) };
            yield return new object[] { new DateTime(1970, 02, 03, 04, 05, 06).AddTicks(120) };
            yield return new object[] { new DateTime(1970, 03, 04, 05, 06, 07).AddTicks(123) };
            yield return new object[] { new DateTime(1970, 04, 05, 06, 07, 08).AddTicks(1230) };
            yield return new object[] { new DateTime(1970, 05, 06, 07, 08, 09).AddTicks(1234) };
            yield return new object[] { new DateTime(1970, 06, 07, 08, 09, 10).AddTicks(12340) };
            yield return new object[] { new DateTime(1970, 07, 08, 09, 10, 11).AddTicks(12345) };
            yield return new object[] { new DateTime(1970, 08, 09, 10, 11, 12).AddTicks(123450) };
            yield return new object[] { new DateTime(1970, 09, 10, 11, 12, 13).AddTicks(123456) };
            yield return new object[] { new DateTime(1970, 10, 15, 20, 25, 30).AddTicks(1234560) };
            yield return new object[] { new DateTime(1970, 05, 10, 15, 20, 25).AddTicks(1234567) };
            yield return new object[] { new DateTime(1970, 10, 30, 10, 30, 10).AddTicks(1000000) };
            yield return new object[] { new DateTime(1970, 01, 02, 03, 04, 05).AddTicks(100000) };
            yield return new object[] { new DateTime(1970, 10, 20, 20, 40, 50).AddTicks(10000) };
            yield return new object[] { new DateTime(1970, 01, 01, 23, 59, 59).AddTicks(1000) };
            yield return new object[] { new DateTime(1970, 12, 31, 23, 59, 59).AddTicks(100) };
        }
    }
}
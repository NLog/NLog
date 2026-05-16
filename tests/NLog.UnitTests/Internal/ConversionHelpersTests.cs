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
    using NLog.Common;
    using Xunit;

    public class ConversionHelpersTests : NLogTestBase
    {
        enum TestEnumType
        {
            None,
            Foo,
            Bar,
        }

        [Flags]
        enum TestEnumTypes
        {
            None = 0,
            Foo = 1,
            Bar = 2,
        }

        #region tryparse - ignorecase parameter: false

        [Fact]
        public void EnumParse1_ignoreCaseFalse()
        {
            TestEnumParseCaseIgnoreCaseParam("Foo", false, TestEnumType.Foo, true);
        }

        [Fact]
        public void EnumParse2_ignoreCaseFalse()
        {
            TestEnumParseCaseIgnoreCaseParam("foo", false, TestEnumType.None, false);
        }

        [Fact]
        public void EnumParseDefault_ignoreCaseFalse()
        {
            TestEnumParseCaseIgnoreCaseParam("BAR", false, TestEnumType.None, false);
        }

        [Fact]
        public void EnumParseDefault2_ignoreCaseFalse()
        {
            TestEnumParseCaseIgnoreCaseParam("x", false, TestEnumType.None, false);
        }

        [Fact]
        public void EnumParseBar_ignoreCaseFalse()
        {
            TestEnumParseCaseIgnoreCaseParam("bar", false, TestEnumType.None, false);
        }

        [Fact]
        public void EnumParseBar2_ignoreCaseFalse()
        {
            TestEnumParseCaseIgnoreCaseParam(" bar ", false, TestEnumType.None, false);
        }

        [Fact]
        public void EnumParseBar3_ignoreCaseFalse()
        {
            TestEnumParseCaseIgnoreCaseParam(" \r\nbar ", false, TestEnumType.None, false);
        }


        [Fact]
        public void EnumParse_null_ignoreCaseFalse()
        {
            TestEnumParseCaseIgnoreCaseParam(null, false, TestEnumType.None, true);
        }

        [Fact]
        public void EnumParse_emptystring_ignoreCaseFalse()
        {
            TestEnumParseCaseIgnoreCaseParam(string.Empty, false, TestEnumType.None, true);
        }

        [Fact]
        public void EnumParse_whitespace_ignoreCaseFalse()
        {
            TestEnumParseCaseIgnoreCaseParam("   ", false, TestEnumType.None, true);
        }

        [Fact]
        public void EnumParse_wrongInput_ignoreCaseFalse()
        {
            TestEnumParseCaseIgnoreCaseParam("not enum", false, TestEnumType.None, false);
        }

        #endregion

        #region tryparse - ignorecase parameter: true

        [Fact]
        public void EnumParse1_ignoreCaseTrue()
        {
            TestEnumParseCaseIgnoreCaseParam("Foo", true, TestEnumType.Foo, true);
        }

        [Fact]
        public void EnumParse2_ignoreCaseTrue()
        {
            TestEnumParseCaseIgnoreCaseParam("foo", true, TestEnumType.Foo, true);
        }

        [Fact]
        public void EnumParseDefault_ignoreCaseTrue()
        {
            TestEnumParseCaseIgnoreCaseParam("BAR", true, TestEnumType.Bar, true);
        }

        [Fact]
        public void EnumParseDefault2_ignoreCaseTrue()
        {
            TestEnumParseCaseIgnoreCaseParam("x", true, TestEnumType.None, false);
        }

        [Fact]
        public void EnumParseBar_ignoreCaseTrue()
        {
            TestEnumParseCaseIgnoreCaseParam("bar", true, TestEnumType.Bar, true);
        }

        [Fact]
        public void EnumParseBar2_ignoreCaseTrue()
        {
            TestEnumParseCaseIgnoreCaseParam(" bar ", true, TestEnumType.Bar, true);
        }

        [Fact]
        public void EnumParseBar3_ignoreCaseTrue()
        {
            TestEnumParseCaseIgnoreCaseParam(" \r\nbar ", true, TestEnumType.Bar, true);
        }

        [Fact]
        public void EnumParse_null_ignoreCaseTrue()
        {
            TestEnumParseCaseIgnoreCaseParam(null, true, TestEnumType.None, true);
        }

        [Fact]
        public void EnumParse_emptystring_ignoreCaseTrue()
        {
            TestEnumParseCaseIgnoreCaseParam(string.Empty, true, TestEnumType.None, true);
        }

        [Fact]
        public void EnumParse_whitespace_ignoreCaseTrue()
        {
            TestEnumParseCaseIgnoreCaseParam("   ", true, TestEnumType.None, true);
        }

        [Fact]
        public void EnumParse_ArgumentException_ignoreCaseTrue()
        {
            double result;
            Assert.Throws<ArgumentException>(() => ConversionHelpers.TryParseEnum("not enum", true, out result));

            Assert.Throws<ArgumentException>(() => ConversionHelpers.TryParseEnum("not enum", typeof(double), out _));
        }

        [Fact]
        public void EnumParse_nonGeneric_nonEnumType_null_returnsFalse()
        {
            // null/whitespace input with a non-enum type must return false + null (no Enum.ToObject call)
            var returnResult = ConversionHelpers.TryParseEnum(null, typeof(double), out var result);
            Assert.False(returnResult);
            Assert.Null(result);
        }

        [Fact]
        public void EnumParseFlags_null_returnsNoneAndTrue()
        {
            var returnResult = ConversionHelpers.TryParseEnum(null, true, out TestEnumTypes result);
            Assert.True(returnResult);
            Assert.Equal(TestEnumTypes.None, result);
        }

        [Fact]
        public void EnumParseFlags_emptystring_returnsNoneAndTrue()
        {
            var returnResult = ConversionHelpers.TryParseEnum(string.Empty, true, out TestEnumTypes result);
            Assert.True(returnResult);
            Assert.Equal(TestEnumTypes.None, result);
        }

        [Fact]
        public void EnumParseFlags_whitespace_returnsNoneAndTrue()
        {
            var returnResult = ConversionHelpers.TryParseEnum("   ", true, out TestEnumTypes result);
            Assert.True(returnResult);
            Assert.Equal(TestEnumTypes.None, result);
        }

        [Fact]
        public void EnumParseFlags_nonGeneric_null_returnsNoneAndTrue()
        {
            var returnResult = ConversionHelpers.TryParseEnum(null, typeof(TestEnumTypes), out var result);
            Assert.True(returnResult);
            Assert.Equal(TestEnumTypes.None, result);
        }

        [Fact]
        public void EnumParseFlags_nonGeneric_whitespace_returnsNoneAndTrue()
        {
            var returnResult = ConversionHelpers.TryParseEnum("   ", typeof(TestEnumTypes), out var result);
            Assert.True(returnResult);
            Assert.Equal(TestEnumTypes.None, result);
        }

        [Fact]
        public void EnumParseFlags_validValue_parsesCorrectly()
        {
            var returnResult = ConversionHelpers.TryParseEnum("Foo, Bar", true, out TestEnumTypes result);
            Assert.True(returnResult);
            Assert.Equal(TestEnumTypes.Foo | TestEnumTypes.Bar, result);
        }

        [Fact]
        public void EnumParse_nonGeneric_isAlwaysCaseInsensitive()
        {
            // The non-generic (Type) overload always parses case-insensitively
            var returnResult = ConversionHelpers.TryParseEnum("foo", typeof(TestEnumType), out var result);
            Assert.True(returnResult);
            Assert.Equal(TestEnumType.Foo, result);
        }

        #endregion

        #region helpers

        private static void TestEnumParseCaseIgnoreCaseParam(string value, bool ignoreCase, TestEnumType expected, bool expectedReturn)
        {
            {
                var returnResult = ConversionHelpers.TryParseEnum(value, ignoreCase, out TestEnumType result);

                Assert.Equal(expected, result);
                Assert.Equal(expectedReturn, returnResult);
            }

            // if true, test also other TryParseEnum
            if (ignoreCase)
            {
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    var returnResult = ConversionHelpers.TryParseEnum<TestEnumType>(value, out var result);
                    Assert.Equal(expected, result);
                    Assert.Equal(expectedReturn, returnResult);
#pragma warning restore CS0618 // Type or member is obsolete
                }
                {
                    var returnResult = ConversionHelpers.TryParseEnum(value, typeof(TestEnumType), out var result);
                    Assert.Equal(expectedReturn, returnResult);
                    if (expectedReturn)
                    {
                        Assert.Equal(expected, result);
                    }
                    else
                    {
                        Assert.Null(result);
                    }
                }
            }
        }

        #endregion
    }
}

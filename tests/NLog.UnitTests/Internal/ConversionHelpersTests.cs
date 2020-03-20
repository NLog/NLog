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

namespace NLog.UnitTests.Internal
{
    using System;
    using NLog.Common;
    using Xunit;

    public class ConversionHelpersTests : NLogTestBase
    {
        enum TestEnum
        {
            Foo,
            bar,
        }

        #region tryparse - ignorecase parameter: false

        [Fact]
        public void EnumParse1_ignoreCaseFalse()
        {
            TestEnumParseCaseIgnoreCaseParam("Foo", false, TestEnum.Foo, true);
        }

        [Fact]
        public void EnumParse2_ignoreCaseFalse()
        {
            TestEnumParseCaseIgnoreCaseParam("foo", false, TestEnum.Foo, false);
        }

        [Fact]
        public void EnumParseDefault_ignoreCaseFalse()
        {
            TestEnumParseCaseIgnoreCaseParam("BAR", false, TestEnum.Foo, false);
        }

        [Fact]
        public void EnumParseDefault2_ignoreCaseFalse()
        {
            TestEnumParseCaseIgnoreCaseParam("x", false, TestEnum.Foo, false);
        }

        [Fact]
        public void EnumParseBar_ignoreCaseFalse()
        {
            TestEnumParseCaseIgnoreCaseParam("bar", false, TestEnum.bar, true);
        }

        [Fact]
        public void EnumParseBar2_ignoreCaseFalse()
        {
            TestEnumParseCaseIgnoreCaseParam(" bar ", false, TestEnum.bar, true);
        }

        [Fact]
        public void EnumParseBar3_ignoreCaseFalse()
        {
            TestEnumParseCaseIgnoreCaseParam(" \r\nbar ", false, TestEnum.bar, true);
        }


        [Fact]
        public void EnumParse_null_ignoreCaseFalse()
        {
            TestEnumParseCaseIgnoreCaseParam(null, false, TestEnum.Foo, false);
        }

        [Fact]
        public void EnumParse_emptystring_ignoreCaseFalse()
        {
            TestEnumParseCaseIgnoreCaseParam(string.Empty, false, TestEnum.Foo, false);
        }

        [Fact]
        public void EnumParse_whitespace_ignoreCaseFalse()
        {
            TestEnumParseCaseIgnoreCaseParam("   ", false, TestEnum.Foo, false);
        }

        [Fact]
        public void EnumParse_wrongInput_ignoreCaseFalse()
        {
            TestEnumParseCaseIgnoreCaseParam("not enum", false, TestEnum.Foo, false);
        }

        #endregion

        #region tryparse - ignorecase parameter: true

        [Fact]
        public void EnumParse1_ignoreCaseTrue()
        {
            TestEnumParseCaseIgnoreCaseParam("Foo", true, TestEnum.Foo, true);
        }

        [Fact]
        public void EnumParse2_ignoreCaseTrue()
        {
            TestEnumParseCaseIgnoreCaseParam("foo", true, TestEnum.Foo, true);
        }

        [Fact]
        public void EnumParseDefault_ignoreCaseTrue()
        {
            TestEnumParseCaseIgnoreCaseParam("BAR", true, TestEnum.bar, true);
        }

        [Fact]
        public void EnumParseDefault2_ignoreCaseTrue()
        {
            TestEnumParseCaseIgnoreCaseParam("x", true, TestEnum.Foo, false);
        }

        [Fact]
        public void EnumParseBar_ignoreCaseTrue()
        {
            TestEnumParseCaseIgnoreCaseParam("bar", true, TestEnum.bar, true);
        }

        [Fact]
        public void EnumParseBar2_ignoreCaseTrue()
        {
            TestEnumParseCaseIgnoreCaseParam(" bar ", true, TestEnum.bar, true);
        }

        [Fact]
        public void EnumParseBar3_ignoreCaseTrue()
        {
            TestEnumParseCaseIgnoreCaseParam(" \r\nbar ", true, TestEnum.bar, true);
        }

        [Fact]
        public void EnumParse_null_ignoreCaseTrue()
        {
            TestEnumParseCaseIgnoreCaseParam(null, true, TestEnum.Foo, false);
        }

        [Fact]
        public void EnumParse_emptystring_ignoreCaseTrue()
        {
            TestEnumParseCaseIgnoreCaseParam(string.Empty, true, TestEnum.Foo, false);
        }

        [Fact]
        public void EnumParse_whitespace_ignoreCaseTrue()
        {
            TestEnumParseCaseIgnoreCaseParam("   ", true, TestEnum.Foo, false);
        }

        [Fact]
        public void EnumParse_ArgumentException_ignoreCaseTrue()
        {
            double result;
            Assert.Throws<ArgumentException>(() => ConversionHelpers.TryParseEnum("not enum", true, out result));
        }

        #endregion

        #region helpers

        private static void TestEnumParseCaseIgnoreCaseParam(string value, bool ignoreCase, TestEnum expected, bool expectedReturn)
        {
            {
                var returnResult = ConversionHelpers.TryParseEnum(value, ignoreCase, out TestEnum result);

                Assert.Equal(expected, result);
                Assert.Equal(expectedReturn, returnResult);
            }

            // if true, test also other TryParseEnum
            if (ignoreCase)
            {
                {
                    var returnResult = ConversionHelpers.TryParseEnum<TestEnum>(value, out var result);
                    Assert.Equal(expected, result);
                    Assert.Equal(expectedReturn, returnResult);
                }
                {
                    var returnResult = ConversionHelpers.TryParseEnum(value, typeof(TestEnum), out var result);
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

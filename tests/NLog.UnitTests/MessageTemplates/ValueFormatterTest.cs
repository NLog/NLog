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

using NLog.MessageTemplates;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xunit;

namespace NLog.UnitTests.MessageTemplates
{
    public class ValueFormatterTest : NLogTestBase
    {
        enum TestData
        {
            Foo,Bar
        };
        private class Test : IFormattable, IConvertible
        {
            public Test()
            {
                Str = "Test";
                Integer = 1;
            }

            public Test(TypeCode typeCode) : this()
            {
                TypeCode = typeCode;
            }

            public TestData Data { get; set; }
            public string Str { get; set; }

            public int Integer { get; set; }

            public TypeCode TypeCode { get; set; }

            public TypeCode GetTypeCode()
            {
                return TypeCode;
            }

            public bool ToBoolean(IFormatProvider provider)
            {
                return true;
            }

            public byte ToByte(IFormatProvider provider)
            {
                return 1;
            }

            public char ToChar(IFormatProvider provider)
            {
                return 't';
            }

            public DateTime ToDateTime(IFormatProvider provider)
            {
                return new DateTime(2019, 7, 28);
            }

            public decimal ToDecimal(IFormatProvider provider)
            {
                return Integer;
            }

            public double ToDouble(IFormatProvider provider)
            {
                return Integer;
            }

            public short ToInt16(IFormatProvider provider)
            {
                return 1;
            }

            public int ToInt32(IFormatProvider provider)
            {
                return Integer;
            }

            public long ToInt64(IFormatProvider provider)
            {
                return Integer;
            }

            public sbyte ToSByte(IFormatProvider provider)
            {
                return 1;
            }

            public float ToSingle(IFormatProvider provider)
            {
                return Integer;
            }

            public string ToString(string format, IFormatProvider formatProvider)
            {
                return Str;
            }

            public string ToString(IFormatProvider provider)
            {
                return Str;
            }

            public object ToType(Type conversionType, IFormatProvider provider)
            {
                return Integer;
            }

            public ushort ToUInt16(IFormatProvider provider)
            {
                return 1;
            }

            public uint ToUInt32(IFormatProvider provider)
            {
                return 1;
            }

            public ulong ToUInt64(IFormatProvider provider)
            {
                return 1;
            }
        }

        private class Test1
        {
            public Test1()
            {
                Str = "Test";
                Integer = 1;
            }
            public string Str { get; set; }

            public int Integer { get; set; }
        }

        private class Test2
        {
            public Test2()
            {
                Str = "Test";
                Integer = 1;
            }
            public string Str { get; set; }

            public int Integer { get; set; }

        }
        
        private class RecursiveTest
        {
            public RecursiveTest(int integer)
            {
                Integer = integer + 1;
            }
            public RecursiveTest Next => new RecursiveTest(Integer);

            public int Integer { get; set; }

        }

        private static ValueFormatter CreateValueFormatter()
        {
            return new ValueFormatter(LogManager.LogFactory.ServiceRepository);
        }

        [Fact]
        public void TestSerialisationOfStringToJsonIsSuccessful()
        {
            var str = "Test";
            StringBuilder builder = new StringBuilder();
            var result = CreateValueFormatter().FormatValue(str, string.Empty, CaptureType.Serialize, null, builder);
            Assert.True(result);
            Assert.Equal("\"Test\"", builder.ToString());
        }

        [Fact]
        public void TestSerialisationOfClassObjectToJsonIsSuccessful()
        {
            var @class = new Test2();
            StringBuilder builder = new StringBuilder();
            var result = CreateValueFormatter().FormatValue(@class, string.Empty, CaptureType.Serialize, null, builder);
            Assert.True(result);
            Assert.Equal("{\"Str\":\"Test\", \"Integer\":1}", builder.ToString());
        }

        [Fact]
        public void TestSerialisationOfRecursiveClassObjectToJsonIsSuccessful()
        {
            var @class = new RecursiveTest(0);
            StringBuilder builder = new StringBuilder();
            var result = CreateValueFormatter().FormatValue(@class, string.Empty, CaptureType.Serialize, null, builder);
            Assert.True(result);
            var actual = builder.ToString();
            
            var deepestInteger = @"""Integer"":10";
            Assert.Contains(deepestInteger, actual);
            
            var deepestNext = @"""Next"":""NLog.UnitTests.MessageTemplates.ValueFormatterTest+RecursiveTest""";
            Assert.Contains(deepestNext, actual);
        }

        [Fact]
        public void TestStringifyOfStringIsSuccessful()
        {
            var @class = "str";
            StringBuilder builder = new StringBuilder();
            var result = CreateValueFormatter().FormatValue(@class, string.Empty, CaptureType.Stringify, new CultureInfo("fr-FR"), builder);
            Assert.True(result);
            Assert.Equal("\"str\"", builder.ToString());
        }

        [Fact]
        public void TestStringifyOfIFormatableObjectIsSuccessful()
        {
            var @class = new Test();
            StringBuilder builder = new StringBuilder();
            var result = CreateValueFormatter().FormatValue(@class, string.Empty, CaptureType.Stringify, new CultureInfo("fr-FR"), builder);
            Assert.True(result);
            Assert.Equal("\"Test\"", builder.ToString());
        }

        [Fact]
        public void TestStringifyOfNonIFormatableObjectIsSuccessful()
        {
            var @class = new Test1();
            StringBuilder builder = new StringBuilder();
            var result = CreateValueFormatter().FormatValue(@class, string.Empty, CaptureType.Stringify, new CultureInfo("fr-FR"), builder);
            Assert.True(result);
            var expectedValue = $"\"{typeof(Test1).FullName}\"";
            Assert.Equal(expectedValue, builder.ToString());
        }

        [Fact]
        public void TestSerializationOfListObjectIsSuccessful()
        {
            var list = new List<int>() { 1, 2, 3, 4, 5, 6 };
            StringBuilder builder = new StringBuilder();
            var result = CreateValueFormatter().FormatValue(list, string.Empty, CaptureType.Normal, new CultureInfo("fr-FR"), builder);
            Assert.True(result);
            Assert.Equal("1, 2, 3, 4, 5, 6", builder.ToString());
        }

        [Fact]
        public void TestSerializationOfDictionaryObjectIsSuccessful()
        {
            var list = new Dictionary<int, object>() { { 1, new Test() }, { 2, new Test1() } };
            StringBuilder builder = new StringBuilder();
            var result = CreateValueFormatter().FormatValue(list, string.Empty, CaptureType.Normal, new CultureInfo("fr-FR"), builder);
            Assert.True(result);
            Assert.Equal($"1=Test, 2={typeof(Test1).FullName}", builder.ToString());
        }

        [Fact]
        public void TestSerializationOfCollectionOfListObjectWithDepth2IsNotSuccessful()
        {
            var list = new List<List<List<List<int>>>>() { new List<List<List<int>>>() { new List<List<int>>() { new List<int>() { 1, 2 }, new List<int>() { 3, 4 } }, new List<List<int>>() { new List<int>() { 4, 5 }, new List<int>() { 6, 7 } } } };
            StringBuilder builder = new StringBuilder();
            var result = CreateValueFormatter().FormatValue(list, string.Empty, CaptureType.Normal, new CultureInfo("fr-FR"), builder);
            Assert.True(result);
            Assert.NotEqual("1,2,3,4,5,6,7", builder.ToString());
        }

        [Fact]
        public void TestSerializationWillbeSkippedForElementsThatHaveRepeatedElements()
        {
            var list = new List<List<List<List<int>>>>() { new List<List<List<int>>>() { new List<List<int>>() { new List<int>() { 1, 2 }, new List<int>() { 1, 2 } }, new List<List<int>>() { new List<int>() { 1, 2 }, new List<int>() { 1, 2 } } } };
            StringBuilder builder = new StringBuilder();
            var result = CreateValueFormatter().FormatValue(list, string.Empty, CaptureType.Normal, new CultureInfo("fr-FR"), builder);
            Assert.True(result);
            Assert.NotEqual("1,2", builder.ToString());
        }
        
        [Theory]
        [InlineData(CaptureType.Normal, "NULL")]
        [InlineData(CaptureType.Serialize, "null")]
        [InlineData(CaptureType.Stringify, "\"\"")]
        public void TestSerializationWillBeSuccessfulForNull(CaptureType captureType, string expected)
        {
            StringBuilder builder = new StringBuilder();
            var result = CreateValueFormatter().FormatValue(null, string.Empty, captureType, null, builder);
            Assert.True(result);
            Assert.Equal(expected, builder.ToString());
        }

        [Fact]
        public void TestSerializationWillBeSuccessfulForNullObjects()
        {
            object list = null;
            StringBuilder builder = new StringBuilder();
            var result = CreateValueFormatter().FormatValue(list, string.Empty, CaptureType.Normal, new CultureInfo("fr-FR"), builder);
            Assert.True(result);
            Assert.Equal("NULL", builder.ToString());
        }

        [Fact]
        public void TestSerializationOfStringIsSuccessful()
        {
            var @class = "str";
            StringBuilder builder = new StringBuilder();
            var result = CreateValueFormatter().FormatValue(@class, string.Empty, CaptureType.Normal, new CultureInfo("fr-FR"), builder);
            Assert.True(result);
            Assert.Equal("\"str\"", builder.ToString());
        }

        [Fact]
        public void TestSerialisationOfIConvertibleObjectIsSuccessful()
        {
            var @class = new Test(TypeCode.Object);
            StringBuilder builder = new StringBuilder();
            var result = CreateValueFormatter().FormatValue(@class, string.Empty, CaptureType.Normal, new CultureInfo("fr-FR"), builder);
            Assert.True(result);
            Assert.Equal("Test", builder.ToString());
        }

        [Fact]
        public void TestSerialisationOfIConvertibleStringObjectIsSuccessful()
        {
            var @class = new Test(TypeCode.String);
            StringBuilder builder = new StringBuilder();
            var result = CreateValueFormatter().FormatValue(@class, string.Empty, CaptureType.Normal, new CultureInfo("fr-FR"), builder);
            Assert.True(result);
            var expectedValue = $"\"{typeof(Test).FullName}\"";
            Assert.Equal(expectedValue, builder.ToString());
        }

        [Fact]
        public void TestSerialisationOfIConvertibleBooleanObjectIsSuccessful()
        {
            var @class = new Test(TypeCode.Boolean);
            StringBuilder builder = new StringBuilder();
            var result = CreateValueFormatter().FormatValue(@class, string.Empty, CaptureType.Normal, new CultureInfo("fr-FR"), builder);
            Assert.True(result);
            Assert.Equal("true", builder.ToString());
        }

        [Fact]
        public void TestSerialisationOfIConvertibleCharObjectIsSuccessful()
        {
            var @class = new Test(TypeCode.Char);
            StringBuilder builder = new StringBuilder();
            var result = CreateValueFormatter().FormatValue(@class, string.Empty, CaptureType.Normal, new CultureInfo("fr-FR"), builder);
            Assert.True(result);
            Assert.Equal("\"t\"", builder.ToString());
        }

        [Theory]
        [InlineData(TypeCode.Byte)]
        [InlineData(TypeCode.SByte)]
        [InlineData(TypeCode.Int16)]
        [InlineData(TypeCode.Int32)]
        [InlineData(TypeCode.Int64)]
        [InlineData(TypeCode.UInt16)]
        [InlineData(TypeCode.UInt32)]
        [InlineData(TypeCode.UInt64)]
        public void TestSerialisationOfIConvertibleNumericObjectIsSuccessful(TypeCode code)
        {
            var @class = new Test(code);
            StringBuilder builder = new StringBuilder();
            var result = CreateValueFormatter().FormatValue(@class, string.Empty, CaptureType.Normal, new CultureInfo("fr-FR"), builder);
            Assert.True(result);
            Assert.Equal("1", builder.ToString());
        }

        [Fact]
        public void TestSerialisationOfIConvertibleEnumObjectIsSuccessful()
        {
            var @class = new Test(TypeCode.Byte);
            StringBuilder builder = new StringBuilder();
            var result = CreateValueFormatter().FormatValue(@class.Data, string.Empty, CaptureType.Normal, new CultureInfo("fr-FR"), builder);
            Assert.True(result);
            Assert.Equal("Foo", builder.ToString());
        }

        [Fact]
        public void TestSerialisationOfIConvertibleDateTimeObjectIsSuccessful()
        {
            var @class = new Test(TypeCode.DateTime);
            StringBuilder builder = new StringBuilder();
            var result = CreateValueFormatter().FormatValue(@class, string.Empty, CaptureType.Normal, new CultureInfo("fr-FR"), builder);
            Assert.True(result);
            Assert.Equal("Test", builder.ToString());
        }
    }
}

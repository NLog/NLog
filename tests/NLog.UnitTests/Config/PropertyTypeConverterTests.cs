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

using NLog.Config;
using System;
using System.Globalization;
using Xunit;

namespace NLog.UnitTests.Config
{
    public class PropertyTypeConverterTests
    {
        private readonly PropertyTypeConverter _sut;

        public PropertyTypeConverterTests()
        {
            _sut = new PropertyTypeConverter();
        }

        [Fact]
        public void Convert_IntToNullableIntTest()
        {
            // Act
            var result = _sut.Convert(123, typeof(int?), null, null);

            // Assert
            // int is correct here, see https://stackoverflow.com/questions/785358/nullable-type-is-not-a-nullable-type
            var resultTyped = Assert.IsType<int>(result);
            Assert.Equal(123, resultTyped);
        }

        [Fact]
        public void Convert_NullableIntToIntTest()
        {
            // Act
            var result = _sut.Convert((int?)123, typeof(int), null, null);

            // Assert
            // int is correct here, see https://stackoverflow.com/questions/785358/nullable-type-is-not-a-nullable-type
            var resultTyped = Assert.IsType<int>(result);
            Assert.Equal(123, resultTyped);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Convert_EmptyStringToNullableIntTest(string value)
        {
            // Act
            var result = _sut.Convert(value, typeof(int?), null, null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Convert_StringToIntTest()
        {
            // Act
            var result = _sut.Convert("123", typeof(int), null, null);

            // Assert
            var resultTyped = Assert.IsType<int>(result);
            Assert.Equal(123, resultTyped);
        }

        [Fact]
        public void Convert_StringToNullableIntTest()
        {
            // Act
            var result = _sut.Convert("123", typeof(int?), null, null);

            // Assert
            // int is correct here, see https://stackoverflow.com/questions/785358/nullable-type-is-not-a-nullable-type
            var resultTyped = Assert.IsType<int>(result);
            Assert.Equal(123, resultTyped);
        }

        [Theory]
        [InlineData(typeof(DateTimeOffset?))]
        [InlineData(typeof(DateTime?))]
        [InlineData(typeof(Guid?))]
        [InlineData(typeof(TimeSpan?))]
        public void Convert_EmptyStringToNullableTypeTest(Type type)
        {
            // Act
            var result = _sut.Convert("", type, null, null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Convert_StringToDecimalTest()
        {
            // Act
            var result = _sut.Convert("123.2", typeof(decimal), null, CultureInfo.InvariantCulture);

            // Assert
            var resultTyped = Assert.IsType<decimal>(result);
            Assert.Equal(123.2M, resultTyped);
        }

        [Fact]
        public void Convert_StringToDecimalWithCultureTest()
        {
            // Act
            var result = _sut.Convert("123,2", typeof(decimal), null, new CultureInfo("NL-nl"));

            // Assert
            var resultTyped = Assert.IsType<decimal>(result);
            Assert.Equal(123.2M, resultTyped);
        }

        [Fact]
        public void Convert_StringToNullableDecimalTest()
        {
            // Act
            var result = _sut.Convert("123.2", typeof(decimal?), null, CultureInfo.InvariantCulture);

            // Assert
            // decimal is correct here, see https://stackoverflow.com/questions/785358/nullable-type-is-not-a-nullable-type
            var resultTyped = Assert.IsType<decimal>(result);
            Assert.Equal(123.2M, resultTyped);
        }

        [Fact]
        public void Convert_StringToDoubleTest()
        {
            // Act
            var result = _sut.Convert("123.2", typeof(double), null, CultureInfo.InvariantCulture);

            // Assert
            var resultTyped = Assert.IsType<double>(result);
            Assert.Equal(123.2, resultTyped);
        }

        [Fact]
        public void Convert_StringToDoubleWithCultureTest()
        {
            // Act
            var result = _sut.Convert("123,2", typeof(double), null, new CultureInfo("NL-nl"));

            // Assert
            var resultTyped = Assert.IsType<double>(result);
            Assert.Equal(123.2, resultTyped);
        }

        [Fact]
        public void Convert_StringToNullableDoubleTest()
        {
            // Act
            var result = _sut.Convert("123.2", typeof(double?), null, CultureInfo.InvariantCulture);

            // Assert
            // double is correct here, see https://stackoverflow.com/questions/785358/nullable-type-is-not-a-nullable-type
            var resultTyped = Assert.IsType<double>(result);
            Assert.Equal(123.2, resultTyped);
        }

        [Fact]
        public void Convert_StringToShortTest()
        {
            // Act
            var result = _sut.Convert("123", typeof(short), null, CultureInfo.InvariantCulture);

            // Assert
            var resultTyped = Assert.IsType<short>(result);
            Assert.Equal(123, resultTyped);
        }

        [Fact]
        public void Convert_StringToNullableShortTest()
        {
            // Act
            var result = _sut.Convert("123", typeof(short), null, CultureInfo.InvariantCulture);

            // Assert
            // short is correct here, see https://stackoverflow.com/questions/785358/nullable-type-is-not-a-nullable-type
            var resultTyped = Assert.IsType<short>(result);
            Assert.Equal(123, resultTyped);
        }

        [Fact]
        public void Convert_FormattableToStringTest()
        {
            // Act
            var result = _sut.Convert(123, typeof(string), "D4", null);

            // Assert
            var resultTyped = Assert.IsType<string>(result);
            Assert.Equal("0123", resultTyped);
        }

        [Fact]
        public void Convert_NullableFormattableToStringTest()
        {
            // Arrange
            int? nullableInt = 123;

            // Act
            var result = _sut.Convert(nullableInt, typeof(string), "D4", null);

            // Assert
            var resultTyped = Assert.IsType<string>(result);
            Assert.Equal("0123", resultTyped);
        }

        [Fact]
        public void Convert_StringToDatetimeWithFormat()
        {
            // Arrange

            // Act
            var result = _sut.Convert("2019", typeof(DateTime), "yyyy", null);

            // Assert
            var resultTyped = Assert.IsType<DateTime>(result);
            Assert.Equal(new DateTime(2019, 1, 1), resultTyped);
        }

        [Fact]
        public void Convert_StringToNullableDatetimeWithFormat()
        {
            // Arrange

            // Act
            var result = _sut.Convert("2019", typeof(DateTime?), "yyyy", null);

            // Assert
            // datetime is correct here, see https://stackoverflow.com/questions/785358/nullable-type-is-not-a-nullable-type
            var resultTyped = Assert.IsType<DateTime>(result);
            Assert.Equal(new DateTime(2019, 1, 1), resultTyped);
        }
    }
}

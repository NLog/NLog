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
        public void Convert_IntToNullableInt_CorrectValue()
        {
            // Act
            var result = _sut.Convert(123, typeof(int?), null, null);

            // Assert
            // int is correct here, see https://stackoverflow.com/questions/785358/nullable-type-is-not-a-nullable-type
            var resultTyped = Assert.IsType<int>(result);
            Assert.Equal(123, resultTyped);
        }

        [Fact]
        public void Convert_NullableIntToInt_CorrectValue()
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
        public void Convert_EmptyStringToNullableInt_CorrectValue(string value)
        {
            // Act
            var result = _sut.Convert(value, typeof(int?), null, null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Convert_StringToInt_CorrectValue()
        {
            // Act
            var result = _sut.Convert("123", typeof(int), null, null);

            // Assert
            var resultTyped = Assert.IsType<int>(result);
            Assert.Equal(123, resultTyped);
        }

        [Fact]
        public void Convert_StringToNullableInt_CorrectValue()
        {
            // Act
            var result = _sut.Convert("123", typeof(int?), null, null);

            // Assert
            // int is correct here, see https://stackoverflow.com/questions/785358/nullable-type-is-not-a-nullable-type
            var resultTyped = Assert.IsType<int>(result);
            Assert.Equal(123, resultTyped);
        }

        [Fact]
        public void Convert_StringToDecimal_CorrectValue()
        {
            // Act
            var result = _sut.Convert("123.2", typeof(decimal), null, CultureInfo.InvariantCulture);

            // Assert
            var resultTyped = Assert.IsType<decimal>(result);
            Assert.Equal(123.2M, resultTyped);
        }

        [Fact]
        public void Convert_StringToDecimalWithCulture_CorrectValue()
        {
            // Act
            var result = _sut.Convert("123,2", typeof(decimal), null, new CultureInfo("NL-nl"));

            // Assert
            var resultTyped = Assert.IsType<decimal>(result);
            Assert.Equal(123.2M, resultTyped);
        }

        [Fact]
        public void Convert_StringToNullableDecimal_CorrectValue()
        {
            // Act
            var result = _sut.Convert("123.2", typeof(decimal?), null, CultureInfo.InvariantCulture);

            // Assert
            // int is correct here, see https://stackoverflow.com/questions/785358/nullable-type-is-not-a-nullable-type
            var resultTyped = Assert.IsType<decimal>(result);
            Assert.Equal(123.2M, resultTyped);
        }

        [Fact]
        public void Convert_StringToDouble_CorrectValue()
        {
            // Act
            var result = _sut.Convert("123.2", typeof(double), null, CultureInfo.InvariantCulture);

            // Assert
            var resultTyped = Assert.IsType<double>(result);
            Assert.Equal(123.2, resultTyped);
        }

        [Fact]
        public void Convert_StringToDoubleWithCulture_CorrectValue()
        {
            // Act
            var result = _sut.Convert("123,2", typeof(double), null, new CultureInfo("NL-nl"));

            // Assert
            var resultTyped = Assert.IsType<double>(result);
            Assert.Equal(123.2, resultTyped);
        }

        [Fact]
        public void Convert_StringToNullableDouble_CorrectValue()
        {
            // Act
            var result = _sut.Convert("123.2", typeof(double?), null, CultureInfo.InvariantCulture);

            // Assert
            // int is correct here, see https://stackoverflow.com/questions/785358/nullable-type-is-not-a-nullable-type
            var resultTyped = Assert.IsType<double>(result);
            Assert.Equal(123.2, resultTyped);
        }

        [Fact]
        public void Convert_StringToShort_CorrectValue()
        {
            // Act
            var result = _sut.Convert("123", typeof(short), null, CultureInfo.InvariantCulture);

            // Assert
            var resultTyped = Assert.IsType<short>(result);
            Assert.Equal(123, resultTyped);
        }

        [Fact]
        public void Convert_StringToNullableShort_CorrectValue()
        {
            // Act
            var result = _sut.Convert("123", typeof(short), null, CultureInfo.InvariantCulture);

            // Assert
            // int is correct here, see https://stackoverflow.com/questions/785358/nullable-type-is-not-a-nullable-type
            var resultTyped = Assert.IsType<short>(result);
            Assert.Equal(123, resultTyped);
        }

        [Fact]
        public void Convert_FormattableToString_CorrectValue()
        {
            // Act
            var result = _sut.Convert(123, typeof(string), "D4", null);

            // Assert
            var resultTyped = Assert.IsType<string>(result);
            Assert.Equal("0123", resultTyped);
        }

        [Fact]
        public void Convert_NullableFormattableToString_CorrectValue()
        {
            // Arrange
            int? nullableInt = 123;

            // Act
            var result = _sut.Convert(nullableInt, typeof(string), "D4", null);

            // Assert
            var resultTyped = Assert.IsType<string>(result);
            Assert.Equal("0123", resultTyped);
        }
    }
}

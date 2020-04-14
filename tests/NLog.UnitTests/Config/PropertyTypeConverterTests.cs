using NLog.Config;
using System;
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
            var result = ConvertTest<int, int?>(123);

            // Assert
            Assert.Equal(123, result);
        }

        [Fact]
        public void Convert_NullableIntToInt_CorrectValue()
        {
            // Arrange
            var result = ConvertTest<int?, int>(123);

            // Act
            Assert.Equal(123, result);
        }
        
        [Fact]
        public void Convert_FormattableToString_CorrectValue()
        {
            // Arrange
            var result = ConvertTest<int, string>(123,"D4");

            // Act
            Assert.Equal("0123", result);
        }  

        [Fact]
        public void Convert_NullableFormattableToString_CorrectValue()
        {
            // Arrange
            int? nullableInt = 123;
            var result = ConvertTest<int?, string>(nullableInt,"D4");

            // Act
            Assert.Equal("0123", result);
        }

        private TTo ConvertTest<TFrom, TTo>(TFrom value, string format = null, IFormatProvider formatProvider = null)
        {
            var result = _sut.Convert(value, typeof(TTo), format, formatProvider);

            // Assert
            return Assert.IsType<TTo>(result);
        }
    }
}

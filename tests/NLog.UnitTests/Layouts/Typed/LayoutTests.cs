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

using NLog.Layouts;
using System;
using System.Globalization;
using System.Threading;
using NLog.Config;
using Xunit;

namespace NLog.UnitTests.Layouts.Typed
{
    public class LayoutTests : NLogTestBase
    {
        [Theory]
        [InlineData(100)]
        [InlineData(100d)]
        [InlineData("100")]
        [InlineData(" 100 ")]
        public void TypedIntLayoutDynamicTest(object value)
        {
            // Arrange
            var layout = CreateLayoutRenderedFromProperty<int>();
            var logEventInfo = CreateLogEventInfoWithValue(value);

            // Act
            var result = layout.RenderToValueOrDefault(logEventInfo);

            // Assert
            Assert.Equal(100, result);
        }

        [Fact]
        public void TypedNullableIntToIntLayoutDynamicTest()
        {
            // Arrange
            var layout = CreateLayoutRenderedFromProperty<int>();
            int? value = 100;
            var logEventInfo = CreateLogEventInfoWithValue(value);

            // Act
            var result = layout.RenderToValueOrDefault(logEventInfo);

            // Assert
            Assert.Equal(100, result);
        }

        [Theory]
        [InlineData(100)]
        [InlineData(100d)]
        [InlineData("100")]
        [InlineData(" 100 ")]
        public void TypedNullableIntLayoutDynamicTest(object value)
        {
            // Arrange
            var layout = CreateLayoutRenderedFromProperty<int?>();
            var logEventInfo = CreateLogEventInfoWithValue(value);

            // Act
            var result = layout.RenderToValueOrDefault(logEventInfo);

            // Assert
            Assert.Equal(100, result);
        }

        [Theory]
        [InlineData(100.5)]
        [InlineData("100.5", "EN-us")]
        [InlineData("  100.5  ", "EN-us")]
        public void TypedDecimalLayoutDynamicTest(object value, string culture = null)
        {
            // Arrange
            var oldCulture = Thread.CurrentThread.CurrentCulture;

            try
            {
                if (!string.IsNullOrEmpty(culture))
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
                }

                var layout = CreateLayoutRenderedFromProperty<decimal>();
                var logEventInfo = CreateLogEventInfoWithValue(value);

                // Act
                var result = layout.RenderToValueOrDefault(logEventInfo);

                // Assert
                decimal expected = 100.5m;
                Assert.Equal(expected, result);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = oldCulture;
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData("true")]
        [InlineData(" true ")]
        public void TypedBoolLayoutDynamicTest(object value)
        {
            // Arrange
            var layout = CreateLayoutRenderedFromProperty<bool>();
            var logEventInfo = CreateLogEventInfoWithValue(value);

            // Act
            var result = layout.RenderToValueOrDefault(logEventInfo);

            // Assert
            Assert.True(result);
        }

        /// <remarks>Cache usage, see coverage result</remarks>
        [Fact]
        public void SameValueShouldUseCacheAndCorrectResult()
        {
            // Arrange
            var layout = CreateLayoutRenderedFromProperty<bool>();
            var logEventInfo1 = CreateLogEventInfoWithValue("true");
            var logEventInfo2 = CreateLogEventInfoWithValue("true");
            var logEventInfo3 = CreateLogEventInfoWithValue("true");

            // Act
            var result1 = layout.RenderToValueOrDefault(logEventInfo1);
            var result2 = layout.RenderToValueOrDefault(logEventInfo2);
            var result3 = layout.RenderToValueOrDefault(logEventInfo3);

            // Assert
            Assert.True(result1);
            Assert.True(result2);
            Assert.True(result3);
        }

        [Fact]
        public void ComplexTypeTest()
        {
            // Arrange
            var value = new TestObject { Value = "123" };
            var layout = CreateLayoutRenderedFromProperty<TestObject>();
            var logEventInfo = CreateLogEventInfoWithValue(value);

            // Act
            var result = layout.RenderToValueOrDefault(logEventInfo);

            // Assert
            Assert.Equal(value, result);
        }

        [Fact]
        public void WrongValueErrorTest()
        {
            // Arrange
            var value = "12312aa3";
            var layout = CreateLayoutRenderedFromProperty<int>();
            var logEventInfo = CreateLogEventInfoWithValue(value);

            // Act
            using (new NoThrowNLogExceptions())
            {
                layout.RenderToValueOrDefault(logEventInfo);
            }

            // Assert
            // No Exception
        }

        [Fact]
        public void WrongValueErrorTestThrowsEception()
        {
            // Arrange
            var value = "12312aa3";
            var layout = CreateLayoutRenderedFromProperty<int>();
            var logEventInfo = CreateLogEventInfoWithValue(value);

            Action action = () => layout.RenderToValueOrDefault(logEventInfo);

            // Assert
            Assert.Throws<FormatException>(action);

        }

        [Theory]
        [InlineData(100, "100")]
        [InlineData(null, null)]
        public void GetFormattedMessageFixedValueTest(int? input, string expected)
        {
            // Arrange
            var layout = new LayoutWithPublicGetFormattedMessage<int?>(input);
            var logEventInfo = LogEventInfo.CreateNullEvent();

            // Act
            var result = layout.GetFormattedMessage(logEventInfo);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("100", "100")]
        public void GetFormattedStringMessageFixedValueTest(string input, string expected)
        {
            // Arrange
            var layout = new LayoutWithPublicGetFormattedMessage<string>(input);
            var logEventInfo = LogEventInfo.CreateNullEvent();

            // Act
            var result = layout.GetFormattedMessage(logEventInfo);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("100", 100, true)]
        [InlineData("1  00", null, false)]
        public void TryGetRawValueTest(string input, int? expected, bool expectedSuccess)
        {
            // Arrange


            var layout = new Layout<int?>("${event-properties:prop1}");
            var logEventInfo = LogEventInfo.CreateNullEvent();
            logEventInfo.Properties["prop1"] = input;

            // Act
            bool success;
            object result;
            using (new NoThrowNLogExceptions())
            {
                success = layout.TryGetRawValue(logEventInfo, out result);
            }

            // Assert
            Assert.Equal(expected, result);
            Assert.Equal(expectedSuccess, success);
        }

        [Fact]
        public void TryConvertToShouldHandleNullLayout()
        {
            // Arrange
            var layout = new Layout<int>((Layout)null);
            var logEventInfo = LogEventInfo.CreateNullEvent();

            // Act
            var success = layout.TryGetRawValue(logEventInfo, out var result);

            // Assert
            Assert.Equal(0, result);
            Assert.True(success);
        }

        [Fact]
        public void RenderShouldHandleInvalidConversionAndLogFactory()
        {
            // Arrange
            var factory = new LogFactory
            {
                ThrowExceptions = false,
            };
            var configuration = new LoggingConfiguration(factory);

            var layout = new Layout<int>("${event-properties:prop1}");
            layout.Initialize(configuration);
            var logEventInfo = LogEventInfo.CreateNullEvent();
            logEventInfo.Properties["prop1"] = "Not a int";

            // Act
            int result;
            using (new NoThrowNLogExceptions())
            {
                result = layout.RenderToValueOrDefault(logEventInfo);
            }

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void RenderShouldHandleInvalidConversionAndLogFactoryAndDefault()
        {
            // Arrange
            var factory = new LogFactory
            {
                ThrowExceptions = false,
            };
            var configuration = new LoggingConfiguration(factory);

            var layout = new Layout<int>("${event-properties:prop1}");
            layout.Initialize(configuration);
            var logEventInfo = LogEventInfo.CreateNullEvent();
            logEventInfo.Properties["prop1"] = "Not a int";

            // Act
            int result;
            using (new NoThrowNLogExceptions())
            {
                result = layout.RenderToValueOrDefault(logEventInfo, 200);
            }

            // Assert
            Assert.Equal(200, result);
        }

        [Fact]
        public void RenderShouldHandleValidConversion()
        {
            // Arrange
            var layout = new Layout<int>("${event-properties:prop1}");
            var logEventInfo = LogEventInfo.CreateNullEvent();
            logEventInfo.Properties["prop1"] = "100";

            // Act
            var result = layout.RenderToValueOrDefault(logEventInfo);

            // Assert
            Assert.Equal(100, result);
        }

        [Fact]
        public void Equals_Fixed_EqualTest()
        {
            // Arrange
            var layout1 = new Layout<int>(1);
            var layout2 = new Layout<int>(1);

            // Act
            var equal = layout1.Equals((object)layout2);

            // Assert
            Assert.True(equal);
        }

        [Fact]
        public void Equals_Fixed_NotEqualTest()
        {
            // Arrange
            var layout1 = new Layout<int>(1);
            var layout2 = new Layout<int>(2);

            // Act
            var equal = layout1.Equals((object)layout2);

            // Assert
            Assert.False(equal);
        }

        [Fact]
        public void Equals_Dynamic_EqualTest()
        {
            // Arrange
            var layout1 = new Layout<int>("${event-properties:prop1}");
            var layout2 = new Layout<int>("${event-properties:prop1}");

            // Act
            var equal = layout1.Equals((object)layout2);

            // Assert
            Assert.True(equal);
        }

        [Fact]
        public void Equals_Dynamic_NotEqualTest()
        {
            // Arrange
            var layout1 = new Layout<int>("${event-properties:prop1}");
            var layout2 = new Layout<int>("${event-properties:prop2}");

            // Act
            var equal = layout1.Equals((object)layout2);

            // Assert
            Assert.False(equal);
        }

        private class TestObject
        {
            public string Value { get; set; }
        }

        private static Layout<T> CreateLayoutRenderedFromProperty<T>()
        {
            var layout = new Layout<T>("${event-properties:value1}");
            return layout;
        }

        private static LogEventInfo CreateLogEventInfoWithValue(object value)
        {
            var logEventInfo = LogEventInfo.Create(LogLevel.Info, "logger1", "message1");
            logEventInfo.Properties.Add("value1", value);
            return logEventInfo;
        }


        private class LayoutWithPublicGetFormattedMessage<T> : Layout<T>
        {
            /// <inheritdoc />
            public LayoutWithPublicGetFormattedMessage(T value) : base(value)
            {
            }

            /// <inheritdoc />
            public LayoutWithPublicGetFormattedMessage(Layout layout) : base(layout)
            {
            }

            #region Overrides of Layout<T>

            public string GetFormattedMessage(LogEventInfo logEvent)
            {
                return base.GetFormattedMessage(logEvent);
            }

            #endregion
        }
    }
}

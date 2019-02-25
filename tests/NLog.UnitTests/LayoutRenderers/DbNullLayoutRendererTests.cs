// 
// Copyright (c) 2004-2019 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
using System.Linq;
using NLog.Internal;
using NLog.LayoutRenderers;
using NLog.Layouts;
using Xunit;

namespace NLog.UnitTests.LayoutRenderers
{
    public class DbNullLayoutRendererTests
    {
        [Fact]
        public void TryGetRawValue_emptyEvent_shouldReturnDbNull()
        {
            // Arrange
            var renderer = new DbNullLayoutRenderer();
            IRawValue rawValueGetter = renderer;

            // Act
            var result = rawValueGetter.TryGetRawValue(LogEventInfo.CreateNullEvent(), out var resultValue);

            // Assert
            Assert.Equal(DBNull.Value, resultValue);
            Assert.True(result);
        }

        [Theory]
        [InlineData("logger", "DBNullValue", true)]
        [InlineData("logger1", null, false)]
        public void WhenDbNullRawValueShouldWork(string loggername, object expectedValue, bool expectedSuccess)
        {
            expectedValue = OptionalConvert(expectedValue);

            //else cannot be invoked ambiently. First param is inner
            SimpleLayout l = @"${when:${db-null}:when=logger=='logger':else=better}";

            var le = LogEventInfo.Create(LogLevel.Info, loggername, "message");
            var success = l.TryGetRawValue(le, out var result);

            Assert.Equal(expectedValue, result);
            Assert.Equal(expectedSuccess, success);
        }       
        [Theory]
        [InlineData("logger1", "DBNullValue", true)]
        [InlineData("logger", null, false)]
        public void WhenDbNullRawValueShouldWorkElse(string loggername, object expectedValue, bool expectedSuccess)
        {

            expectedValue = OptionalConvert(expectedValue);

            //else cannot be invoked ambiently. First param is inner
            SimpleLayout l = @"${when:something:when=logger=='logger':else=${db-null}}";

            var le = LogEventInfo.Create(LogLevel.Info, loggername, "message");
            var success = l.TryGetRawValue(le, out var result);

            Assert.Equal(expectedValue, result);
            Assert.Equal(expectedSuccess, success);
        }

        private static object OptionalConvert(object expectedValue)
        {
            if (expectedValue is string s && s == "DBNullValue")
            {
                expectedValue = DBNull.Value;
            }

            return expectedValue;
        }
    }
}

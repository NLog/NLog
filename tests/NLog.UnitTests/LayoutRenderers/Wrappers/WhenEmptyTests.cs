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

using NLog.Internal;

namespace NLog.UnitTests.LayoutRenderers.Wrappers
{
    using System;
    using NLog;
    using NLog.Layouts;
    using Xunit;

    public class WhenEmptyTests : NLogTestBase
    {
        [Fact]
        public void CoalesceTest()
        {
            SimpleLayout l = @"${message:whenEmpty=<no message>}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
            Assert.Equal("message", l.Render(le));

            // empty log message
            var le2 = LogEventInfo.Create(LogLevel.Info, "logger", "");
            Assert.Equal("<no message>", l.Render(le2));
        }

        [Fact]
        public void CoalesceWithANestedLayout()
        {
            SimpleLayout l = @"${message:whenEmpty=${logger} emitted empty message}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
            Assert.Equal("message", l.Render(le));

            // empty log message
            var le2 = LogEventInfo.Create(LogLevel.Info, "mylogger", "");
            Assert.Equal("mylogger emitted empty message", l.Render(le2));
        }

        [Fact]
        public void WhenEmpty_MissingInner_ShouldNotThrow()
        {
            using (new NoThrowNLogExceptions())
            {
                SimpleLayout l = @"${whenEmpty:whenEmpty=${literal:text=c:\logs\}:inner=${environment:LOG_DIR_XXX}}api.log";
                var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
                LogManager.ThrowExceptions = true;
                Assert.Equal("api.log", l.Render(le));
            }
        }

        [Fact]
        public void WhenDbNullRawValueShouldWork()
        {
            SimpleLayout l = @"${event-properties:prop1:whenEmpty=${db-null}}";
            {
                var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
                le.Properties["prop1"] = 1;
                var success = l.TryGetRawValue(le, out var rawValue);
                Assert.True(success);
                Assert.Equal(1, rawValue);
            }
            // empty log message
            {
                var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
                var success = l.TryGetRawValue(le, out var rawValue);
                Assert.True(success);
                Assert.Equal(DBNull.Value, rawValue);
            }

        }

        [Theory]
        [InlineData("message", "message")]
        [InlineData("", "default")]
        public void GetStringValueShouldWork(string message, string expected)
        {
            // Arrange
            SimpleLayout layout = @"${message:whenEmpty=default}";
            var stringValueRenderer = (IStringValueRenderer)layout.Renderers[0];
            var logEvent = LogEventInfo.Create(LogLevel.Info, "logger", message);

            // Act
            var result = stringValueRenderer.GetFormattedString(logEvent);

            // Assert
            Assert.Equal(expected, result);


        }
    }
}
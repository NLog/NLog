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
using NLog.Config;

namespace NLog.UnitTests.LayoutRenderers.Wrappers
{
    using NLog;
    using NLog.Layouts;
    using Xunit;

    public class WhenTests : NLogTestBase
    {
        [Fact]
        public void PositiveWhenTest()
        {
            SimpleLayout l = @"${message:when=logger=='logger'}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
            Assert.Equal("message", l.Render(le));
        }

        [Fact]
        public void NegativeWhenTest()
        {
            SimpleLayout l = @"${message:when=logger=='logger'}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger2", "message");
            Assert.Equal("", l.Render(le));
        }

        [Fact]
        public void ComplexWhenTest()
        {
            // condition is pretty complex here and includes nested layout renderers
            // we are testing here that layout parsers property invokes Condition parser to consume the right number of characters
            SimpleLayout l = @"${message:when='${pad:${logger}:padding=10:padCharacter=X}'=='XXXXlogger':padding=-10:padCharacter=Y}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
            Assert.Equal("messageYYY", l.Render(le));
        }

        [Fact]
        public void ComplexWhenTest2()
        {
            // condition is pretty complex here and includes nested layout renderers
            // we are testing here that layout parsers property invokes Condition parser to consume the right number of characters
            SimpleLayout l = @"${message:padding=-10:padCharacter=Y:when='${pad:${logger}:padding=10:padCharacter=X}'=='XXXXlogger'}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
            Assert.Equal("messageYYY", l.Render(le));
        }

        [Fact]
        public void WhenElseCase()
        {
            //else cannot be invoked ambiently. First param is inner
            SimpleLayout l = @"${when:good:when=logger=='logger':else=better}";

            {
                var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
                Assert.Equal("good", l.Render(le));
            }
            {
                var le = LogEventInfo.Create(LogLevel.Info, "logger1", "message");
                Assert.Equal("better", l.Render(le));
            }
        }

        [Fact]
        public void WhenElseCase_empty_when()
        {
            using (new NoThrowNLogExceptions())
            {
                //else cannot be invoked ambiently. First param is inner
                SimpleLayout l = @"${when:good:else=better}";

                {
                    var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
                    Assert.Equal("good", l.Render(le));
                }
                {
                    var le = LogEventInfo.Create(LogLevel.Info, "logger1", "message");
                    Assert.Equal("good", l.Render(le));
                }
            }
        }

        [Fact]
        public void WhenElseCase_noIf()
        {
            //else cannot be invoked ambiently. First param is inner
            SimpleLayout l = @"${when:when=logger=='logger':else=better}";

            {
                var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
                Assert.Equal("", l.Render(le));
            }
            {
                var le = LogEventInfo.Create(LogLevel.Info, "logger1", "message");
                Assert.Equal("better", l.Render(le));
            }
        }

        [Fact]
        public void WhenLogLevelConditionTestLayoutRenderer()
        {
            //else cannot be invoked ambiently. First param is inner
            SimpleLayout l = @"${when:when=level<=LogLevel.Info:inner=Good:else=Bad}";

            {
                var le = LogEventInfo.Create(LogLevel.Debug, "logger", "message");
                Assert.Equal("Good", l.Render(le));
            }
            {
                var le = LogEventInfo.Create(LogLevel.Error, "logger1", "message");
                Assert.Equal("Bad", l.Render(le));
            }

        }

        [Fact]
        public void WhenLogLevelConditionTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog throwExceptions='true'>
                <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Trace' writeTo='debug'>
                    <filters>
                        <when condition=""level>=LogLevel.Info"" action=""Log""></when>
                        <when condition='true' action='Ignore' />
                    </filters>
                    </logger>
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Trace("Test");
           
            AssertDebugCounter("debug", 0);
            logger.Debug("Test");
            AssertDebugCounter("debug", 0);
            logger.Info("Test");
            AssertDebugCounter("debug", 1);
            logger.Warn("Test");
            AssertDebugCounter("debug", 2);
            logger.Error("Test");
            AssertDebugCounter("debug", 3);
            logger.Fatal("Test");
            AssertDebugCounter("debug", 4);
          
        }


        [Fact]
        public void WhenNumericAndPropertyConditionTest()
        {
            //else cannot be invoked ambiently. First param is inner
            SimpleLayout l = @"${when:when=100 < '${event-properties:item=Elapsed}':inner=Slow:else=Fast}";

//            WhenNumericAndPropertyConditionTest_inner(l, "a", false);
            WhenNumericAndPropertyConditionTest_inner(l, 101, false);
            WhenNumericAndPropertyConditionTest_inner(l, 11, true);
            WhenNumericAndPropertyConditionTest_inner(l, 100, true);
            WhenNumericAndPropertyConditionTest_inner(l, 1, true);
            WhenNumericAndPropertyConditionTest_inner(l, 2, true);
            WhenNumericAndPropertyConditionTest_inner(l, 20, true);
            WhenNumericAndPropertyConditionTest_inner(l, 100000, false);
        }

        private static void WhenNumericAndPropertyConditionTest_inner(SimpleLayout l, object time, bool fast)
        {
            var le = LogEventInfo.Create(LogLevel.Debug, "logger", "message");
            le.Properties["Elapsed"] = time;
            Assert.Equal(fast ? "Fast" : "Slow", l.Render(le));
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
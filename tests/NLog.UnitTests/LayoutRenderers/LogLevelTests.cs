// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.UnitTests.LayoutRenderers
{
    using System;
    using Xunit;

    public class LogLevelTests : NLogTestBase
    {
        [Fact]
        public void LogLevelTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${level} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            logger.Debug("a");
            logFactory.AssertDebugLastMessage("Debug a");
            logger.Info("a");
            logFactory.AssertDebugLastMessage("Info a");
            logger.Warn("a");
            logFactory.AssertDebugLastMessage("Warn a");
            logger.Error("a");
            logFactory.AssertDebugLastMessage("Error a");
            logger.Fatal("a");
            logFactory.AssertDebugLastMessage("Fatal a");
        }

        [Fact]
        public void LogLevelUppercaseTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${level:uppercase=true} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            ILogger logger = logFactory.GetLogger("A");
            logger.Debug("a");
            logFactory.AssertDebugLastMessage("DEBUG a");
            logger.Info("a");
            logFactory.AssertDebugLastMessage("INFO a");
            logger.Warn("a");
            logFactory.AssertDebugLastMessage("WARN a");
            logger.Error("a");
            logFactory.AssertDebugLastMessage("ERROR a");
            logger.Fatal("a");
            logFactory.AssertDebugLastMessage("FATAL a");
        }

        [Fact]
        public void LogLevelSingleCharacterTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${level:format=FirstCharacter} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Trace' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            logger.Trace("a");
            logFactory.AssertDebugLastMessage("T a");
            logger.Debug("a");
            logFactory.AssertDebugLastMessage("D a");
            logger.Info("a");
            logFactory.AssertDebugLastMessage("I a");
            logger.Warn("a");
            logFactory.AssertDebugLastMessage("W a");
            logger.Error("a");
            logFactory.AssertDebugLastMessage("E a");
            logger.Fatal("a");
            logFactory.AssertDebugLastMessage("F a");
        }

        [Fact]
        public void LogLevelOrdinalTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${level:format=Ordinal} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Trace' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            logger.Trace("a");
            logFactory.AssertDebugLastMessage("0 a");
            logger.Debug("a");
            logFactory.AssertDebugLastMessage("1 a");
            logger.Info("a");
            logFactory.AssertDebugLastMessage("2 a");
            logger.Warn("a");
            logFactory.AssertDebugLastMessage("3 a");
            logger.Error("a");
            logFactory.AssertDebugLastMessage("4 a");
            logger.Fatal("a");
            logFactory.AssertDebugLastMessage("5 a");
        }


        [Fact]
        public void LogLevelFullNameTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${level:format=FullName} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Trace' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            logger.Trace("a");
            logFactory.AssertDebugLastMessage("Trace a");
            logger.Debug("a");
            logFactory.AssertDebugLastMessage("Debug a");
            logger.Info("a");
            logFactory.AssertDebugLastMessage("Information a");
            logger.Warn("a");
            logFactory.AssertDebugLastMessage("Warning a");
            logger.Error("a");
            logFactory.AssertDebugLastMessage("Error a");
            logger.Fatal("a");
            logFactory.AssertDebugLastMessage("Fatal a");
        }

        [Fact]
        public void LogLevelTriLetterTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${level:format=TriLetter} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Trace' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            logger.Trace("a");
            logFactory.AssertDebugLastMessage("Trc a");
            logger.Debug("a");
            logFactory.AssertDebugLastMessage("Dbg a");
            logger.Info("a");
            logFactory.AssertDebugLastMessage("Inf a");
            logger.Warn("a");
            logFactory.AssertDebugLastMessage("Wrn a");
            logger.Error("a");
            logFactory.AssertDebugLastMessage("Err a");
            logger.Fatal("a");
            logFactory.AssertDebugLastMessage("Ftl a");
        }

        [Fact]
        public void LogLevelGetTypeCodeTest()
        {
            // Arrange
            var logLevel = LogLevel.Info;

            // Act
            var result = Convert.GetTypeCode(logLevel);

            // Assert
            Assert.Equal(TypeCode.Object, result);
        }

        [Theory]
        [InlineData(typeof(int), 2)]
        [InlineData(typeof(uint), (uint)2)]
        [InlineData(typeof(string), "Info")]
        public void LogLevelConvertTest(Type type, object expected)
        {
            // Arrange
            IFormattable logLevel = LogLevel.Info;
            var logConverter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(LogLevel));
                
            // Act
            var convertToResult = logConverter.CanConvertTo(type) ? logConverter.ConvertTo(logLevel, type) : null;
            var convertFromResult = logConverter.CanConvertFrom(expected.GetType()) ? logConverter.ConvertFrom(expected) : null;

            // Assert
            Assert.Equal(expected, convertToResult);
            Assert.Equal(logLevel, convertFromResult);
        }
    }
}
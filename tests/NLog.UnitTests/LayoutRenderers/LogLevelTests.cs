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

namespace NLog.UnitTests.LayoutRenderers
{
    using Xunit;

    public class LogLevelTests : NLogTestBase
    {
        [Fact]
        public void LogLevelTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${level} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Debug("a");
            AssertDebugLastMessage("debug", "Debug a");
            logger.Info("a");
            AssertDebugLastMessage("debug", "Info a");
            logger.Warn("a");
            AssertDebugLastMessage("debug", "Warn a");
            logger.Error("a");
            AssertDebugLastMessage("debug", "Error a");
            logger.Fatal("a");
            AssertDebugLastMessage("debug", "Fatal a");
        }

        [Fact]
        public void LogLevelSingleCharacterTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${level:format=FirstCharacter} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Trace' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Trace("a");
            AssertDebugLastMessage("debug", "T a");
            logger.Debug("a");
            AssertDebugLastMessage("debug", "D a");
            logger.Info("a");
            AssertDebugLastMessage("debug", "I a");
            logger.Warn("a");
            AssertDebugLastMessage("debug", "W a");
            logger.Error("a");
            AssertDebugLastMessage("debug", "E a");
            logger.Fatal("a");
            AssertDebugLastMessage("debug", "F a");
        }

        [Fact]
        public void LogLevelOrdinalTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${level:format=Ordinal} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Trace' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Trace("a");
            AssertDebugLastMessage("debug", "0 a");
            logger.Debug("a");
            AssertDebugLastMessage("debug", "1 a");
            logger.Info("a");
            AssertDebugLastMessage("debug", "2 a");
            logger.Warn("a");
            AssertDebugLastMessage("debug", "3 a");
            logger.Error("a");
            AssertDebugLastMessage("debug", "4 a");
            logger.Fatal("a");
            AssertDebugLastMessage("debug", "5 a");
        }


        [Fact]
        public void LogLevelFullNameTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${level:format=FullName} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Trace' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Trace("a");
            AssertDebugLastMessage("debug", "Trace a");
            logger.Debug("a");
            AssertDebugLastMessage("debug", "Debug a");
            logger.Info("a");
            AssertDebugLastMessage("debug", "Information a");
            logger.Warn("a");
            AssertDebugLastMessage("debug", "Warning a");
            logger.Error("a");
            AssertDebugLastMessage("debug", "Error a");
            logger.Fatal("a");
            AssertDebugLastMessage("debug", "Fatal a");
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
            IConvertible logLevel = LogLevel.Info;
            var logConverter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(LogLevel));
                
            // Act
            var changeTypeResult = Convert.ChangeType(logLevel, type);
            var changeToResult = logLevel.ToType(type, System.Globalization.CultureInfo.CurrentCulture);
            var convertToResult = logConverter.CanConvertTo(type) ? logConverter.ConvertTo(logLevel, type) : null;
            var convertFromResult = logConverter.CanConvertFrom(expected.GetType()) ? logConverter.ConvertFrom(expected) : null;

            // Assert
            Assert.Equal(expected, changeTypeResult);
            Assert.Equal(expected, changeToResult);
            Assert.Equal(expected, convertToResult);
            Assert.Equal(logLevel, convertFromResult);
        }
    }
}
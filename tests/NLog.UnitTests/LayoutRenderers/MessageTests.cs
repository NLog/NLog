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
    using System.Globalization;
    using Xunit;

    public class MessageTests : NLogTestBase
    {
        [Fact]
        public void MessageWithoutPaddingTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            logger.Debug("a");
            logFactory.AssertDebugLastMessage("a");
            logger.Debug("a{0}", 1);
            logFactory.AssertDebugLastMessage("a1");
            logger.Debug("a{0}{1}", 1, "2");
            logFactory.AssertDebugLastMessage("a12");
            logger.Debug(CultureInfo.InvariantCulture, "a{0}", new DateTime(2005, 1, 1));
            logFactory.AssertDebugLastMessage("a01/01/2005 00:00:00");
        }

        [Fact]
        public void MessageRightPaddingTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message:padding=3}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            logger.Debug("a");
            logFactory.AssertDebugLastMessage("  a");
            logger.Debug("a{0}", 1);
            logFactory.AssertDebugLastMessage(" a1");
            logger.Debug("a{0}{1}", 1, "2");
            logFactory.AssertDebugLastMessage("a12");
            logger.Debug(CultureInfo.InvariantCulture, "a{0}", new DateTime(2005, 1, 1));
            logFactory.AssertDebugLastMessage("a01/01/2005 00:00:00");
        }


        [Fact]
        public void MessageFixedLengthRightPaddingLeftAlignmentTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message:padding=3:fixedlength=true}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            logger.Debug("a");
            logFactory.AssertDebugLastMessage("  a");
            logger.Debug("a{0}", 1);
            logFactory.AssertDebugLastMessage(" a1");
            logger.Debug("a{0}{1}", 1, "2");
            logFactory.AssertDebugLastMessage("a12");
            logger.Debug(CultureInfo.InvariantCulture, "a{0}", new DateTime(2005, 1, 1));
            logFactory.AssertDebugLastMessage("a01");
        }

        [Fact]
        public void MessageFixedLengthRightPaddingRightAlignmentTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message:padding=3:fixedlength=true:alignmentOnTruncation=right}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            logger.Debug("a");
            logFactory.AssertDebugLastMessage("  a");
            logger.Debug("a{0}", 1);
            logFactory.AssertDebugLastMessage(" a1");
            logger.Debug("a{0}{1}", 1, "2");
            logFactory.AssertDebugLastMessage("a12");
            logger.Debug(CultureInfo.InvariantCulture, "a{0}", new DateTime(2005, 1, 1));
            logFactory.AssertDebugLastMessage(":00");
        }

        [Fact]
        public void MessageLeftPaddingTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message:padding=-3:padcharacter=x}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            logger.Debug("a");
            logFactory.AssertDebugLastMessage("axx");
            logger.Debug("a{0}", 1);
            logFactory.AssertDebugLastMessage("a1x");
            logger.Debug("a{0}{1}", 1, "2");
            logFactory.AssertDebugLastMessage("a12");
            logger.Debug(CultureInfo.InvariantCulture, "a{0}", new DateTime(2005, 1, 1));
            logFactory.AssertDebugLastMessage("a01/01/2005 00:00:00");
        }

        [Fact]
        public void MessageFixedLengthLeftPaddingLeftAlignmentTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message:padding=-3:padcharacter=x:fixedlength=true}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            logger.Debug("a");
            logFactory.AssertDebugLastMessage("axx");
            logger.Debug("a{0}", 1);
            logFactory.AssertDebugLastMessage("a1x");
            logger.Debug("a{0}{1}", 1, "2");
            logFactory.AssertDebugLastMessage("a12");
            logger.Debug(CultureInfo.InvariantCulture, "a{0}", new DateTime(2005, 1, 1));
            logFactory.AssertDebugLastMessage("a01");
        }

        [Fact]
        public void MessageFixedLengthLeftPaddingRightAlignmentTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message:padding=-3:padcharacter=x:fixedlength=true:alignmentOnTruncation=right}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            logger.Debug("a");
            logFactory.AssertDebugLastMessage("axx");
            logger.Debug("a{0}", 1);
            logFactory.AssertDebugLastMessage("a1x");
            logger.Debug("a{0}{1}", 1, "2");
            logFactory.AssertDebugLastMessage("a12");
            logger.Debug(CultureInfo.InvariantCulture, "a{0}", new DateTime(2005, 1, 1));
            logFactory.AssertDebugLastMessage(":00");
        }

        [Fact]
        public void MessageWithExceptionAndCustomSeparatorTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message:withException=true:exceptionSeparator=,}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            logger.Debug("a");
            logFactory.AssertDebugLastMessage("a");

            var ex = new InvalidOperationException("Exception message.");
#if !NET35
            logger.Debug(new AggregateException(ex), "Foo");
#else
            logger.Debug(ex, "Foo");
#endif
            logFactory.AssertDebugLastMessage("Foo," + ex.ToString());

            logger.Debug(ex);
            logFactory.AssertDebugLastMessage(ex.ToString());
        }

        [Fact]
        public void SingleParameterException_OutputsSingleStackTrace()
        {
            // Arrange
            var logTarget = new NLog.Targets.DebugTarget("debug") { Layout = "${message}|${exception}" };
            var logFactory = new LogFactory().Setup().LoadConfiguration(builder =>
            {
                builder.ForLogger().WriteTo(logTarget);
            }).LogFactory;

            var logger = logFactory.GetLogger("SingleParameterException");

            // Act
            try
            {
                logger.Info("Hello");
                Exception argumentException = new ArgumentException("Holy Moly");
#if !NET35
                argumentException = new AggregateException(argumentException);
#endif
                throw argumentException;
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
            }

            // Assert
            Assert.StartsWith("System.ArgumentException: Holy Moly|System.ArgumentException", logTarget.LastMessage);
        }
    }
}
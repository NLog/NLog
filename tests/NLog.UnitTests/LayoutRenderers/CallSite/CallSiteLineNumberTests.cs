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

using System;
using System.Threading.Tasks;
using NLog.Config;
using Xunit;

namespace NLog.UnitTests.LayoutRenderers
{
    public class CallSiteLineNumberTests : NLogTestBase
    {
#if !DEBUG
        [Fact(Skip = "RELEASE not working, only DEBUG")]
#else
        [Fact]
#endif
        public void LineNumberOnlyTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite-linenumber} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");

#if DEBUG
#line 100000
#endif
            logger.Debug("msg");
            var linenumber = GetPrevLineNumber();
            var lastMessage = GetDebugLastMessage("debug", logFactory);
            // There's a difference in handling line numbers between .NET and Mono
            // We're just interested in checking if it's above 100000
            Assert.StartsWith(linenumber.ToString(), lastMessage);  // Expect prefix of 10000
#if DEBUG
#line default
#endif
        }

#if !MONO
        [Fact]
#else
        [Fact(Skip = "MONO is not good with callsite line numbers")]
#endif
        public void LineNumberOnlyAsyncTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
        <nlog>
            <targets><target name='debug' type='Debug' layout='${callsite-linenumber}' /></targets>
            <rules>
                <logger name='*' minlevel='Debug' writeTo='debug' />
            </rules>
        </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            Func<string> getLastMessage = () => GetDebugLastMessage("debug", logFactory);
            logger.Debug("msg");
            var lastMessage = getLastMessage();
            Assert.NotEqual(0, int.Parse(lastMessage));
            WriteMessages(logger, getLastMessage).Wait();
        }

        [Fact]
        public void LineNumberNoCaptureStackTraceTest()
        {
            // Arrange
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
        <nlog>
            <targets><target name='debug' type='Debug' layout='${callsite-linenumber:captureStackTrace=false} ${message}' /></targets>
            <rules>
                <logger name='*' minlevel='Debug' writeTo='debug' />
            </rules>
        </nlog>").LogFactory;

            // Act
            logFactory.GetLogger("A").Debug("msg");

            // Assert
            logFactory.AssertDebugLastMessage(" msg");
        }

        [Fact]
        public void LineNumberNoCaptureStackTraceWithStackTraceTest()
        {
            // Arrange
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
        <nlog>
            <targets><target name='debug' type='Debug' layout='${callsite-linenumber:captureStackTrace=false} ${message}' /></targets>
            <rules>
                <logger name='*' minlevel='Debug' writeTo='debug' />
            </rules>
        </nlog>").LogFactory;

            // Act
            var logEvent = new LogEventInfo(LogLevel.Info, null, "msg");
            logEvent.SetStackTrace(new System.Diagnostics.StackTrace(true), 0);
            logFactory.GetLogger("A").Log(logEvent);

            // Assert
            logFactory.AssertDebugLastMessageContains(" msg");
            Assert.NotEqual(" msg", GetDebugLastMessage("debug", logFactory));
        }

        private static async Task WriteMessages(Logger logger, Func<string> getLastMessage)
        {
            logger.Info("Line number should be non-zero");
            var lastMessage1 = getLastMessage();
            Assert.NotEqual(0, int.Parse(lastMessage1));

            try
            {
                await Task.Delay(1);

                logger.Info("Line number should be non-zero");
                var lastMessage2 = getLastMessage();
                Assert.NotEqual(0, int.Parse(lastMessage2));

                // Here I have some other logic ...
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            logger.Info("Line number should be non-zero");
            var lastMessage3 = getLastMessage();
            Assert.NotEqual(0, int.Parse(lastMessage3));
        }
    }
}
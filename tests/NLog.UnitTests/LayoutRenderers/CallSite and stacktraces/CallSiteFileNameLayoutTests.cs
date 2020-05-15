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
using System.IO;
using System.Threading.Tasks;
using NLog.Config;
using Xunit;

namespace NLog.UnitTests.LayoutRenderers
{
    public class CallSiteFileNameLayoutTests : NLogTestBase
    {
#if !MONO
        [Fact]
#else
        [Fact(Skip = "MONO is not good with callsite line numbers")]
#endif
        public void ShowFileNameOnlyTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite-filename:includeSourcePath=False}|${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");

            logger.Debug("msg");
            var lastMessage = GetDebugLastMessage("debug");
            var lastMessageArray = lastMessage.Split('|');
            Assert.Equal("CallSiteFileNameLayoutTests.cs", lastMessageArray[0]);
            Assert.Equal("msg", lastMessageArray[1]);
        }

#if !MONO
        [Fact]
#else
        [Fact(Skip = "MONO is not good with callsite line numbers")]
#endif
        public void CallSiteFileNameNoCaptureStackTraceTest()
        {
            // Arrange
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite-filename:captureStackTrace=False}|${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            // Act
            LogManager.GetLogger("A").Debug("msg");

            // Assert
            AssertDebugLastMessage("debug", "|msg");
        }

#if !MONO
        [Fact]
#else
        [Fact(Skip = "MONO is not good with callsite line numbers")]
#endif
        public void CallSiteFileNameNoCaptureStackTraceWithStackTraceTest()
        {
            // Arrange
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite-filename:captureStackTrace=False}|${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            // Act
            var logEvent = new LogEventInfo(LogLevel.Info, null, "msg");
            logEvent.SetStackTrace(new System.Diagnostics.StackTrace(true), 0);
            LogManager.GetLogger("A").Log(logEvent);

            // Assert
            var lastMessage = GetDebugLastMessage("debug");
            var lastMessageArray = lastMessage.Split('|');
            Assert.Contains("CallSiteFileNameLayoutTests.cs", lastMessageArray[0]);
            Assert.Equal("msg", lastMessageArray[1]);
        }

#if !MONO
        [Fact]
#else
        [Fact(Skip = "MONO is not good with callsite line numbers")]
#endif
        public void ShowFullPathTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite-filename:includeSourcePath=True}|${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");

            logger.Debug("msg");
            var lastMessage = GetDebugLastMessage("debug");
            var lastMessageArray = lastMessage.Split('|');
            Assert.Contains("CallSiteFileNameLayoutTests.cs", lastMessageArray[0]);
            Assert.False(lastMessageArray[0].StartsWith("CallSiteFileNameLayoutTests.cs"));
            Assert.True(Path.IsPathRooted(lastMessageArray[0]));
            Assert.Equal("msg", lastMessageArray[1]);
        }

#if !MONO
        [Fact]
#else
        [Fact(Skip = "MONO is not good with callsite line numbers")]
#endif
        public void ShowFileNameOnlyAsyncTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite-filename:includeSourcePath=False}|${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            AsyncMethod().Wait();

            var lastMessage = GetDebugLastMessage("debug");
            var lastMessageArray = lastMessage.Split('|');
            Assert.Equal("CallSiteFileNameLayoutTests.cs", lastMessageArray[0]);
            Assert.Equal("msg", lastMessageArray[1]);
        }

#if !MONO
        [Fact]
#else
        [Fact(Skip = "MONO is not good with callsite line numbers")]
#endif
        public void ShowFullPathAsyncTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite-filename:includeSourcePath=True}|${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");

            AsyncMethod().Wait();

            var lastMessage = GetDebugLastMessage("debug");
            var lastMessageArray = lastMessage.Split('|');
            Assert.Contains("CallSiteFileNameLayoutTests.cs", lastMessageArray[0]);
            Assert.False(lastMessageArray[0].StartsWith("CallSiteFileNameLayoutTests.cs"));
            Assert.True(Path.IsPathRooted(lastMessageArray[0]));
            Assert.Equal("msg", lastMessageArray[1]);
        }

        private async Task AsyncMethod()
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Warn("msg");
            var reader = new StreamReader(new MemoryStream(new byte[0]));
            await reader.ReadLineAsync();
        }
    }
}
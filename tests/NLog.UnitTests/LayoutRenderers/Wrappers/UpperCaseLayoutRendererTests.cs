﻿// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
using System.Threading;

using NLog.Common;
using NLog.Layouts;
using Xunit;

namespace NLog.UnitTests.LayoutRenderers.Wrappers
{
    public class UpperCaseLayoutRendererTests
    {
        [Fact]
        public void ShouldMakeTextUpperCase()
        {
            SimpleLayout l = "${message:uppercase=true}";

            var helloWorld = "hello world";
            var logEvent = LogEventInfo.Create(LogLevel.Info, "test", helloWorld);
            var logMessage = l.Render(logEvent);

            Assert.True(logMessage.IndexOf(helloWorld, StringComparison.Ordinal) == -1);
            Assert.True(logMessage.IndexOf(helloWorld.ToUpper(), StringComparison.Ordinal) > -1);
        }

        [Fact]
        public void LogLevelRenderedTwiceToUpperShouldBeSameReference()
        {
            SimpleLayout l = "${level:uppercase=true}";

            var helloWorld = "hello world";
            var logEvent = LogEventInfo.Create(LogLevel.Info, "test", helloWorld);
            var logEvent2 = LogEventInfo.Create(LogLevel.Info, "test", helloWorld);
            var logMessage = l.Render(logEvent);
            var logMessage2 = l.Render(logEvent2);

            Assert.Equal(logMessage, logMessage2);

        }

        [Fact]
        public void LogLevelRenderedTwiceToUpperShouldBeSameReferenceDifferentLayoyt()
        {
            InternalLogger.LogLevel = LogLevel.Trace;
            InternalLogger.LogToConsole = true;
            SimpleLayout l = "${uppercase:${level}";

            var helloWorld = "hello world";
            var logEvent = LogEventInfo.Create(LogLevel.Info, "test", helloWorld);
            
            var logMessage = l.Render(logEvent);
            Thread.Sleep(100);
            var logEvent2 = LogEventInfo.Create(LogLevel.Info, "test", helloWorld);
            var logMessage2 = l.Render(logEvent2);

            Assert.Equal(logMessage, logMessage2);

        }
    }
}

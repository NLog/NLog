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

using NLog.Config;

namespace NLog.UnitTests.LayoutRenderers
{
    using System.Runtime.CompilerServices;
    using Xunit;

    public class StackTraceRendererTests : NLogTestBase
    {
        [Fact]
        public void RenderStackTrace()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message} ${stacktrace}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            RenderMe("I am:");
            AssertDebugLastMessageContains("debug", " => StackTraceRendererTests.RenderStackTrace => StackTraceRendererTests.RenderMe");
        }

        [Fact]
        public void RenderStackTraceReversed()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message} ${stacktrace:reverse=true}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            RenderMe("I am:");
            AssertDebugLastMessageContains("debug", "StackTraceRendererTests.RenderMe => StackTraceRendererTests.RenderStackTraceReversed => ");
        }

        [Fact]
        public void RenderStackTraceNoCaptureStackTrace()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message} ${stacktrace:captureStackTrace=false}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            RenderMe("I am:");
            AssertDebugLastMessage("debug", "I am: ");
        }

        [Fact]
        public void RenderStackTraceNoCaptureStackTraceWithStackTrace()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message} ${stacktrace:captureStackTrace=false}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            var logEvent = new LogEventInfo(LogLevel.Info, null, "I am:");
            logEvent.SetStackTrace(new System.Diagnostics.StackTrace(true), 0);
            LogManager.GetCurrentClassLogger().Log(logEvent);
            AssertDebugLastMessageContains("debug", $" => {nameof(StackTraceRendererTests)}.{nameof(RenderStackTraceNoCaptureStackTraceWithStackTrace)}");
        }

        [Fact]
        public void RenderStackTrace_topframes()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message} ${stacktrace:topframes=2}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            RenderMe("I am:");
            AssertDebugLastMessage("debug", "I am: StackTraceRendererTests.RenderStackTrace_topframes => StackTraceRendererTests.RenderMe");
        }

        [Fact]
        public void RenderStackTrace_skipframes()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message} ${stacktrace:skipframes=1}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            RenderMe("I am:");
            AssertDebugLastMessageContains("debug", " => StackTraceRendererTests.RenderStackTrace_skipframes");
        }


        [Fact]
        public void RenderStackTrace_raw()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message} ${stacktrace:format=Raw}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            RenderMe("I am:");

            AssertDebugLastMessageContains("debug", "RenderStackTrace_raw at offset ");
            AssertDebugLastMessageContains("debug", "RenderMe at offset ");
#if !MONO
            AssertDebugLastMessageContains("debug", "StackTraceRendererTests.cs");
#endif

            string debugLastMessage = GetDebugLastMessage("debug");
            int index0 = debugLastMessage.IndexOf("RenderStackTraceReversed_raw at offset ");
            int index1 = debugLastMessage.IndexOf("RenderMe at offset ");
            Assert.True(index0 < index1);
        }

        [Fact]
        public void RenderStackTraceReversed_raw()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message} ${stacktrace:format=Raw:reverse=true}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            RenderMe("I am:");

            AssertDebugLastMessageContains("debug", "RenderMe at offset ");
            AssertDebugLastMessageContains("debug", "RenderStackTraceReversed_raw at offset ");
#if !MONO
            AssertDebugLastMessageContains("debug", "StackTraceRendererTests.cs");
#endif

            string debugLastMessage = GetDebugLastMessage("debug");
            int index0 = debugLastMessage.IndexOf("RenderMe at offset ");
            int index1 = debugLastMessage.IndexOf("RenderStackTraceReversed_raw at offset ");
            Assert.True(index0 < index1);
        }

        [Fact]
        public void RenderStackTrace_DetailedFlat()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message} ${stacktrace:format=DetailedFlat}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            RenderMe("I am:");
            AssertDebugLastMessageContains("debug", " => [Void RenderStackTrace_DetailedFlat()] => [Void RenderMe(System.String)]");
        }

        [Fact]
        public void RenderStackTraceReversed_DetailedFlat()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message} ${stacktrace:format=DetailedFlat:reverse=true}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            RenderMe("I am:");
            AssertDebugLastMessageContains("debug", "[Void RenderMe(System.String)] => [Void RenderStackTraceReversed_DetailedFlat()] => ");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void RenderMe(string message)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }

}
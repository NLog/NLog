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
    using System.Runtime.CompilerServices;
    using Xunit;

    public class StackTraceRendererTests : NLogTestBase
    {
        [Fact]
        public void RenderStackTrace()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message} ${stacktrace}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            RenderMe(logFactory, "I am:");
            logFactory.AssertDebugLastMessageContains(" => StackTraceRendererTests.RenderStackTrace => StackTraceRendererTests.RenderMe");
        }

        [Fact]
        public void RenderStackTraceAndCallsite()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message} ${stacktrace} ${callsite:className=false}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            RenderMe(logFactory, "I am:");

            logFactory.AssertDebugLastMessageContains(" => StackTraceRendererTests.RenderStackTraceAndCallsite => StackTraceRendererTests.RenderMe");
        }

        [Fact]
        public void RenderStackTraceReversed()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message} ${stacktrace:reverse=true}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            RenderMe(logFactory, "I am:");
            logFactory.AssertDebugLastMessageContains("StackTraceRendererTests.RenderMe => StackTraceRendererTests.RenderStackTraceReversed => ");
        }

        [Fact]
        public void RenderStackTraceNoCaptureStackTrace()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message} ${stacktrace:captureStackTrace=false}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            RenderMe(logFactory, "I am:");
            logFactory.AssertDebugLastMessage("I am: ");
        }

        [Fact]
        public void RenderStackTraceNoCaptureStackTraceWithStackTrace()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message} ${stacktrace:captureStackTrace=false}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logEvent = new LogEventInfo(LogLevel.Info, null, "I am:");
            logEvent.SetStackTrace(new System.Diagnostics.StackTrace(true), 0);
            logFactory.GetCurrentClassLogger().Log(logEvent);
            logFactory.AssertDebugLastMessageContains($" => {nameof(StackTraceRendererTests)}.{nameof(RenderStackTraceNoCaptureStackTraceWithStackTrace)}");
        }

        [Fact]
        public void RenderStackTrace_topframes()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message} ${stacktrace:topframes=2}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            RenderMe(logFactory, "I am:");
            logFactory.AssertDebugLastMessage("I am: StackTraceRendererTests.RenderStackTrace_topframes => StackTraceRendererTests.RenderMe");
        }

        [Fact]
        public void RenderStackTrace_skipframes()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message} ${stacktrace:skipframes=1}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            RenderMe(logFactory, "I am:");
            logFactory.AssertDebugLastMessageContains(" => StackTraceRendererTests.RenderStackTrace_skipframes");
        }

        [Fact]
        public void RenderStackTrace_raw()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message} ${stacktrace:format=Raw}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            RenderMe(logFactory, "I am:");

            logFactory.AssertDebugLastMessageContains("RenderStackTrace_raw at offset ");
            logFactory.AssertDebugLastMessageContains("RenderMe at offset ");
#if !MONO
            logFactory.AssertDebugLastMessageContains("StackTraceRendererTests.cs");
#endif

            string debugLastMessage = GetDebugLastMessage("debug", logFactory);
            int index0 = debugLastMessage.IndexOf("RenderStackTraceReversed_raw at offset ");
            int index1 = debugLastMessage.IndexOf("RenderMe at offset ");
            Assert.True(index0 < index1);
        }

        [Fact]
        public void RenderStackTraceSeperator_raw()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message} ${stacktrace:format=Raw:separator= \=&gt; }' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            RenderMe(logFactory, "I am:");

            logFactory.AssertDebugLastMessageContains(" => RenderStackTraceSeperator_raw at offset ");
            logFactory.AssertDebugLastMessageContains(" => RenderMe at offset ");
#if !MONO
            logFactory.AssertDebugLastMessageContains("StackTraceRendererTests.cs");
#endif

            string debugLastMessage = GetDebugLastMessage("debug", logFactory);
            int index0 = debugLastMessage.IndexOf(" => RenderStackTraceSeperator_raw at offset ");
            int index1 = debugLastMessage.IndexOf(" => RenderMe at offset ");
            Assert.True(index0 < index1);
        }

        [Fact]
        public void RenderStackTraceReversed_raw()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message} ${stacktrace:format=Raw:reverse=true}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            RenderMe(logFactory, "I am:");

            logFactory.AssertDebugLastMessageContains("RenderMe at offset ");
            logFactory.AssertDebugLastMessageContains("RenderStackTraceReversed_raw at offset ");
#if !MONO
            logFactory.AssertDebugLastMessageContains("StackTraceRendererTests.cs");
#endif

            string debugLastMessage = GetDebugLastMessage("debug", logFactory);
            int index0 = debugLastMessage.IndexOf("RenderMe at offset ");
            int index1 = debugLastMessage.IndexOf("RenderStackTraceReversed_raw at offset ");
            Assert.True(index0 < index1);
        }

        [Fact]
        public void RenderStackTrace_DetailedFlat()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message} ${stacktrace:format=DetailedFlat}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            RenderMe(logFactory, "I am:");
            logFactory.AssertDebugLastMessageContains(" => [Void RenderStackTrace_DetailedFlat()] => [Void RenderMe(NLog.LogFactory, System.String)]");
        }

        [Fact]
        public void RenderStackTraceReversed_DetailedFlat()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message} ${stacktrace:format=DetailedFlat:reverse=true}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            RenderMe(logFactory, "I am:");
            logFactory.AssertDebugLastMessageContains("[Void RenderMe(NLog.LogFactory, System.String)] => [Void RenderStackTraceReversed_DetailedFlat()] => ");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void RenderMe(LogFactory logFactory, string message)
        {
            var logger = logFactory.GetCurrentClassLogger();
            logger.ForInfoEvent().Message(message).Log();
        }
    }

}
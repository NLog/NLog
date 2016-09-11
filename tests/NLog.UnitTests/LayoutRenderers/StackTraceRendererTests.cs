// 
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

#if !SILVERLIGHT

namespace NLog.UnitTests.LayoutRenderers
{
    using System.Runtime.CompilerServices;
    using Xunit;

    public class StackTraceRendererTests : NLogTestBase
    {
        [Fact]
        public void RenderStackTrace()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message} ${stacktrace}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            RenderMe("I am:");
            AssertDebugLastMessage("debug", "I am: RuntimeMethodHandle.InvokeMethod => StackTraceRendererTests.RenderStackTrace => StackTraceRendererTests.RenderMe");
        }

        [Fact]
        public void RenderStackTrace_topframes()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
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
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message} ${stacktrace:skipframes=1}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            RenderMe("I am:");
            AssertDebugLastMessage("debug", "I am: RuntimeMethodHandle.InvokeMethod => StackTraceRendererTests.RenderStackTrace_skipframes");
        }


        [Fact]
        public void RenderStackTrace_raw()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message} ${stacktrace:format=Raw}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            RenderMe("I am:");

            var message = GetDebugLastMessage("debug");

            //remove newlines
            message = message.Replace("\n", "").Replace("\r", "");
            Assert.Equal("I am: InvokeMethod at offset 0 in file:line:column <filename unknown>:0:0RenderStackTrace_raw at offset 85 in file:line:column <filename unknown>:0:0RenderMe at offset 66 in file:line:column <filename unknown>:0:0", message);
        }

        [Fact]
        public void RenderStackTrace_DetailedFlat()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message} ${stacktrace:format=DetailedFlat}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            RenderMe("I am:");
            AssertDebugLastMessage("debug", "I am: [System.Object InvokeMethod(System.Object, System.Object[], System.Signature, Boolean)] => [Void RenderStackTrace_DetailedFlat()] => [Void RenderMe(System.String)]");
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        private void RenderMe(string message)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }

}

#endif
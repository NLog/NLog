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
using NLog.LayoutRenderers;
using NLog.Config;
using Xunit;

namespace NLog.UnitTests.LayoutRenderers
{
    public class FuncLayoutRendererTests : NLogTestBase
    {
        [Fact]
        public void RegisterCustomFuncLayoutRendererTest()
        {
            // Arrange
            var logFactory = new LogFactory().Setup().SetupExtensions(ext => ext.RegisterLayoutRenderer<MyFuncLayoutRenderer>("the-answer-new"))
                .LoadConfigurationFromXml(@"<nlog throwExceptions='true'>
                <targets>
                    <target name='debug' type='Debug' layout= 'TheAnswer=${the-answer-new:Format=D3}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            // Act
            var logger = logFactory.GetCurrentClassLogger();
            logger.Debug("test1");

            // Assert
            AssertDebugLastMessage("debug", "TheAnswer=042", logFactory);
        }

        [Fact]
        public void RegisterCustomFuncLayoutRendererTestOldStyle()
        {
            // Arrange
            var funcLayoutRenderer = new MyFuncLayoutRenderer("the-answer-new");

            // Act
            var logFactory = new LogFactory().Setup()
                .SetupExtensions(ext => ext.RegisterLayoutRenderer(funcLayoutRenderer))
                .LoadConfigurationFromXml(@"<nlog throwExceptions='true'>
                <targets>
                    <target name='debug' type='Debug' layout= 'TheAnswer=${the-answer-new:Format=D3}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetCurrentClassLogger();
            logger.Debug("test1");

            // Assert
            AssertDebugLastMessage("debug", "TheAnswer=042", logFactory);
        }

        private sealed class MyFuncLayoutRenderer : FuncLayoutRenderer
        {
            public MyFuncLayoutRenderer() : base(string.Empty)
            {
            }

            public MyFuncLayoutRenderer(string layoutRendererName) : base(layoutRendererName)
            {
            }

            /// <inheritdoc/>
            protected override object RenderValue(LogEventInfo logEvent)
            {
                return 42;
            }
        }
    }
}

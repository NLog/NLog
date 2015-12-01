// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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
#if NET4_5
    using System.Xml.Linq;
    using NLog.Config;
    using NLog.LayoutRenderers;
    using NLog.Targets;
    using Xunit;

    public class MdlcLayoutRendererTests
    {
        private static DebugTarget _target;

        public MdlcLayoutRendererTests()
        {
            ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition("mdlc", typeof(MdlcLayoutRenderer));

            const string configXml = @"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${mdlc:Item=myitem}${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>";

            var element = XElement.Parse(configXml);
            var config = new XmlLoggingConfiguration(element.CreateReader(), null);
            LogManager.Configuration = config;

            _target = LogManager.Configuration.FindTargetByName("debug") as DebugTarget;

            MappedDiagnosticsLogicalContext.Clear();
        }

        [Fact]
        public void given_item_does_not_exist_when_rendering_item_and_message_should_render_only_message()
        {
            const string message = "message";
            LogManager.GetLogger("A").Debug(message);
            Assert.Equal(message, _target.LastMessage);
        }

        [Fact]
        public void given_item_exists_when_rendering_item_and_message_should_render_item_and_message()
        {
            const string message = "message";
            const string key = "myitem";
            const string item = "item";

            MappedDiagnosticsLogicalContext.Set(key, item);
            LogManager.GetLogger("A").Debug(message);

            Assert.Equal(item + message, _target.LastMessage);
        }
    }
#endif
}
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
    using Xunit;

    public class GDCTests : NLogTestBase
    {
        [Fact]
        public void GDCTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${gdc:item=myitem} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            GlobalDiagnosticsContext.Set("myitem", "myvalue");
            LogManager.GetLogger("A").Debug("a");
            AssertDebugLastMessage("debug", "myvalue a");

            GlobalDiagnosticsContext.Set("myitem", "value2");
            LogManager.GetLogger("A").Debug("b");
            AssertDebugLastMessage("debug", "value2 b");

            GlobalDiagnosticsContext.Remove("myitem");
            LogManager.GetLogger("A").Debug("c");
            AssertDebugLastMessage("debug", " c");
        }

        [Fact]
        public void GDCFormatTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${gdc:item=appid:format=@} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            GlobalDiagnosticsContext.Clear();
            GlobalDiagnosticsContext.Set("appid", new { AppId = 123 });
            LogManager.GetLogger("A").Debug("a");
            AssertDebugLastMessage("debug", "{\"AppId\":123} a");
        }
    }
}
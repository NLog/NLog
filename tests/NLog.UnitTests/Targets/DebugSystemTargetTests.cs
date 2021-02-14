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

using System.Diagnostics;
using Xunit;

namespace NLog.UnitTests.Targets
{
    public class DebugSystemTargetTests : NLogTestBase
    {
        [Fact]
        public void DebugWriteLineTest()
        {
            var sw = new System.IO.StringWriter();

            try
            {
                // Arrange
#if !NETSTANDARD
                Debug.Listeners.Clear();
                Debug.Listeners.Add(new TextWriterTraceListener(sw));
#endif
                var logger = new LogFactory().Setup().LoadConfigurationFromXml(@"
                <nlog>
                    <targets>
                        <target type='debugsystem' name='Debug' layout='${message}' />
                    </targets>
                    <rules>
                        <logger name='*' writeTo='Debug' />
                    </rules>
                </nlog>").GetCurrentClassLogger();

                // Act
                logger.Info("Hello World");

                // Assert
#if !NETSTANDARD
                Assert.Contains("Hello World", sw.ToString());
#endif
            }
            finally
            {
#if !NETSTANDARD
                Debug.Listeners.Clear();
#endif
            }
        }

        [Fact]
        public void DebugWriteLineHeaderFooterTest()
        {
            var sw = new System.IO.StringWriter();

            try
            {
                // Arrange
#if !NETSTANDARD
                Debug.Listeners.Clear();
                Debug.Listeners.Add(new TextWriterTraceListener(sw));
#endif
                var logger = new LogFactory().Setup().LoadConfigurationFromXml(@"
                <nlog>
                    <targets>
                        <target type='debugsystem' name='Debug' layout='${message}'>
                            <header>Startup</header>
                            <footer>Shutdown</footer>
                        </target>
                    </targets>
                    <rules>
                        <logger name='*' writeTo='Debug' />
                    </rules>
                </nlog>").GetCurrentClassLogger();

                // Act
                logger.Info("Hello World");

                // Assert
#if !NETSTANDARD
                Assert.Contains("Startup", sw.ToString());
                Assert.Contains("Hello World", sw.ToString());
                Assert.DoesNotContain("Shutdown", sw.ToString());

                logger.Factory.Shutdown();
                Assert.Contains("Shutdown", sw.ToString());
#endif


            }
            finally
            {
#if !NETSTANDARD
                Debug.Listeners.Clear();
#endif
            }
        }
    }
}

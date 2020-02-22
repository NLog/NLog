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

namespace NLog.UnitTests.Config
{
    using Xunit;


    public class DuplicateConfigurationAttributeTests : NLogTestBase
    {
        [Fact]
        public void ShouldWriteLogsOnDuplicateAttributeTest()
        {
            using (new NoThrowNLogExceptions())
            {
                LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
                    <nlog>
                        <targets><target name='debug' type='debug' layout='${message}' /></targets>
                        <rules>
                            <logger name='*' minlevel='info' minLevel='info' appendto='debug'>
                               <filters defaultAction='log'>
                                    <whencontains layout='${message}' substring='msg' action='ignore' />
                                </filters>
                            </logger>
                        </rules>
                    </nlog>");

                var logger = LogManager.GetLogger("A");
                string expectedMesssage = "some message";
                logger.Info(expectedMesssage);
                var actualMessage = GetDebugLastMessage("debug");
                Assert.Equal(expectedMesssage, actualMessage);
            }
        }

        [Fact]
        public void ShoudWriteToInternalLogOnDuplicateAttributeTest()
        {
            var internalLog = RunAndCaptureInternalLog(() =>
            {
                using (new NoThrowNLogExceptions())
                {
                    LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
                    <nlog>
                        <targets><target name='debug' type='debug' layout='${message}' /></targets>
                        <rules>
                            <logger name='*' minlevel='info' minLevel='trace' appendto='debug'>
                               <filters defaultAction='log'>
                                    <whencontains layout='${message}' substring='msg' Substring='msg1' action='ignore' />
                                </filters>
                            </logger>
                        </rules>
                    </nlog>");
                }
            }, LogLevel.Error);

            Assert.True(internalLog.Contains("Skipping Duplicate value for 'logger'. PropertyName=minLevel. Skips Value=trace. Existing Value=info"), internalLog);
            Assert.True(internalLog.Contains("Skipping Duplicate value for 'whencontains'. PropertyName=Substring. Skips Value=msg1. Existing Value=msg"), internalLog);
        }

        [Fact]
        public void ShoudThrowExceptionOnDuplicateAttributeWhenOptionIsEnabledTest()
        {
            Assert.Throws<NLogConfigurationException>(() =>
            {
                LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
                <nlog throwExceptions='true'>
                    <targets><target name='debug' type='debug' layout='${message}' /></targets>
                    <rules>
                        <logger name='*' minlevel='info' minLevel='info' appendto='debug'>
                           <filters defaultAction='log'>
                                <whencontains layout='${message}' substring='msg' action='ignore' />
                            </filters>
                        </logger>
                    </rules>
                </nlog>");
            });

            Assert.Throws<NLogConfigurationException>(() =>
            {
                LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
                <nlog throwConfigExceptions='true'>
                    <targets><target name='debug' type='debug' layout='${message}' /></targets>
                    <rules>
                        <logger name='*' minlevel='info' minLevel='info' appendto='debug'>
                           <filters defaultAction='log'>
                                <whencontains layout='${message}' substring='msg' action='ignore' />
                            </filters>
                        </logger>
                    </rules>
                </nlog>");
            });
        }
    }
}
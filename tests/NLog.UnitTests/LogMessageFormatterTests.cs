// 
// Copyright (c) 2004-2017 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.UnitTests
{
    using System.Collections.Generic;
    using NLog.MessageTemplates;
    using Xunit;

    public class LogMessageFormatterTests : NLogTestBase
    {
        [Fact]
        public void ExtensionsLoggingFormatTest()
        {
            LogEventInfo logEventInfo = new LogEventInfo(LogLevel.Info, "MyLogger", "Login request from {Username} for {Application}", new[]
            {
                new MessageTemplateParameter("Username", "John", null),
                new MessageTemplateParameter("Application", "BestApplicationEver", null)
            });
            logEventInfo.Parameters = new object[] { "Login request from John for BestApplicationEver" };
            logEventInfo.MessageFormatter = (logEvent) =>
            {
                if (logEvent.Parameters != null && logEvent.Parameters.Length > 0)
                {
                    return logEvent.Parameters[logEvent.Parameters.Length - 1] as string ?? logEvent.Message;
                }
                return logEvent.Message;
            };

            LogManager.Configuration = CreateConfigurationFromString(@"
                <nlog throwExceptions='true'>
                    <targets>
                        <target name='debug' type='Debug'  >
                                <layout type='JsonLayout' IncludeAllProperties='true'>
                                    <attribute name='LogMessage' layout='${message:raw=true}' />
                                </layout>
                        </target>
                    </targets>
                    <rules>
                        <logger name='*' levels='Info' writeTo='debug' />
                    </rules>
                </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logEventInfo.LoggerName = logger.Name;
            logger.Log(logEventInfo);
            AssertDebugLastMessage("debug", "{ \"LogMessage\": \"Login request from {Username} for {Application}\", \"Username\": \"John\", \"Application\": \"BestApplicationEver\" }");

            Assert.Equal("Login request from John for BestApplicationEver", logEventInfo.FormattedMessage);

            Assert.Contains(new KeyValuePair<object, object>("Username", "John"), logEventInfo.Properties);
            Assert.Contains(new KeyValuePair<object, object>("Application", "BestApplicationEver"), logEventInfo.Properties);
            Assert.Contains(new MessageTemplateParameter("Username", "John", null), logEventInfo.MessageTemplateParameters);
            Assert.Contains(new MessageTemplateParameter("Application", "BestApplicationEver", null), logEventInfo.MessageTemplateParameters);
        }

        [Fact]
        public void NormalStringFormatTest()
        {
            LogEventInfo logEventInfo = new LogEventInfo(LogLevel.Info, "MyLogger", null, "Login request from {0} for {1}", new object[]
            {
                "John",
                "BestApplicationEver"
            });

            LogManager.Configuration = CreateConfigurationFromString(@"
                <nlog throwExceptions='true'>
                    <targets>
                        <target name='debug' type='Debug'  >
                                <layout type='JsonLayout' IncludeAllProperties='true'>
                                    <attribute name='LogMessage' layout='${message:raw=true}' />
                                </layout>
                        </target>
                    </targets>
                    <rules>
                        <logger name='*' levels='Info' writeTo='debug' />
                    </rules>
                </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logEventInfo.LoggerName = logger.Name;
            logger.Log(logEventInfo);
            AssertDebugLastMessage("debug", "{ \"LogMessage\": \"Login request from {0} for {1}\" }");

            Assert.Equal("Login request from John for BestApplicationEver", logEventInfo.FormattedMessage);

            Assert.Contains(new MessageTemplateParameter("0", "John", null), logEventInfo.MessageTemplateParameters);
            Assert.Contains(new MessageTemplateParameter("1", "BestApplicationEver", null), logEventInfo.MessageTemplateParameters);
        }

        [Fact]
        public void MessageTemplateFormatTest()
        {
            LogEventInfo logEventInfo = new LogEventInfo(LogLevel.Info, "MyLogger", null, "Login request from {@Username} for {Application:l}", new object[]
            {
                "John",
                "BestApplicationEver"
            });

            LogManager.Configuration = CreateConfigurationFromString(@"
                <nlog throwExceptions='true'>
                    <targets>
                        <target name='debug' type='Debug'  >
                                <layout type='JsonLayout' IncludeAllProperties='true'>
                                    <attribute name='LogMessage' layout='${message:raw=true}' />
                                </layout>
                        </target>
                    </targets>
                    <rules>
                        <logger name='*' levels='Info' writeTo='debug' />
                    </rules>
                </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logEventInfo.LoggerName = logger.Name;
            logger.Log(logEventInfo);
            AssertDebugLastMessage("debug", "{ \"LogMessage\": \"Login request from {@Username} for {Application:l}\", \"Username\": \"John\", \"Application\": \"BestApplicationEver\" }");

            Assert.Equal("Login request from \"John\" for BestApplicationEver", logEventInfo.FormattedMessage);

            Assert.Contains(new KeyValuePair<object, object>("Username", "John"), logEventInfo.Properties);
            Assert.Contains(new KeyValuePair<object, object>("Application", "BestApplicationEver"), logEventInfo.Properties);
            Assert.Contains(new MessageTemplateParameter("Username", "John", "@"), logEventInfo.MessageTemplateParameters);
            Assert.Contains(new MessageTemplateParameter("Application", "BestApplicationEver", "l"), logEventInfo.MessageTemplateParameters);
        }
    }
}

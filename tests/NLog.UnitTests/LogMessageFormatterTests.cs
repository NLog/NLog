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

namespace NLog.UnitTests
{
    using NLog.MessageTemplates;
    using Xunit;

    public class LogMessageFormatterTests : NLogTestBase
    {
        [Fact]
        public void ExtensionsLoggingFormatTest()
        {
            LogEventInfo logEventInfo = new LogEventInfo(LogLevel.Info, "MyLogger", "Login request from {Username} for {Application}", new[]
            {
                new MessageTemplateParameter("Username", "John", null, CaptureType.Normal),
                new MessageTemplateParameter("Application", "BestApplicationEver", null, CaptureType.Normal)
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

            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
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

            AssertContainsInDictionary(logEventInfo.Properties, "Username", "John");
            AssertContainsInDictionary(logEventInfo.Properties, "Application", "BestApplicationEver");
            Assert.Contains(new MessageTemplateParameter("Username", "John", null, CaptureType.Normal), logEventInfo.MessageTemplateParameters);
            Assert.Contains(new MessageTemplateParameter("Application", "BestApplicationEver", null, CaptureType.Normal), logEventInfo.MessageTemplateParameters);
        }

        [Fact]
        public void NormalStringFormatTest()
        {
            LogEventInfo logEventInfo = new LogEventInfo(LogLevel.Info, "MyLogger", null, "{0:X} - Login request from {1} for {2} with userid {0}", new object[]
            {
                42,
                "John",
                "BestApplicationEver"
            });

            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
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
            AssertDebugLastMessage("debug", "{ \"LogMessage\": \"{0:X} - Login request from {1} for {2} with userid {0}\" }");

            Assert.Equal("2A - Login request from John for BestApplicationEver with userid 42", logEventInfo.FormattedMessage);

            Assert.Contains(new MessageTemplateParameter("0", 42, "X", CaptureType.Normal), logEventInfo.MessageTemplateParameters);
            Assert.Contains(new MessageTemplateParameter("1", "John", null, CaptureType.Normal), logEventInfo.MessageTemplateParameters);
            Assert.Contains(new MessageTemplateParameter("2", "BestApplicationEver", null, CaptureType.Normal), logEventInfo.MessageTemplateParameters);
            Assert.Contains(new MessageTemplateParameter("0", 42, null, CaptureType.Normal), logEventInfo.MessageTemplateParameters);
        }

        [Fact]
        public void MessageTemplateFormatTest()
        {
            LogEventInfo logEventInfo = new LogEventInfo(LogLevel.Info, "MyLogger", null, "Login request from {@Username} for {Application:l}", new object[]
            {
                "John",
                "BestApplicationEver"
            });

            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
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

            AssertContainsInDictionary(logEventInfo.Properties, "Username", "John");
            AssertContainsInDictionary(logEventInfo.Properties, "Application", "BestApplicationEver");
            Assert.Contains(new MessageTemplateParameter("Username", "John", null, CaptureType.Serialize), logEventInfo.MessageTemplateParameters);
            Assert.Contains(new MessageTemplateParameter("Application", "BestApplicationEver", "l", CaptureType.Normal), logEventInfo.MessageTemplateParameters);
        }
    }
}

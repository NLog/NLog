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

namespace NLog.UnitTests.Config
{
    using NLog.Common;
    using Xunit;

    public class InternalLoggingTests : NLogTestBase
    {
        [Fact]
        public void InternalLoggingConfigTest1()
        {
            InternalLoggingConfigTest(LogLevel.Trace, true, true, LogLevel.Warn, true, true, @"c:\temp\nlog\file.txt", true);
        }

        [Fact]
        public void InternalLoggingConfigTest2()
        {
            InternalLoggingConfigTest(LogLevel.Error, false, false, LogLevel.Info, false, false, @"c:\temp\nlog\file2.txt", false);
        }

        [Fact]
        public void InternalLoggingConfigTes3()
        {
            InternalLoggingConfigTest(LogLevel.Info, false, false, LogLevel.Trace, false, null, @"c:\temp\nlog\file3.txt", false);
        }

        [Fact]
        public void InternalLoggingConfigTestDefaults()
        {
            using (new InternalLoggerScope())
            {
                InternalLogger.LogLevel = LogLevel.Error;
                InternalLogger.LogToConsole = true;
                InternalLogger.LogToConsoleError = true;
                LogManager.GlobalThreshold = LogLevel.Fatal;
                LogManager.ThrowExceptions = true;
                LogManager.ThrowConfigExceptions = null;
#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
                InternalLogger.LogToTrace = true;
#endif

                CreateConfigurationFromString(@"
<nlog>
</nlog>");

                Assert.Same(LogLevel.Error, InternalLogger.LogLevel);
                Assert.True(InternalLogger.LogToConsole);
                Assert.True(InternalLogger.LogToConsoleError);
                Assert.Same(LogLevel.Fatal, LogManager.GlobalThreshold);
                Assert.True(LogManager.ThrowExceptions);
                Assert.Null(LogManager.ThrowConfigExceptions);
#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
                Assert.True(InternalLogger.LogToTrace);
#endif
            }
        }


        private void InternalLoggingConfigTest(LogLevel logLevel, bool logToConsole, bool logToConsoleError, LogLevel globalThreshold, bool throwExceptions, bool? throwConfigExceptions, string file, bool logToTrace)
        {
            var logLevelString = logLevel.ToString();
            var internalLogToConsoleString = logToConsole.ToString().ToLower();
            var internalLogToConsoleErrorString = logToConsoleError.ToString().ToLower();
            var globalThresholdString = globalThreshold.ToString();
            var throwExceptionsString = throwExceptions.ToString().ToLower();
            var throwConfigExceptionsString = throwConfigExceptions == null ? "" : throwConfigExceptions.ToString().ToLower();
            var logToTraceString = logToTrace.ToString().ToLower();

            using (new InternalLoggerScope())
            {
                CreateConfigurationFromString(string.Format(@"
<nlog internalLogFile='{0}' internalLogLevel='{1}' internalLogToConsole='{2}' internalLogToConsoleError='{3}' globalThreshold='{4}' throwExceptions='{5}' throwConfigExceptions='{6}' internalLogToTrace='{7}'>
</nlog>", file, logLevelString, internalLogToConsoleString, internalLogToConsoleErrorString, globalThresholdString, throwExceptionsString, throwConfigExceptionsString, logToTraceString));

                Assert.Same(logLevel, InternalLogger.LogLevel);

                Assert.Equal(file, InternalLogger.LogFile);

                Assert.Equal(logToConsole, InternalLogger.LogToConsole);

                Assert.Equal(logToConsoleError, InternalLogger.LogToConsoleError);

                Assert.Same(globalThreshold, LogManager.GlobalThreshold);

                Assert.Equal(throwExceptions, LogManager.ThrowExceptions);

                Assert.Equal(throwConfigExceptions, LogManager.ThrowConfigExceptions);

#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
                Assert.Equal(logToTrace, InternalLogger.LogToTrace);
#endif
            }
        }
    }
}
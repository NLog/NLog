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


using NLog.Common;
using Xunit;

namespace NLog.UnitTests.Config
{
    public class XmlConfigTests : NLogTestBase
    {
        [Fact]
        void ParseNLogOptionsDefaultTest()
        {
            var xml = "<nlog></nlog>";
            var config = CreateConfigurationFromString(xml);

            Assert.Equal(false, config.AutoReload);
            Assert.Equal(true, config.InitializeSucceeded);
            Assert.Equal("", InternalLogger.LogFile);
            Assert.Equal(true, InternalLogger.IncludeTimestamp);
            Assert.Equal(false, InternalLogger.LogToConsole);
            Assert.Equal(false, InternalLogger.LogToConsoleError);
            Assert.Equal(null, InternalLogger.LogWriter);

        }

        [Fact]
        void ParseNLogOptionsTest()
        {
            var xml = "<nlog autoreload='true' logfile='test.txt' internalLogIncludeTimestamp='false' internalLogToConsole='true' internalLogToConsoleError='true'></nlog>";
            var config = CreateConfigurationFromString(xml);

            Assert.Equal(true, config.AutoReload);
            Assert.Equal(true, config.InitializeSucceeded);
            Assert.Equal("", InternalLogger.LogFile);
            Assert.Equal(false, InternalLogger.IncludeTimestamp);
            Assert.Equal(true, InternalLogger.LogToConsole);
            Assert.Equal(true, InternalLogger.LogToConsoleError);
            Assert.Equal(null, InternalLogger.LogWriter);

        }

    }
}

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

namespace NLog.UnitTests.Targets.Wrappers
{
    using NLog.Targets;
    using NLog.Targets.Wrappers;
    using Xunit;

    public class GroupByTargetWrapperTests : NLogTestBase
	{
        [Fact]
        public void SimpleGroupByTest()
        {
            // Arrange
            var memoryTarget = new MemoryTarget("memory") { Layout = "${level}" };
            var groupByTarget = new GroupByTargetWrapper("groupby", memoryTarget, "${logger}");
            var bufferTarget = new BufferingTargetWrapper("buffer", groupByTarget);

            var logFactory = new LogFactory();
            var logConfig = new NLog.Config.LoggingConfiguration(logFactory);
            logConfig.AddRule(LogLevel.Info, LogLevel.Fatal, bufferTarget);
            logFactory.Configuration = logConfig;

            var logger1 = logFactory.GetLogger("Logger1");
            var logger2 = logFactory.GetLogger("Logger2");
            var logger3 = logFactory.GetLogger("Logger3");

            // Act
            logger1.Trace("Ignore Me");
            logger2.Warn("Special Warning");
            logger1.Debug("Hello world");
            logger1.Fatal("Catastropic Goodbye");
            logger2.Error("General Error");
            logFactory.Flush();
            groupByTarget.WriteAsyncLogEvent(LogEventInfo.Create(LogLevel.Info, logger1.Name, "Special Hello").WithContinuation((ex) => { }));

            // Assert
            Assert.Equal(4, memoryTarget.Logs.Count);
            Assert.Equal("Warn", memoryTarget.Logs[0]);
            Assert.Equal("Error", memoryTarget.Logs[1]);
            Assert.Equal("Fatal", memoryTarget.Logs[2]);
            Assert.Equal("Info", memoryTarget.Logs[3]);
        }
    }
}

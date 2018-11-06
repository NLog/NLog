// 
// Copyright (c) 2004-2018 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System;
    using System.Linq;
    using NLog.Targets;
    using Xunit;

    public class ConfigurationBuilderTests
    {
        [Fact]
        void ConfigurationBuilder_FilterMinLevel()
        {
            var logFactory = new LogFactory().BuildConfig(c => c.FilterMinLevel(LogLevel.Debug, w => w.WriteToTarget(new DebugTarget() { Layout = "${message}" })));
            var logger = logFactory.GetCurrentClassLogger();
            var target = logFactory.Configuration.AllTargets.OfType<DebugTarget>().FirstOrDefault();
            Assert.NotNull(logFactory.Configuration);
            Assert.Single(logFactory.Configuration.AllTargets);
            Assert.NotNull(target);

            logger.Info("Info Level");
            Assert.Equal("Info Level", target.LastMessage);

            logger.Trace("Trace Level");
            Assert.Equal("Info Level", target.LastMessage);
        }

        [Fact]
        void ConfigurationBuilder_FilterLevel()
        {
            var logFactory = new LogFactory().BuildConfig(c => c.FilterMinLevel(LogLevel.Debug, w => w.WriteToTarget(new DebugTarget() { Layout = "${message}" })));
            var logger = logFactory.GetCurrentClassLogger();
            var target = logFactory.Configuration.AllTargets.OfType<DebugTarget>().FirstOrDefault();
            Assert.NotNull(logFactory.Configuration);
            Assert.Single(logFactory.Configuration.AllTargets);
            Assert.NotNull(target);

            logger.Debug("Debug Level");
            Assert.Equal("Debug Level", target.LastMessage);

            logger.Trace("Trace Level");
            Assert.Equal("Debug Level", target.LastMessage);

            logger.Trace("Error Level");
            Assert.Equal("Debug Level", target.LastMessage);
        }

        [Fact]
        void ConfigurationBuilder_MultipleTargets()
        {
            string lastMessage = null;

            var logFactory = new LogFactory().BuildConfig(c => c.FilterMinLevel(LogLevel.Debug, w => w.WriteToMethod((l,o) => lastMessage = l.FormattedMessage).WriteToTarget(new DebugTarget() { Layout = "${message}" })));
            var logger = logFactory.GetCurrentClassLogger();
            var target = logFactory.Configuration.AllTargets.OfType<DebugTarget>().FirstOrDefault();
            Assert.NotNull(logFactory.Configuration);
            Assert.Equal(2, logFactory.Configuration.AllTargets.Count);
            Assert.NotNull(target);

            logger.Debug("Debug Level");
            Assert.Equal("Debug Level", target.LastMessage);
            Assert.Equal("Debug Level", lastMessage);
        }

        [Fact]
        void ConfigurationBuilder_WriteToWithBuffering()
        {
            var logFactory = new LogFactory().BuildConfig(c => c.FilterLevel(LogLevel.Debug, w => w.WriteToWithBuffering(b => b.WriteToTarget(new DebugTarget() { Layout = "${message}" }))));
            var logger = logFactory.GetCurrentClassLogger();
            var target = logFactory.Configuration.AllTargets.OfType<DebugTarget>().FirstOrDefault();
            Assert.NotNull(logFactory.Configuration);
            Assert.Equal(2, logFactory.Configuration.AllTargets.Count);

            Assert.NotNull(target);
            logger.Debug("Debug Level");

            Assert.Equal("", target.LastMessage ?? string.Empty);

            logFactory.Flush();
            Assert.Equal("Debug Level", target.LastMessage);
        }

        [Fact]
        void ConfigurationBuilder_WriteToWithAsync()
        {
            var logFactory = new LogFactory().BuildConfig(c => c.FilterLevel(LogLevel.Debug, w => w.WriteToWithAsync(b => b.WriteToTarget(new DebugTarget() { Layout = "${message}" }))));
            var logger = logFactory.GetCurrentClassLogger();
            var target = logFactory.Configuration.AllTargets.OfType<DebugTarget>().FirstOrDefault();
            Assert.NotNull(logFactory.Configuration);
            Assert.Equal(2, logFactory.Configuration.AllTargets.Count);

            Assert.NotNull(target);
            logger.Debug("Debug Level");

            logFactory.Flush();
            Assert.Equal("Debug Level", target.LastMessage);
        }

        [Fact]
        void ConfigurationBuilder_WriteToWithFallback()
        {
            bool exceptionWasThrown = false;

            var logFactory = new LogFactory().BuildConfig(c => c.FilterLevel(LogLevel.Debug, w => w.WriteToWithFallback(f => f.WriteToMethod((l, o) => { exceptionWasThrown = true; throw new Exception("Abort"); }).WriteToTarget(new DebugTarget() { Layout = "${message}" }))));
            var logger = logFactory.GetCurrentClassLogger();
            var target = logFactory.Configuration.AllTargets.OfType<DebugTarget>().FirstOrDefault();
            Assert.NotNull(logFactory.Configuration);
            Assert.Equal(3, logFactory.Configuration.AllTargets.Count);

            Assert.NotNull(target);

            using (new NLogTestBase.NoThrowNLogExceptions())
            {
                logger.Debug("Debug Level");
                Assert.Equal("Debug Level", target.LastMessage);
                Assert.True(exceptionWasThrown);
            }
        }

        [Fact]
        void ConfigurationBuilder_WriteToWithRetry()
        {
            int methodCalls = 0;

            var logFactory = new LogFactory().BuildConfig(c => c.FilterLevel(LogLevel.Debug, w => w.WriteToWithRetry(r => r.WriteToMethod((l, o) => { if (methodCalls++ > 0) return; throw new Exception("Abort"); }))));
            var logger = logFactory.GetCurrentClassLogger();
            Assert.NotNull(logFactory.Configuration);
            Assert.Equal(2, logFactory.Configuration.AllTargets.Count);

            using (new NLogTestBase.NoThrowNLogExceptions())
            {
                logger.Debug("Debug Level");
                Assert.Equal(2, methodCalls);
            }
        }
    }
}

// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

using System;
using System.Collections.Generic;
using System.Text;
using NLog.Internal.PoolFactory;
using Xunit;

namespace NLog.UnitTests.Internal.PoolFactory
{
    public class PoolConfigurationTests : NLogTestBase
    {
        private List<T> TestPoolTwice<T>(ILogEventObjectFactory pool, Func<ILogEventObjectFactory,T> createObject, Action<ILogEventObjectFactory, T> releaseObject, int objectCount) where T : class
        {
            List<T> createdItems = new List<T>();
            for (int i = 0; i < objectCount; ++i)
            {
                createdItems.Add(createObject(pool));
            }
            for (int i = createdItems.Count -1; i >= 0; --i)
            {
                releaseObject(pool, createdItems[i]);
            }
            for (int i = 0; i < objectCount; ++i)
            {
                T item = createObject(pool);
                if (!createdItems.Contains(item))
                    createdItems.Add(item);
            }
            return createdItems;
        }

        [Fact]
        public void TestPoolSetupNone()
        {
            PoolConfiguration poolConfig = new PoolConfiguration();

            ILogEventObjectFactory pool = null;
            poolConfig.ConfigurePool(ref pool, "Test", NLog.Common.PoolSetup.None, false, 0);
            Assert.NotNull(pool);
            Assert.Equal(pool.PoolSetup, NLog.Common.PoolSetup.None);
            var createdItems = TestPoolTwice(pool, (p) => p.CreateLogEvent(LogLevel.Off, "", null, "", null, null), (p,obj) => p.ReleaseLogEvent(obj), 2);
            Assert.Equal(4, createdItems.Count);

            StringBuilder sb = new StringBuilder();
            poolConfig.GetPoolStatistics(sb);
            Assert.Equal(0, sb.Length);
        }

        [Fact]
        public void TestPoolSetupActive()
        {
            PoolConfiguration poolConfig = new PoolConfiguration();
            
            ILogEventObjectFactory pool = null;
            poolConfig.ConfigurePool(ref pool, "Test", NLog.Common.PoolSetup.Active, false, 0);

            Assert.NotNull(pool);
            Assert.Equal(pool.PoolSetup, NLog.Common.PoolSetup.Active);
            var createdItems = TestPoolTwice(pool, (p) => p.CreateLogEvent(LogLevel.Off, "", null, "", null, null), (p, obj) => p.ReleaseLogEvent(obj), 2);
            Assert.Equal(2, createdItems.Count);

            StringBuilder sb = new StringBuilder();
            poolConfig.GetPoolStatistics(sb);
            Assert.NotEqual(0, sb.Length);  // Report with Name and numbers
            sb.Clear();

            poolConfig.GetPoolStatistics(sb);
            Assert.NotEqual(0, sb.Length);  // Report without numbers
        }

#if MONO || SILVERLIGHT
        [Fact(Skip="Not working under MONO / Silverlight - Probably the forced GC calls")]
#else
        [Fact]
#endif
        public void TestPoolWeakReference()
        {
            PoolConfiguration poolConfig = new PoolConfiguration();

            ILogEventObjectFactory pool = null;
            poolConfig.ConfigurePool(ref pool, "Test", NLog.Common.PoolSetup.Active, false, 0);

            Assert.NotNull(pool);
            Assert.Equal(pool.PoolSetup, NLog.Common.PoolSetup.Active);
            var createdItems = TestPoolTwice(pool, (p) => p.CreateLogEvent(LogLevel.Off, "", null, "", null, null), (p, obj) => p.ReleaseLogEvent(obj), 2);
            Assert.Equal(2, createdItems.Count);

            createdItems.Clear();
            pool = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
#if !SILVERLIGHT
            GC.WaitForFullGCComplete();
#endif
            GC.Collect();

            StringBuilder sb = new StringBuilder();
            poolConfig.GetPoolStatistics(sb);
            Assert.Equal(0, sb.Length); // Empty Report
        }

        [Fact]
        public void TestActivePoolSetupConfig()
        {
            var configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true' defaultPoolSetup='Active'>
    <targets>
        <target name='dtarget' type='Debug' layout='${message}' poolSetup='Active' />
    </targets>

    <rules>
      <logger name='*' writeTo='dtarget' minLevel=""Trace"">
      </logger>
    </rules>
</nlog>");

            LogManager.Configuration = configuration;
            var logger = LogManager.GetLogger("dLoggersync");
            Assert.Equal(NLog.Common.PoolSetup.Active, logger.PoolSetup);

            var target = (NLog.Targets.DebugTarget)configuration.FindTargetByName("dtarget");
            Assert.Equal(NLog.Common.PoolSetup.Active, target.PoolSetup);

            for (int i = 0; i < 1000; ++i)
                logger.Debug("Test");    // Sync-call

            StringBuilder sb = new StringBuilder();
            LogManager.LogFactory.GetPoolStatistics(sb);
            string report = sb.ToString();
            Assert.NotEqual(string.Empty, sb.ToString());
            Assert.True(report.Contains("dtarget"), "dtarget missing pool -> " + report);
            Assert.False(report.Contains("dLoggersync"), "dLoggersync not using default pool -> " + report);
            Assert.Equal(1000, target.Counter);
        }

        [Fact]
        public void TestActivePoolSetupAsyncConfig()
        {
            var configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true' defaultPoolSetup='Active'>
    <targets>
        <target name='dtarget' type='AsyncWrapper' overflowAction='Block' queueLimit='500' batchSize='100' timeToSleepBetweenBatches='0' poolSetup='Active' >
            <target name='dtargetwrapped' type='Debug' layout='${longdate} ${uppercase:${level}} ${message} ${exception}' />
        </target>
    </targets>

    <rules>
      <logger name='*' writeTo='dtarget' minLevel=""Trace"" >
      </logger>
    </rules>
</nlog>");

            LogManager.Configuration = configuration;
            var logger = LogManager.GetLogger("dLoggerasync");
            Assert.Equal(NLog.Common.PoolSetup.Active, logger.PoolSetup);

            var target = configuration.FindTargetByName("dtarget");
            Assert.Equal(NLog.Common.PoolSetup.Active, target.PoolSetup);

            var wrappedTarget = (NLog.Targets.DebugTarget)configuration.FindTargetByName("dtargetwrapped");

            for (int i = 0; i < 1000; ++i)
                logger.Debug("Test");    // Sync-call

            StringBuilder sb = new StringBuilder();
            LogManager.LogFactory.GetPoolStatistics(sb);
            string report = sb.ToString();
            Assert.NotEqual(string.Empty, sb.ToString());
            Assert.True(report.Contains("dtarget"), "dtarget missing pool -> " + report);
            Assert.False(report.Contains("dLoggerasync"), "dLoggerasync not using default pool -> " + report);

            LogManager.Configuration = null;    // Flush
            Assert.Equal(1000, wrappedTarget.Counter);
        }

        [Fact]
        public void TestActiveLoggerPoolSetupConfig()
        {
            var configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true' defaultPoolSetup='Active'>
    <targets>
        <target name='dtarget' type='Debug' layout='${message}' poolSetup='Active' />
    </targets>

    <rules>
      <logger name='*' writeTo='dtarget' minLevel=""Trace"">
      </logger>
    </rules>
</nlog>");

            LogManager.Configuration = configuration;
            var logger = LogManager.GetLogger("dLoggeractive", NLog.Common.PoolSetup.Large);
            Assert.Equal(NLog.Common.PoolSetup.Large, logger.PoolSetup);

            var target = (NLog.Targets.DebugTarget)configuration.FindTargetByName("dtarget");
            Assert.Equal(NLog.Common.PoolSetup.Active, target.PoolSetup);

            for (int i = 0; i < 1000; ++i)
                logger.Debug("Test");    // Sync-call

            StringBuilder sb = new StringBuilder();
            LogManager.LogFactory.GetPoolStatistics(sb);
            string report = sb.ToString();
            Assert.NotEqual(string.Empty, sb.ToString());
            Assert.True(report.Contains("dtarget"), "dtarget missing pool -> " + report);
            Assert.True(report.Contains("dLoggeractive"), "dLoggeractive missing pool -> " + report);
            Assert.Equal(1000, target.Counter);
        }

        [Fact]
        public void TestDisabledLoggerPoolSetupConfig()
        {
            var configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true' defaultPoolSetup='Active'>
    <targets async='true'>
        <target name='dtargetnopool' type='Debug' layout='${message}' poolSetup='None' />
    </targets>

    <rules>
      <logger name='*' writeTo='dtargetnopool' minLevel=""Trace"">
      </logger>
    </rules>
</nlog>");

            LogManager.Configuration = configuration;
            var logger = LogManager.GetLogger("dLoggernopool", NLog.Common.PoolSetup.None);
            Assert.Equal(NLog.Common.PoolSetup.None, logger.PoolSetup);

            var asynctarget = (NLog.Targets.Wrappers.AsyncTargetWrapper)configuration.FindTargetByName("dtargetnopool");
            Assert.Equal(NLog.Common.PoolSetup.None, asynctarget.PoolSetup);
            var target = (NLog.Targets.DebugTarget)asynctarget.WrappedTarget;
            Assert.Equal(NLog.Common.PoolSetup.None, target.PoolSetup);

            for (int i = 0; i < 1000; ++i)
                logger.Debug("Test");

            StringBuilder sb = new StringBuilder();
            LogManager.LogFactory.GetPoolStatistics(sb);
            string report = sb.ToString();
            Assert.False(report.Contains("dtargetnopool"), "dtargetnopool not using default pool -> " + report);
            Assert.False(report.Contains("dLoggernopool"), "dLoggernopool not using default pool -> " + report);

            LogManager.Configuration = null;    // Flush
            Assert.Equal(1000, target.Counter);
        }
    }
}

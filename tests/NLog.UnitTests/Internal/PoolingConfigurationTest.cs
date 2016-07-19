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
using System.IO;
using System.Threading;

using NLog.Common;
using NLog.Config;
using NLog.Internal.Pooling;
using NLog.Internal.Pooling.Pools;
using NLog.Targets;

using Xunit;

namespace NLog.UnitTests.Internal
{
    public class PoolingConfigurationTest : NLogTestBase
    {
        const string Poolingenabled = @"
<nlog throwExceptions='true'>

<pooling enabled=""true"" autoIncreasePoolSizes=""false"" prefillPools=""false"" outputPoolStatisticsInLogFiles=""false"" estimatedMaxMessageSize=""2048"" estimatedLogEventsPerSecond=""1"" />
    <targets>
        <target name='d' type='Debug' layout='${message}' />
    </targets>

    <rules>
      <logger name='*' writeTo='d' minLevel=""Trace"">
      </logger>
    </rules>
</nlog>";
        const string PoolingDisabled = @"
<nlog throwExceptions='true'>
    <pooling enabled=""false"" autoIncreasePoolSizes=""false"" prefillPools=""false"" outputPoolStatisticsInLogFiles=""false"" estimatedMaxMessageSize=""2048"" estimatedLogEventsPerSecond=""1"" />
    <targets>
        <target name='d' type='Debug' layout='${message}' />
    </targets>

    <rules>
      <logger name='*' writeTo='d' minLevel=""Trace"">
      </logger>
    </rules>
</nlog>";


        [Fact]
        public void TurningPoolingOnShouldResultInObjectsBeingReused()
        {
            var configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true'>
    <pooling enabled=""true"" autoIncreasePoolSizes=""true"" prefillPools=""false"" outputPoolStatisticsInLogFiles=""false"" estimatedMaxMessageSize=""2048"" estimatedLogEventsPerSecond=""1"" />
    <targets>
        <target name='d' type='Debug' layout='${message}' />
    </targets>

    <rules>
      <logger name='*' writeTo='d' minLevel=""Trace"">
      </logger>
    </rules>
</nlog>");

            LogManager.Configuration = configuration;
            var list = this.WarmUpPool(configuration, 1);
            var logger = LogManager.GetLogger("test");

            logger.Info("Test");
            DebugTarget target = (DebugTarget)configuration.FindTargetByName("d");

            var loggedEvent = target.LastLogEvent;
            LogManager.Configuration.FlushAllTargets(ex => { });


            Assert.True(list.Contains(loggedEvent), " Not same log event, which is unexpected, since we only have one target and not prefilling");

        }

        private List<LogEventInfo> WarmUpPool(LoggingConfiguration configuration, int estimatedMaxMessageSize)
        {
            List<LogEventInfo> list = new List<LogEventInfo>();
            var pool = configuration.PoolFactory.Get<LogEventInfoPool, LogEventInfo>();
            for (int x = 0; x < estimatedMaxMessageSize * 4; x++)
            {
                var evt = new LogEventInfo(pool);
                list.Add(evt);
                pool.PutBack(evt);
            }
            return list;
        }

        [Fact]
        public void TurningPoolingOffShouldResultInObjectsNotBeingReused()
        {
            var configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true'>
    <pooling enabled=""false"" autoIncreasePoolSizes=""false"" prefillPools=""false"" outputPoolStatisticsInLogFiles=""false"" estimatedMaxMessageSize=""2048"" estimatedLogEventsPerSecond=""100"" />
    <targets>
        <target name='d' type='Debug' layout='${message}' />
    </targets>

    <rules>
      <logger name='*' writeTo='d' minLevel=""Trace"">
      </logger>
    </rules>
</nlog>");

            var firstLogEvent = configuration.PoolFactory.Get<LogEventInfoPool, LogEventInfo>().Get();
            configuration.PoolFactory.Get<LogEventInfoPool, LogEventInfo>().PutBack(firstLogEvent);

            LogManager.Configuration = configuration;

            var logger = LogManager.GetLogger("test");

            logger.Info("Test");
            DebugTarget target = (DebugTarget)configuration.FindTargetByName("d");

            var loggedEvent = target.LastLogEvent;

            Assert.True(firstLogEvent != loggedEvent, " Same log event, which is unexpected, since pooling is turned off");

        }

        [Fact]
        public void PuttingObjectsBackIntoPoolShouldNotReuseThemIfPoolIsAlreadyFull()
        {
            var configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true'>
    <pooling enabled=""true"" autoIncreasePoolSizes=""false"" prefillPools=""false"" outputPoolStatisticsInLogFiles=""false"" estimatedMaxMessageSize=""2048"" estimatedLogEventsPerSecond=""1"" />
    <targets>
        <target name='d' type='Debug' layout='${message}' />
    </targets>

    <rules>
      <logger name='*' writeTo='d' minLevel=""Trace"">
      </logger>
    </rules>
</nlog>");
            LogManager.Configuration = configuration;
            
            var pool = configuration.PoolFactory.Get<LogEventInfoPool, LogEventInfo>();
            var poolSize = pool.PoolSize;
            int created = poolSize + 10;
            int expectedThrownAway = created - poolSize - pool.FreeSpace;

            List<LogEventInfo> events = new List<LogEventInfo>();

            for (int x = 0; x < poolSize + 10; x++)
            {
                var evt = pool.Get();
                events.Add(evt);
            }

            foreach (var evt in events)
            {
                pool.PutBack(evt);
            }

            


            Assert.True(expectedThrownAway <= pool.ThrownAwayObjects);
        }

        [Fact]
        public void PuttingObjectsBackIntoPoolShouldPutThemIntoThePoolIfAutoIncreaseIsSet()
        {
            var configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true'>
    <pooling enabled=""true"" autoIncreasePoolSizes=""true"" prefillPools=""false"" outputPoolStatisticsInLogFiles=""false"" estimatedMaxMessageSize=""2048"" estimatedLogEventsPerSecond=""1"" />
    <targets>
        <target name='d' type='Debug' layout='${message}' />
    </targets>

    <rules>
      <logger name='*' writeTo='d' minLevel=""Trace"">
      </logger>
    </rules>
</nlog>");

            // Log events default pool size is 2 * estimatedLogEventsPerSecond

            LogManager.Configuration = configuration;

            var pool = configuration.PoolFactory.Get<LogEventInfoPool, LogEventInfo>();

            var list = this.WarmUpPool(configuration, 1);

            for (int x = 0; x < 2; x++)
            {
                
                var evt = pool.Get();

                Assert.True(list.Contains(evt));
            }

            if (pool.SupportsAutoIncrease)
            {
                Assert.Equal(0, configuration.PoolFactory.Get<LogEventInfoPool, LogEventInfo>().ThrownAwayObjects);
            }
            else
            {
                Assert.Equal(2, pool.ThrownAwayObjects);
            }
        }

        [Fact]
        public void TurningOnThreadOptimisationShouldUseRingBuffer()
        {
            var configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true'>
    <pooling enabled=""true"" autoIncreasePoolSizes=""true"" prefillPools=""false"" outputPoolStatisticsInLogFiles=""false"" estimatedMaxMessageSize=""2048"" estimatedLogEventsPerSecond=""1"" OptimiseForManyThreads=""true"" />
    <targets>
        <target name='d' type='Debug' layout='${message}' />
    </targets>

    <rules>
      <logger name='*' writeTo='d' minLevel=""Trace"">
      </logger>
    </rules>
</nlog>");

            // Log events default pool size is 4 * estimatedLogEventsPerSecond

            LogManager.Configuration = configuration;

            // ring buffer does not support auto increase

            var pool = configuration.PoolFactory.Get<LogEventInfoPool, LogEventInfo>();

            Assert.False(pool.SupportsAutoIncrease);
            Assert.Equal(0, pool.ThrownAwayObjects);
            Assert.Equal(0, pool.ObjectsInPool);
            Assert.Equal(4, pool.FreeSpace);

        }

        [Fact]
        public void ShouldLogWithConfiguredLoggerName()
        {
            var configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true'>
    <pooling enabled=""true"" autoIncreasePoolSizes=""true"" prefillPools=""false"" outputPoolStatisticsInLogFiles=""true"" outputPoolStatisticsInterval=""1"" estimatedMaxMessageSize=""2048"" estimatedLogEventsPerSecond=""1"" OptimiseForManyThreads=""false""   poolStatisticsLoggerName=""StatsLogger"" />
    <targets>
        <target name='d' type='Debug' layout='${message}' />
    </targets>

    <rules>
      <logger name='*' writeTo='d' minLevel=""Trace"">
      </logger>
    </rules>
</nlog>");

            
            LogManager.Configuration = configuration;
            var logger = LogManager.GetLogger("Test");
            logger.Info("{0}", 1);
            LogManager.Flush(Console.WriteLine);
            // Sleep at least until pool stats has been logged
            Thread.Sleep(1500);
            ManualResetEvent evt = new ManualResetEvent(false);
            LogManager.Flush(ex =>
                {
                    
                    try
                    {
                        var target = configuration.FindTargetByName<DebugTarget>("d");
                        var logEvent = target.LastLogEvent;

                        Assert.NotNull(logEvent);
                        Assert.Equal("StatsLogger", logEvent.LoggerName);
                    }
                    finally
                    {
                        evt.Set();
                    }
                },1000);

            evt.WaitOne(4000);
        }

        [Fact]
        public void TurningOffPoolingAfterStartupShouldStopUsingPooledObjects()
        {
            LogManager.Configuration = CreateConfigurationFromString(Poolingenabled);
            var list = this.WarmUpPool(LogManager.Configuration, 1);

            var firstLogEvent = LogManager.Configuration.PoolFactory.Get<LogEventInfoPool, LogEventInfo>().Get();
            LogManager.Configuration.PoolFactory.Get<LogEventInfoPool, LogEventInfo>().PutBack(firstLogEvent);

            var logger = LogManager.GetLogger("test");

            logger.Info("Test");
            DebugTarget target = (DebugTarget)LogManager.Configuration.FindTargetByName("d");

            var loggedEvent = target.LastLogEvent;

            Assert.True(list.Contains(loggedEvent), " Not Same log event, which is unexpected, since pooling is turned on");

            LogManager.Configuration = CreateConfigurationFromString(PoolingDisabled);

            LogManager.GetLogger("test");

            logger.Info("Test");
            target = (DebugTarget)LogManager.Configuration.FindTargetByName("d");

            loggedEvent = target.LastLogEvent;

            Assert.False(list.Contains(loggedEvent), " Not Same log event, which is unexpected, since pooling is turned off");
        }
        [Fact]
        public void LoggingAMessageShouldAutomaticallyPutLogEventBackIntoPool()
        {
            InternalLogger.LogLevel = LogLevel.Trace;
            InternalLogger.LogToConsole = true;
            var configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true'>
    <pooling enabled=""true"" autoIncreasePoolSizes=""false"" prefillPools=""true"" outputPoolStatisticsInLogFiles=""false"" outputPoolStatisticsInterval=""0"" estimatedMaxMessageSize=""2048"" estimatedLogEventsPerSecond=""1"" />
    <targets>
        <target name='async' type='AsyncWrapper' overflowAction='Block' batchSize='100' queueLimit='200' timeToSleepBetweenBatches='1'>
            <target name='d' type='Debug' layout='${message}'/>
        </target>
    </targets>

    <rules>
      <logger name='*' writeTo='async' minLevel=""Trace"">
      </logger>
    </rules>
</nlog>");

            LogManager.Configuration = configuration;
            var list = this.WarmUpPool(configuration, 1);
            var pool = configuration.PoolFactory.Get<LogEventInfoPool, LogEventInfo>();
            var poolSize = pool.PoolSize;
            var logger = LogManager.GetLogger("test");

            logger.Info("Test");
            Thread.Sleep(1000);
            LogManager.Flush(Console.WriteLine);
            
            // DebugTarget keeps one object from the pool, so we have to add one
            Assert.True(poolSize <= pool.ObjectsInPool+1);
        }

        [Fact]
        public void ManualInspectionTest()
        {
            InternalLogger.LogLevel = LogLevel.Trace;
            InternalLogger.LogToConsole = true;
            var configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true'>
    <pooling enabled=""true"" autoIncreasePoolSizes=""false"" prefillPools=""true"" outputPoolStatisticsInLogFiles=""true"" outputPoolStatisticsInterval=""1"" estimatedMaxMessageSize=""12"" estimatedLogEventsPerSecond=""1""/>
    <targets>
        <target name='async' type='AsyncWrapper' overflowAction='Block' batchSize='10000' queueLimit='20000'>
            <target name='d' type='Debug' layout='${message}'/>
        </target>
    </targets>

    <rules>
      <logger name='*' writeTo='async' minLevel=""Trace"">
      </logger>
    </rules>
</nlog>");
            LogManager.Configuration = configuration;
            var pool = LogManager.Configuration.PoolFactory.Get<AsyncLogEventInfoArrayPool, AsyncLogEventInfo[]>();

            var sizeBefore = pool.PoolSize;
            var arraySizeBefore = pool.IndividualArraySize;
            var objectsBefore = pool.ObjectsInPool;

            Console.WriteLine("PoolSize:" + sizeBefore);
            Console.WriteLine("ArraySize:" + arraySizeBefore);
            Console.WriteLine("Objects In Pool:" + objectsBefore);



            var logger = LogManager.GetLogger("test");

            for (int x = 0; x < 10; x++)
            {
                logger.Info("Test");
                LogManager.Flush(ex => { },  TimeSpan.FromSeconds(5));
            }

         

            LogManager.Flush(ex =>
                {
                    Console.WriteLine("PoolSize:" + pool.PoolSize);
                    Console.WriteLine("ArraySize:" + pool.IndividualArraySize);
                    Console.WriteLine("Objects In Pool:" + pool.ObjectsInPool);

                    Assert.Equal(sizeBefore, pool.PoolSize);
                    Assert.Equal(arraySizeBefore, pool.IndividualArraySize);
                    Assert.Equal(objectsBefore, pool.ObjectsInPool);
                });

        }



        [Fact]
        public void TurningOffPoolingAfterStartupShouldEmptyPools()
        {
            LogManager.Configuration = CreateConfigurationFromString(Poolingenabled);
            var list = this.WarmUpPool(LogManager.Configuration,1);
            var firstLogEvent = LogManager.Configuration.PoolFactory.Get<LogEventInfoPool, LogEventInfo>().Get();
            LogManager.Configuration.PoolFactory.Get<LogEventInfoPool, LogEventInfo>().PutBack(firstLogEvent);

            var logger = LogManager.GetLogger("test");

            logger.Info("Test");
            DebugTarget target = (DebugTarget)LogManager.Configuration.FindTargetByName("d");

            var loggedEvent = target.LastLogEvent;
            LogManager.Flush(Console.WriteLine);
            var pool = LogManager.Configuration.PoolFactory.Get<LogEventInfoPool, LogEventInfo>();
            // DebugTarget keeps one object from the pool, so we have to add one
            Assert.True(1 <= pool.ObjectsInPool+1);

            Assert.True(list.Contains(loggedEvent), " Not Same log event, which is unexpected, since pooling is turned on");

            LogManager.Configuration = CreateConfigurationFromString(PoolingDisabled);
            pool = LogManager.Configuration.PoolFactory.Get<LogEventInfoPool, LogEventInfo>();
            Assert.Equal(0, pool.ObjectsInPool);
        }

        [Fact]
        public void StatsReportingShouldWriteInformation()
        {
            var writer = new StringWriter();

            InternalLogger.LogWriter = writer;
            InternalLogger.IncludeTimestamp = true;
            InternalLogger.LogLevel = LogLevel.Trace;

            var configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true'>
    <pooling enabled='true' autoIncreasePoolSizes='true' prefillPools='false' outputPoolStatisticsInLogFiles='true' outputPoolStatisticsInterval='1' estimatedMaxMessageSize='2048' estimatedLogEventsPerSecond='10' />
    <targets>
        <target name='async' type='AsyncWrapper' overflowAction='Block' batchSize='10000' queueLimit='20000'>
            <target name='d' type='Debug' layout='${message}'/>
        </target>
    </targets>

    <rules>
      <logger name='*' writeTo='async' minLevel=""Trace"">
      </logger>
    </rules>
</nlog>");



            LogManager.Configuration = configuration;


            var list = this.WarmUpPool(configuration, 1);


            var logger = LogManager.GetLogger("test");

            for (int x = 0; x < 1000; x++)
            {
                logger.Info("Hello");
            }

            ManualResetEvent wait = new ManualResetEvent(false);

            // Sleep for at least one second, since timer fires every second to write stats
            Thread.Sleep(2500);
            LogManager.Flush(ex => wait.Set(), 5000);

            wait.WaitOne(5000);
            string logText = writer.ToString();

            Assert.True(logText.IndexOf("Pool Size") > -1, string.Format("Pool stats not found in text:{0}", logText));
            Console.WriteLine(writer);
        }
    }
}
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
using System.IO;

using NLog.Common;
using NLog.Targets;
using NLog.Targets.Wrappers;

using Xunit;

namespace NLog.UnitTests.Internal.Pooling
{
    public class PoolingEnabledLoggingTest : NLogTestBase
    {
        [Fact]
        public void HavingPoolingEnabledShouldNotFail()
        {

            InternalLogger.LogLevel = LogLevel.Trace;
            InternalLogger.LogToConsole = true;
            var configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true'>
    <pooling enabled=""true"" autoIncreasePoolSizes=""false"" prefillPools=""true"" outputPoolStatisticsInLogFiles=""false"" outputPoolStatisticsInterval=""0"" estimatedMaxMessageSize=""2048"" estimatedLogEventsPerSecond=""1"" />
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

            var logger = LogManager.GetLogger("pooling");
            for (int x = 0; x < 1000; x++)
            {
                logger.Info("hello" + x);
            }
            LogManager.Flush(Console.WriteLine);
            
            var target = configuration.FindTargetByName<DebugTarget>("d");

            var lastMessage = target.LastMessage;

            Assert.Equal("hello999", lastMessage);
        }

        [Fact]
        public void HavingPoolingEnabledWithFileTargetShouldNotFail()
        {
            var fileName = Path.GetTempFileName();
            try
            {
                InternalLogger.LogLevel = LogLevel.Trace;
                InternalLogger.LogToConsole = true;
                
                var configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true'>
    <pooling enabled=""true"" autoIncreasePoolSizes=""false"" prefillPools=""true"" outputPoolStatisticsInLogFiles=""false"" outputPoolStatisticsInterval=""0"" estimatedMaxMessageSize=""2048"" estimatedLogEventsPerSecond=""1"" />
    <targets>
 <target name='async' type='AsyncWrapper' overflowAction='Block' batchSize='10000' queueLimit='20000'>
        <target name='d' type='File' layout='${message}' fileName="""+fileName+@"""/>
        </target>
    </targets>

    <rules>
      <logger name='*' writeTo='async' minLevel=""Trace"">
      </logger>
    </rules>
</nlog>");

                LogManager.Configuration = configuration;

                var logger = LogManager.GetLogger("pooling");
                for (int x = 0; x < 1000; x++)
                {
                    logger.Info("hello" + x);
                }

                LogManager.Flush(ex =>
                    {
                        
                    });
                LogManager.Configuration = null;

                StreamReader sr = new StreamReader(fileName);
                string fullLog = sr.ReadToEnd();

                Assert.True(fullLog.Trim().EndsWith("999"));
            }
            finally
            {
                try
                {
                    File.Delete(fileName);
                }
                catch
                {
                }
            }
        }

    }
}
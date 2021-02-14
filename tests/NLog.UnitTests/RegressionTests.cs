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

namespace NLog.UnitTests
{
    using System;
    using NLog.Targets;
    using NLog.Config;
    using Xunit;

    public class RegressionTests : NLogTestBase
    {
        [Fact]
        public void Bug4655UnableToReconfigureExistingLoggers()
        {
            var debugTarget1 = new DebugTarget();
            var debugTarget2 = new DebugTarget();

            SimpleConfigurator.ConfigureForTargetLogging(debugTarget1, LogLevel.Debug);

            ILogger logger = LogManager.GetLogger(Guid.NewGuid().ToString("N"));

            logger.Info("foo");

            Assert.Equal(1, debugTarget1.Counter);
            Assert.Equal(0, debugTarget2.Counter);

            LogManager.Configuration.AddTarget("DesktopConsole", debugTarget2);
            LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, debugTarget2));
            LogManager.ReconfigExistingLoggers();

            logger.Info("foo");

            Assert.Equal(2, debugTarget1.Counter);
            Assert.Equal(1, debugTarget2.Counter);
        }

        [Fact]
        public void Bug5965StackOverflow()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
<nlog xmlns='http://www.nlog-project.org/schemas/NLog.xsd'
      xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>
  
  <targets>
    <target name='file'  xsi:type='AsyncWrapper' queueLimit='5000' overflowAction='Discard'  >
      <target xsi:type='Debug'>
        <layout xsi:type='CSVLayout'>
          <column name='counter' layout='${counter}' />
          <column name='time' layout='${longdate}' />
          <column name='message' layout='${message}' />
        </layout>
      </target>
    </target>
  </targets>

  <rules>
    <logger name='*' minlevel='Trace' writeTo='file' />
  </rules>


</nlog>
");

            var log = LogManager.GetLogger("x");
            log.Fatal("Test");

            LogManager.Configuration = null;
        }
    }
}
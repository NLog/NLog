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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using NLog;
using NLog.Targets;
using NLog.Config;

namespace NLog.UnitTests.Targets
{
    public class MemoryTargetTests : NLogTestBase
    {
        [Fact]
        public void MemoryTarget_LogLevelTest()
        { 
            ILogger logger = LogManager.GetCurrentClassLogger(); 

            var memoryTarget = new MemoryTarget
            {
                Layout = "${level} ${message}" 
            };

            SimpleConfigurator.ConfigureForTargetLogging(memoryTarget, LogLevel.Trace);

            Assert.True(memoryTarget.Logs.Count() == 0);
            logger.Trace("TTT");
            logger.Debug("DDD");
            logger.Info("III");
            logger.Warn("WWW");
            logger.Error("EEE");
            logger.Fatal("FFF"); 
            
            LogManager.Configuration = null;

            Assert.True(memoryTarget.Logs.Count() == 6);
            Assert.True(memoryTarget.Logs[0] == "Trace TTT");
            Assert.True(memoryTarget.Logs[1] == "Debug DDD");
            Assert.True(memoryTarget.Logs[2] == "Info III");
            Assert.True(memoryTarget.Logs[3] == "Warn WWW");
            Assert.True(memoryTarget.Logs[4] == "Error EEE");
            Assert.True(memoryTarget.Logs[5] == "Fatal FFF");
        }

        [Fact]
        public void MemoryTarget_ReconfigureTest_SameTarget_ExpectLogsEmptied()
        {
            ILogger logger = LogManager.GetCurrentClassLogger();

            var memoryTarget = new MemoryTarget
            {
                Layout = "${level} ${message}"
            };

            SimpleConfigurator.ConfigureForTargetLogging(memoryTarget, LogLevel.Trace);

            logger.Debug("DDD");
            logger.Info("III");
            logger.Warn("WWW");

            Assert.True(memoryTarget.Logs.Count() == 3);
            Assert.True(memoryTarget.Logs[0] == "Debug DDD");
            Assert.True(memoryTarget.Logs[1] == "Info III");
            Assert.True(memoryTarget.Logs[2] == "Warn WWW");

            LogManager.Configuration = null;

            //
            // Reconfigure the logger using the same MemoryTarget.

            memoryTarget = new MemoryTarget
            {
                Layout = "${level} ${message}"
            };

            SimpleConfigurator.ConfigureForTargetLogging(memoryTarget, LogLevel.Trace);

            logger.Trace("TTT");
            logger.Error("EEE");
            logger.Fatal("FFF");

            Assert.True(memoryTarget.Logs.Count() == 3);
            Assert.True(memoryTarget.Logs[0] == "Trace TTT");
            Assert.True(memoryTarget.Logs[1] == "Error EEE");
            Assert.True(memoryTarget.Logs[2] == "Fatal FFF");
        }

        [Fact]
        public void MemoryTarget_ClearLogsTest()
        {
            ILogger logger = LogManager.GetCurrentClassLogger();

            var memoryTarget = new MemoryTarget
            {
                Layout = "${level} ${message}"
            };

            SimpleConfigurator.ConfigureForTargetLogging(memoryTarget, LogLevel.Trace);

            logger.Warn("WWW");
            logger.Error("EEE");
            logger.Fatal("FFF");

            memoryTarget.Logs.Clear();
            logger.Trace("TTT");
            logger.Debug("DDD");
            logger.Info("III");

            LogManager.Configuration = null;

            Assert.True(memoryTarget.Logs.Count() == 3);
            Assert.True(memoryTarget.Logs[0] == "Trace TTT");
            Assert.True(memoryTarget.Logs[1] == "Debug DDD");
            Assert.True(memoryTarget.Logs[2] == "Info III");

        }

        [Fact]
        public void MemoryTarget_NullMessageTest()
        {
            ILogger logger = LogManager.GetCurrentClassLogger();

            var memoryTarget = new MemoryTarget
            {
                Layout = "${level} ${message}"
            };

            SimpleConfigurator.ConfigureForTargetLogging(memoryTarget, LogLevel.Trace);

            string nullMessage = null;

            logger.Trace("TTT");
            logger.Debug((String)null);
            logger.Info("III");
            logger.Warn(nullMessage);
            logger.Error("EEE");

            LogManager.Configuration = null;

            Assert.True(memoryTarget.Logs.Count() == 5);
            Assert.True(memoryTarget.Logs[0] == "Trace TTT");
            Assert.True(memoryTarget.Logs[1] == "Debug ");
            Assert.True(memoryTarget.Logs[2] == "Info III");
            Assert.True(memoryTarget.Logs[3] == "Warn ");
            Assert.True(memoryTarget.Logs[4] == "Error EEE");
        }

        [Fact]
        public void MemoryTarget_EmptyMessageTest()
        {
            ILogger logger = LogManager.GetCurrentClassLogger();

            var memoryTarget = new MemoryTarget
            {
                Layout = "${level} ${message}"
            };

            SimpleConfigurator.ConfigureForTargetLogging(memoryTarget, LogLevel.Trace);

            logger.Trace("TTT");
            logger.Debug(String.Empty);
            logger.Info("III");
            logger.Warn("");
            logger.Error("EEE");

            LogManager.Configuration = null;

            Assert.True(memoryTarget.Logs.Count() == 5);
            Assert.True(memoryTarget.Logs[0] == "Trace TTT");
            Assert.True(memoryTarget.Logs[1] == "Debug ");
            Assert.True(memoryTarget.Logs[2] == "Info III");
            Assert.True(memoryTarget.Logs[3] == "Warn ");
            Assert.True(memoryTarget.Logs[4] == "Error EEE");
        }
    }
}

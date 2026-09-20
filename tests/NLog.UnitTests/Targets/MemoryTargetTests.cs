//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.UnitTests.Targets
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NLog.Internal;
    using NLog.Targets;
    using Xunit;

    public class MemoryTargetTests : NLogTestBase
    {
        [Fact]
        public void MemoryTarget_LogLevelTest()
        {
            var memoryTarget = new MemoryTarget
            {
                Layout = "${level} ${message}"
            };
            var logger = new LogFactory().Setup().LoadConfiguration(builder =>
            {
                builder.ForLogger().WriteTo(memoryTarget);
            }).GetCurrentClassLogger();

            Assert.Empty(memoryTarget.Logs);
            Assert.False(memoryTarget.HasLogLevel(LogLevel.Trace));
            logger.Trace("TTT");
            logger.Debug("DDD");
            logger.Info("III");
            logger.Warn("WWW");
            logger.Error("EEE");
            logger.Fatal("FFF");

            logger.Factory.Configuration = null;

            Assert.Equal(6, memoryTarget.Logs.Count);
            Assert.Equal("Trace TTT", memoryTarget.Logs[0]);
            Assert.Equal("Debug DDD", memoryTarget.Logs[1]);
            Assert.Equal("Info III", memoryTarget.Logs[2]);
            Assert.Equal("Warn WWW", memoryTarget.Logs[3]);
            Assert.Equal("Error EEE", memoryTarget.Logs[4]);
            Assert.Equal("Fatal FFF", memoryTarget.Logs[5]);
            Assert.True(memoryTarget.HasLogLevel(LogLevel.Error));
            Assert.True(memoryTarget.HasLogLevel(LogLevel.Fatal));
            Assert.True(memoryTarget.HasLogLevelOrException(LogLevel.Error));
            Assert.True(memoryTarget.HasLogLevelOrException(LogLevel.Fatal));
            Assert.NotEmpty(memoryTarget.Dump());
            memoryTarget.Logs.Clear();
            Assert.False(memoryTarget.HasLogLevel(LogLevel.Error));
            Assert.False(memoryTarget.HasLogLevel(LogLevel.Fatal));
            Assert.False(memoryTarget.HasLogLevelOrException(LogLevel.Error));
            Assert.False(memoryTarget.HasLogLevelOrException(LogLevel.Fatal));
            Assert.Empty(memoryTarget.Dump());
        }

        [Fact]
        public void MemoryTarget_ReconfigureTest_SameTarget_ExpectLogsEmptied()
        {
            var memoryTarget = new MemoryTarget
            {
                Layout = "${level} ${message}"
            };

            var logger = new LogFactory().Setup().LoadConfiguration(builder =>
            {
                builder.ForLogger().WriteTo(memoryTarget);
            }).GetCurrentClassLogger();

            logger.Debug("DDD");
            logger.Info("III");
            logger.Warn("WWW");

            Assert.Equal(3, memoryTarget.Logs.Count);
            Assert.Equal("Debug DDD", memoryTarget.Logs[0]);
            Assert.Equal("Info III", memoryTarget.Logs[1]);
            Assert.Equal("Warn WWW", memoryTarget.Logs[2]);

            logger.Factory.Configuration = null;

            // Reconfigure the logger to use a new MemoryTarget.
            memoryTarget = new MemoryTarget
            {
                Layout = "${level} ${message}"
            };

            logger.Factory.Setup().LoadConfiguration(builder =>
            {
                builder.ForLogger().WriteTo(memoryTarget);
            }).GetCurrentClassLogger();

            logger.Trace("TTT");
            logger.Error("EEE");
            logger.Fatal("FFF");

            Assert.Equal(3, memoryTarget.Logs.Count);
            Assert.Equal("Trace TTT", memoryTarget.Logs[0]);
            Assert.Equal("Error EEE", memoryTarget.Logs[1]);
            Assert.Equal("Fatal FFF", memoryTarget.Logs[2]);
        }

        [Fact]
        public void MemoryTarget_ClearLogsTest()
        {
            var memoryTarget = new MemoryTarget
            {
                Layout = "${level} ${message}"
            };

            var logger = new LogFactory().Setup().LoadConfiguration(builder =>
            {
                builder.ForLogger().WriteTo(memoryTarget);
            }).GetCurrentClassLogger();

            logger.Warn("WWW");
            logger.Error("EEE");
            logger.Fatal("FFF");

            memoryTarget.Logs.Clear();
            logger.Trace("TTT");
            logger.Debug("DDD");
            logger.Info("III");

            logger.Factory.Configuration = null;

            Assert.Equal(3, memoryTarget.Logs.Count);
            Assert.Equal("Trace TTT", memoryTarget.Logs[0]);
            Assert.Equal("Debug DDD", memoryTarget.Logs[1]);
            Assert.Equal("Info III", memoryTarget.Logs[2]);

            Assert.True(memoryTarget.Logs.All(l => !string.IsNullOrEmpty(l)));
            Assert.True(memoryTarget.Logs.Contains(memoryTarget.Logs[0]));
            Assert.False(memoryTarget.Logs.Contains(string.Empty));
            Assert.True(memoryTarget.Logs.Remove(memoryTarget.Logs[0]));
            Assert.False(memoryTarget.Logs.Remove(string.Empty));
            Assert.Equal(2, memoryTarget.Logs.Count);
            Assert.Equal(0, memoryTarget.Logs.IndexOf(memoryTarget.Logs[0]));
            Assert.Equal(1, memoryTarget.Logs.IndexOf(memoryTarget.Logs[1]));
            Assert.Equal(-1, memoryTarget.Logs.IndexOf(string.Empty));
            memoryTarget.Logs.RemoveAt(1);
            Assert.Single(memoryTarget.Logs);
            memoryTarget.Logs[0] = "Hello World";
            Assert.Contains("Hello World", memoryTarget.Logs);
            memoryTarget.Logs.Insert(1, "Goodbye World");
            Assert.Equal("Hello World", memoryTarget.Logs[0]);
            Assert.Equal("Goodbye World", memoryTarget.Logs[1]);
            Assert.Equal(2, memoryTarget.Logs.Count);
        }

        [Fact]
        public void MemoryTarget_NullMessageTest()
        {
            var memoryTarget = new MemoryTarget
            {
                Layout = "${level} ${message}"
            };

            var logger = new LogFactory().Setup().LoadConfiguration(builder =>
            {
                builder.ForLogger().WriteTo(memoryTarget);
            }).GetCurrentClassLogger();

            string nullMessage = null;

            logger.Trace("TTT");
            logger.Debug((String)null);
            logger.Info("III");
            logger.Warn(nullMessage);
            logger.Error("EEE");

            logger.Factory.Configuration = null;

            Assert.Equal(5, memoryTarget.Logs.Count);
            Assert.Equal("Trace TTT", memoryTarget.Logs[0]);
            Assert.Equal("Debug ", memoryTarget.Logs[1]);
            Assert.Equal("Info III", memoryTarget.Logs[2]);
            Assert.Equal("Warn ", memoryTarget.Logs[3]);
            Assert.Equal("Error EEE", memoryTarget.Logs[4]);
        }

        [Fact]
        public void MemoryTarget_EmptyMessageTest()
        {
            var memoryTarget = new MemoryTarget
            {
                Layout = "${level} ${message}"
            };

            var logger = new LogFactory().Setup().LoadConfiguration(builder =>
            {
                builder.ForLogger().WriteTo(memoryTarget);
            }).GetCurrentClassLogger();

            logger.Trace("TTT");
            logger.Debug(String.Empty);
            logger.Info("III");
            logger.Warn("");
            logger.Error("EEE");

            logger.Factory.Configuration = null;

            Assert.Equal(5, memoryTarget.Logs.Count);
            Assert.Equal("Trace TTT", memoryTarget.Logs[0]);
            Assert.Equal("Debug ", memoryTarget.Logs[1]);
            Assert.Equal("Info III", memoryTarget.Logs[2]);
            Assert.Equal("Warn ", memoryTarget.Logs[3]);
            Assert.Equal("Error EEE", memoryTarget.Logs[4]);
        }

        [Fact]
        public void MemoryTarget_IList_GetEnumerator()
        {
            var memoryTarget = new MemoryTarget();

            Assert.Empty(memoryTarget.Logs);

            memoryTarget.Logs.Add("A");
            Assert.Equal(new[] { "A" }, memoryTarget.Logs);

            memoryTarget.Logs.Add("B");
            Assert.Equal(new[] { "A", "B" }, memoryTarget.Logs);

            // Force the internal ring buffer to wrap.
            memoryTarget.MaxLogsCount = 3;
            memoryTarget.Logs.Add("C");
            memoryTarget.Logs.Add("D");

            Assert.Equal(new[] { "B", "C", "D" }, memoryTarget.Logs);
            Assert.Equal(new[] { "B", "C", "D" }, memoryTarget.Logs.ToList());
        }

        [Fact]
        public void MemoryTarget_IList_Add()
        {
            var memoryTarget = new MemoryTarget();

            memoryTarget.Logs.Add("A");
            Assert.Equal(new[] { "A" }, memoryTarget.Logs);

            memoryTarget.Logs.Add("B");
            Assert.Equal(new[] { "A", "B" }, memoryTarget.Logs);

            // Exercise normal growth.
            memoryTarget.Logs.Add("C");
            memoryTarget.Logs.Add("D");
            Assert.Equal(new[] { "A", "B", "C", "D" }, memoryTarget.Logs);

            // Exercise ring-buffer wrapping.
            memoryTarget.MaxLogsCount = 3;

            memoryTarget.Logs.Add("E");
            Assert.Equal(new[] { "C", "D", "E" }, memoryTarget.Logs);

            memoryTarget.Logs.Add("F");
            Assert.Equal(new[] { "D", "E", "F" }, memoryTarget.Logs);

            memoryTarget.Logs.Add("G");
            Assert.Equal(new[] { "E", "F", "G" }, memoryTarget.Logs);

            memoryTarget.MaxLogsCount = 0;
            memoryTarget.Logs.Add("H");
            Assert.Equal(new[] { "E", "F", "G", "H" }, memoryTarget.Logs);
        }

        [Fact]
        public void MemoryTarget_IList_IndexOperator()
        {
            var memoryTarget = new MemoryTarget();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ = memoryTarget.Logs[0]);

            memoryTarget.Logs.Add("A");

            Assert.Equal("A", memoryTarget.Logs[0]);

            memoryTarget.Logs[0] = "X";
            Assert.Equal("X", memoryTarget.Logs[0]);

            memoryTarget.Logs.Add("B");

            Assert.Equal("X", memoryTarget.Logs[0]);
            Assert.Equal("B", memoryTarget.Logs[1]);

            memoryTarget.Logs[1] = "Y";
            Assert.Equal(new[] { "X", "Y" }, memoryTarget.Logs);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ = memoryTarget.Logs[-1]);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ = memoryTarget.Logs[memoryTarget.Logs.Count]);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => memoryTarget.Logs[memoryTarget.Logs.Count] = "Z");

            // Exercise logical indexing after ring-buffer wrap.
            memoryTarget.MaxLogsCount = 3;
            memoryTarget.Logs.Add("C");
            memoryTarget.Logs.Add("D");

            Assert.Equal(new[] { "Y", "C", "D" }, memoryTarget.Logs);

            Assert.Equal("Y", memoryTarget.Logs[0]);
            Assert.Equal("C", memoryTarget.Logs[1]);
            Assert.Equal("D", memoryTarget.Logs[2]);

            memoryTarget.Logs[1] = "X";
            Assert.Equal(new[] { "Y", "X", "D" }, memoryTarget.Logs);
        }

        [Fact]
        public void MemoryTarget_NonBlockingEnumeration_WithoutMaxLogsCount()
        {
            var memoryTarget = new MemoryTarget
            {
                Layout = "${message}",
                BlockingEnumeration = false
            };
            var logger = new LogFactory().Setup().LoadConfiguration(builder =>
            {
                builder.ForLogger().WriteTo(memoryTarget);
            }).GetCurrentClassLogger();

            logger.Info("A");
            logger.Info("B");
            logger.Info("C");

            var seen = new List<string>();
            foreach (var line in memoryTarget.Logs)
            {
                seen.Add(line);
                logger.Info("x" + seen.Count);
            }

            Assert.Contains("A", seen);
            Assert.Contains("B", seen);
            Assert.Contains("C", seen);
        }

        [Fact]
        public void MemoryTarget_NonBlockingEnumeration_WithMaxLogsCount()
        {
            var memoryTarget = new MemoryTarget
            {
                Layout = "${message}",
                MaxLogsCount = 5,
                BlockingEnumeration = false
            };
            var logger = new LogFactory().Setup().LoadConfiguration(builder =>
            {
                builder.ForLogger().WriteTo(memoryTarget);
            }).GetCurrentClassLogger();

            for (var i = 0; i < 10; i++)
                logger.Info(i.ToString());

            var seen = new List<string>();
            foreach (var line in memoryTarget.Logs)
            {
                seen.Add(line);
                for (var i = 0; i < 5; i++)
                    logger.Info("x" + seen.Count + i);
            }

            Assert.Equal("5", seen[0]);
            Assert.InRange(seen.Count, 1, 5);
            Assert.DoesNotContain("6", seen);
            Assert.DoesNotContain("7", seen);
            Assert.DoesNotContain("8", seen);
            Assert.DoesNotContain("9", seen);
        }
    }
}

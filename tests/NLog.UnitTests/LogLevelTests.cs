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

namespace NLog.UnitTests
{
    using System;
    using System.Collections.Generic;
    using Xunit;

    public class LogLevelTests : NLogTestBase
    {
        [Fact]
        [Trait("Component", "Core")]
        public void OrdinalTest()
        {
            Assert.True(LogLevel.Trace < LogLevel.Debug);
            Assert.True(LogLevel.Debug < LogLevel.Info);
            Assert.True(LogLevel.Info < LogLevel.Warn);
            Assert.True(LogLevel.Warn < LogLevel.Error);
            Assert.True(LogLevel.Error < LogLevel.Fatal);
            Assert.True(LogLevel.Fatal < LogLevel.Off);

            Assert.False(LogLevel.Trace > LogLevel.Debug);
            Assert.False(LogLevel.Debug > LogLevel.Info);
            Assert.False(LogLevel.Info > LogLevel.Warn);
            Assert.False(LogLevel.Warn > LogLevel.Error);
            Assert.False(LogLevel.Error > LogLevel.Fatal);
            Assert.False(LogLevel.Fatal > LogLevel.Off);

            Assert.True(LogLevel.Trace <= LogLevel.Debug);
            Assert.True(LogLevel.Debug <= LogLevel.Info);
            Assert.True(LogLevel.Info <= LogLevel.Warn);
            Assert.True(LogLevel.Warn <= LogLevel.Error);
            Assert.True(LogLevel.Error <= LogLevel.Fatal);
            Assert.True(LogLevel.Fatal <= LogLevel.Off);

            Assert.False(LogLevel.Trace >= LogLevel.Debug);
            Assert.False(LogLevel.Debug >= LogLevel.Info);
            Assert.False(LogLevel.Info >= LogLevel.Warn);
            Assert.False(LogLevel.Warn >= LogLevel.Error);
            Assert.False(LogLevel.Error >= LogLevel.Fatal);
            Assert.False(LogLevel.Fatal >= LogLevel.Off);
        }

        [Fact]
        [Trait("Component", "Core")]
        public void LogLevelEqualityTest()
        {
            LogLevel levelTrace = LogLevel.Trace;
            LogLevel levelInfo = LogLevel.Info;

            Assert.True(LogLevel.Trace == levelTrace);
            Assert.True(LogLevel.Info == levelInfo);
            Assert.False(LogLevel.Trace == levelInfo);

            Assert.False(LogLevel.Trace != levelTrace);
            Assert.False(LogLevel.Info != levelInfo);
            Assert.True(LogLevel.Trace != levelInfo);
        }

        [Fact]
        [Trait("Component", "Core")]
        public void LogLevelFromOrdinal_InputInRange_ExpectValidLevel()
        {
            Assert.Same(LogLevel.Trace, LogLevel.FromOrdinal(0));
            Assert.Same(LogLevel.Debug, LogLevel.FromOrdinal(1));
            Assert.Same(LogLevel.Info, LogLevel.FromOrdinal(2));
            Assert.Same(LogLevel.Warn, LogLevel.FromOrdinal(3));
            Assert.Same(LogLevel.Error, LogLevel.FromOrdinal(4));
            Assert.Same(LogLevel.Fatal, LogLevel.FromOrdinal(5));
            Assert.Same(LogLevel.Off, LogLevel.FromOrdinal(6));
        }

        [Fact]
        [Trait("Component", "Core")]
        public void LogLevelFromOrdinal_InputOutOfRange_ExpectException()
        {
            Assert.Throws<ArgumentException>(() => LogLevel.FromOrdinal(100));

            // Boundary conditions.
            Assert.Throws<ArgumentException>(() => LogLevel.FromOrdinal(-1));
            Assert.Throws<ArgumentException>(() => LogLevel.FromOrdinal(7));
        }

        [Fact]
        [Trait("Component", "Core")]
        public void FromStringTest()
        {
            Assert.Same(LogLevel.Trace, LogLevel.FromString("trace"));
            Assert.Same(LogLevel.Debug, LogLevel.FromString("debug"));
            Assert.Same(LogLevel.Info, LogLevel.FromString("info"));
            Assert.Same(LogLevel.Warn, LogLevel.FromString("warn"));
            Assert.Same(LogLevel.Error, LogLevel.FromString("error"));
            Assert.Same(LogLevel.Fatal, LogLevel.FromString("fatal"));
            Assert.Same(LogLevel.Off, LogLevel.FromString("off"));

            Assert.Same(LogLevel.Trace, LogLevel.FromString("Trace"));
            Assert.Same(LogLevel.Debug, LogLevel.FromString("Debug"));
            Assert.Same(LogLevel.Info, LogLevel.FromString("Info"));
            Assert.Same(LogLevel.Warn, LogLevel.FromString("Warn"));
            Assert.Same(LogLevel.Error, LogLevel.FromString("Error"));
            Assert.Same(LogLevel.Fatal, LogLevel.FromString("Fatal"));
            Assert.Same(LogLevel.Off, LogLevel.FromString("Off"));

            Assert.Same(LogLevel.Trace, LogLevel.FromString("TracE"));
            Assert.Same(LogLevel.Debug, LogLevel.FromString("DebuG"));
            Assert.Same(LogLevel.Info, LogLevel.FromString("InfO"));
            Assert.Same(LogLevel.Warn, LogLevel.FromString("WarN"));
            Assert.Same(LogLevel.Error, LogLevel.FromString("ErroR"));
            Assert.Same(LogLevel.Fatal, LogLevel.FromString("FataL"));

            Assert.Same(LogLevel.Trace, LogLevel.FromString("TRACE"));
            Assert.Same(LogLevel.Debug, LogLevel.FromString("DEBUG"));
            Assert.Same(LogLevel.Info, LogLevel.FromString("INFO"));
            Assert.Same(LogLevel.Warn, LogLevel.FromString("WARN"));
            Assert.Same(LogLevel.Error, LogLevel.FromString("ERROR"));
            Assert.Same(LogLevel.Fatal, LogLevel.FromString("FATAL"));

            Assert.Same(LogLevel.Off, LogLevel.FromString("NoNe"));
            Assert.Same(LogLevel.Info, LogLevel.FromString("iNformaTION"));
            Assert.Same(LogLevel.Warn, LogLevel.FromString("WarNING"));
        }

        [Fact]
        [Trait("Component", "Core")]
        public void FromStringFailingTest()
        {
            Assert.Throws<ArgumentException>(() => LogLevel.FromString("zzz"));
            Assert.Throws<ArgumentException>(() => LogLevel.FromString(string.Empty));
            Assert.Throws<ArgumentNullException>(() => LogLevel.FromString(null));
        }

        [Fact]
        [Trait("Component", "Core")]
        public void LogLevelNullComparison()
        {
            LogLevel level = LogLevel.Info;
            Assert.False(level is null);
            Assert.True(level != null);
            Assert.False(null == level);
            Assert.True(null != level);

            level = null;
            Assert.True(level is null);
            Assert.False(level != null);
            Assert.True(null == level);
            Assert.False(null != level);
        }

        [Fact]
        [Trait("Component", "Core")]
        public void ToStringTest()
        {
            Assert.Equal("Trace", LogLevel.Trace.ToString());
            Assert.Equal("Debug", LogLevel.Debug.ToString());
            Assert.Equal("Info", LogLevel.Info.ToString());
            Assert.Equal("Warn", LogLevel.Warn.ToString());
            Assert.Equal("Error", LogLevel.Error.ToString());
            Assert.Equal("Fatal", LogLevel.Fatal.ToString());
        }

        [Fact]
        [Trait("Component", "Core")]
        public void LogLevelCompareTo_ValidLevels_ExpectIntValues()
        {
            LogLevel levelTrace = LogLevel.Trace;
            LogLevel levelDebug = LogLevel.Debug;
            LogLevel levelInfo = LogLevel.Info;
            LogLevel levelWarn = LogLevel.Warn;
            LogLevel levelError = LogLevel.Error;
            LogLevel levelFatal = LogLevel.Fatal;
            LogLevel levelOff = LogLevel.Off;

            LogLevel levelMin = LogLevel.MinLevel;
            LogLevel levelMax = LogLevel.MaxLevel;

            Assert.Equal(-1, LogLevel.Trace.CompareTo(levelDebug));
            Assert.Equal(-1, LogLevel.Debug.CompareTo(levelInfo));
            Assert.Equal(-1, LogLevel.Info.CompareTo(levelWarn));
            Assert.Equal(-1, LogLevel.Warn.CompareTo(levelError));
            Assert.Equal(-1, LogLevel.Error.CompareTo(levelFatal));
            Assert.Equal(-1, LogLevel.Fatal.CompareTo(levelOff));

            Assert.Equal(1, LogLevel.Debug.CompareTo(levelTrace));
            Assert.Equal(1, LogLevel.Info.CompareTo(levelDebug));
            Assert.Equal(1, LogLevel.Warn.CompareTo(levelInfo));
            Assert.Equal(1, LogLevel.Error.CompareTo(levelWarn));
            Assert.Equal(1, LogLevel.Fatal.CompareTo(levelError));
            Assert.Equal(1, LogLevel.Off.CompareTo(levelFatal));

            Assert.Equal(0, LogLevel.Debug.CompareTo(levelDebug));
            Assert.Equal(0, LogLevel.Info.CompareTo(levelInfo));
            Assert.Equal(0, LogLevel.Warn.CompareTo(levelWarn));
            Assert.Equal(0, LogLevel.Error.CompareTo(levelError));
            Assert.Equal(0, LogLevel.Fatal.CompareTo(levelFatal));
            Assert.Equal(0, LogLevel.Off.CompareTo(levelOff));

            Assert.Equal(0, LogLevel.Trace.CompareTo(levelMin));
            Assert.Equal(1, LogLevel.Debug.CompareTo(levelMin));
            Assert.Equal(2, LogLevel.Info.CompareTo(levelMin));
            Assert.Equal(3, LogLevel.Warn.CompareTo(levelMin));
            Assert.Equal(4, LogLevel.Error.CompareTo(levelMin));
            Assert.Equal(5, LogLevel.Fatal.CompareTo(levelMin));
            Assert.Equal(6, LogLevel.Off.CompareTo(levelMin));

            Assert.Equal(-5, LogLevel.Trace.CompareTo(levelMax));
            Assert.Equal(-4, LogLevel.Debug.CompareTo(levelMax));
            Assert.Equal(-3, LogLevel.Info.CompareTo(levelMax));
            Assert.Equal(-2, LogLevel.Warn.CompareTo(levelMax));
            Assert.Equal(-1, LogLevel.Error.CompareTo(levelMax));
            Assert.Equal(0, LogLevel.Fatal.CompareTo(levelMax));
            Assert.Equal(1, LogLevel.Off.CompareTo(levelMax));
        }

        [Fact]
        [Trait("Component", "Core")]
        public void LogLevelCompareTo_Null_ExpectLevelOff()
        {
            Assert.True(LogLevel.MinLevel.CompareTo(null) < 0);
            Assert.True(LogLevel.MaxLevel.CompareTo(null) < 0);
            Assert.Equal(0, LogLevel.Off.CompareTo(null));
        }

        [Fact]
        [Trait("Component", "Core")]
        public void LogLevel_MinMaxLevels_ExpectConstantValues()
        {
            Assert.Same(LogLevel.Trace, LogLevel.MinLevel);
            Assert.Same(LogLevel.Fatal, LogLevel.MaxLevel);
        }

        [Fact]
        [Trait("Component", "Core")]
        public void LogLevelGetHashCode()
        {
            Assert.Equal(0, LogLevel.Trace.GetHashCode());
            Assert.Equal(1, LogLevel.Debug.GetHashCode());
            Assert.Equal(2, LogLevel.Info.GetHashCode());
            Assert.Equal(3, LogLevel.Warn.GetHashCode());
            Assert.Equal(4, LogLevel.Error.GetHashCode());
            Assert.Equal(5, LogLevel.Fatal.GetHashCode());
            Assert.Equal(6, LogLevel.Off.GetHashCode());
        }

        [Fact]
        [Trait("Component", "Core")]
        public void LogLevelEquals_Null_ExpectFalse()
        {
            Assert.False(LogLevel.Debug.Equals(null));

            LogLevel logLevel = null;
            Assert.False(LogLevel.Debug.Equals(logLevel));

            Object obj = logLevel;
            Assert.False(LogLevel.Debug.Equals(obj));
        }

        [Fact]
        public void LogLevelEqual_TypeOfObject()
        {
            // Objects of any other type should always return false.
            Assert.False(LogLevel.Debug.Equals((int)1));
            Assert.False(LogLevel.Debug.Equals((string)"Debug"));

            // Valid LogLevel objects boxed as Object type.
            Object levelTrace = LogLevel.Trace;
            Object levelDebug = LogLevel.Debug;
            Object levelInfo = LogLevel.Info;
            Object levelWarn = LogLevel.Warn;
            Object levelError = LogLevel.Error;
            Object levelFatal = LogLevel.Fatal;
            Object levelOff = LogLevel.Off;

            Assert.False(LogLevel.Warn.Equals(levelTrace));
            Assert.False(LogLevel.Warn.Equals(levelDebug));
            Assert.False(LogLevel.Warn.Equals(levelInfo));
            Assert.True(LogLevel.Warn.Equals(levelWarn));
            Assert.False(LogLevel.Warn.Equals(levelError));
            Assert.False(LogLevel.Warn.Equals(levelFatal));
            Assert.False(LogLevel.Warn.Equals(levelOff));
        }

        [Fact]
        public void LogLevelEqual_TypeOfLogLevel()
        {
            Assert.False(LogLevel.Warn.Equals(LogLevel.Trace));
            Assert.False(LogLevel.Warn.Equals(LogLevel.Debug));
            Assert.False(LogLevel.Warn.Equals(LogLevel.Info));
            Assert.True(LogLevel.Warn.Equals(LogLevel.Warn));
            Assert.False(LogLevel.Warn.Equals(LogLevel.Error));
            Assert.False(LogLevel.Warn.Equals(LogLevel.Fatal));
            Assert.False(LogLevel.Warn.Equals(LogLevel.Off));
        }

        [Fact]
        public void LogLevel_GetAllLevels()
        {
            Assert.Equal(
                new List<LogLevel>() { LogLevel.Trace, LogLevel.Debug, LogLevel.Info, LogLevel.Warn, LogLevel.Error, LogLevel.Fatal, LogLevel.Off },
                LogLevel.AllLevels);
        }

        [Fact]
        public void LogLevel_SetAllLevels()
        {
            Assert.Throws<NotSupportedException>(() => ((ICollection<LogLevel>)LogLevel.AllLevels).Add(LogLevel.Fatal));
        }

        [Fact]
        public void LogLevel_GetAllLoggingLevels()
        {
            Assert.Equal(
                new List<LogLevel>() { LogLevel.Trace, LogLevel.Debug, LogLevel.Info, LogLevel.Warn, LogLevel.Error, LogLevel.Fatal },
                LogLevel.AllLoggingLevels);
        }

        [Fact]
        public void LogLevel_SetAllLoggingLevels()
        {
            Assert.Throws<NotSupportedException>(() => ((ICollection<LogLevel>)LogLevel.AllLoggingLevels).Add(LogLevel.Fatal));
        }
    }
}

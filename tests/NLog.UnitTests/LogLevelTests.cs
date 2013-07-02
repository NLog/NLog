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

namespace NLog.UnitTests
{
    using System;
    using Xunit;

    public class LogLevelTests : NLogTestBase
    {
        [Fact]
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
        public void FromStringTest()
        {
            Assert.Same(LogLevel.FromString("trace"), LogLevel.Trace);
            Assert.Same(LogLevel.FromString("debug"), LogLevel.Debug);
            Assert.Same(LogLevel.FromString("info"), LogLevel.Info);
            Assert.Same(LogLevel.FromString("warn"), LogLevel.Warn);
            Assert.Same(LogLevel.FromString("error"), LogLevel.Error);
            Assert.Same(LogLevel.FromString("fatal"), LogLevel.Fatal);
            Assert.Same(LogLevel.FromString("off"), LogLevel.Off);

            Assert.Same(LogLevel.FromString("Trace"), LogLevel.Trace);
            Assert.Same(LogLevel.FromString("Debug"), LogLevel.Debug);
            Assert.Same(LogLevel.FromString("Info"), LogLevel.Info);
            Assert.Same(LogLevel.FromString("Warn"), LogLevel.Warn);
            Assert.Same(LogLevel.FromString("Error"), LogLevel.Error);
            Assert.Same(LogLevel.FromString("Fatal"), LogLevel.Fatal);
            Assert.Same(LogLevel.FromString("Off"), LogLevel.Off);

            Assert.Same(LogLevel.FromString("TracE"), LogLevel.Trace);
            Assert.Same(LogLevel.FromString("DebuG"), LogLevel.Debug);
            Assert.Same(LogLevel.FromString("InfO"), LogLevel.Info);
            Assert.Same(LogLevel.FromString("WarN"), LogLevel.Warn);
            Assert.Same(LogLevel.FromString("ErroR"), LogLevel.Error);
            Assert.Same(LogLevel.FromString("FataL"), LogLevel.Fatal);

            Assert.Same(LogLevel.FromString("TRACE"), LogLevel.Trace);
            Assert.Same(LogLevel.FromString("DEBUG"), LogLevel.Debug);
            Assert.Same(LogLevel.FromString("INFO"), LogLevel.Info);
            Assert.Same(LogLevel.FromString("WARN"), LogLevel.Warn);
            Assert.Same(LogLevel.FromString("ERROR"), LogLevel.Error);
            Assert.Same(LogLevel.FromString("FATAL"), LogLevel.Fatal);
        }

        [Fact]
        public void FromStringFailingTest()
        {
            Assert.Throws<ArgumentException>(() => LogLevel.FromString("zzz"));
        }

        [Fact]
        public void LogLevelNullComparison()
        {
            LogLevel level = LogLevel.Info;
            Assert.False(level == null);
            Assert.True(level != null);
            Assert.False(null == level);
            Assert.True(null != level);

            level = null;
            Assert.True(level == null);
            Assert.False(level != null);
            Assert.True(null == level);
            Assert.False(null != level);
        }

        [Fact]
        public void ToStringTest()
        {
            Assert.Equal(LogLevel.Trace.ToString(), "Trace");
            Assert.Equal(LogLevel.Debug.ToString(), "Debug");
            Assert.Equal(LogLevel.Info.ToString(), "Info");
            Assert.Equal(LogLevel.Warn.ToString(), "Warn");
            Assert.Equal(LogLevel.Error.ToString(), "Error");
            Assert.Equal(LogLevel.Fatal.ToString(), "Fatal");
        }
    }
}

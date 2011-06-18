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

using System;

using NUnit.Framework;

#if !NUNIT
    using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
    using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
    using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
    using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
    using ExpectedException = Microsoft.VisualStudio.TestTools.UnitTesting.ExpectedExceptionAttribute;
#endif

namespace NLog.UnitTests
{
    [TestFixture]
    public class LogLevelTests : NLogTestBase
    {
        [Test]
        public void OrdinalTest()
        {
            Assert.IsTrue(LogLevel.Trace < LogLevel.Debug);
            Assert.IsTrue(LogLevel.Debug < LogLevel.Info);
            Assert.IsTrue(LogLevel.Info < LogLevel.Warn);
            Assert.IsTrue(LogLevel.Warn < LogLevel.Error);
            Assert.IsTrue(LogLevel.Error < LogLevel.Fatal);
            Assert.IsTrue(LogLevel.Fatal < LogLevel.Off);

            Assert.IsFalse(LogLevel.Trace > LogLevel.Debug);
            Assert.IsFalse(LogLevel.Debug > LogLevel.Info);
            Assert.IsFalse(LogLevel.Info > LogLevel.Warn);
            Assert.IsFalse(LogLevel.Warn > LogLevel.Error);
            Assert.IsFalse(LogLevel.Error > LogLevel.Fatal);
            Assert.IsFalse(LogLevel.Fatal > LogLevel.Off);

            Assert.IsTrue(LogLevel.Trace <= LogLevel.Debug);
            Assert.IsTrue(LogLevel.Debug <= LogLevel.Info);
            Assert.IsTrue(LogLevel.Info <= LogLevel.Warn);
            Assert.IsTrue(LogLevel.Warn <= LogLevel.Error);
            Assert.IsTrue(LogLevel.Error <= LogLevel.Fatal);
            Assert.IsTrue(LogLevel.Fatal <= LogLevel.Off);

            Assert.IsFalse(LogLevel.Trace >= LogLevel.Debug);
            Assert.IsFalse(LogLevel.Debug >= LogLevel.Info);
            Assert.IsFalse(LogLevel.Info >= LogLevel.Warn);
            Assert.IsFalse(LogLevel.Warn >= LogLevel.Error);
            Assert.IsFalse(LogLevel.Error >= LogLevel.Fatal);
            Assert.IsFalse(LogLevel.Fatal >= LogLevel.Off);
        }

        [Test]
        public void FromStringTest()
        {
            Assert.AreSame(LogLevel.FromString("trace"), LogLevel.Trace);
            Assert.AreSame(LogLevel.FromString("debug"), LogLevel.Debug);
            Assert.AreSame(LogLevel.FromString("info"), LogLevel.Info);
            Assert.AreSame(LogLevel.FromString("warn"), LogLevel.Warn);
            Assert.AreSame(LogLevel.FromString("error"), LogLevel.Error);
            Assert.AreSame(LogLevel.FromString("fatal"), LogLevel.Fatal);
            Assert.AreSame(LogLevel.FromString("off"), LogLevel.Off);

            Assert.AreSame(LogLevel.FromString("Trace"), LogLevel.Trace);
            Assert.AreSame(LogLevel.FromString("Debug"), LogLevel.Debug);
            Assert.AreSame(LogLevel.FromString("Info"), LogLevel.Info);
            Assert.AreSame(LogLevel.FromString("Warn"), LogLevel.Warn);
            Assert.AreSame(LogLevel.FromString("Error"), LogLevel.Error);
            Assert.AreSame(LogLevel.FromString("Fatal"), LogLevel.Fatal);
            Assert.AreSame(LogLevel.FromString("Off"), LogLevel.Off);

            Assert.AreSame(LogLevel.FromString("TracE"), LogLevel.Trace);
            Assert.AreSame(LogLevel.FromString("DebuG"), LogLevel.Debug);
            Assert.AreSame(LogLevel.FromString("InfO"), LogLevel.Info);
            Assert.AreSame(LogLevel.FromString("WarN"), LogLevel.Warn);
            Assert.AreSame(LogLevel.FromString("ErroR"), LogLevel.Error);
            Assert.AreSame(LogLevel.FromString("FataL"), LogLevel.Fatal);

            Assert.AreSame(LogLevel.FromString("TRACE"), LogLevel.Trace);
            Assert.AreSame(LogLevel.FromString("DEBUG"), LogLevel.Debug);
            Assert.AreSame(LogLevel.FromString("INFO"), LogLevel.Info);
            Assert.AreSame(LogLevel.FromString("WARN"), LogLevel.Warn);
            Assert.AreSame(LogLevel.FromString("ERROR"), LogLevel.Error);
            Assert.AreSame(LogLevel.FromString("FATAL"), LogLevel.Fatal);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void FromStringFailingTest()
        {
            LogLevel.FromString("zzz");
        }

        [Test]
        public void LogLevelNullComparison()
        {
            LogLevel level = LogLevel.Info;
            Assert.IsFalse(level == null);
            Assert.IsTrue(level != null);
            Assert.IsFalse(null == level);
            Assert.IsTrue(null != level);

            level = null;
            Assert.IsTrue(level == null);
            Assert.IsFalse(level != null);
            Assert.IsTrue(null == level);
            Assert.IsFalse(null != level);
        }

        [Test]
        public void ToStringTest()
        {
            Assert.AreEqual(LogLevel.Trace.ToString(), "Trace");
            Assert.AreEqual(LogLevel.Debug.ToString(), "Debug");
            Assert.AreEqual(LogLevel.Info.ToString(), "Info");
            Assert.AreEqual(LogLevel.Warn.ToString(), "Warn");
            Assert.AreEqual(LogLevel.Error.ToString(), "Error");
            Assert.AreEqual(LogLevel.Fatal.ToString(), "Fatal");
        }
    }
}

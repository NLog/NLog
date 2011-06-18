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

#pragma warning disable 0618

namespace NLog.UnitTests.Contexts
{
    using NUnit.Framework;

#if !NUNIT
    using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
    using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
    using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
    using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif

    [TestFixture]
    public class GlobalDiagnosticsContextTests
    {
        [Test]
        public void GDCTest1()
        {
            GlobalDiagnosticsContext.Clear();
            Assert.IsFalse(GlobalDiagnosticsContext.Contains("foo"));
            Assert.AreEqual(string.Empty, GlobalDiagnosticsContext.Get("foo"));
            Assert.IsFalse(GlobalDiagnosticsContext.Contains("foo2"));
            Assert.AreEqual(string.Empty, GlobalDiagnosticsContext.Get("foo2"));

            GlobalDiagnosticsContext.Set("foo", "bar");
            GlobalDiagnosticsContext.Set("foo2", "bar2");

            Assert.IsTrue(GlobalDiagnosticsContext.Contains("foo"));
            Assert.AreEqual("bar", GlobalDiagnosticsContext.Get("foo"));

            GlobalDiagnosticsContext.Remove("foo");
            Assert.IsFalse(GlobalDiagnosticsContext.Contains("foo"));
            Assert.AreEqual(string.Empty, GlobalDiagnosticsContext.Get("foo"));

            Assert.IsTrue(GlobalDiagnosticsContext.Contains("foo2"));
            Assert.AreEqual("bar2", GlobalDiagnosticsContext.Get("foo2"));
        }

        [Test]
        public void GDCTest2()
        {
            GDC.Clear();
            Assert.IsFalse(GDC.Contains("foo"));
            Assert.AreEqual(string.Empty, GDC.Get("foo"));
            Assert.IsFalse(GDC.Contains("foo2"));
            Assert.AreEqual(string.Empty, GDC.Get("foo2"));

            GDC.Set("foo", "bar");
            GDC.Set("foo2", "bar2");

            Assert.IsTrue(GDC.Contains("foo"));
            Assert.AreEqual("bar", GDC.Get("foo"));

            GDC.Remove("foo");
            Assert.IsFalse(GDC.Contains("foo"));
            Assert.AreEqual(string.Empty, GDC.Get("foo"));

            Assert.IsTrue(GDC.Contains("foo2"));
            Assert.AreEqual("bar2", GDC.Get("foo2"));
        }
    }
}
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

using System.Text;

#pragma warning disable 0618

namespace NLog.UnitTests.Contexts
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using NUnit.Framework;

#if !NUNIT
    using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
    using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
    using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
    using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif

    [TestFixture]
    public class MappedDiagnosticsContextTests
    {
        /// <summary>
        /// Same as <see cref="MappedDiagnosticsContext" />, but there is one <see cref="MappedDiagnosticsContext"/> per each thread.
        /// </summary>
        [Test]
        public void MDCTest1()
        {
            List<Exception> exceptions = new List<Exception>();
            ManualResetEvent mre = new ManualResetEvent(false);
            int counter = 100;
            int remaining = counter;

            for (int i = 0; i < counter; ++i)
            {
                ThreadPool.QueueUserWorkItem(
                    s =>
                        {
                            try
                            {
                                MappedDiagnosticsContext.Clear();
                                Assert.IsFalse(MappedDiagnosticsContext.Contains("foo"), "#1");
                                Assert.AreEqual(string.Empty, MappedDiagnosticsContext.Get("foo"), "#2");
                                Assert.IsFalse(MappedDiagnosticsContext.Contains("foo2"), "#3");
                                Assert.AreEqual(string.Empty, MappedDiagnosticsContext.Get("foo2"), "#4");

                                MappedDiagnosticsContext.Set("foo", "bar");
                                MappedDiagnosticsContext.Set("foo2", "bar2");

                                Assert.IsTrue(MappedDiagnosticsContext.Contains("foo"));
                                Assert.AreEqual("bar", MappedDiagnosticsContext.Get("foo"));

                                MappedDiagnosticsContext.Remove("foo");
                                Assert.IsFalse(MappedDiagnosticsContext.Contains("foo"));
                                Assert.AreEqual(string.Empty, MappedDiagnosticsContext.Get("foo"));

                                Assert.IsTrue(MappedDiagnosticsContext.Contains("foo2"));
                                Assert.AreEqual("bar2", MappedDiagnosticsContext.Get("foo2"));
                            }
                            catch (Exception exception)
                            {
                                lock (exceptions)
                                {
                                    exceptions.Add(exception);
                                }
                            }
                            finally
                            {
                                if (Interlocked.Decrement(ref remaining) == 0)
                                {
                                    mre.Set();
                                }
                            }
                        });
            }

            mre.WaitOne();
            StringBuilder exceptionsMessage = new StringBuilder();
            foreach (var ex in exceptions)
            {
                if (exceptionsMessage.Length > 0)
                {
                    exceptionsMessage.Append("\r\n");
                }

                exceptionsMessage.Append(ex.ToString());
            }

            Assert.AreEqual(0, exceptions.Count, exceptionsMessage.ToString());
        }

        [Test]
        public void MDCTest2()
        {
            List<Exception> exceptions = new List<Exception>();
            ManualResetEvent mre = new ManualResetEvent(false);
            int counter = 100;
            int remaining = counter;

            for (int i = 0; i < counter; ++i)
            {
                ThreadPool.QueueUserWorkItem(
                    s =>
                    {
                        try
                        {
                            MDC.Clear();
                            Assert.IsFalse(MDC.Contains("foo"));
                            Assert.AreEqual(string.Empty, MDC.Get("foo"));
                            Assert.IsFalse(MDC.Contains("foo2"));
                            Assert.AreEqual(string.Empty, MDC.Get("foo2"));

                            MDC.Set("foo", "bar");
                            MDC.Set("foo2", "bar2");

                            Assert.IsTrue(MDC.Contains("foo"));
                            Assert.AreEqual("bar", MDC.Get("foo"));

                            MDC.Remove("foo");
                            Assert.IsFalse(MDC.Contains("foo"));
                            Assert.AreEqual(string.Empty, MDC.Get("foo"));

                            Assert.IsTrue(MDC.Contains("foo2"));
                            Assert.AreEqual("bar2", MDC.Get("foo2"));
                        }
                        catch (Exception ex)
                        {
                            lock (exceptions)
                            {
                                exceptions.Add(ex);
                            }
                        }
                        finally
                        {
                            if (Interlocked.Decrement(ref remaining) == 0)
                            {
                                mre.Set();
                            }
                        }
                    });
            }

            mre.WaitOne();
            StringBuilder exceptionsMessage = new StringBuilder();
            foreach (var ex in exceptions)
            {
                if (exceptionsMessage.Length > 0)
                {
                    exceptionsMessage.Append("\r\n");
                }

                exceptionsMessage.Append(ex.ToString());
            }

            Assert.AreEqual(0, exceptions.Count, exceptionsMessage.ToString());
        }
    }
}
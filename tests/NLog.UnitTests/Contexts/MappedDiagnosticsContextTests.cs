// 
// Copyright (c) 2004-2010 Jaroslaw Kowalski <jaak@jkowalski.net>
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
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NLog.Contexts;

    [TestClass]
    public class MappedDiagnosticsContextTests
    {
        /// <summary>
        /// Same as <see cref="MappedDiagnosticsContext" />, but there is one <see cref="MappedDiagnosticsContext"/> per each thread.
        /// </summary>
        [TestMethod]
        public void MDCTest1()
        {
            List<Exception> exceptions = new List<Exception>();
            ManualResetEvent mre = new ManualResetEvent(false);
            int counter = 500;
            int remaining = counter;

            for (int i = 0; i < counter; ++i)
            {
                ThreadPool.QueueUserWorkItem(
                    s =>
                        {
                            try
                            {
                                MappedDiagnosticsContext.Clear();
                                Assert.IsFalse(MappedDiagnosticsContext.Contains("foo"));
                                Assert.AreEqual(string.Empty, MappedDiagnosticsContext.Get("foo"));
                                Assert.IsFalse(MappedDiagnosticsContext.Contains("foo2"));
                                Assert.AreEqual(string.Empty, MappedDiagnosticsContext.Get("foo2"));

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
            Assert.AreEqual(0, exceptions.Count);
        }

        [TestMethod]
        public void MDCTest2()
        {
            List<Exception> exceptions = new List<Exception>();
            ManualResetEvent mre = new ManualResetEvent(false);
            int counter = 500;
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
            Assert.AreEqual(0, exceptions.Count);
        }
    }
}
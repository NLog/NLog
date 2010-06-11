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

    [TestClass]
    public class NestedDiagnosticsContextTests
    {
        [TestMethod]
        public void NDCTest1()
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
                                NestedDiagnosticsContext.Clear();
                                Assert.AreEqual(string.Empty, NestedDiagnosticsContext.GetTopMessage());
                                Assert.AreEqual(string.Empty, NestedDiagnosticsContext.Pop());
                                AssertContents(NestedDiagnosticsContext.GetAllMessages());
                                using (NestedDiagnosticsContext.Push("foo"))
                                {
                                    Assert.AreEqual("foo", NestedDiagnosticsContext.GetTopMessage());
                                    AssertContents(NestedDiagnosticsContext.GetAllMessages(), "foo");
                                    using (NestedDiagnosticsContext.Push("bar"))
                                    {
                                        AssertContents(NestedDiagnosticsContext.GetAllMessages(), "bar", "foo");
                                        Assert.AreEqual("bar", NestedDiagnosticsContext.GetTopMessage());
                                        NestedDiagnosticsContext.Push("baz");
                                        AssertContents(NestedDiagnosticsContext.GetAllMessages(), "baz", "bar", "foo");
                                        Assert.AreEqual("baz", NestedDiagnosticsContext.GetTopMessage());
                                        Assert.AreEqual("baz", NestedDiagnosticsContext.Pop());

                                        AssertContents(NestedDiagnosticsContext.GetAllMessages(), "bar", "foo");
                                        Assert.AreEqual("bar", NestedDiagnosticsContext.GetTopMessage());
                                    }

                                    AssertContents(NestedDiagnosticsContext.GetAllMessages(), "foo");
                                    Assert.AreEqual("foo", NestedDiagnosticsContext.GetTopMessage());
                                }

                                AssertContents(NestedDiagnosticsContext.GetAllMessages());
                                Assert.AreEqual(string.Empty, NestedDiagnosticsContext.Pop());
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
        public void NDCTest2()
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
                            NDC.Clear();
                            Assert.AreEqual(string.Empty, NDC.GetTopMessage());
                            Assert.AreEqual(string.Empty, NDC.Pop());
                            AssertContents(NDC.GetAllMessages());
                            using (NDC.Push("foo"))
                            {
                                Assert.AreEqual("foo", NDC.GetTopMessage());
                                AssertContents(NDC.GetAllMessages(), "foo");
                                using (NDC.Push("bar"))
                                {
                                    AssertContents(NDC.GetAllMessages(), "bar", "foo");
                                    Assert.AreEqual("bar", NDC.GetTopMessage());
                                    NDC.Push("baz");
                                    AssertContents(NDC.GetAllMessages(), "baz", "bar", "foo");
                                    Assert.AreEqual("baz", NDC.GetTopMessage());
                                    Assert.AreEqual("baz", NDC.Pop());

                                    AssertContents(NDC.GetAllMessages(), "bar", "foo");
                                    Assert.AreEqual("bar", NDC.GetTopMessage());
                                }

                                AssertContents(NDC.GetAllMessages(), "foo");
                                Assert.AreEqual("foo", NDC.GetTopMessage());
                            }

                            AssertContents(NDC.GetAllMessages());
                            Assert.AreEqual(string.Empty, NDC.Pop());
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

        private static void AssertContents(string[] actual, params string[] expected)
        {
            Assert.AreEqual(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; ++i)
            {
                Assert.AreEqual(expected[i], actual[i]);
            }
        }
    }
}
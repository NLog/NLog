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

namespace NLog.UnitTests.Contexts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using Xunit;

    [Obsolete("Replaced by ScopeContext.PushNestedState or Logger.PushScopeState using ${scopenested}. Marked obsolete on NLog 5.0")]
    public class NestedDiagnosticsContextTests
    {
        [Fact]
        public void NDCTest1()
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
                                NestedDiagnosticsContext.Clear();
                                Assert.Equal(string.Empty, NestedDiagnosticsContext.TopMessage);
                                Assert.Equal(string.Empty, NestedDiagnosticsContext.Pop());
                                AssertContents(NestedDiagnosticsContext.GetAllMessages());
                                using (NestedDiagnosticsContext.Push("foo"))
                                {
                                    Assert.Equal("foo", NestedDiagnosticsContext.TopMessage);
                                    AssertContents(NestedDiagnosticsContext.GetAllMessages(), "foo");
                                    using (NestedDiagnosticsContext.Push("bar"))
                                    {
                                        AssertContents(NestedDiagnosticsContext.GetAllMessages(), "bar", "foo");
                                        Assert.Equal("bar", NestedDiagnosticsContext.TopMessage);
                                        NestedDiagnosticsContext.Push("baz");
                                        AssertContents(NestedDiagnosticsContext.GetAllMessages(), "baz", "bar", "foo");
                                        Assert.Equal("baz", NestedDiagnosticsContext.TopMessage);
                                        Assert.Equal("baz", NestedDiagnosticsContext.Pop());

                                        AssertContents(NestedDiagnosticsContext.GetAllMessages(), "bar", "foo");
                                        Assert.Equal("bar", NestedDiagnosticsContext.TopMessage);
                                    }

                                    AssertContents(NestedDiagnosticsContext.GetAllMessages(), "foo");
                                    Assert.Equal("foo", NestedDiagnosticsContext.TopMessage);
                                }

                                AssertContents(NestedDiagnosticsContext.GetAllMessages());
                                Assert.Equal(string.Empty, NestedDiagnosticsContext.Pop());
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

            Assert.True(exceptions.Count == 0, exceptionsMessage.ToString());
        }

        [Fact]
        public void NDCTest2_object()
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
                            NestedDiagnosticsContext.Clear();
                            Assert.Null(NestedDiagnosticsContext.TopObject);
                            Assert.Null(NestedDiagnosticsContext.PopObject());
                            AssertContents(NestedDiagnosticsContext.GetAllMessages());
                            using (NestedDiagnosticsContext.Push("foo"))
                            {
                                Assert.Equal("foo", NestedDiagnosticsContext.TopObject);
                                AssertContents(NestedDiagnosticsContext.GetAllObjects(), "foo");
                                using (NestedDiagnosticsContext.Push("bar"))
                                {
                                    AssertContents(NestedDiagnosticsContext.GetAllObjects(), "bar", "foo");
                                    Assert.Equal("bar", NestedDiagnosticsContext.TopObject);
                                    NestedDiagnosticsContext.Push("baz");
                                    AssertContents(NestedDiagnosticsContext.GetAllObjects(), "baz", "bar", "foo");
                                    Assert.Equal("baz", NestedDiagnosticsContext.TopObject);
                                    Assert.Equal("baz", NestedDiagnosticsContext.PopObject());

                                    AssertContents(NestedDiagnosticsContext.GetAllObjects(), "bar", "foo");
                                    Assert.Equal("bar", NestedDiagnosticsContext.TopObject);
                                }

                                AssertContents(NestedDiagnosticsContext.GetAllObjects(), "foo");
                                Assert.Equal("foo", NestedDiagnosticsContext.TopObject);
                            }

                            AssertContents(NestedDiagnosticsContext.GetAllMessages());
                            Assert.Null(NestedDiagnosticsContext.PopObject());
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

            Assert.True(exceptions.Count == 0, exceptionsMessage.ToString());
        }
        private static void AssertContents(object[] actual, params string[] expected)
        {
            Assert.Equal(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; ++i)
            {
                Assert.Equal(expected[i], actual[i]);
            }
        }

        private static void AssertContents(string[] actual, params string[] expected)
        {
            Assert.Equal(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; ++i)
            {
                Assert.Equal(expected[i], actual[i]);
            }
        }
    }
}
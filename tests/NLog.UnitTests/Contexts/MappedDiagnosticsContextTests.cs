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
    using System.Text;
    using System.Threading;
    using Xunit;

    [Obsolete("Replaced by ScopeContext.PushProperty or Logger.PushScopeProperty using ${scopeproperty}. Marked obsolete on NLog 5.0")]
    public class MappedDiagnosticsContextTests
    {
        /// <summary>
        /// Same as <see cref="MappedDiagnosticsContext" />, but there is one <see cref="MappedDiagnosticsContext"/> per each thread.
        /// </summary>
        [Fact]
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
                                Assert.False(MappedDiagnosticsContext.Contains("foo"));
                                Assert.Equal(string.Empty, MappedDiagnosticsContext.Get("foo"));
                                Assert.False(MappedDiagnosticsContext.Contains("foo2"));
                                Assert.Equal(string.Empty, MappedDiagnosticsContext.Get("foo2"));
                                Assert.Equal(0, MappedDiagnosticsContext.GetNames().Count);

                                MappedDiagnosticsContext.Set("foo", "bar");
                                MappedDiagnosticsContext.Set("foo2", "bar2");

                                Assert.True(MappedDiagnosticsContext.Contains("foo"));
                                Assert.Equal("bar", MappedDiagnosticsContext.Get("foo"));
                                Assert.Equal(2, MappedDiagnosticsContext.GetNames().Count);

                                MappedDiagnosticsContext.Remove("foo");
                                Assert.False(MappedDiagnosticsContext.Contains("foo"));
                                Assert.Equal(string.Empty, MappedDiagnosticsContext.Get("foo"));

                                Assert.True(MappedDiagnosticsContext.Contains("foo2"));
                                Assert.Equal("bar2", MappedDiagnosticsContext.Get("foo2"));

                                Assert.Equal(1, MappedDiagnosticsContext.GetNames().Count);
                                Assert.True(MappedDiagnosticsContext.GetNames().Contains("foo2"));

                                Assert.Null(MappedDiagnosticsContext.GetObject("foo3"));
                                MappedDiagnosticsContext.Set("foo3", new { One = 1 });
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

            Assert.True(exceptions.Count == 0, exceptionsMessage.ToString());
        }

        [Fact]
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
                            MappedDiagnosticsContext.Clear();
                            Assert.False(MappedDiagnosticsContext.Contains("foo"));
                            Assert.Equal(string.Empty, MappedDiagnosticsContext.Get("foo"));
                            Assert.False(MappedDiagnosticsContext.Contains("foo2"));
                            Assert.Equal(string.Empty, MappedDiagnosticsContext.Get("foo2"));

                            MappedDiagnosticsContext.Set("foo", "bar");
                            MappedDiagnosticsContext.Set("foo2", "bar2");

                            Assert.True(MappedDiagnosticsContext.Contains("foo"));
                            Assert.Equal("bar", MappedDiagnosticsContext.Get("foo"));

                            MappedDiagnosticsContext.Remove("foo");
                            Assert.False(MappedDiagnosticsContext.Contains("foo"));
                            Assert.Equal(string.Empty, MappedDiagnosticsContext.Get("foo"));

                            Assert.True(MappedDiagnosticsContext.Contains("foo2"));
                            Assert.Equal("bar2", MappedDiagnosticsContext.Get("foo2"));

                            Assert.Null(MappedDiagnosticsContext.GetObject("foo3"));
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
        public void timer_cannot_inherit_mappedcontext()
        {
            object getObject = null;
            string getValue = null;

            var mre = new ManualResetEvent(false);
            Timer thread = new Timer((s) =>
            {
                try
                {
                    getObject = MappedDiagnosticsContext.GetObject("DoNotExist");
                    getValue = MappedDiagnosticsContext.Get("DoNotExistEither");
                }
                finally
                {
                    mre.Set();
                }
            });
            thread.Change(0, Timeout.Infinite);
            mre.WaitOne();
            Assert.Null(getObject);
            Assert.Empty(getValue);
        }

        [Fact]
        public void disposable_removes_item()
        {
            const string itemNotRemovedKey = "itemNotRemovedKey";
            const string itemRemovedKey = "itemRemovedKey";

            MappedDiagnosticsContext.Clear();
            MappedDiagnosticsContext.Set(itemNotRemovedKey, "itemNotRemoved");
            using (MappedDiagnosticsContext.SetScoped(itemRemovedKey, "itemRemoved"))
            {
                Assert.Equal(MappedDiagnosticsContext.GetNames(), new[] { itemNotRemovedKey, itemRemovedKey });
            }

            Assert.Equal(MappedDiagnosticsContext.GetNames(), new[] { itemNotRemovedKey });
        }

        [Fact]
        public void dispose_is_idempotent()
        {
            const string itemKey = "itemKey";

            MappedDiagnosticsContext.Clear();            
            IDisposable disposable = MappedDiagnosticsContext.SetScoped(itemKey, "item1");

            disposable.Dispose();
            Assert.False(MappedDiagnosticsContext.Contains(itemKey));

            //This item shouldn't be removed since it is not the disposable one
            MappedDiagnosticsContext.Set(itemKey, "item2");
            disposable.Dispose();

            Assert.True(MappedDiagnosticsContext.Contains(itemKey));
        }
    }
}
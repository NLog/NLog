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
    using NLog.Contexts;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using Xunit;

    public class ThreadContextTests
    {
        /// <summary>
        /// Same as <see cref="MappedDiagnosticsContext" />, but there is one <see cref="MappedDiagnosticsContext"/> per each thread.
        /// </summary>
        [Fact]
        public void ThreadContextTest1()
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
        public void ThreadContextTest2()
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
                            Assert.False(MDC.Contains("foo"));
                            Assert.Equal(string.Empty, MDC.Get("foo"));
                            Assert.False(MDC.Contains("foo2"));
                            Assert.Equal(string.Empty, MDC.Get("foo2"));

                            MDC.Set("foo", "bar");
                            MDC.Set("foo2", "bar2");

                            Assert.True(MDC.Contains("foo"));
                            Assert.Equal("bar", MDC.Get("foo"));

                            MDC.Remove("foo");
                            Assert.False(MDC.Contains("foo"));
                            Assert.Equal(string.Empty, MDC.Get("foo"));

                            Assert.True(MDC.Contains("foo2"));
                            Assert.Equal("bar2", MDC.Get("foo2"));
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
        public void ThreadContextTest3()
        {
            List<Exception> exceptions = new List<Exception>();
            ManualResetEvent mre = new ManualResetEvent(false);
            int counter = 1;
            int remaining = counter;

            for (int i = 0; i < counter; ++i)
            {
                ThreadPool.QueueUserWorkItem(
                    s =>
                    {
                        try
                        {
                            ThreadContext.Instance.Clear();
                            Assert.False(ThreadContext.Instance.Contains("foo"));
                            Assert.Null(ThreadContext.Instance["foo"]);
                            Assert.False(ThreadContext.Instance.Contains("foo2"));
                            Assert.Null(ThreadContext.Instance["foo2"]);

                            ThreadContext.Instance["foo"] = "bar";
                            Assert.True(ThreadContext.Instance.Contains("foo"));
                            Assert.Equal("bar", ThreadContext.Instance["foo"]);

                            ThreadContext.Instance.Remove("foo");
                            Assert.False(ThreadContext.Instance.Contains("foo"));
                            Assert.Null(ThreadContext.Instance["foo"]);

                            ThreadContext.Instance["foo2"] = "bar2";
                            Assert.True(ThreadContext.Instance.Contains("foo2"));
                            Assert.Equal("bar2", ThreadContext.Instance["foo2"]);

                            ThreadContext.Instance.Clear();
                            ThreadContext.Instance["foo1"] = "Test1";
                            ThreadContext.Instance["foo2"] = "Test2";
                            ThreadContext.Instance["foo3"] = "Test3";

                            var count = 0;
                            foreach (var item in ThreadContext.Instance)
                                count++;

                            Assert.Equal(3, count);

                            ThreadContext.Instance.Clear();
                            Assert.Equal(0, ThreadContext.Instance.Count);
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
    }
}

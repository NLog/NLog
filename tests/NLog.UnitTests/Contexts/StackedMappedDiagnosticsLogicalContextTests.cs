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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLog.UnitTests.Contexts
{
    public class StackedMappedDiagnosticsLogicalContextTests
    {
        // Set resets stack, SetScope stacks values

        public StackedMappedDiagnosticsLogicalContextTests()
        {
            MappedDiagnosticsLogicalContext.Clear();
            MappedDiagnosticsLogicalContext.UseStackedStore = true;

            // Tests in this class are only completing tests from class MappedDiagnosticsLogicalContextTests,
            // so they should all work like before providing MappedDiagnosticsLogicalContext.UseStackedStore = false
            // MappedDiagnosticsLogicalContextTests tests also may all work when MappedDiagnosticsLogicalContext.UseStackedStore = true
            // providing they are not testing lack of stacking the values (which is not the case I right now)
        }

        [Fact]
        public void given_multiple_set_invocations_mdlc_persists_only_last_value()
        {
            const string key = "key";

            MappedDiagnosticsLogicalContext.Set(key, "1");

            Assert.Equal("1", MappedDiagnosticsLogicalContext.Get(key));

            MappedDiagnosticsLogicalContext.Set(key, 2);
            MappedDiagnosticsLogicalContext.Set(key, "3");

            Assert.Equal("3", MappedDiagnosticsLogicalContext.Get(key));

            MappedDiagnosticsLogicalContext.Remove(key);

            Assert.True(string.IsNullOrEmpty(MappedDiagnosticsLogicalContext.Get(key)));
        }

        [Fact]
        public void given_multiple_setscoped_invocations_mdlc_persists_all_values()
        {
            const string key = "key";

            using (MappedDiagnosticsLogicalContext.SetScoped(key, "1"))
            {
                Assert.Equal("1", MappedDiagnosticsLogicalContext.Get(key));

                using (MappedDiagnosticsLogicalContext.SetScoped(key, 2))
                {
                    Assert.Equal(2.ToString(), MappedDiagnosticsLogicalContext.Get(key));

                    using (MappedDiagnosticsLogicalContext.SetScoped(key, 3))
                    {
                        Assert.Equal(3.ToString(), MappedDiagnosticsLogicalContext.Get(key));
                    }

                    Assert.Equal(2.ToString(), MappedDiagnosticsLogicalContext.Get(key));
                }

                Assert.Equal("1", MappedDiagnosticsLogicalContext.Get(key));
            }

            Assert.True(string.IsNullOrEmpty(MappedDiagnosticsLogicalContext.Get(key)));
        }

        [Fact]
        public void given_multiple_multikey_setscoped_invocations_mdlc_persists_all_values()
        {
            const string key1 = "key1";
            const string key2 = "key2";
            const string key3 = "key3";

            using (MappedDiagnosticsLogicalContext.SetScoped(key1, "1"))
            {
                Assert.Equal("1", MappedDiagnosticsLogicalContext.Get(key1));

                using (MappedDiagnosticsLogicalContext.SetScoped(key2, 2))
                {
                    using (MappedDiagnosticsLogicalContext.SetScoped(key3, 3))
                    {
                        using (MappedDiagnosticsLogicalContext.SetScoped(key2, 22))
                        {
                            Assert.Equal(22.ToString(), MappedDiagnosticsLogicalContext.Get(key2));
                        }

                        Assert.Equal("1", MappedDiagnosticsLogicalContext.Get(key1));
                        Assert.Equal(2.ToString(), MappedDiagnosticsLogicalContext.Get(key2));
                        Assert.Equal(3.ToString(), MappedDiagnosticsLogicalContext.Get(key3));
                    }
                }

                Assert.Equal("1", MappedDiagnosticsLogicalContext.Get(key1));
            }

            Assert.True(string.IsNullOrEmpty(MappedDiagnosticsLogicalContext.Get(key1)));
        }

        [Fact]
        public void given_multiple_multikey_setscoped_invocations_mdlc_persists_all_values_2()
        {
            const string key1 = "key1";
            const string key2 = "key2";
            const string key3 = "key3";

            MappedDiagnosticsLogicalContext.SetScoped(key1, "1");

            Assert.Equal("1", MappedDiagnosticsLogicalContext.Get(key1));

            MappedDiagnosticsLogicalContext.SetScoped(key2, 2);

            MappedDiagnosticsLogicalContext.SetScoped(key3, 3);

            MappedDiagnosticsLogicalContext.SetScoped(key2, 22);

            Assert.Equal(22.ToString(), MappedDiagnosticsLogicalContext.Get(key2));

            MappedDiagnosticsLogicalContext.Remove(key2);

            Assert.Equal("1", MappedDiagnosticsLogicalContext.Get(key1));
            Assert.Equal(2.ToString(), MappedDiagnosticsLogicalContext.Get(key2));
            Assert.Equal(3.ToString(), MappedDiagnosticsLogicalContext.Get(key3));

            MappedDiagnosticsLogicalContext.Remove(key3);

            MappedDiagnosticsLogicalContext.Remove(key2);

            Assert.Equal("1", MappedDiagnosticsLogicalContext.Get(key1));

            MappedDiagnosticsLogicalContext.Remove(key1);

            Assert.True(string.IsNullOrEmpty(MappedDiagnosticsLogicalContext.Get(key1)));
        }

        [Fact]
        public void given_multiple_setscoped_invocations_set_reset_value_stack()
        {
            const string key = "key";

            using (MappedDiagnosticsLogicalContext.SetScoped(key, "1"))
            {
                using (MappedDiagnosticsLogicalContext.SetScoped(key, 2))
                {
                    using (MappedDiagnosticsLogicalContext.SetScoped(key, 3))
                    {
                        Assert.Equal(3.ToString(), MappedDiagnosticsLogicalContext.Get(key));
                    }

                    // 'Set' resets the stack to contain only one value, so disposing same key will leave the stack empty prematurely
                    // So the strategy should be to use either 'Set' or 'SetScoped' to avoid such situation
                    MappedDiagnosticsLogicalContext.Set(key, "x");

                    Assert.Equal("x", MappedDiagnosticsLogicalContext.Get(key));
                }

                Assert.True(string.IsNullOrEmpty(MappedDiagnosticsLogicalContext.Get(key)));
            }

            Assert.True(string.IsNullOrEmpty(MappedDiagnosticsLogicalContext.Get(key)));
        }

        [Fact]
        public void given_multiple_threads_running_asynchronously_when_setting_and_getting_values_should_return_thread_specific_values()
        {
            const string key = "Key";
            const string initValue = "InitValue";
            const string valueForLogicalThread1 = "ValueForTask1";
            const string valueForLogicalThread1Next = "ValueForTask1Next";
            const string valueForLogicalThread2 = "ValueForTask2";
            const string valueForLogicalThread3 = "ValueForTask3";

            MappedDiagnosticsLogicalContext.Clear(true);
            MappedDiagnosticsLogicalContext.UseStackedStore = true;

            MappedDiagnosticsLogicalContext.Set(key, initValue);
            Assert.Equal(initValue, MappedDiagnosticsLogicalContext.Get(key));

            var task1 = Task.Factory.StartNew(() => {
                MappedDiagnosticsLogicalContext.SetScoped(key, valueForLogicalThread1);
                Task.Delay(200).Wait();
                MappedDiagnosticsLogicalContext.SetScoped(key, valueForLogicalThread1Next);
                return MappedDiagnosticsLogicalContext.Get(key);
            });

            var task2 = Task.Factory.StartNew(() => {
                MappedDiagnosticsLogicalContext.SetScoped(key, valueForLogicalThread2);
                return MappedDiagnosticsLogicalContext.Get(key);
            });

            var task3 = Task.Factory.StartNew(() => {
                MappedDiagnosticsLogicalContext.SetScoped(key, valueForLogicalThread3);
                return MappedDiagnosticsLogicalContext.Get(key);
            });

            Task.WaitAll();

            Assert.Equal(task1.Result, valueForLogicalThread1Next);
            Assert.Equal(task2.Result, valueForLogicalThread2);
            Assert.Equal(task3.Result, valueForLogicalThread3);
        }
    }
}

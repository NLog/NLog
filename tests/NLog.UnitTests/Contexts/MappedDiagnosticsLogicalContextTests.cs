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

using System.Linq;
using System.Threading;

namespace NLog.UnitTests.Contexts
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;

    public class MappedDiagnosticsLogicalContextTests
    {
        public MappedDiagnosticsLogicalContextTests()
        {
            MappedDiagnosticsLogicalContext.Clear();
        }

        [Fact]
        public void given_item_exists_when_getting_item_should_return_item_for_objecttype_2()
        {
            string key = "testKey1";
            object value = 5;

            MappedDiagnosticsLogicalContext.Set(key, value);

            string expected = "5";
            string actual = MappedDiagnosticsLogicalContext.Get(key);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void given_item_exists_when_getting_item_should_return_item_for_objecttype()
        {
            string key = "testKey2";
            object value = DateTime.Now;

            MappedDiagnosticsLogicalContext.Set(key, value);

            object actual = MappedDiagnosticsLogicalContext.GetObject(key);
            Assert.Equal(value, actual);
        }

        [Fact]
        public void given_no_item_exists_when_getting_item_should_return_null()
        {
            Assert.Null(MappedDiagnosticsLogicalContext.GetObject("itemThatShouldNotExist"));
        }

        [Fact]
        public void given_no_item_exists_when_getting_item_should_return_empty_string()
        {
            Assert.Empty(MappedDiagnosticsLogicalContext.Get("itemThatShouldNotExist"));
        }

        [Fact]
        public void given_item_exists_when_getting_item_should_return_item()
        {
            const string key = "Key";
            const string item = "Item";
            MappedDiagnosticsLogicalContext.Set(key, item);

            Assert.Equal(item, MappedDiagnosticsLogicalContext.Get(key));
        }

        [Fact]
        public void given_item_does_not_exist_when_setting_item_should_contain_item()
        {
            const string key = "Key";
            const string item = "Item";

            MappedDiagnosticsLogicalContext.Set(key, item);

            Assert.True(MappedDiagnosticsLogicalContext.Contains(key));
        }

        [Fact]
        public void given_item_exists_when_setting_item_should_not_throw()
        {
            const string key = "Key";
            const string item = "Item";
            MappedDiagnosticsLogicalContext.Set(key, item);

            var exRecorded = Record.Exception(() => MappedDiagnosticsLogicalContext.Set(key, item));
            Assert.Null(exRecorded);
        }

        [Fact]
        public void given_item_exists_when_setting_item_should_update_item()
        {
            const string key = "Key";
            const string item = "Item";
            const string newItem = "NewItem";
            MappedDiagnosticsLogicalContext.Set(key, item);

            MappedDiagnosticsLogicalContext.Set(key, newItem);

            Assert.Equal(newItem, MappedDiagnosticsLogicalContext.Get(key));
        }

        [Fact]
        public void given_no_item_exists_when_getting_items_should_return_empty_collection()
        {
            Assert.Equal(0,MappedDiagnosticsLogicalContext.GetNames().Count);
        }

        [Fact]
        public void given_item_exists_when_getting_items_should_return_that_item()
        {
            const string key = "Key";
            MappedDiagnosticsLogicalContext.Set(key, "Item");

            Assert.Equal(1, MappedDiagnosticsLogicalContext.GetNames().Count);
            Assert.True(MappedDiagnosticsLogicalContext.GetNames().Contains("Key"));

        }

        [Fact]
        public void given_item_exists_after_removing_item_when_getting_items_should_not_contain_item()
        {
            const string keyThatRemains1 = "Key1";
            const string keyThatRemains2 = "Key2";
            const string keyThatIsRemoved = "KeyR";

            MappedDiagnosticsLogicalContext.Set(keyThatRemains1, "7");
            MappedDiagnosticsLogicalContext.Set(keyThatIsRemoved, 7);
            MappedDiagnosticsLogicalContext.Set(keyThatRemains2, 8);

            MappedDiagnosticsLogicalContext.Remove(keyThatIsRemoved);

            Assert.Equal(2, MappedDiagnosticsLogicalContext.GetNames().Count);
            Assert.False(MappedDiagnosticsLogicalContext.GetNames().Contains(keyThatIsRemoved));
        }

        [Fact]
        public void given_item_does_not_exist_when_checking_if_context_contains_should_return_false()
        {
            Assert.False(MappedDiagnosticsLogicalContext.Contains("keyForItemThatDoesNotExist"));
        }

        [Fact]
        public void given_item_exists_when_checking_if_context_contains_should_return_true()
        {
            const string key = "Key";
            MappedDiagnosticsLogicalContext.Set(key, "Item");

            Assert.True(MappedDiagnosticsLogicalContext.Contains(key));

        }

        [Fact]
        public void given_item_exists_when_removing_item_should_not_contain_item()
        {
            const string keyForItemThatShouldExist = "Key";
            const string itemThatShouldExist = "Item";
            MappedDiagnosticsLogicalContext.Set(keyForItemThatShouldExist, itemThatShouldExist);

            MappedDiagnosticsLogicalContext.Remove(keyForItemThatShouldExist);

            Assert.False(MappedDiagnosticsLogicalContext.Contains(keyForItemThatShouldExist));
        }

        [Fact]
        public void given_item_does_not_exist_when_removing_item_should_not_throw()
        {
            const string keyForItemThatShouldExist = "Key";

            var exRecorded = Record.Exception(() => MappedDiagnosticsLogicalContext.Remove(keyForItemThatShouldExist));
            Assert.Null(exRecorded);
        }

        [Fact]
        public void given_item_does_not_exist_when_clearing_should_not_throw()
        {
            var exRecorded = Record.Exception(() => MappedDiagnosticsLogicalContext.Clear());
            Assert.Null(exRecorded);
        }

        [Fact]
        public void given_item_exists_when_clearing_should_not_contain_item()
        {
            const string key = "Key";
            MappedDiagnosticsLogicalContext.Set(key, "Item");

            MappedDiagnosticsLogicalContext.Clear();

            Assert.False(MappedDiagnosticsLogicalContext.Contains(key));
        }

        [Fact]
        public void given_multiple_threads_running_asynchronously_when_setting_and_getting_values_should_return_thread_specific_values()
        {
            const string key = "Key";
            const string valueForLogicalThread1 = "ValueForTask1";
            const string valueForLogicalThread2 = "ValueForTask2";
            const string valueForLogicalThread3 = "ValueForTask3";


            MappedDiagnosticsLogicalContext.Clear(true);

            var task1 = Task.Factory.StartNew(() => {
                MappedDiagnosticsLogicalContext.Set(key, valueForLogicalThread1);
                return MappedDiagnosticsLogicalContext.Get(key);
            });

            var task2 = Task.Factory.StartNew(() => {
                MappedDiagnosticsLogicalContext.Set(key, valueForLogicalThread2);
                return MappedDiagnosticsLogicalContext.Get(key);
            });

            var task3 = Task.Factory.StartNew(() => {
                MappedDiagnosticsLogicalContext.Set(key, valueForLogicalThread3);
                return MappedDiagnosticsLogicalContext.Get(key);
            });

            Task.WaitAll();

            Assert.Equal(task1.Result, valueForLogicalThread1);
            Assert.Equal(task2.Result, valueForLogicalThread2);
            Assert.Equal(task3.Result, valueForLogicalThread3);
        }

        [Fact]
        public void parent_thread_assigns_different_values_to_childs()
        {
            const string parentKey = "ParentKey";
            const string parentValueForLogicalThread1 = "Parent1";
            const string parentValueForLogicalThread2 = "Parent2";

            const string childKey = "ChildKey";
            const string valueForChildThread1 = "Child1";
            const string valueForChildThread2 = "Child2";

            MappedDiagnosticsLogicalContext.Clear(true);
            var exitAllTasks = new ManualResetEvent(false);

            MappedDiagnosticsLogicalContext.Set(parentKey, parentValueForLogicalThread1);

            var task1 = Task.Factory.StartNew(() =>
            {
                MappedDiagnosticsLogicalContext.Set(childKey, valueForChildThread1);
                exitAllTasks.WaitOne();
                return MappedDiagnosticsLogicalContext.Get(parentKey) + "," + MappedDiagnosticsLogicalContext.Get(childKey);
            });

            MappedDiagnosticsLogicalContext.Set(parentKey, parentValueForLogicalThread2);

            var task2 = Task.Factory.StartNew(() =>
            {
                MappedDiagnosticsLogicalContext.Set(childKey, valueForChildThread2);
                exitAllTasks.WaitOne();
                return MappedDiagnosticsLogicalContext.Get(parentKey) + "," + MappedDiagnosticsLogicalContext.Get(childKey);
            });

            exitAllTasks.Set();
            Task.WaitAll();

            Assert.Equal(task1.Result, parentValueForLogicalThread1 + "," + valueForChildThread1);
            Assert.Equal(task2.Result, parentValueForLogicalThread2 + "," + valueForChildThread2);
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
                    getObject = MappedDiagnosticsLogicalContext.GetObject("DoNotExist");
                    getValue = MappedDiagnosticsLogicalContext.Get("DoNotExistEither");
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

            MappedDiagnosticsLogicalContext.Clear();
            MappedDiagnosticsLogicalContext.Set(itemNotRemovedKey, "itemNotRemoved");
            using (MappedDiagnosticsLogicalContext.SetScoped(itemRemovedKey, "itemRemoved"))
            {
                Assert.Equal(MappedDiagnosticsLogicalContext.GetNames(), new[] { itemNotRemovedKey, itemRemovedKey });
            }

            Assert.Equal(MappedDiagnosticsLogicalContext.GetNames(), new[] { itemNotRemovedKey });
        }

        [Fact]
        public void dispose_is_idempotent()
        {
            const string itemKey = "itemKey";

            MappedDiagnosticsLogicalContext.Clear();
            IDisposable disposable = MappedDiagnosticsLogicalContext.SetScoped(itemKey, "item1");

            disposable.Dispose();
            Assert.False(MappedDiagnosticsLogicalContext.Contains(itemKey));

            //This item shouldn't be removed since it is not the disposable one
            MappedDiagnosticsLogicalContext.Set(itemKey, "item2");
            disposable.Dispose();

            Assert.True(MappedDiagnosticsLogicalContext.Contains(itemKey));
        }

#if NET4_5
        [Fact]
        public void disposable_multiple_items()
        {
            const string itemNotRemovedKey = "itemNotRemovedKey";
            const string item1Key = "item1Key";
            const string item2Key = "item2Key";
            const string item3Key = "item3Key";
            const string item4Key = "item4Key";

            MappedDiagnosticsLogicalContext.Clear();
            MappedDiagnosticsLogicalContext.Set(itemNotRemovedKey, "itemNotRemoved");
            using (MappedDiagnosticsLogicalContext.SetScoped(new[] { new KeyValuePair<string, object>(item1Key, "1"), new KeyValuePair<string, object>(item2Key, "2") }))
            {
                Assert.Equal(MappedDiagnosticsLogicalContext.GetNames(), new[] { itemNotRemovedKey, item1Key, item2Key });
            }

            Assert.Equal(MappedDiagnosticsLogicalContext.GetNames(), new[] { itemNotRemovedKey });

            using (MappedDiagnosticsLogicalContext.SetScoped(new[] { new KeyValuePair<string, object>(item1Key, "1"), new KeyValuePair<string, object>(item2Key, "2"), new KeyValuePair<string, object>(item3Key, "3"), new KeyValuePair<string, object>(item4Key, "4") }))
            {
                Assert.Equal(MappedDiagnosticsLogicalContext.GetNames(), new[] { itemNotRemovedKey, item1Key, item2Key, item3Key, item4Key });
            }

            Assert.Equal(MappedDiagnosticsLogicalContext.GetNames(), new[] { itemNotRemovedKey });
        }

        [Fact]
        public void disposable_fast_clear_multiple_items()
        {
            const string item1Key = "item1Key";
            const string item2Key = "item2Key";
            const string item3Key = "item3Key";
            const string item4Key = "item4Key";

            MappedDiagnosticsLogicalContext.Clear();
            using (MappedDiagnosticsLogicalContext.SetScoped(new[] { new KeyValuePair<string, object>(item1Key, "1"), new KeyValuePair<string, object>(item2Key, "2") }))
            {
                Assert.Equal(MappedDiagnosticsLogicalContext.GetNames(), new[] {  item1Key, item2Key });
            }

            Assert.Equal(MappedDiagnosticsLogicalContext.GetNames(), new string[] { });

            using (MappedDiagnosticsLogicalContext.SetScoped(new[] { new KeyValuePair<string, object>(item1Key, "1"), new KeyValuePair<string, object>(item2Key, "2"), new KeyValuePair<string, object>(item3Key, "3"), new KeyValuePair<string, object>(item4Key, "4") }))
            {
                Assert.Equal(MappedDiagnosticsLogicalContext.GetNames(), new[] { item1Key, item2Key, item3Key, item4Key });
            }

            Assert.Equal(MappedDiagnosticsLogicalContext.GetNames(), new string[] { });
        }
#endif
    }
}
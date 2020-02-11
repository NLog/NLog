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
using System.Linq;
using System.Collections.Generic;
using NLog.Internal;
using NLog.MessageTemplates;
using Xunit;

namespace NLog.UnitTests.Internal
{
    public class PropertiesDictionaryTests : NLogTestBase
    {
        [Fact]
        public void DefaultPropertiesDictionary()
        {
            LogEventInfo logEvent = new LogEventInfo();
            IDictionary<object, object> dictionary = logEvent.Properties;
            Assert.Empty(dictionary);
            foreach (var item in dictionary)
                Assert.False(true, "Should be empty");
            foreach (var item in dictionary.Keys)
                Assert.False(true, "Should be empty");
            foreach (var item in dictionary.Values)
                Assert.False(true, "Should be empty");
            Assert.DoesNotContain("Hello World", dictionary);
            Assert.False(dictionary.ContainsKey("Hello World"));
            Assert.False(dictionary.Keys.Contains("Hello World"));
            Assert.False(dictionary.Values.Contains(42));
            object value;
            Assert.False(dictionary.TryGetValue("Hello World", out value));
            Assert.Null(value);
            Assert.False(dictionary.Remove("Hello World"));
            dictionary.CopyTo(new KeyValuePair<object, object>[0], 0);
            dictionary.Values.CopyTo(new object[0], 0);
            dictionary.Keys.CopyTo(new object[0], 0);
            dictionary.Clear();
        }

        [Fact]
        public void EmptyEventPropertiesDictionary()
        {
            LogEventInfo logEvent = new LogEventInfo();
            IDictionary<object, object> dictionary = logEvent.Properties;
            dictionary.Add("Hello World", 42);
            Assert.True(dictionary.Remove("Hello World"));
            Assert.Empty(dictionary);
            foreach (var item in dictionary)
                Assert.False(true, "Should be empty");
            foreach (var item in dictionary.Keys)
                Assert.False(true, "Should be empty");
            foreach (var item in dictionary.Values)
                Assert.False(true, "Should be empty");

            Assert.DoesNotContain("Hello World", dictionary);
            Assert.False(dictionary.ContainsKey("Hello World"));
            Assert.False(dictionary.Keys.Contains("Hello World"));
            Assert.False(dictionary.Values.Contains(42));
            object value;
            Assert.False(dictionary.TryGetValue("Hello World", out value));
            Assert.Null(value);
            Assert.False(dictionary.Remove("Hello World"));
            dictionary.CopyTo(new KeyValuePair<object, object>[0], 0);
            dictionary.Values.CopyTo(new object[0], 0);
            dictionary.Keys.CopyTo(new object[0], 0);
            dictionary.Clear();
        }

        [Fact]
        public void EmptyMessagePropertiesDictionary()
        {
            LogEventInfo logEvent = new LogEventInfo(LogLevel.Info, "MyLogger", string.Empty, (IList<MessageTemplateParameter>)null);
            IDictionary<object, object> dictionary = logEvent.Properties;
            Assert.Empty(dictionary);
            foreach (var item in dictionary)
                Assert.False(true, "Should be empty");
            foreach (var item in dictionary.Keys)
                Assert.False(true, "Should be empty");
            foreach (var item in dictionary.Values)
                Assert.False(true, "Should be empty");
            Assert.False(dictionary.ContainsKey("Hello World"));
            Assert.False(dictionary.Keys.Contains("Hello World"));
            Assert.False(dictionary.Values.Contains(42));
            Assert.DoesNotContain("Hello World", dictionary);
            object value;
            Assert.False(dictionary.TryGetValue("Hello World", out value));
            Assert.Null(value);
            Assert.False(dictionary.Remove("Hello World"));
            dictionary.CopyTo(new KeyValuePair<object, object>[0], 0);
            dictionary.Values.CopyTo(new object[0], 0);
            dictionary.Keys.CopyTo(new object[0], 0);
            dictionary.Clear();
        }

        [Fact]
        public void EmptyPropertiesDictionary()
        {
            LogEventInfo logEvent = new LogEventInfo(LogLevel.Info, "MyLogger", string.Empty, (IList<MessageTemplateParameter>)null);
            IDictionary<object, object> dictionary = logEvent.Properties;
            dictionary.Add("Hello World", null);
            Assert.True(dictionary.Remove("Hello World"));
            Assert.Empty(dictionary);
            foreach (var item in dictionary)
                Assert.False(true, "Should be empty");
            foreach (var item in dictionary.Keys)
                Assert.False(true, "Should be empty");
            foreach (var item in dictionary.Values)
                Assert.False(true, "Should be empty");
            Assert.False(dictionary.ContainsKey("Hello World"));
            Assert.False(dictionary.Keys.Contains("Hello World"));
            Assert.False(dictionary.Values.Contains(42));
            Assert.DoesNotContain("Hello World", dictionary);
            object value;
            Assert.False(dictionary.TryGetValue("Hello World", out value));
            Assert.Null(value);
            Assert.False(dictionary.Remove("Hello World"));
            dictionary.CopyTo(new KeyValuePair<object, object>[0], 0);
            dictionary.Values.CopyTo(new object[0], 0);
            dictionary.Keys.CopyTo(new object[0], 0);
            dictionary.Clear();
        }

        [Fact]
        public void SingleItemEventPropertiesDictionary()
        {
            LogEventInfo logEvent = new LogEventInfo();
            IDictionary<object, object> dictionary = logEvent.Properties;
            dictionary.Add("Hello World", 42);
            Assert.Single(dictionary);
            foreach (var item in dictionary)
            {
                Assert.Equal("Hello World", item.Key);
                Assert.Equal(42, item.Value);
            }
            foreach (var item in dictionary.Keys)
                Assert.Equal("Hello World", item);
            foreach (var item in dictionary.Values)
                Assert.Equal(42, item);
            AssertContainsInDictionary(dictionary, "Hello World", 42);
            Assert.True(dictionary.ContainsKey("Hello World"));
            Assert.True(dictionary.Keys.Contains("Hello World"));
            Assert.True(dictionary.Values.Contains(42));
            Assert.False(dictionary.ContainsKey("Goodbye World"));
            Assert.False(dictionary.Keys.Contains("Goodbye World"));
            Assert.DoesNotContain("Goodbye World", dictionary);
            object value;
            Assert.True(dictionary.TryGetValue("Hello World", out value));
            Assert.Equal(42, value);
            Assert.False(dictionary.TryGetValue("Goodbye World", out value));
            Assert.Null(value);
            var copyToArray = new KeyValuePair<object, object>[1];
            dictionary.CopyTo(copyToArray, 0);
            Assert.Equal("Hello World", copyToArray[0].Key);
            Assert.Equal(42, copyToArray[0].Value);
            var copyToValuesArray = new object[1];
            dictionary.Values.CopyTo(copyToValuesArray, 0);
            Assert.Equal(42, copyToValuesArray[0]);
            var copyToKeysArray = new object[1];
            dictionary.Keys.CopyTo(copyToKeysArray, 0);
            Assert.Equal("Hello World", copyToKeysArray[0]);
            Assert.True(dictionary.Remove("Hello World"));
            Assert.Empty(dictionary);
            dictionary["Hello World"] = 42;
            Assert.Single(dictionary);
            dictionary.Clear();
            Assert.Empty(dictionary);
        }

        [Fact]
        public void SingleItemMessagePropertiesDictionaryNoLookup()
        {
            LogEventInfo logEvent = new LogEventInfo(LogLevel.Info, "MyLogger", string.Empty, new[] { new MessageTemplateParameter("Hello World", 42, null, CaptureType.Normal) });
            IDictionary<object, object> dictionary = logEvent.Properties;
            Assert.Single(dictionary);
            foreach (var item in dictionary)
            {
                Assert.Equal("Hello World", item.Key);
                Assert.Equal(42, item.Value);
            }
            foreach (var item in dictionary.Keys)
                Assert.Equal("Hello World", item);
            foreach (var item in dictionary.Values)
                Assert.Equal(42, item);

            var copyToArray = new KeyValuePair<object, object>[1];
            dictionary.CopyTo(copyToArray, 0);
            Assert.Equal("Hello World", copyToArray[0].Key);
            Assert.Equal(42, copyToArray[0].Value);
            var copyToValuesArray = new object[1];
            dictionary.Values.CopyTo(copyToValuesArray, 0);
            Assert.Equal(42, copyToValuesArray[0]);
            var copyToKeysArray = new object[1];
            dictionary.Keys.CopyTo(copyToKeysArray, 0);
            Assert.Equal("Hello World", copyToKeysArray[0]);

            dictionary.Clear();
            Assert.Empty(dictionary);
        }

        [Fact]
        public void SingleItemMessagePropertiesDictionaryWithLookup()
        {
            LogEventInfo logEvent = new LogEventInfo(LogLevel.Info, "MyLogger", string.Empty, new[] { new MessageTemplateParameter("Hello World", 42, null, CaptureType.Normal) });
            IDictionary<object, object> dictionary = logEvent.Properties;

            Assert.Single(dictionary);

            AssertContainsInDictionary(dictionary, "Hello World", 42);
            Assert.True(dictionary.ContainsKey("Hello World"));
            Assert.True(dictionary.Keys.Contains("Hello World"));
            Assert.True(dictionary.Values.Contains(42));
            Assert.False(dictionary.ContainsKey("Goodbye World"));
            Assert.False(dictionary.Keys.Contains("Goodbye World"));
            Assert.DoesNotContain("Goodbye World", dictionary);
            object value;
            Assert.True(dictionary.TryGetValue("Hello World", out value));
            Assert.Equal(42, value);
            Assert.False(dictionary.TryGetValue("Goodbye World", out value));
            Assert.Null(value);

            var copyToArray = new KeyValuePair<object, object>[1];
            dictionary.CopyTo(copyToArray, 0);
            Assert.Equal("Hello World", copyToArray[0].Key);
            Assert.Equal(42, copyToArray[0].Value);
            var copyToValuesArray = new object[1];
            dictionary.Values.CopyTo(copyToValuesArray, 0);
            Assert.Equal(42, copyToValuesArray[0]);
            var copyToKeysArray = new object[1];
            dictionary.Keys.CopyTo(copyToKeysArray, 0);
            Assert.Equal("Hello World", copyToKeysArray[0]);

            dictionary.Clear();
            Assert.Empty(dictionary);
        }

        [Fact]
        public void MultiItemPropertiesDictionary()
        {
            LogEventInfo logEvent = new LogEventInfo(LogLevel.Info, "MyLogger", string.Empty, new[] { new MessageTemplateParameter("Hello World", 42, null, CaptureType.Normal) });
            IDictionary<object, object> dictionary = logEvent.Properties;

            dictionary["Goodbye World"] = 666;
            Assert.Equal(2, dictionary.Count);
            int i = 0;
            foreach (var item in dictionary)
            {
                switch (i++)
                {
                    case 0:
                        Assert.Equal("Hello World", item.Key);
                        Assert.Equal(42, item.Value);
                        break;
                    case 1:
                        Assert.Equal("Goodbye World", item.Key);
                        Assert.Equal(666, item.Value);
                        break;
                }
            }
            Assert.Equal(2, i);

            i = 0;
            foreach (var item in dictionary.Keys)
            {
                switch (i++)
                {
                    case 0:
                        Assert.Equal("Hello World", item);
                        break;
                    case 1:
                        Assert.Equal("Goodbye World", item);
                        break;
                }
            }
            Assert.Equal(2, i);

            i = 0;
            foreach (var item in dictionary.Values)
            {
                switch (i++)
                {
                    case 0:
                        Assert.Equal(42, item);
                        break;
                    case 1:
                        Assert.Equal(666, item);
                        break;
                }
            }
            Assert.True(dictionary.ContainsKey("Hello World"));
            AssertContainsInDictionary(dictionary, "Hello World", 42);
            Assert.True(dictionary.Keys.Contains("Hello World"));
            Assert.True(dictionary.Values.Contains(42));
            Assert.True(dictionary.ContainsKey("Goodbye World"));
            AssertContainsInDictionary(dictionary, "Goodbye World", 666);
            Assert.True(dictionary.Keys.Contains("Goodbye World"));
            Assert.True(dictionary.Values.Contains(666));
            Assert.False(dictionary.Keys.Contains("Mad World"));
            Assert.False(dictionary.ContainsKey("Mad World"));
            object value;
            Assert.True(dictionary.TryGetValue("Hello World", out value));
            Assert.Equal(42, value);
            Assert.True(dictionary.TryGetValue("Goodbye World", out value));
            Assert.Equal(666, value);
            Assert.False(dictionary.TryGetValue("Mad World", out value));
            Assert.Null(value);
            var copyToArray = new KeyValuePair<object, object>[2];
            dictionary.CopyTo(copyToArray, 0);
            Assert.Contains(new KeyValuePair<object, object>("Hello World", 42), copyToArray);
            Assert.Contains(new KeyValuePair<object, object>("Goodbye World", 666), copyToArray);
            var copyToValuesArray = new object[2];
            dictionary.Values.CopyTo(copyToValuesArray, 0);
            Assert.Contains(42, copyToValuesArray);
            Assert.Contains(666, copyToValuesArray);
            var copyToKeysArray = new object[2];
            dictionary.Keys.CopyTo(copyToKeysArray, 0);
            Assert.Contains("Hello World", copyToKeysArray);
            Assert.Contains("Goodbye World", copyToKeysArray);
            Assert.True(dictionary.Remove("Goodbye World"));
            Assert.Single(dictionary);
            dictionary["Goodbye World"] = 666;
            Assert.Equal(2, dictionary.Count);
            dictionary.Clear();
            Assert.Empty(dictionary);
        }



        [Fact]
        public void OverrideMessagePropertiesDictionary()
        {
            LogEventInfo logEvent = new LogEventInfo(LogLevel.Info, "MyLogger", string.Empty, new[]
            {
                new MessageTemplateParameter("Hello World", 42, null, CaptureType.Normal),
                new MessageTemplateParameter("Goodbye World", 666, null, CaptureType.Normal)
            });
            IDictionary<object, object> dictionary = logEvent.Properties;

            Assert.Equal(42, dictionary["Hello World"]);
            dictionary["Hello World"] = 999;
            Assert.Equal(999, dictionary["Hello World"]);
            Assert.True(dictionary.Values.Contains(999));
            Assert.True(dictionary.Values.Contains(666));
            Assert.False(dictionary.Values.Contains(42));

            int i = 0;
            foreach (var item in dictionary)
            {
                switch (i++)
                {
                    case 1:
                        Assert.Equal("Hello World", item.Key);
                        Assert.Equal(999, item.Value);
                        break;
                    case 0:
                        Assert.Equal("Goodbye World", item.Key);
                        Assert.Equal(666, item.Value);
                        break;
                }
            }
            Assert.Equal(2, i);

            i = 0;
            foreach (var item in dictionary.Keys)
            {
                switch (i++)
                {
                    case 1:
                        Assert.Equal("Hello World", item);
                        break;
                    case 0:
                        Assert.Equal("Goodbye World", item);
                        break;
                }
            }
            Assert.Equal(2, i);

            i = 0;
            foreach (var item in dictionary.Values)
            {
                switch (i++)
                {
                    case 1:
                        Assert.Equal(999, item);
                        break;
                    case 0:
                        Assert.Equal(666, item);
                        break;
                }
            }

            dictionary["Goodbye World"] = 42;
            i = 0;
            foreach (var item in dictionary.Keys)
            {
                switch (i++)
                {
                    case 0:
                        Assert.Equal("Hello World", item);
                        break;
                    case 1:
                        Assert.Equal("Goodbye World", item);
                        break;
                }
            }
            Assert.Equal(2, i);

            dictionary.Remove("Hello World");
            Assert.Single(dictionary);
            dictionary.Remove("Goodbye World");
            Assert.Empty(dictionary);
        }

        [Fact]
        public void NonUniqueMessagePropertiesDictionary()
        {
            LogEventInfo logEvent = new LogEventInfo(LogLevel.Info, "MyLogger", string.Empty, new[]
{
                new MessageTemplateParameter("Hello World", 42, null, CaptureType.Normal),
                new MessageTemplateParameter("Hello World", 666, null, CaptureType.Normal)
            });
            IDictionary<object, object> dictionary = logEvent.Properties;

            Assert.Single(dictionary);
            Assert.Equal(42, dictionary["Hello World"]);

            List<MessageTemplateParameter> parameters = new List<MessageTemplateParameter>();
            parameters.Add(new MessageTemplateParameter("Hello World", 42, null, CaptureType.Normal));
            for (int i = 1; i < 100; ++i)
                parameters.Add(new MessageTemplateParameter("Hello World", 666, null, CaptureType.Normal));
            logEvent = new LogEventInfo(LogLevel.Info, "MyLogger", string.Empty, new[]
            {
                new MessageTemplateParameter("Hello World", 42, null, CaptureType.Normal),
                new MessageTemplateParameter("Hello World", 666, null, CaptureType.Normal)
            });
            Assert.Single(dictionary);
            Assert.Equal(42, dictionary["Hello World"]);
        }
    }
}

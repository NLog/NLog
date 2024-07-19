// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.UnitTests.Targets
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Xunit;
    using NLog.Targets;
    using NLog.Targets.Wrappers;

    public class TargetWithContextTest : NLogTestBase
    {
        public class CustomTargetWithContext : TargetWithContext
        {
            public bool SkipAssert { get; set; }

            public class CustomTargetPropertyWithContext : TargetPropertyWithContext
            {
                public string Hello { get; set; }
            }

            [NLog.Config.ArrayParameter(typeof(CustomTargetPropertyWithContext), "contextproperty")]
            public override IList<TargetPropertyWithContext> ContextProperties { get; }

            public CustomTargetWithContext()
            {
                ContextProperties = new List<TargetPropertyWithContext>();
            }

            public IDictionary<string, object> LastCombinedProperties;
            public List<KeyValuePair<string, object>> ContextPropertyList;
            public string LastMessage;

            protected override void Write(LogEventInfo logEvent)
            {
                if (!SkipAssert)
                {
                    Assert.True(logEvent.HasStackTrace);

                    var scopeProperties = ScopeContext.GetAllProperties();  // See that async-timer cannot extract anything from scope-context
                    Assert.Empty(scopeProperties);

                    var scopeNested = ScopeContext.GetAllNestedStates();    // See that async-timer cannot extract anything from scope-context
                    Assert.Empty(scopeNested);
                }

                LastCombinedProperties = base.GetAllProperties(logEvent);
                ContextPropertyList = base.GetAllPropertiesList(logEvent).ToList();

                var nestedStates = base.GetScopeContextNested(logEvent);
                if (nestedStates.Count != 0)
                    LastCombinedProperties["TestKey"] = nestedStates[0];

                LastMessage = base.RenderLogEvent(Layout, logEvent);
            }
        }

        [Theory]
        [InlineData(false, false, "Kenny", "name")]
        [InlineData(true, false, null, "ScopeKey")]
        [InlineData(false, true, null, "GlobalKey")]
        public void GetContextPropertyListTest(bool includeScopeProperties, bool includeGdc, string name, string expected)
        {
            CustomTargetWithContext target = new CustomTargetWithContext();            
            target.IncludeScopeNested = true;
            target.IncludeCallSite = true;
            target.IncludeScopeProperties = includeScopeProperties;
            target.IncludeGdc = includeGdc;

            if(!string.IsNullOrEmpty(name))
            {
                target.ContextProperties.Add(new TargetPropertyWithContext("name", name));
            }

            AsyncTargetWrapper wrapper = new AsyncTargetWrapper();
            wrapper.WrappedTarget = target;
            wrapper.TimeToSleepBetweenBatches = 0;

            var logger = new LogFactory().Setup().LoadConfiguration(builder =>
            {
                builder.ForLogger().WriteTo(wrapper);
            }).GetLogger("Example");

            GlobalDiagnosticsContext.Clear();
            ScopeContext.Clear();            
            GlobalDiagnosticsContext.Set("GlobalKey", "Hello Global World");
            ScopeContext.PushProperty("ScopeKey", "Hello Async World");                        
            logger.Debug("log message");
            Assert.True(WaitForLastMessage(target));

            var keys = target.ContextPropertyList.Select(x => x.Key).ToList();
            Assert.Single(keys);
            Assert.Contains(expected, keys);
        }

        [Fact]        
        public void TargetWithContextAsyncTest()
        {
            CustomTargetWithContext target = new CustomTargetWithContext();
            target.ContextProperties.Add(new TargetPropertyWithContext("threadid", "${threadid}"));
            target.IncludeScopeProperties = true;
            target.IncludeGdc = true;
            target.IncludeScopeNested = true;
            target.IncludeCallSite = true;

            AsyncTargetWrapper wrapper = new AsyncTargetWrapper();
            wrapper.WrappedTarget = target;
            wrapper.TimeToSleepBetweenBatches = 0;

            var logger = new LogFactory().Setup().LoadConfiguration(builder =>
            {
                builder.ForLogger().WriteTo(wrapper);
            }).GetLogger("Example");

            GlobalDiagnosticsContext.Clear();
            ScopeContext.Clear();
            GlobalDiagnosticsContext.Set("TestKey", "Hello Global World");
            GlobalDiagnosticsContext.Set("GlobalKey", "Hello Global World");
            ScopeContext.PushProperty("TestKey", "Hello Async World");
            ScopeContext.PushProperty("AsyncKey", "Hello Async World");
            logger.Debug("log message");
            Assert.True(WaitForLastMessage(target));

            Assert.NotEqual(0, target.LastMessage.Length);
            Assert.NotNull(target.LastCombinedProperties);
            Assert.NotEmpty(target.LastCombinedProperties);
            Assert.Equal(5, target.LastCombinedProperties.Count);

            Assert.NotNull(target.ContextPropertyList);
            Assert.NotEmpty(target.ContextPropertyList);
            Assert.Equal(5, target.ContextPropertyList.Count);

            new[]
            {
                new KeyValuePair<string, object>("GlobalKey", "Hello Global World"),
                new KeyValuePair<string, object>("AsyncKey", "Hello Async World"),
                new KeyValuePair<string, object>("TestKey", "Hello Async World"),
                new KeyValuePair<string, object>("threadid", CurrentManagedThreadId.ToString())
            }.ToList().ForEach(pair => 
            {
                Assert.Contains(pair, target.LastCombinedProperties);
                Assert.Contains(pair, target.ContextPropertyList);
            });

            var alteredPair = new KeyValuePair<string, object>("TestKey_1", "Hello Global World");
            Assert.Contains(alteredPair, target.LastCombinedProperties);
            Assert.DoesNotContain(alteredPair, target.ContextPropertyList);
            
            Assert.Equal(2, target.ContextPropertyList.Count(prop => prop.Key == "TestKey"));            
        }

        private static bool WaitForLastMessage(CustomTargetWithContext target)
        {
            System.Threading.Thread.Sleep(1);
            for (int i = 0; i < 1000; ++i)
            {
                if (target.LastMessage != null)
                    return true;

                System.Threading.Thread.Sleep(1);
            }
            return false;
        }

        [Fact]
        [Obsolete("Replaced by ScopeContext.PushProperty or Logger.PushScopeProperty using ${scopeproperty}. Marked obsolete on NLog 5.0")]
        public void TargetWithContextMdcSerializeTest()
        {
            MappedDiagnosticsContext.Clear();
            MappedDiagnosticsContext.Set("TestKey", new { a = "b" });

            CustomTargetWithContext target = new CustomTargetWithContext() { IncludeMdc = true, SkipAssert = true };

            WriteAndAssertSingleKey(target);
        }

        [Fact]
        [Obsolete("Replaced by ScopeContext.PushProperty or Logger.PushScopeProperty using ${scopeproperty}. Marked obsolete on NLog 5.0")]
        public void TargetWithContextMdlcSerializeTest()
        {
            MappedDiagnosticsLogicalContext.Clear();
            MappedDiagnosticsLogicalContext.Set("TestKey", new { a = "b" });

            CustomTargetWithContext target = new CustomTargetWithContext() { IncludeMdlc = true, SkipAssert = true };

            WriteAndAssertSingleKey(target);
        }

        [Fact]
        [Obsolete("Replaced by dispose of return value from ScopeContext.PushNestedState or Logger.PushScopeNested. Marked obsolete on NLog 5.0")]
        public void TargetWithContextNdcSerializeTest()
        {
            NestedDiagnosticsContext.Clear();
            NestedDiagnosticsContext.Push(new { a = "b" });

            CustomTargetWithContext target = new CustomTargetWithContext() { IncludeNdc = true, SkipAssert = true };

            WriteAndAssertSingleKey(target);
        }

        [Fact]
        [Obsolete("Replaced by dispose of return value from ScopeContext.PushNestedState or Logger.PushScopeNested. Marked obsolete on NLog 5.0")]
        public void TargetWithContextNdlcSerializeTest()
        {
            NestedDiagnosticsLogicalContext.Clear();
            NestedDiagnosticsLogicalContext.Push(new { a = "b" });

            CustomTargetWithContext target = new CustomTargetWithContext() { IncludeNdlc = true, SkipAssert = true };

            WriteAndAssertSingleKey(target);
        }

        private static void WriteAndAssertSingleKey(CustomTargetWithContext target)
        {
            AsyncTargetWrapper wrapper = new AsyncTargetWrapper { WrappedTarget = target, TimeToSleepBetweenBatches = 0 };

            var logger = new LogFactory().Setup().LoadConfiguration(builder =>
            {
                builder.ForLogger().WriteTo(wrapper);
            }).GetLogger("Example");

            logger.Debug("log message");

            Assert.True(WaitForLastMessage(target));

            Assert.Equal("{ a = b }", target.LastCombinedProperties["TestKey"]);
        }

        [Fact]
        public void TargetWithContextConfigTest()
        {
            var logFactory = new LogFactory().Setup()
                                             .SetupExtensions(ext => ext.RegisterTarget<CustomTargetWithContext>("contexttarget"))
                                             .LoadConfigurationFromXml(@"
                <nlog throwExceptions='true'>
                    <targets>
                        <target name='debug' type='contexttarget' includeCallSite='true'>
                            <contextproperty name='threadid' layout='${threadid}' hello='world' />
                        </target>
                    </targets>
                    <rules>
                        <logger name='*' levels='Error' writeTo='debug' />
                    </rules>
                </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            ScopeContext.Clear();
            logger.Error("log message");
            var target = logFactory.Configuration.FindTargetByName("debug") as CustomTargetWithContext;
            Assert.NotEqual(0, target.LastMessage.Length);
            var lastCombinedProperties = target.LastCombinedProperties;
            var contextPropertyList = target.ContextPropertyList;
            var expected = new KeyValuePair<string, object>("threadid", CurrentManagedThreadId.ToString());
            Assert.NotEmpty(lastCombinedProperties);
            Assert.Contains(expected, lastCombinedProperties);
            Assert.Contains(expected, contextPropertyList);
        }

        [Fact]
        public void TargetWithContextAsyncPropertyTest()
        {
            var logFactory = new LogFactory().Setup()
                                             .SetupExtensions(ext => ext.RegisterTarget<CustomTargetWithContext>("contexttarget"))
                                             .LoadConfigurationFromXml(@"
                <nlog throwExceptions='true'>
                    <targets>
                        <default-wrapper type='AsyncWrapper' timeToSleepBetweenBatches='0' overflowAction='Block' />
                        <target name='debug' type='contexttarget' includeCallSite='true' includeEventProperties='true' excludeProperties='password' />
                    </targets>
                    <rules>
                        <logger name='*' levels='Error' writeTo='debug' />
                    </rules>
                </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            var target = logFactory.Configuration.AllTargets.OfType<CustomTargetWithContext>().FirstOrDefault();

            LogEventInfo logEvent = LogEventInfo.Create(LogLevel.Error, logger.Name, "Hello");
            logEvent.Properties["name"] = "Kenny";
            logEvent.Properties["password"] = "123Password";
            logger.Error(logEvent);
            logFactory.Flush();
            Assert.NotEqual(0, target.LastMessage.Length);
            var lastCombinedProperties = target.LastCombinedProperties;
            Assert.Single(lastCombinedProperties);
            Assert.Contains(new KeyValuePair<string, object>("name", "Kenny"), lastCombinedProperties);

            logger.Error("Hello {name}", "Cartman");
            logEvent.Properties["Password"] = "123Password";
            logFactory.Flush();
            lastCombinedProperties = target.LastCombinedProperties;
            Assert.Single(lastCombinedProperties);
            Assert.Contains(new KeyValuePair<string, object>("name", "Cartman"), lastCombinedProperties);
        }

        [Fact]
        public void TargetWithContextAsyncBufferScopePropertyTest()
        {
            var logFactory = new LogFactory().Setup()
                                 .SetupExtensions(ext => ext.RegisterTarget<CustomTargetWithContext>("contexttarget"))
                                 .LoadConfigurationFromXml(@"
                <nlog throwExceptions='true'>
                    <targets>
                        <default-wrapper type='AsyncWrapper' timeToSleepBetweenBatches='0' overflowAction='Block' />
                        <target name='debug_buffer' type='BufferingWrapper'>
                           <target name='debug' type='contexttarget' includeCallSite='true' includeScopeProperties='true' excludeProperties='password' />
                        </target>
                    </targets>
                    <rules>
                        <logger name='*' levels='Error' writeTo='debug_buffer' />
                    </rules>
                </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            var target = logFactory.Configuration.AllTargets.OfType<CustomTargetWithContext>().FirstOrDefault();

            using (logger.PushScopeProperty("name", "Kenny"))
            using (logger.PushScopeProperty("password", "123Password"))
            {
                logger.Error("Hello");
            }
            logFactory.Flush();
            Assert.NotEqual(0, target.LastMessage.Length);
            var lastCombinedProperties = target.LastCombinedProperties;
            var contextPropertyList = target.ContextPropertyList;
            var expected = new KeyValuePair<string, object>("name", "Kenny");
            Assert.Single(lastCombinedProperties);
            Assert.Single(contextPropertyList);
            Assert.Contains(expected, lastCombinedProperties);
            Assert.Contains(expected, contextPropertyList);
        }

        [Fact]
        public void TargetWithContextJsonTest()
        {
            var logFactory = new LogFactory().Setup()
                                             .SetupExtensions(ext => ext.RegisterTarget<CustomTargetWithContext>("contexttarget"))
                                             .LoadConfigurationFromXml(@"
                <nlog throwExceptions='true'>
                    <targets>
                        <default-wrapper type='AsyncWrapper' timeToSleepBetweenBatches='0' overflowAction='Block' />
                        <target name='debug' type='contexttarget' includeCallSite='true'>
                            <layout type='JsonLayout' includeScopeProperties='true'>
                                <attribute name='level' layout='${level:upperCase=true}'/>
                                <attribute name='message' layout='${message}' />
                                <attribute name='exception' layout='${exception}' />
                                <attribute name='threadid' layout='${threadid}' />
                            </layout>
                        </target>
                    </targets>
                    <rules>
                        <logger name='*' levels='Error' writeTo='debug' />
                    </rules>
                </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");

            ScopeContext.Clear();
            ScopeContext.PushProperty("TestKey", "Hello Thread World");
            logger.Error("log message");
            var target = logFactory.Configuration.AllTargets.OfType<CustomTargetWithContext>().FirstOrDefault();
            System.Threading.Thread.Sleep(1);
            for (int i = 0; i < 1000; ++i)
            {
                if (target.LastMessage != null)
                    break;

                System.Threading.Thread.Sleep(1);
            }

            Assert.NotEqual(0, target.LastMessage.Length);
            Assert.Contains(CurrentManagedThreadId.ToString(), target.LastMessage);
            var lastCombinedProperties = target.LastCombinedProperties;
            var contextPropertyList = target.ContextPropertyList;
            Assert.Empty(lastCombinedProperties);
            Assert.Empty(contextPropertyList);
        }

        [Fact]
        public void TargetWithContextPropertyTypeTest()
        {
            var logFactory = new LogFactory().Setup()
                                             .SetupExtensions(ext => ext.RegisterTarget<CustomTargetWithContext>("contexttarget"))
                                             .LoadConfigurationFromXml(@"
                <nlog throwExceptions='true'>
                    <targets>
                        <default-wrapper type='AsyncWrapper' timeToSleepBetweenBatches='0' overflowAction='Block' />
                        <target name='debug' type='contexttarget' includeCallSite='true'>
                            <contextproperty name='threadid' layout='${threadid}' propertyType='System.Int32' />
                            <contextproperty name='processid' layout='${processid}' propertyType='System.Int32' />
                            <contextproperty name='timestamp' layout='${date}' propertyType='System.DateTime' />
                            <contextproperty name='int-non-existing' layout='${event-properties:non-existing}' propertyType='System.Int32' includeEmptyValue='true' />
                            <contextproperty name='int-non-existing-empty' layout='${event-properties:non-existing}' propertyType='System.Int32' includeEmptyValue='false' />
                            <contextproperty name='object-non-existing' layout='${event-properties:non-existing}' propertyType='System.Object' includeEmptyValue='true' />
                            <contextproperty name='object-non-existing-empty' layout='${event-properties:non-existing}' propertyType='System.Object' includeEmptyValue='false' />
                       </target>
                    </targets>
                    <rules>
                        <logger name='*' levels='Error' writeTo='debug' />
                    </rules>
                </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            ScopeContext.Clear();

            var logEvent = new LogEventInfo() { Message = "log message" };
            logger.Error(logEvent);
            logFactory.Flush();
            var target = logFactory.Configuration.AllTargets.OfType<CustomTargetWithContext>().FirstOrDefault();
            Assert.NotEqual(0, target.LastMessage.Length);
            var lastCombinedProperties = target.LastCombinedProperties;
            var contextPropertyListKeys = target.ContextPropertyList.Select(x => x.Key).ToList();
            Assert.NotEmpty(lastCombinedProperties);
            Assert.Equal(lastCombinedProperties.Keys, contextPropertyListKeys);
            Assert.Contains(new KeyValuePair<string, object>("threadid", CurrentManagedThreadId), lastCombinedProperties);
            Assert.Contains(new KeyValuePair<string, object>("processid", CurrentProcessId), lastCombinedProperties);
            Assert.Contains(new KeyValuePair<string, object>("int-non-existing", 0), lastCombinedProperties);
            Assert.DoesNotContain("int-non-existing-empty", lastCombinedProperties.Keys);
            Assert.Contains(new KeyValuePair<string, object>("object-non-existing", ""), lastCombinedProperties);
            Assert.DoesNotContain("object-non-existing-empty", lastCombinedProperties.Keys);
        }
    }
}

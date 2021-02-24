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

namespace NLog.UnitTests.Targets
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Xunit;
    using NLog.Config;
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

                var nestedStates = base.GetScopeContextNestedStates(logEvent);
                if (nestedStates.Count != 0)
                    LastCombinedProperties["TestKey"] = nestedStates[0];

                LastMessage = base.RenderLogEvent(Layout, logEvent);
            }
        }

        [Fact]
        public void TargetWithContextAsyncTest()
        {
            CustomTargetWithContext target = new CustomTargetWithContext();
            target.ContextProperties.Add(new TargetPropertyWithContext("threadid", "${threadid}"));
            target.IncludeScopeProperties = true;
            target.IncludeGdc = true;
            target.IncludeScopeNestedStates = true;
            target.IncludeCallSite = true;

            AsyncTargetWrapper wrapper = new AsyncTargetWrapper();
            wrapper.WrappedTarget = target;
            wrapper.TimeToSleepBetweenBatches = 0;

            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(wrapper, LogLevel.Debug);

            Logger logger = LogManager.GetLogger("Example");

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
            Assert.Contains(new KeyValuePair<string, object>("GlobalKey", "Hello Global World"), target.LastCombinedProperties);
            Assert.Contains(new KeyValuePair<string, object>("AsyncKey", "Hello Async World"), target.LastCombinedProperties);
            Assert.Contains(new KeyValuePair<string, object>("TestKey", "Hello Async World"), target.LastCombinedProperties);
            Assert.Contains(new KeyValuePair<string, object>("TestKey_1", "Hello Global World"), target.LastCombinedProperties);
            Assert.Contains(new KeyValuePair<string, object>("threadid", System.Environment.CurrentManagedThreadId.ToString()), target.LastCombinedProperties);
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
        [Obsolete("Replaced by dispose of return value from ScopeContext.PushNestedState or Logger.PushScopeState. Marked obsolete on NLog 5.0")]
        public void TargetWithContextNdcSerializeTest()
        {
            NestedDiagnosticsContext.Clear();
            NestedDiagnosticsContext.Push(new { a = "b" });

            CustomTargetWithContext target = new CustomTargetWithContext() { IncludeNdc = true, SkipAssert = true };

            WriteAndAssertSingleKey(target);
        }

        [Fact]
        [Obsolete("Replaced by dispose of return value from ScopeContext.PushNestedState or Logger.PushScopeState. Marked obsolete on NLog 5.0")]
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

            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(wrapper, LogLevel.Debug);

            Logger logger = LogManager.GetLogger("Example");

            logger.Debug("log message");

            Assert.True(WaitForLastMessage(target));

            Assert.Equal("{ a = b }", target.LastCombinedProperties["TestKey"]);
        }

        [Fact]
        public void TargetWithContextConfigTest()
        {
            Target.Register("contexttarget", typeof(CustomTargetWithContext));

            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
                <nlog throwExceptions='true'>
                    <targets>
                        <target name='debug' type='contexttarget' includeCallSite='true'>
                            <contextproperty name='threadid' layout='${threadid}' hello='world' />
                        </target>
                    </targets>
                    <rules>
                        <logger name='*' levels='Error' writeTo='debug' />
                    </rules>
                </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            ScopeContext.Clear();
            logger.Error("log message");
            var target = LogManager.Configuration.FindTargetByName("debug") as CustomTargetWithContext;
            Assert.NotEqual(0, target.LastMessage.Length);
            var lastCombinedProperties = target.LastCombinedProperties;
            Assert.NotEmpty(lastCombinedProperties);
            Assert.Contains(new KeyValuePair<string, object>("threadid", System.Environment.CurrentManagedThreadId.ToString()), lastCombinedProperties);
        }

        [Fact]
        public void TargetWithContextAsyncPropertyTest()
        {
            Target.Register("contexttarget", typeof(CustomTargetWithContext));

            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
                <nlog throwExceptions='true'>
                    <targets>
                        <default-wrapper type='AsyncWrapper' timeToSleepBetweenBatches='0' overflowAction='Block' />
                        <target name='debug' type='contexttarget' includeCallSite='true' includeEventProperties='true' />
                    </targets>
                    <rules>
                        <logger name='*' levels='Error' writeTo='debug' />
                    </rules>
                </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            var target = LogManager.Configuration.AllTargets.OfType<CustomTargetWithContext>().FirstOrDefault();

            LogEventInfo logEvent = LogEventInfo.Create(LogLevel.Error, logger.Name, "Hello");
            logEvent.Properties["name"] = "Kenny";
            logger.Error(logEvent);
            LogManager.Flush();
            Assert.NotEqual(0, target.LastMessage.Length);
            var lastCombinedProperties = target.LastCombinedProperties;
            Assert.NotEmpty(lastCombinedProperties);
            Assert.Contains(new KeyValuePair<string, object>("name", "Kenny"), lastCombinedProperties);

            logger.Error("Hello {name}", "Cartman");
            LogManager.Flush();
            lastCombinedProperties = target.LastCombinedProperties;
            Assert.NotEmpty(lastCombinedProperties);
            Assert.Contains(new KeyValuePair<string, object>("name", "Cartman"), lastCombinedProperties);
        }

        [Fact]
        public void TargetWithContextJsonTest()
        {
            Target.Register("contexttarget", typeof(CustomTargetWithContext));

            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
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
                </nlog>");

            ILogger logger = LogManager.GetLogger("A");

            ScopeContext.Clear();
            ScopeContext.PushProperty("TestKey", "Hello Thread World");
            logger.Error("log message");
            var target = LogManager.Configuration.AllTargets.OfType<CustomTargetWithContext>().FirstOrDefault();
            System.Threading.Thread.Sleep(1);
            for (int i = 0; i < 1000; ++i)
            {
                if (target.LastMessage != null)
                    break;

                System.Threading.Thread.Sleep(1);
            }

            Assert.NotEqual(0, target.LastMessage.Length);
            Assert.Contains(System.Environment.CurrentManagedThreadId.ToString(), target.LastMessage);
            var lastCombinedProperties = target.LastCombinedProperties;
            Assert.Empty(lastCombinedProperties);
        }

        [Fact]
        public void TargetWithContextPropertyTypeTest()
        {
            Target.Register("contexttarget", typeof(CustomTargetWithContext));

            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
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
                </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            ScopeContext.Clear();

            var logEvent = new LogEventInfo() { Message = "log message" };
            logger.Error(logEvent);
            LogManager.Flush();
            var target = LogManager.Configuration.AllTargets.OfType<CustomTargetWithContext>().FirstOrDefault();
            Assert.NotEqual(0, target.LastMessage.Length);
            var lastCombinedProperties = target.LastCombinedProperties;
            Assert.NotEmpty(lastCombinedProperties);
            Assert.Contains(new KeyValuePair<string, object>("threadid", System.Environment.CurrentManagedThreadId), lastCombinedProperties);
            Assert.Contains(new KeyValuePair<string, object>("processid", System.Diagnostics.Process.GetCurrentProcess().Id), lastCombinedProperties);
            Assert.Contains(new KeyValuePair<string, object>("int-non-existing", 0), lastCombinedProperties);
            Assert.DoesNotContain("int-non-existing-empty", lastCombinedProperties.Keys);
            Assert.Contains(new KeyValuePair<string, object>("object-non-existing", ""), lastCombinedProperties);
            Assert.DoesNotContain("object-non-existing-empty", lastCombinedProperties.Keys);
        }
    }
}

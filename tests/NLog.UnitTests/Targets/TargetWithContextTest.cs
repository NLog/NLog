// 
// Copyright (c) 2004-2018 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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


using NLog.Config;

namespace NLog.UnitTests.Targets
{
    using System.Linq;
    using System.Collections.Generic;
    using Xunit;
    using NLog.Targets;
    using NLog.Targets.Wrappers;

    public class TargetWithContextTest : NLogTestBase
    {
        public class CustomTargetWithContext : TargetWithContext
        {
            public class CustomTargetPropertyWithContext : TargetPropertyWithContext
            {
                public string Hello { get; set; }
            }

            [NLog.Config.ArrayParameter(typeof(CustomTargetPropertyWithContext), "contextproperty")]
            public override IList<TargetPropertyWithContext> ContextProperties { get;  }

            public CustomTargetWithContext()
            {
                ContextProperties = new List<TargetPropertyWithContext>();
            }

            public IDictionary<string, object> LastCombinedProperties;
            public string LastMessage;

            protected override void Write(LogEventInfo logEvent)
            {
                var test = MappedDiagnosticsLogicalContext.GetNames();
                Assert.Empty(test);
                Assert.True(logEvent.HasStackTrace);
                LastCombinedProperties = base.GetAllProperties(logEvent);
                LastMessage = base.RenderLogEvent(Layout, logEvent);
            }
        }

        [Fact]
        public void TargetWithContextAsyncTest()
        {
            CustomTargetWithContext target = new CustomTargetWithContext();
            target.ContextProperties.Add(new TargetPropertyWithContext("threadid", "${threadid}"));
            target.IncludeMdlc = true;
            target.IncludeMdc = true;
            target.IncludeGdc = true;
            target.IncludeNdc = true;
            target.IncludeNdlc = true;
            target.IncludeCallSite = true;

            AsyncTargetWrapper wrapper = new AsyncTargetWrapper();
            wrapper.WrappedTarget = target;
            wrapper.TimeToSleepBetweenBatches = 0;

            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(wrapper, LogLevel.Debug);

            Logger logger = LogManager.GetLogger("Example");

            GlobalDiagnosticsContext.Clear();
            GlobalDiagnosticsContext.Set("TestKey", "Hello Global World");
            GlobalDiagnosticsContext.Set("GlobalKey", "Hello Global World");
            MappedDiagnosticsContext.Clear();
            MappedDiagnosticsContext.Set("TestKey", "Hello Thread World");
            MappedDiagnosticsContext.Set("ThreadKey", "Hello Thread World");
            MappedDiagnosticsLogicalContext.Clear();
            MappedDiagnosticsLogicalContext.Set("TestKey", "Hello Async World");
            MappedDiagnosticsLogicalContext.Set("AsyncKey", "Hello Async World");
            logger.Debug("log message");
            System.Threading.Thread.Sleep(1);
            for (int i = 0; i < 1000; ++i)
            {
                if (target.LastMessage != null)
                    break;

                System.Threading.Thread.Sleep(1);
            }

            Assert.NotEqual(0, target.LastMessage.Length);
            Assert.NotNull(target.LastCombinedProperties);
            Assert.NotEmpty(target.LastCombinedProperties);
            Assert.Equal(7, target.LastCombinedProperties.Count);
            Assert.Contains(new KeyValuePair<string, object>("GlobalKey", "Hello Global World"), target.LastCombinedProperties);
            Assert.Contains(new KeyValuePair<string, object>("ThreadKey", "Hello Thread World"), target.LastCombinedProperties);
            Assert.Contains(new KeyValuePair<string, object>("AsyncKey", "Hello Async World"), target.LastCombinedProperties);
            Assert.Contains(new KeyValuePair<string, object>("TestKey", "Hello Async World"), target.LastCombinedProperties);
            Assert.Contains(new KeyValuePair<string, object>("TestKey_1", "Hello Thread World"), target.LastCombinedProperties);
            Assert.Contains(new KeyValuePair<string, object>("TestKey_2", "Hello Global World"), target.LastCombinedProperties);
            Assert.Contains(new KeyValuePair<string, object>("threadid", System.Environment.CurrentManagedThreadId.ToString()), target.LastCombinedProperties);
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
            MappedDiagnosticsLogicalContext.Clear();
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
                <nlog throwExceptions='true' optimizeBufferReuse='false'>
                    <targets>
                        <default-wrapper type='AsyncWrapper' timeToSleepBetweenBatches='0' overflowAction='Block' />
                        <target name='debug' type='contexttarget' includeCallSite='true' optimizeBufferReuse='false'>
                            <layout type='JsonLayout' includeMdc='true'>
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
            MappedDiagnosticsLogicalContext.Clear();
            MappedDiagnosticsContext.Clear();
            MappedDiagnosticsContext.Set("TestKey", "Hello Thread World");
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
    }
}

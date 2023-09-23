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

namespace NLog.UnitTests.Config
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using NLog.Config;
    using NLog.Filters;
    using NLog.Internal;
    using Xunit;

    public class RuleConfigurationTests : NLogTestBase
    {
        [Fact]
        public void NoRulesTest()
        {
            LoggingConfiguration c = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='d1' type='Debug' />
                </targets>

                <rules>
                </rules>
            </nlog>");

            Assert.Equal(0, c.LoggingRules.Count);
        }

        [Fact]
        public void SimpleRuleTest()
        {
            LoggingConfiguration c = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='d1' type='Debug' />
                </targets>

                <rules>
                    <logger name='*' minLevel='Info' writeTo='d1' />
                </rules>
            </nlog>");

            Assert.Equal(1, c.LoggingRules.Count);
            var rule = c.LoggingRules[0];
            Assert.Equal("*", rule.LoggerNamePattern);
            Assert.Equal(FilterResult.Ignore, rule.FilterDefaultAction);
            Assert.Equal(4, rule.Levels.Count);
            Assert.Contains(LogLevel.Info, rule.Levels);
            Assert.Contains(LogLevel.Warn, rule.Levels);
            Assert.Contains(LogLevel.Error, rule.Levels);
            Assert.Contains(LogLevel.Fatal, rule.Levels);
            Assert.Equal(1, rule.Targets.Count);
            Assert.Same(c.FindTargetByName("d1"), rule.Targets[0]);
            Assert.False(rule.Final);
            Assert.Equal(0, rule.Filters.Count);
        }

        [Fact]
        public void SingleLevelTest()
        {
            LoggingConfiguration c = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='d1' type='Debug' />
                </targets>

                <rules>
                    <logger name='*' level='Warn' writeTo='d1' />
                </rules>
            </nlog>");

            Assert.Equal(1, c.LoggingRules.Count);
            var rule = c.LoggingRules[0];
            Assert.Single(rule.Levels);
            Assert.Contains(LogLevel.Warn, rule.Levels);
        }

        [Fact]
        public void MinMaxLevelTest()
        {
            LoggingConfiguration c = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='d1' type='Debug' />
                </targets>

                <rules>
                    <logger name='*' minLevel='Info' maxLevel='Warn' writeTo='d1' />
                </rules>
            </nlog>");

            Assert.Equal(1, c.LoggingRules.Count);
            var rule = c.LoggingRules[0];
            Assert.Equal(2, rule.Levels.Count);
            Assert.Contains(LogLevel.Info, rule.Levels);
            Assert.Contains(LogLevel.Warn, rule.Levels);
        }

        [Fact]
        public void FinalMinLevelTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>
                    <target name='defaultTarget' type='Debug' layout='${message}' />
                    <target name='requestTarget' type='Debug' layout='Request-${message}' />
                </targets>

                <rules>
                    <logger name='*' finalMinLevel='Info' />
                    <logger name='Microsoft*' finalMinLevel='Warn' />
                    <logger name='Microsoft.Hosting.Lifetime*' finalMinLevel='Info' />
                    <logger name='System*' finalMinLevel='Warn' />

                    <logger name='RequestLogger' minLevel='Debug' finalMinLevel='Error' writeTo='requestTarget' />

                    <logger writeTo='defaultTarget' />
                </rules>
            </nlog>").LogFactory;

            var requestLogger = logFactory.GetLogger("RequestLogger");
            var defaultLogger = logFactory.GetLogger("DefaultLogger");
            var microsoftLogger = logFactory.GetLogger("Microsoft.Hosting");
            var lifetimeLogger = logFactory.GetLogger("Microsoft.Hosting.Lifetime");

            requestLogger.Error("Important Noise");
            logFactory.AssertDebugLastMessage("defaultTarget", "Important Noise");
            logFactory.AssertDebugLastMessage("requestTarget", "Request-Important Noise");

            defaultLogger.Info("Other Noise");
            logFactory.AssertDebugLastMessage("defaultTarget", "Other Noise");
            logFactory.AssertDebugLastMessage("requestTarget", "Request-Important Noise");

            requestLogger.Debug("Debug Noise");
            logFactory.AssertDebugLastMessage("defaultTarget", "Other Noise");
            logFactory.AssertDebugLastMessage("requestTarget", "Request-Debug Noise");

            requestLogger.Warn("Good Noise");
            logFactory.AssertDebugLastMessage("defaultTarget", "Other Noise");
            logFactory.AssertDebugLastMessage("requestTarget", "Request-Good Noise");

            requestLogger.Trace("Unwanted Noise");
            logFactory.AssertDebugLastMessage("defaultTarget", "Other Noise");
            logFactory.AssertDebugLastMessage("requestTarget", "Request-Good Noise");

            lifetimeLogger.Error("Important Noise");
            logFactory.AssertDebugLastMessage("defaultTarget", "Important Noise");
            logFactory.AssertDebugLastMessage("requestTarget", "Request-Good Noise");

            lifetimeLogger.Info("Other Noise");
            logFactory.AssertDebugLastMessage("defaultTarget", "Other Noise");
            logFactory.AssertDebugLastMessage("requestTarget", "Request-Good Noise");

            lifetimeLogger.Warn("Good Noise");
            logFactory.AssertDebugLastMessage("defaultTarget", "Good Noise");
            logFactory.AssertDebugLastMessage("requestTarget", "Request-Good Noise");

            microsoftLogger.Error("Important Noise");
            logFactory.AssertDebugLastMessage("defaultTarget", "Important Noise");
            logFactory.AssertDebugLastMessage("requestTarget", "Request-Good Noise");

            microsoftLogger.Info("Other Noise");
            logFactory.AssertDebugLastMessage("defaultTarget", "Important Noise");
            logFactory.AssertDebugLastMessage("requestTarget", "Request-Good Noise");

            microsoftLogger.Warn("Good Noise");
            logFactory.AssertDebugLastMessage("defaultTarget", "Good Noise");
            logFactory.AssertDebugLastMessage("requestTarget", "Request-Good Noise");
        }

        [Fact]
        public void NoLevelsTest()
        {
            LoggingConfiguration c = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='d1' type='Debug' />
                </targets>

                <rules>
                    <logger name='*' writeTo='d1' />
                </rules>
            </nlog>");

            Assert.Equal(1, c.LoggingRules.Count);
            var rule = c.LoggingRules[0];
            Assert.Equal(6, rule.Levels.Count);
            Assert.Contains(LogLevel.Trace, rule.Levels);
            Assert.Contains(LogLevel.Debug, rule.Levels);
            Assert.Contains(LogLevel.Info, rule.Levels);
            Assert.Contains(LogLevel.Warn, rule.Levels);
            Assert.Contains(LogLevel.Error, rule.Levels);
            Assert.Contains(LogLevel.Fatal, rule.Levels);
        }

        [Fact]
        public void ExplicitLevelsTest()
        {
            LoggingConfiguration c = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='d1' type='Debug' />
                </targets>

                <rules>
                    <logger name='*' levels='Trace,Info,Warn' writeTo='d1' />
                </rules>
            </nlog>");

            Assert.Equal(1, c.LoggingRules.Count);
            var rule = c.LoggingRules[0];
            Assert.Equal(3, rule.Levels.Count);
            Assert.Contains(LogLevel.Trace, rule.Levels);
            Assert.Contains(LogLevel.Info, rule.Levels);
            Assert.Contains(LogLevel.Warn, rule.Levels);
        }

        [Fact]
        public void LogThresholdTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${level}' /></targets>
                <rules>
                    <logger name='*' minlevel='Info' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");

            logger.Fatal("hello");
            logFactory.AssertDebugLastMessage(nameof(LogLevel.Fatal));

            logger.Error("hello");
            logFactory.AssertDebugLastMessage(nameof(LogLevel.Error));

            logger.Warn("hello");
            logFactory.AssertDebugLastMessage(nameof(LogLevel.Warn));

            logger.Info("hello");
            logFactory.AssertDebugLastMessage(nameof(LogLevel.Info));

            logger.Debug("hello");
            logFactory.AssertDebugLastMessage(nameof(LogLevel.Info));
        }

        [Fact]
        public void LogThresholdTest2()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${level}' />
                    <target name='debug2' type='Debug' layout='${level}' />
                    <target name='debug3' type='Debug' layout='${level}' />
                    <target name='debug4' type='Debug' layout='${level}' />
                    <target name='debug5' type='Debug' layout='${level}' />
                </targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug1' />
                    <logger name='*' minlevel='Info' writeTo='debug2' />
                    <logger name='*' minlevel='Warn' writeTo='debug3' />
                    <logger name='*' minlevel='Error' writeTo='debug4' />
                    <logger name='*' minlevel='Fatal' writeTo='debug5' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");

            logger.Fatal("hello");
            logFactory.AssertDebugLastMessage("Debug1", nameof(LogLevel.Fatal));
            logFactory.AssertDebugLastMessage("Debug2", nameof(LogLevel.Fatal));
            logFactory.AssertDebugLastMessage("Debug3", nameof(LogLevel.Fatal));
            logFactory.AssertDebugLastMessage("Debug4", nameof(LogLevel.Fatal));
            logFactory.AssertDebugLastMessage("Debug5", nameof(LogLevel.Fatal));

            logger.Error("hello");
            logFactory.AssertDebugLastMessage("Debug1", nameof(LogLevel.Error));
            logFactory.AssertDebugLastMessage("Debug2", nameof(LogLevel.Error));
            logFactory.AssertDebugLastMessage("Debug3", nameof(LogLevel.Error));
            logFactory.AssertDebugLastMessage("Debug4", nameof(LogLevel.Error));
            logFactory.AssertDebugLastMessage("Debug5", nameof(LogLevel.Fatal));

            logger.Warn("hello");
            logFactory.AssertDebugLastMessage("Debug1", nameof(LogLevel.Warn));
            logFactory.AssertDebugLastMessage("Debug2", nameof(LogLevel.Warn));
            logFactory.AssertDebugLastMessage("Debug3", nameof(LogLevel.Warn));
            logFactory.AssertDebugLastMessage("Debug4", nameof(LogLevel.Error));
            logFactory.AssertDebugLastMessage("Debug5", nameof(LogLevel.Fatal));

            logger.Info("hello");
            logFactory.AssertDebugLastMessage("Debug1", nameof(LogLevel.Info));
            logFactory.AssertDebugLastMessage("Debug2", nameof(LogLevel.Info));
            logFactory.AssertDebugLastMessage("Debug3", nameof(LogLevel.Warn));
            logFactory.AssertDebugLastMessage("Debug4", nameof(LogLevel.Error));
            logFactory.AssertDebugLastMessage("Debug5", nameof(LogLevel.Fatal));

            logger.Debug("hello");
            logFactory.AssertDebugLastMessage("Debug1", nameof(LogLevel.Debug));
            logFactory.AssertDebugLastMessage("Debug2", nameof(LogLevel.Info));
            logFactory.AssertDebugLastMessage("Debug3", nameof(LogLevel.Warn));
            logFactory.AssertDebugLastMessage("Debug4", nameof(LogLevel.Error));
            logFactory.AssertDebugLastMessage("Debug5", nameof(LogLevel.Fatal));
        }

        [Fact]
        public void LoggerNameMatchTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${logger}' />
                    <target name='debug2' type='Debug' layout='${logger}' />
                    <target name='debug3' type='Debug' layout='${logger}' />
                    <target name='debug4' type='Debug' layout='${logger}' />
                </targets>
                <rules>
                    <logger name='A' minlevel='Info' writeTo='debug1' />
                    <logger name='A*' minlevel='Info' writeTo='debug2' />
                    <logger name='*A*' minlevel='Info' writeTo='debug3' />
                    <logger name='*A' minlevel='Info' writeTo='debug4' />
                </rules>
            </nlog>").LogFactory;

            logFactory.GetLogger("A").Info("message"); // matches 1st, 2nd, 3rd and 4th rule
            logFactory.AssertDebugLastMessage("Debug1", "A");
            logFactory.AssertDebugLastMessage("Debug2", "A");
            logFactory.AssertDebugLastMessage("Debug3", "A");
            logFactory.AssertDebugLastMessage("Debug4", "A");

            logFactory.GetLogger("A2").Info("message"); // matches 2nd rule and 3rd rule
            logFactory.AssertDebugLastMessage("Debug1", "A");
            logFactory.AssertDebugLastMessage("Debug2", "A2");
            logFactory.AssertDebugLastMessage("Debug3", "A2");
            logFactory.AssertDebugLastMessage("Debug4", "A");

            logFactory.GetLogger("BAD").Info("message"); // matches 3rd rule
            logFactory.AssertDebugLastMessage("Debug1", "A");
            logFactory.AssertDebugLastMessage("Debug2", "A2");
            logFactory.AssertDebugLastMessage("Debug3", "BAD");
            logFactory.AssertDebugLastMessage("Debug4", "A");

            logFactory.GetLogger("BA").Info("message"); // matches 3rd and 4th rule
            logFactory.AssertDebugLastMessage("Debug1", "A");
            logFactory.AssertDebugLastMessage("Debug2", "A2");
            logFactory.AssertDebugLastMessage("Debug3", "BA");
            logFactory.AssertDebugLastMessage("Debug4", "BA");
        }

        [Fact]
        public void MultiAppenderTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${logger}' />
                    <target name='debug2' type='Debug' layout='${logger}' />
                    <target name='debug3' type='Debug' layout='${logger}' />
                    <target name='debug4' type='Debug' layout='${logger}' />
                </targets>
                <rules>
                    <logger name='A' minlevel='Info' writeTo='debug1' />
                    <logger name='A' minlevel='Info' writeTo='debug2' />
                    <logger name='B' minlevel='Info' writeTo='debug1,debug2' />
                    <logger name='C' minlevel='Info' writeTo='debug1,debug2,debug3' />
                    <logger name='D' minlevel='Info' writeTo='debug1,debug2' />
                    <logger name='D' minlevel='Info' writeTo='debug3,debug4' />
                </rules>
            </nlog>").LogFactory;

            logFactory.GetLogger("D").Info("message");
            logFactory.AssertDebugLastMessage("Debug1", "D");
            logFactory.AssertDebugLastMessage("Debug2", "D");
            logFactory.AssertDebugLastMessage("Debug3", "D");
            logFactory.AssertDebugLastMessage("Debug4", "D");

            logFactory.GetLogger("C").Info("message");
            logFactory.AssertDebugLastMessage("Debug1", "C");
            logFactory.AssertDebugLastMessage("Debug2", "C");
            logFactory.AssertDebugLastMessage("Debug3", "C");
            logFactory.AssertDebugLastMessage("Debug4", "D");

            logFactory.GetLogger("B").Info("message");
            logFactory.AssertDebugLastMessage("Debug1", "B");
            logFactory.AssertDebugLastMessage("Debug2", "B");
            logFactory.AssertDebugLastMessage("Debug3", "C");
            logFactory.AssertDebugLastMessage("Debug4", "D");

            logFactory.GetLogger("A").Info("message");
            logFactory.AssertDebugLastMessage("Debug1", "A");
            logFactory.AssertDebugLastMessage("Debug2", "A");
            logFactory.AssertDebugLastMessage("Debug3", "C");
            logFactory.AssertDebugLastMessage("Debug4", "D");
        }

        [Fact]
        public void MultipleTargetsTest()
        {
            LoggingConfiguration c = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='d1' type='Debug' />
                    <target name='d2' type='Debug' />
                    <target name='d3' type='Debug' />
                    <target name='d4' type='Debug' />
                </targets>

                <rules>
                    <logger name='*' level='Warn' writeTo='d1,d2,d3' />
                </rules>
            </nlog>");

            Assert.Equal(1, c.LoggingRules.Count);
            var rule = c.LoggingRules[0];
            Assert.Equal(3, rule.Targets.Count);
            Assert.Same(c.FindTargetByName("d1"), rule.Targets[0]);
            Assert.Same(c.FindTargetByName("d2"), rule.Targets[1]);
            Assert.Same(c.FindTargetByName("d3"), rule.Targets[2]);
        }

        [Fact]
        public void MultipleTargetsTest_RemoveDuplicate()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>
                    <target name='d1' type='Memory' />
                    <target name='d2' type='Memory' />
                    <target name='d3' type='Memory' />
                </targets>

                <rules>
                    <logger name='*' level='Warn' writeTo='d1,d2,d3,d3' />
                    <logger name='*' level='Warn' writeTo='d3' />
                </rules>
            </nlog>").LogFactory;

            Assert.Equal(2, logFactory.Configuration.LoggingRules.Count);
            Assert.Equal(3, logFactory.Configuration.AllTargets.Count);

            var logger = logFactory.GetCurrentClassLogger();
            logger.Warn("Hello");

            foreach (var target in logFactory.Configuration.AllTargets.OfType<NLog.Targets.MemoryTarget>())
            {
                Assert.Equal(1, target.Logs.Count);
            }
        }

        [Fact]
        public void MultipleRulesSameTargetTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>
                    <target name='d1' type='Debug' layout='${message}' />
                    <target name='d2' type='Debug' layout='${message}' />
                    <target name='d3' type='Debug' layout='${message}' />
                    <target name='d4' type='Debug' layout='${message}' />
                </targets>

                <rules>
                    <logger name='*' level='Warn' writeTo='d1' />
                    <logger name='*' level='Warn' writeTo='d2' />
                    <logger name='*' level='Warn' writeTo='d3' />
                </rules>
            </nlog>").LogFactory;

            var loggerConfig = logFactory.BuildLoggerConfiguration("AAA", logFactory.Configuration?.GetLoggingRulesThreadSafe());
            var targets = loggerConfig[LogLevel.Warn.Ordinal];
            Assert.Equal("d1", targets.Target.Name);
            Assert.Equal("d2", targets.NextInChain.Target.Name);
            Assert.Equal("d3", targets.NextInChain.NextInChain.Target.Name);
            Assert.Null(targets.NextInChain.NextInChain.NextInChain);

            var logger = logFactory.GetLogger("BBB");
            logger.Warn("test1234");

            logFactory.AssertDebugLastMessage("d1", "test1234");
            logFactory.AssertDebugLastMessage("d2", "test1234");
            logFactory.AssertDebugLastMessage("d3", "test1234");
            logFactory.AssertDebugLastMessage("d4", string.Empty);
        }

        [Fact]
        public void ChildRulesTest()
        {
            LoggingConfiguration c = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='d1' type='Debug' />
                    <target name='d2' type='Debug' />
                    <target name='d3' type='Debug' />
                    <target name='d4' type='Debug' />
                </targets>

                <rules>
                    <logger name='*' level='Warn' writeTo='d1,d2,d3'>
                        <logger name='Foo*' writeTo='d4' />
                        <logger name='Bar*' writeTo='d4' />
                    </logger>
                </rules>
            </nlog>");

            Assert.Equal(1, c.LoggingRules.Count);
            var rule = c.LoggingRules[0];
            Assert.Equal(2, rule.ChildRules.Count);
            Assert.Equal("Foo*", rule.ChildRules[0].LoggerNamePattern);
            Assert.Equal("Bar*", rule.ChildRules[1].LoggerNamePattern);
        }

        [Fact]
        public void FiltersTest()
        {
            LoggingConfiguration c = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='d1' type='Debug' />
                    <target name='d2' type='Debug' />
                    <target name='d3' type='Debug' />
                    <target name='d4' type='Debug' />
                </targets>

                <rules>
                    <logger name='*' level='Warn' writeTo='d1,d2,d3'>
                       <filters defaultAction='log'>
                            <when condition=""starts-with(message, 'x')"" action='Ignore' />
                            <when condition=""starts-with(message, 'z')"" action='Ignore' />
                        </filters>
                    </logger>
                </rules>
            </nlog>");

            Assert.Equal(1, c.LoggingRules.Count);
            var rule = c.LoggingRules[0];
            Assert.Equal(2, rule.Filters.Count);
            var conditionBasedFilter = rule.Filters[0] as ConditionBasedFilter;
            Assert.NotNull(conditionBasedFilter);
            Assert.Equal("starts-with(message, 'x')", conditionBasedFilter.Condition.ToString());
            Assert.Equal(FilterResult.Ignore, conditionBasedFilter.Action);

            conditionBasedFilter = rule.Filters[1] as ConditionBasedFilter;
            Assert.NotNull(conditionBasedFilter);
            Assert.Equal("starts-with(message, 'z')", conditionBasedFilter.Condition.ToString());
            Assert.Equal(FilterResult.Ignore, conditionBasedFilter.Action);
        }

        [Fact]
        public void FiltersTest_ignoreFinal()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>
                    <target name='d1' type='Debug' layout='${message}' />
                    <target name='d2' type='Debug' layout='${message}' />
                </targets>

                <rules>
                    <logger name='*' level='Warn' writeTo='d1'>
                        <filters defaultAction='log'>
                            <when condition=""starts-with(message, 'x')"" action='IgnoreFinal' />
                        </filters>
                    </logger>
                     <logger name='*' level='Warn' writeTo='d2'>
                    </logger>
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("logger1");
            logger.Warn("test 1");
            AssertDebugLastMessage("d1", "test 1", logFactory);
            AssertDebugLastMessage("d2", "test 1", logFactory);

            logger.Warn("x-mass");
            AssertDebugLastMessage("d1", "test 1", logFactory);
            AssertDebugLastMessage("d2", "test 1", logFactory);
        }

        [Fact]
        public void FiltersTest_logFinal()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>
                    <target name='d1' type='Debug' layout='${message}' />
                    <target name='d2' type='Debug' layout='${message}' />
                </targets>

                <rules>
                    <logger name='*' level='Warn' writeTo='d1'>
                        <filters>
                            <when condition=""starts-with(message, 'x')"" action='LogFinal' />                      
                        </filters>
                    </logger>
                     <logger name='*' level='Warn' writeTo='d2'>
                    </logger>
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("logger1");
            logger.Warn("test 1");
            AssertDebugLastMessage("d1", "", logFactory);
            AssertDebugLastMessage("d2", "test 1", logFactory);

            logger.Warn("x-mass");
            AssertDebugLastMessage("d1", "x-mass", logFactory);
            AssertDebugLastMessage("d2", "test 1", logFactory);
        }


        [Fact]
        public void FiltersTest_ignore()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>
                    <target name='d1' type='Debug' layout='${message}' />
                    <target name='d2' type='Debug' layout='${message}' />
                </targets>

                <rules>
                    <logger name='*' level='Warn' writeTo='d1'>
                        <filters defaultAction='log'>
                            <when condition=""starts-with(message, 'x')"" action='Ignore' />
                      
                        </filters>
                    </logger>
                     <logger name='*' level='Warn' writeTo='d2'>
                    </logger>
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("logger1");
            logger.Warn("test 1");
            AssertDebugLastMessage("d1", "test 1", logFactory);
            AssertDebugLastMessage("d2", "test 1", logFactory);

            logger.Warn("x-mass");
            AssertDebugLastMessage("d1", "test 1", logFactory);
            AssertDebugLastMessage("d2", "x-mass", logFactory);
        }

        [Fact]
        public void FiltersTest_DefaultAction()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>
                    <target name='d1' type='Debug' layout='${message}' />
                </targets>

                <rules>
                    <logger name='*' level='Warn' writeTo='d1'>
                        <filters>
                            <when condition=""starts-with(message, 't')"" action='Log' />
                        </filters>
                    </logger>
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("logger1");
            logger.Warn("test 1");
            AssertDebugLastMessage("d1", "test 1", logFactory);

            logger.Warn("x-mass");
            AssertDebugLastMessage("d1", "test 1", logFactory);
        }

        [Fact]
        public void FiltersTest_FilterDefaultAction()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>
                    <target name='d1' type='Debug' layout='${message}' />
                </targets>

                <rules>
                    <logger name='*' level='Warn' writeTo='d1'>
                        <filters filterDefaultAction='Ignore'>
                            <filter type='when' condition=""starts-with(message, 't')"" action='Log' />
                        </filters>
                    </logger>
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("logger1");
            logger.Warn("test 1");
            AssertDebugLastMessage("d1", "test 1", logFactory);

            logger.Warn("x-mass");
            AssertDebugLastMessage("d1", "test 1", logFactory);
        }

        [Fact]
        public void FiltersTest_DefaultAction_noRules()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>
                    <target name='d1' type='Debug' layout='${message}' />
                </targets>

                <rules>
                    <logger name='*' level='Warn' writeTo='d1'>
                       <filters>
                      
                        </filters>
                    </logger>
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("logger1");
            logger.Warn("test 1");
            AssertDebugLastMessage("d1", "test 1", logFactory);
        }

        [Fact]
        public void LoggingRule_Final_SuppressesOnlyMatchingLevels()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>
                    <target name='d1' type='Debug' layout='${message}' />
                </targets>

                <rules>
                    <logger name='a' level='Debug' final='true' />
                    <logger name='*' minlevel='Debug' writeTo='d1' />
                </rules>
            </nlog>").LogFactory;

            Logger a = logFactory.GetLogger("a");
            Assert.False(a.IsDebugEnabled);
            Assert.True(a.IsInfoEnabled);
            a.Info("testInfo");
            a.Debug("suppressedDebug");
            AssertDebugLastMessage("d1", "testInfo", logFactory);

            Logger b = logFactory.GetLogger("b");
            b.Debug("testDebug");
            AssertDebugLastMessage("d1", "testDebug", logFactory);
        }

        [Fact]
        public void UnusedTargetsShouldBeLoggedToInternalLogger()
        {
            string tempFileName = Path.GetTempFileName();

            try
            {
                var config = XmlLoggingConfiguration.CreateFromXmlString("<nlog internalLogFile='" + tempFileName + @"' internalLogLevel='Warn'>
                    <targets>
                        <target name='d1' type='Debug' />
                        <target name='d2' type='Debug' />
                        <target name='d3' type='Debug' />
                        <target name='d4' type='Debug' />
                        <target name='d5' type='Debug' />
                    </targets>

                    <rules>
                           <logger name='*' level='Debug' writeTo='d1' />
                           <logger name='*' level='Debug' writeTo='d1,d2,d3' />
                    </rules>
                </nlog>");

                var logFactory = new LogFactory();
                logFactory.Configuration = config;

                AssertFileContains(tempFileName, "Unused target detected. Add a rule for this target to the configuration. TargetName: d4", Encoding.UTF8);

                AssertFileContains(tempFileName, "Unused target detected. Add a rule for this target to the configuration. TargetName: d5", Encoding.UTF8);
            }
            finally
            {
                NLog.Common.InternalLogger.Reset();
                if (File.Exists(tempFileName))
                {
                    File.Delete(tempFileName);
                }
            }
        }

        [Fact]
        public void UnusedTargetsShouldBeLoggedToInternalLogger_PermitWrapped()
        {
            string tempFileName = Path.GetTempFileName();

            try
            {
                var logFactory = new LogFactory().Setup()
                    .SetupExtensions(ext => ext.RegisterTarget<Targets.Mocks.MockTargetWrapper >())
                    .LoadConfigurationFromXml("<nlog internalLogFile='" + tempFileName + @"' internalLogLevel='Warn'>
                    <targets async='true'>
                        <target name='d1' type='Debug' />
                        <target name='d2' type='MockWrapper'>
                            <target name='d3' type='Debug' />
                        </target>
                        <target name='d4' type='Debug' />
                        <target name='d5' type='Debug' />
                    </targets>

                    <rules>
                           <logger name='*' level='Debug' writeTo='d1' />
                           <logger name='*' level='Debug' writeTo='d1,d2,d4' />
                    </rules>
                </nlog>");

                AssertFileNotContains(tempFileName, "Unused target detected. Add a rule for this target to the configuration. TargetName: d2", Encoding.UTF8);

                AssertFileNotContains(tempFileName, "Unused target detected. Add a rule for this target to the configuration. TargetName: d3", Encoding.UTF8);

                AssertFileNotContains(tempFileName, "Unused target detected. Add a rule for this target to the configuration. TargetName: d4", Encoding.UTF8);

                AssertFileContains(tempFileName, "Unused target detected. Add a rule for this target to the configuration. TargetName: d5", Encoding.UTF8);
            }
            finally
            {
                NLog.Common.InternalLogger.Reset();
                if (File.Exists(tempFileName))
                {
                    File.Delete(tempFileName);
                }
            }
        }

        [Fact]
        public void LoggingRule_LevelOff_NotSetAsActualLogLevel()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>
                    <target name='l1' type='Debug' layout='${message}' />
                    <target name='l2' type='Debug' layout='${message}' />
                </targets>

                <rules>
                    <logger name='a' level='Off' appendTo='l1' />
                    <logger name='a' minlevel='Debug' appendTo='l2' />
                </rules>
            </nlog>").LogFactory;

            var c = logFactory.Configuration;
            logFactory.GetLogger("a");

            Assert.Equal(2, c.LoggingRules.Count);
            Assert.False(c.LoggingRules[0].IsLoggingEnabledForLevel(LogLevel.Off), "Log level Off should always return false.");
            // The two functions below should not throw an exception.
            c.LoggingRules[0].EnableLoggingForLevel(LogLevel.Debug);
            c.LoggingRules[0].DisableLoggingForLevel(LogLevel.Debug);
        }

        [Theory]
        [MemberData(nameof(LoggingRule_LevelsLayout_ParseLevel_TestCases))]
        public void LoggingRule_LevelsLayout_ParseLevel(string levelsVariable, LogLevel[] expectedLevels)
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
                <nlog>"
    + (levelsVariable != null ? $"<variable name='var_levels' value='{levelsVariable}'/>" : "") +
    @"<targets>
                        <target name='d1' type='Debug' layout='${message}' />
                    </targets>
                    <rules>
                        <logger name='*' levels='${var:var_levels}' writeTo='d1' />
                    </rules>
                </nlog>").LogFactory;

            var logger = logFactory.GetLogger(nameof(LoggingRule_LevelsLayout_ParseLevel));

            AssertLogLevelEnabled(logger, expectedLevels);

            // Verify that runtime override also works
            logFactory.Configuration.Variables["var_levels"] = LogLevel.Fatal.ToString();
            logFactory.ReconfigExistingLoggers();

            AssertLogLevelEnabled(logger, LogLevel.Fatal);
        }

        public static IEnumerable<object[]> LoggingRule_LevelsLayout_ParseLevel_TestCases()
        {
            yield return new object[] { "Off", new[] { LogLevel.Off } };
            yield return new object[] { "Off, Trace", new[] { LogLevel.Off, LogLevel.Trace } };
            yield return new object[] { " ", new[] { LogLevel.Off } };
            yield return new object[] { " , Debug", new[] { LogLevel.Off, LogLevel.Debug } };
            yield return new object[] { null, new[] { LogLevel.Off } };
            yield return new object[] { "", new[] { LogLevel.Off } };
            yield return new object[] { ",Info", new[] { LogLevel.Off, LogLevel.Info } };
            yield return new object[] { "Error, Error", new[] { LogLevel.Error, LogLevel.Error } };
            yield return new object[] { " error", new[] { LogLevel.Error } };
            yield return new object[] { " error, Warn", new[] { LogLevel.Error, LogLevel.Warn } };
            yield return new object[] { "Wrong", new[] { LogLevel.Off } };
            yield return new object[] { "Wrong, Fatal", new[] { LogLevel.Off, LogLevel.Fatal } };
            yield return new object[] { "Trace", new[] { LogLevel.Trace } };
            yield return new object[] { "Debug", new[] { LogLevel.Debug } };
            yield return new object[] { "Info", new[] { LogLevel.Info } };
            yield return new object[] { "Warn", new[] { LogLevel.Warn } };
            yield return new object[] { "Error", new[] { LogLevel.Error } };
            yield return new object[] { "Fatal", new[] { LogLevel.Fatal } };
        }

        [Theory]
        [MemberData(nameof(LoggingRule_FinalMinLevel_TestCases))]
        public void LoggingRule_FinalMinLevelLayoutAsVar_EnablesExpectedLevels(string levelVariable, LogLevel[] expectedLevels)
        {
            LogFactory logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>"
                + (levelVariable != null ? $"<variable name='var_level' value='{levelVariable}'/>" : "") +
                @"<targets>
                    <target name='d1' type='Debug' layout='${message}' />
                </targets>
                <rules>
                    <logger name='*' finalMinLevel='${var:var_level}' writeTo='d1' />
                </rules>
            </nlog>").LogFactory;

            Logger logger = logFactory.GetLogger(nameof(LoggingRule_FinalMinLevelLayoutAsVar_EnablesExpectedLevels));

            AssertLogLevelEnabled(logger, expectedLevels);

            // Verify that runtime override also works
            logFactory.Configuration.Variables["var_level"] = LogLevel.Fatal.ToString();
            logFactory.ReconfigExistingLoggers();

            AssertLogLevelEnabled(logger, LogLevel.Fatal);
        }

        public static IEnumerable<object[]> LoggingRule_FinalMinLevel_TestCases()
        {
            yield return new object[] { "Off", new[] { LogLevel.Off } };
            yield return new object[] { "Wrong", new[] { LogLevel.Off } };
            yield return new object[] { " ", new[] { LogLevel.Off } };
            yield return new object[] { "", new[] { LogLevel.Off } };
            yield return new object[] { "Trace", new[] { LogLevel.Trace, LogLevel.Debug, LogLevel.Info, LogLevel.Warn, LogLevel.Error, LogLevel.Fatal } };
            yield return new object[] { "Debug", new[] { LogLevel.Debug, LogLevel.Info, LogLevel.Warn, LogLevel.Error, LogLevel.Fatal } };
            yield return new object[] { "Info", new[] { LogLevel.Info, LogLevel.Warn, LogLevel.Error, LogLevel.Fatal } };
            yield return new object[] { "Warn", new[] { LogLevel.Warn, LogLevel.Error, LogLevel.Fatal } };
            yield return new object[] { "Error", new[] { LogLevel.Error, LogLevel.Fatal } };
            yield return new object[] { "Fatal", new[] { LogLevel.Fatal } };
            yield return new object[] { " FataL ", new[] { LogLevel.Fatal } };
        }

        [Theory]
        [MemberData(nameof(LoggingRule_MinMaxLayout_ParseLevel_TestCases))]
        public void LoggingRule_MinMaxLayout_ParseLevel(string minLevel, string maxLevel, LogLevel[] expectedLevels)
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>"
                + (!string.IsNullOrEmpty(minLevel) ? $"<variable name='var_minlevel' value='{minLevel}'/>" : "")
                + (!string.IsNullOrEmpty(maxLevel) ? $"<variable name='var_maxlevel' value='{maxLevel}'/>" : "") +
                @"<targets>
                    <target name='d1' type='Debug' layout='${message}' />
                </targets>
                <rules>
                    <logger name='*' minlevel='${var:var_minlevel}' maxlevel='${var:var_maxlevel}' writeTo='d1' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger(nameof(LoggingRule_MinMaxLayout_ParseLevel));

            AssertLogLevelEnabled(logger, expectedLevels);

            // Verify that runtime override also works
            logFactory.Configuration.Variables["var_minlevel"] = LogLevel.Fatal.ToString();
            logFactory.Configuration.Variables["var_maxlevel"] = LogLevel.Fatal.ToString();
            logFactory.ReconfigExistingLoggers();

            AssertLogLevelEnabled(logger, LogLevel.Fatal);
        }

        public static IEnumerable<object[]> LoggingRule_MinMaxLayout_ParseLevel_TestCases()
        {
            yield return new object[] { "Off", "", ArrayHelper.Empty<LogLevel>() };
            yield return new object[] { "Off", "Fatal", ArrayHelper.Empty<LogLevel>() };
            yield return new object[] { "Error", "Debug", ArrayHelper.Empty<LogLevel>() };
            yield return new object[] { " ", "", ArrayHelper.Empty<LogLevel>() };
            yield return new object[] { " ", "Fatal", ArrayHelper.Empty<LogLevel>() };
            yield return new object[] { "", "", ArrayHelper.Empty<LogLevel>() };
            yield return new object[] { "", "Off", new[] { LogLevel.Trace, LogLevel.Debug, LogLevel.Info, LogLevel.Warn, LogLevel.Error, LogLevel.Fatal } };
            yield return new object[] { "", "Fatal", new[] { LogLevel.Trace, LogLevel.Debug, LogLevel.Info, LogLevel.Warn, LogLevel.Error, LogLevel.Fatal } };
            yield return new object[] { "", "Debug", new[] { LogLevel.Trace, LogLevel.Debug } };
            yield return new object[] { "", "Trace", new[] { LogLevel.Trace } };
            yield return new object[] { "", " error", new[] { LogLevel.Trace, LogLevel.Debug, LogLevel.Info, LogLevel.Warn, LogLevel.Error } };
            yield return new object[] { "", "Wrong", ArrayHelper.Empty<LogLevel>() };
            yield return new object[] { "Wrong", "", ArrayHelper.Empty<LogLevel>() };
            yield return new object[] { "Wrong", "Fatal", ArrayHelper.Empty<LogLevel>() };
            yield return new object[] { " error", "Debug", ArrayHelper.Empty<LogLevel>() };
            yield return new object[] { " error", "Fatal", new[] { LogLevel.Error, LogLevel.Fatal } };
            yield return new object[] { " error", "", new[] { LogLevel.Error, LogLevel.Fatal } };
            yield return new object[] { "Error", "", new[] { LogLevel.Error, LogLevel.Fatal } };
            yield return new object[] { "Fatal", "", new[] { LogLevel.Fatal } };
            yield return new object[] { "Off", "", ArrayHelper.Empty<LogLevel>() };
            yield return new object[] { "Trace", " ", ArrayHelper.Empty<LogLevel>() };
            yield return new object[] { "Trace", "", new[] { LogLevel.Trace, LogLevel.Debug, LogLevel.Info, LogLevel.Warn, LogLevel.Error, LogLevel.Fatal } };
            yield return new object[] { "Trace", "Debug", new[] { LogLevel.Trace, LogLevel.Debug } };
            yield return new object[] { "Trace", "Trace", new[] { LogLevel.Trace, LogLevel.Trace } };
        }

        private static void AssertLogLevelEnabled(ILogger logger, LogLevel expectedLogLevel)
        {
            AssertLogLevelEnabled(logger, new[] { expectedLogLevel });
        }

        private static void AssertLogLevelEnabled(ILogger logger, LogLevel[] expectedLogLevels)
        {
            for (int i = LogLevel.MinLevel.Ordinal; i <= LogLevel.MaxLevel.Ordinal; ++i)
            {
                var logLevel = LogLevel.FromOrdinal(i);
                if (expectedLogLevels.Contains(logLevel))
                    Assert.True(logger.IsEnabled(logLevel), $"{logLevel} expected as true");
                else
                    Assert.False(logger.IsEnabled(logLevel), $"{logLevel} expected as false");
            }
        }
    }
}

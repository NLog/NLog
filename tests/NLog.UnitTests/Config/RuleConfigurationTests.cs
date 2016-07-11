// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.IO;
    using System.Text;
    using NLog.Config;
    using NLog.Filters;
    using Xunit;

    public class RuleConfigurationTests : NLogTestBase
    {
        [Fact]
        public void NoRulesTest()
        {
            LoggingConfiguration c = CreateConfigurationFromString(@"
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
            LoggingConfiguration c = CreateConfigurationFromString(@"
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
            Assert.Equal(4, rule.Levels.Count);
            Assert.True(rule.Levels.Contains(LogLevel.Info));
            Assert.True(rule.Levels.Contains(LogLevel.Warn));
            Assert.True(rule.Levels.Contains(LogLevel.Error));
            Assert.True(rule.Levels.Contains(LogLevel.Fatal));
            Assert.Equal(1, rule.Targets.Count);
            Assert.Same(c.FindTargetByName("d1"), rule.Targets[0]);
            Assert.False(rule.Final);
            Assert.Equal(0, rule.Filters.Count);
        }

        [Fact]
        public void SingleLevelTest()
        {
            LoggingConfiguration c = CreateConfigurationFromString(@"
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
            Assert.Equal(1, rule.Levels.Count);
            Assert.True(rule.Levels.Contains(LogLevel.Warn));
        }

        [Fact]
        public void MinMaxLevelTest()
        {
            LoggingConfiguration c = CreateConfigurationFromString(@"
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
            Assert.True(rule.Levels.Contains(LogLevel.Info));
            Assert.True(rule.Levels.Contains(LogLevel.Warn));
        }

        [Fact]
        public void NoLevelsTest()
        {
            LoggingConfiguration c = CreateConfigurationFromString(@"
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
            Assert.True(rule.Levels.Contains(LogLevel.Trace));
            Assert.True(rule.Levels.Contains(LogLevel.Debug));
            Assert.True(rule.Levels.Contains(LogLevel.Info));
            Assert.True(rule.Levels.Contains(LogLevel.Warn));
            Assert.True(rule.Levels.Contains(LogLevel.Error));
            Assert.True(rule.Levels.Contains(LogLevel.Fatal));
        }

        [Fact]
        public void ExplicitLevelsTest()
        {
            LoggingConfiguration c = CreateConfigurationFromString(@"
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
            Assert.True(rule.Levels.Contains(LogLevel.Trace));
            Assert.True(rule.Levels.Contains(LogLevel.Info));
            Assert.True(rule.Levels.Contains(LogLevel.Warn));
        }

        [Fact]
        public void MultipleTargetsTest()
        {
            LoggingConfiguration c = CreateConfigurationFromString(@"
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
        public void MultipleRulesSameTargetTest()
        {
            LoggingConfiguration c = CreateConfigurationFromString(@"
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
            </nlog>");

            LogFactory factory = new LogFactory(c);
            var loggerConfig = factory.GetConfigurationForLogger("AAA", c);
            var targets = loggerConfig.GetTargetsForLevel(LogLevel.Warn);
            Assert.Equal("d1", targets.Target.Name);
            Assert.Equal("d2", targets.NextInChain.Target.Name);
            Assert.Equal("d3", targets.NextInChain.NextInChain.Target.Name);
            Assert.Null(targets.NextInChain.NextInChain.NextInChain);

            LogManager.Configuration = c;

            var logger = LogManager.GetLogger("BBB");
            logger.Warn("test1234");

            this.AssertDebugLastMessage("d1", "test1234");
            this.AssertDebugLastMessage("d2", "test1234");
            this.AssertDebugLastMessage("d3", "test1234");
            this.AssertDebugLastMessage("d4", string.Empty);
        }

        [Fact]
        public void ChildRulesTest()
        {
            LoggingConfiguration c = CreateConfigurationFromString(@"
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
            LoggingConfiguration c = CreateConfigurationFromString(@"
            <nlog>
                <targets>
                    <target name='d1' type='Debug' />
                    <target name='d2' type='Debug' />
                    <target name='d3' type='Debug' />
                    <target name='d4' type='Debug' />
                </targets>

                <rules>
                    <logger name='*' level='Warn' writeTo='d1,d2,d3'>
                        <filters>
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
            LoggingConfiguration c = CreateConfigurationFromString(@"
            <nlog>
                <targets>
                    <target name='d1' type='Debug' layout='${message}' />
                    <target name='d2' type='Debug' layout='${message}' />
                </targets>

                <rules>
                    <logger name='*' level='Warn' writeTo='d1'>
                        <filters>
                            <when condition=""starts-with(message, 'x')"" action='IgnoreFinal' />
                      
                        </filters>
                    </logger>
                     <logger name='*' level='Warn' writeTo='d2'>
                    </logger>
                </rules>
            </nlog>");

            LogManager.Configuration = c;
            var logger = LogManager.GetLogger("logger1");
            logger.Warn("test 1");
            AssertDebugLastMessage("d1", "test 1");
            AssertDebugLastMessage("d2", "test 1");

            logger.Warn("x-mass");
            AssertDebugLastMessage("d1", "test 1");
            AssertDebugLastMessage("d2", "test 1");
        }

        [Fact]
        public void FiltersTest_logFinal()
        {
            LoggingConfiguration c = CreateConfigurationFromString(@"
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
            </nlog>");

            LogManager.Configuration = c;
            var logger = LogManager.GetLogger("logger1");
            logger.Warn("test 1");
            AssertDebugLastMessage("d1", "test 1");
            AssertDebugLastMessage("d2", "test 1");

            logger.Warn("x-mass");
            AssertDebugLastMessage("d1", "x-mass");
            AssertDebugLastMessage("d2", "test 1");
        }


        [Fact]
        public void FiltersTest_ignore()
        {
            LoggingConfiguration c = CreateConfigurationFromString(@"
            <nlog>
                <targets>
                    <target name='d1' type='Debug' layout='${message}' />
                    <target name='d2' type='Debug' layout='${message}' />
                </targets>

                <rules>
                    <logger name='*' level='Warn' writeTo='d1'>
                        <filters>
                            <when condition=""starts-with(message, 'x')"" action='Ignore' />
                      
                        </filters>
                    </logger>
                     <logger name='*' level='Warn' writeTo='d2'>
                    </logger>
                </rules>
            </nlog>");

            LogManager.Configuration = c;
            var logger = LogManager.GetLogger("logger1");
            logger.Warn("test 1");
            AssertDebugLastMessage("d1", "test 1");
            AssertDebugLastMessage("d2", "test 1");

            logger.Warn("x-mass");
            AssertDebugLastMessage("d1", "test 1");
            AssertDebugLastMessage("d2", "x-mass");

        }

        [Fact]
        public void LoggingRule_Final_SuppressesOnlyMatchingLevels()
        {
            LoggingConfiguration c = CreateConfigurationFromString(@"
            <nlog>
                <targets>
                    <target name='d1' type='Debug' layout='${message}' />
                </targets>

                <rules>
                    <logger name='a' level='Debug' final='true' />
                    <logger name='*' minlevel='Debug' writeTo='d1' />
                </rules>
            </nlog>");

            LogManager.Configuration = c;
            Logger a = LogManager.GetLogger("a");
            Assert.False(a.IsDebugEnabled);
            Assert.True(a.IsInfoEnabled);
            a.Info("testInfo");
            a.Debug("suppressedDebug");
            AssertDebugLastMessage("d1", "testInfo");

            Logger b = LogManager.GetLogger("b");
            b.Debug("testDebug");
            AssertDebugLastMessage("d1", "testDebug");
        }

        [Fact]
        public void UnusedTargetsShouldBeLoggedToInternalLogger()
        {
            string tempFileName = Path.GetTempFileName();

            try
            {
                CreateConfigurationFromString(
                "<nlog internalLogFile='" + tempFileName + @"' internalLogLevel='Warn'>
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

                AssertFileContains(tempFileName, "Unused target detected. Add a rule for this target to the configuration. TargetName: d4", Encoding.UTF8);

                AssertFileContains(tempFileName, "Unused target detected. Add a rule for this target to the configuration. TargetName: d5", Encoding.UTF8);
            }
            finally
            {
                if (File.Exists(tempFileName))
                {
                    File.Delete(tempFileName);
                }
            }
        }
                
        [Fact]
        public void LoggingRule_LevelOff_NotSetAsActualLogLevel()
        {
            LoggingConfiguration c = CreateConfigurationFromString(@"
            <nlog>
                <targets>
                    <target name='l1' type='Debug' layout='${message}' />
                    <target name='l2' type='Debug' layout='${message}' />
                </targets>

                <rules>
                    <logger name='a' level='Off' appendTo='l1' />
                    <logger name='a' minlevel='Debug' appendTo='l2' />
                </rules>
            </nlog>");

            LogManager.Configuration = c;
            Logger a = LogManager.GetLogger("a");

            Assert.True(c.LoggingRules.Count == 2, "All rules should have been loaded.");
            Assert.False(c.LoggingRules[0].IsLoggingEnabledForLevel(LogLevel.Off), "Log level Off should always return false.");
            // The two functions below should not throw an exception.
            c.LoggingRules[0].EnableLoggingForLevel(LogLevel.Debug);
            c.LoggingRules[0].DisableLoggingForLevel(LogLevel.Debug);
        }
    }
}
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

namespace NLog.UnitTests.Config
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using NLog.Config;
    using NLog.Filters;
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
            Assert.Equal(FilterResult.Ignore, rule.DefaultFilterResult);
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
        public void MultipleRulesSameTargetTest()
        {
            LogFactory logFactory = new LogFactory();
            LoggingConfiguration c = XmlLoggingConfiguration.CreateFromXmlString(@"
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
            </nlog>", logFactory);

            logFactory.Configuration = c;
            var loggerConfig = logFactory.GetConfigurationForLogger("AAA", c);
            var targets = loggerConfig.GetTargetsForLevel(LogLevel.Warn);
            Assert.Equal("d1", targets.Target.Name);
            Assert.Equal("d2", targets.NextInChain.Target.Name);
            Assert.Equal("d3", targets.NextInChain.NextInChain.Target.Name);
            Assert.Null(targets.NextInChain.NextInChain.NextInChain);

            LogManager.Configuration = c;

            var logger = LogManager.GetLogger("BBB");
            logger.Warn("test1234");

            AssertDebugLastMessage("d1", "test1234");
            AssertDebugLastMessage("d2", "test1234");
            AssertDebugLastMessage("d3", "test1234");
            AssertDebugLastMessage("d4", string.Empty);
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
            LoggingConfiguration c = XmlLoggingConfiguration.CreateFromXmlString(@"
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
            LoggingConfiguration c = XmlLoggingConfiguration.CreateFromXmlString(@"
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
            AssertDebugLastMessage("d1", "");
            AssertDebugLastMessage("d2", "test 1");

            logger.Warn("x-mass");
            AssertDebugLastMessage("d1", "x-mass");
            AssertDebugLastMessage("d2", "test 1");
        }


        [Fact]
        public void FiltersTest_ignore()
        {
            LoggingConfiguration c = XmlLoggingConfiguration.CreateFromXmlString(@"
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
        public void FiltersTest_defaultFilterAction()
        {
            LoggingConfiguration c = XmlLoggingConfiguration.CreateFromXmlString(@"
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
            </nlog>");

            LogManager.Configuration = c;
            var logger = LogManager.GetLogger("logger1");
            logger.Warn("test 1");
            AssertDebugLastMessage("d1", "test 1");

            logger.Warn("x-mass");
            AssertDebugLastMessage("d1", "test 1");
        }

        [Fact]
        public void FiltersTest_defaultFilterAction_noRules()
        {
            LoggingConfiguration c = XmlLoggingConfiguration.CreateFromXmlString(@"
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
            </nlog>");

            LogManager.Configuration = c;
            var logger = LogManager.GetLogger("logger1");
            logger.Warn("test 1");
            AssertDebugLastMessage("d1", "test 1");
        }

        [Fact]
        public void LoggingRule_Final_SuppressesOnlyMatchingLevels()
        {
            LoggingConfiguration c = XmlLoggingConfiguration.CreateFromXmlString(@"
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
                var config = XmlLoggingConfiguration.CreateFromXmlString("<nlog internalLogFile='" + tempFileName + @"' internalLogLevel='Warn'>
                    <extensions>
                        <add assembly='NLog.UnitTests'/> 
                    </extensions>
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

                var logFactory = new LogFactory();
                logFactory.Configuration = config;

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
            LoggingConfiguration c = XmlLoggingConfiguration.CreateFromXmlString(@"
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
            LogManager.GetLogger("a");

            Assert.Equal(2, c.LoggingRules.Count);
            Assert.False(c.LoggingRules[0].IsLoggingEnabledForLevel(LogLevel.Off), "Log level Off should always return false.");
            // The two functions below should not throw an exception.
            c.LoggingRules[0].EnableLoggingForLevel(LogLevel.Debug);
            c.LoggingRules[0].DisableLoggingForLevel(LogLevel.Debug);
        }

        [Theory]
        [InlineData("Off")]
        [InlineData("")]
        [InlineData((string)null)]
        [InlineData("Trace")]
        [InlineData("Debug")]
        [InlineData("Info")]
        [InlineData("Warn")]
        [InlineData("Error")]
        [InlineData(" error")]
        [InlineData("Fatal")]
        [InlineData("Wrong")]
        public void LoggingRule_LevelLayout_ParseLevel(string levelVariable)
        {
            var config = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>"
                + (levelVariable != null ? $"<variable name='var_level' value='{levelVariable}'/>" : "") +
                @"<targets>
                    <target name='d1' type='Debug' layout='${message}' />
                </targets>
                <rules>
                    <logger name='*' level='${var:var_level}' writeTo='d1' />
                </rules>
            </nlog>");

            LogManager.Configuration = config;
            Logger logger = LogManager.GetLogger(nameof(LoggingRule_LevelLayout_ParseLevel));

            LogLevel expectedLogLevel = (NLog.Internal.StringHelpers.IsNullOrWhiteSpace(levelVariable) || levelVariable == "Wrong") ? LogLevel.Off : LogLevel.FromString(levelVariable.Trim());

            AssertLogLevelEnabled(logger, expectedLogLevel);

            // Verify that runtime override also works
            LogManager.Configuration.Variables["var_level"] = LogLevel.Fatal.ToString();
            LogManager.ReconfigExistingLoggers();

            AssertLogLevelEnabled(logger, LogLevel.Fatal);
        }

        [Theory]
        [MemberData(nameof(LoggingRule_LevelsLayout_ParseLevel_TestCases))]
        public void LoggingRule_LevelsLayout_ParseLevel(string levelsVariable, LogLevel[] expectedLevels)
        {
            var config = XmlLoggingConfiguration.CreateFromXmlString(@"
                <nlog>"
    + (!string.IsNullOrEmpty(levelsVariable) ? $"<variable name='var_levels' value='{levelsVariable}'/>" : "") +
    @"<targets>
                        <target name='d1' type='Debug' layout='${message}' />
                    </targets>
                    <rules>
                        <logger name='*' levels='${var:var_levels}' writeTo='d1' />
                    </rules>
                </nlog>");

            LogManager.Configuration = config;
            var logger = LogManager.GetLogger(nameof(LoggingRule_LevelsLayout_ParseLevel));

            AssertLogLevelEnabled(logger, expectedLevels);

            // Verify that runtime override also works
            LogManager.Configuration.Variables["var_levels"] = LogLevel.Fatal.ToString();
            LogManager.ReconfigExistingLoggers();

            AssertLogLevelEnabled(logger, LogLevel.Fatal);
        }

        public static IEnumerable<object[]> LoggingRule_LevelsLayout_ParseLevel_TestCases()
        {
            yield return new object[] { "Off", new[] { LogLevel.Off } };
            yield return new object[] { "Off, Trace", new[] { LogLevel.Off, LogLevel.Trace } };
            yield return new object[] { " ", new[] { LogLevel.Off } };
            yield return new object[] { " , Debug", new[] { LogLevel.Off, LogLevel.Debug } };
            yield return new object[] { "", new[] { LogLevel.Off } };
            yield return new object[] { ",Info", new[] { LogLevel.Off, LogLevel.Info } };
            yield return new object[] { "Error, Error", new[] { LogLevel.Error, LogLevel.Error } };
            yield return new object[] { " error", new[] { LogLevel.Error } };
            yield return new object[] { " error, Warn", new[] { LogLevel.Error, LogLevel.Warn } };
            yield return new object[] { "Wrong", new[] { LogLevel.Off } };
            yield return new object[] { "Wrong, Fatal", new[] { LogLevel.Off, LogLevel.Fatal } };
        }

        [Theory]
        [MemberData(nameof(LoggingRule_MinMaxLayout_ParseLevel_TestCases2))]
        public void LoggingRule_MinMaxLayout_ParseLevel(string minLevel, string maxLevel, LogLevel[] expectedLevels)
        {
            var config = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>"
                + (!string.IsNullOrEmpty(minLevel) ? $"<variable name='var_minlevel' value='{minLevel}'/>" : "")
                + (!string.IsNullOrEmpty(maxLevel) ? $"<variable name='var_maxlevel' value='{maxLevel}'/>" : "") +
                @"<targets>
                    <target name='d1' type='Debug' layout='${message}' />
                </targets>
                <rules>
                    <logger name='*' minlevel='${var:var_minlevel}' maxlevel='${var:var_maxlevel}' writeTo='d1' />
                </rules>
            </nlog>");

            LogManager.Configuration = config;
            var logger = LogManager.GetLogger(nameof(LoggingRule_MinMaxLayout_ParseLevel));

            AssertLogLevelEnabled(logger, expectedLevels);

            // Verify that runtime override also works
            LogManager.Configuration.Variables["var_minlevel"] = LogLevel.Fatal.ToString();
            LogManager.Configuration.Variables["var_maxlevel"] = LogLevel.Fatal.ToString();
            LogManager.ReconfigExistingLoggers();

            AssertLogLevelEnabled(logger, LogLevel.Fatal);
        }

        public static IEnumerable<object[]> LoggingRule_MinMaxLayout_ParseLevel_TestCases2()
        {
            yield return new object[] { "Off", "", new LogLevel[] { } };
            yield return new object[] { "Off", "Fatal", new LogLevel[] { } };
            yield return new object[] { "Error", "Debug", new LogLevel[] { } };
            yield return new object[] { " ", "", new LogLevel[] { } };
            yield return new object[] { " ", "Fatal", new LogLevel[] { } };
            yield return new object[] { "", "", new LogLevel[] { } };
            yield return new object[] { "", "Off", new[] { LogLevel.Trace, LogLevel.Debug, LogLevel.Info, LogLevel.Warn, LogLevel.Error, LogLevel.Fatal } };
            yield return new object[] { "", "Fatal", new[] { LogLevel.Trace, LogLevel.Debug, LogLevel.Info, LogLevel.Warn, LogLevel.Error, LogLevel.Fatal } };
            yield return new object[] { "", "Debug", new[] { LogLevel.Trace, LogLevel.Debug } };
            yield return new object[] { "", "Trace", new[] { LogLevel.Trace } };
            yield return new object[] { "", " error", new[] { LogLevel.Trace, LogLevel.Debug, LogLevel.Info, LogLevel.Warn, LogLevel.Error } };
            yield return new object[] { "", "Wrong", new LogLevel[] { } };
            yield return new object[] { "Wrong", "", new LogLevel[] { } };
            yield return new object[] { "Wrong", "Fatal", new LogLevel[] { } };
            yield return new object[] { " error", "Debug", new LogLevel[] { } };
            yield return new object[] { " error", "Fatal", new[] { LogLevel.Error, LogLevel.Fatal } };
            yield return new object[] { " error", "", new[] { LogLevel.Error, LogLevel.Fatal } };
            yield return new object[] { "Error", "", new[] { LogLevel.Error, LogLevel.Fatal } };
            yield return new object[] { "Fatal", "", new[] { LogLevel.Fatal } };
            yield return new object[] { "Off", "", new LogLevel[] { } };
            yield return new object[] { "Trace", " ", new LogLevel[] { } };
            yield return new object[] { "Trace", "", new[] { LogLevel.Trace, LogLevel.Debug, LogLevel.Info, LogLevel.Warn, LogLevel.Error, LogLevel.Fatal } };
            yield return new object[] { "Trace", "Debug", new[] { LogLevel.Trace, LogLevel.Debug } };
            yield return new object[] { "Trace", "Trace", new[] { LogLevel.Trace, LogLevel.Trace } };
        }

        private static void AssertLogLevelEnabled(ILoggerBase logger, LogLevel expectedLogLevel)
        {
            AssertLogLevelEnabled(logger, new[] {expectedLogLevel });
        }

        private static void AssertLogLevelEnabled(ILoggerBase logger, LogLevel[] expectedLogLevels)
        {
            for (int i = LogLevel.MinLevel.Ordinal; i <= LogLevel.MaxLevel.Ordinal; ++i)
            {
                var logLevel = LogLevel.FromOrdinal(i);
                if (expectedLogLevels.Contains(logLevel))
                    Assert.True(logger.IsEnabled(logLevel),$"{logLevel} expected as true");
                else
                    Assert.False(logger.IsEnabled(logLevel),$"{logLevel} expected as false");
            }
        }
    }
}
// 
// Copyright (c) 2004-2010 Jaroslaw Kowalski <jaak@jkowalski.net>
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
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NLog.Config;
    using NLog.Filters;

    [TestClass]
    public class RuleConfigurationTests : NLogTestBase
    {
        [TestMethod]
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

            Assert.AreEqual(0, c.LoggingRules.Count);
        }

        [TestMethod]
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

            Assert.AreEqual(1, c.LoggingRules.Count);
            var rule = c.LoggingRules[0];
            Assert.AreEqual("*", rule.LoggerNamePattern);
            Assert.AreEqual(4, rule.Levels.Count);
            Assert.IsTrue(rule.Levels.Contains(LogLevel.Info));
            Assert.IsTrue(rule.Levels.Contains(LogLevel.Warn));
            Assert.IsTrue(rule.Levels.Contains(LogLevel.Error));
            Assert.IsTrue(rule.Levels.Contains(LogLevel.Fatal));
            Assert.AreEqual(1, rule.Targets.Count);
            Assert.AreSame(c.FindTargetByName("d1"), rule.Targets[0]);
            Assert.IsFalse(rule.Final);
            Assert.AreEqual(0, rule.Filters.Count);
        }

        [TestMethod]
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

            Assert.AreEqual(1, c.LoggingRules.Count);
            var rule = c.LoggingRules[0];
            Assert.AreEqual(1, rule.Levels.Count);
            Assert.IsTrue(rule.Levels.Contains(LogLevel.Warn));
        }

        [TestMethod]
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

            Assert.AreEqual(1, c.LoggingRules.Count);
            var rule = c.LoggingRules[0];
            Assert.AreEqual(2, rule.Levels.Count);
            Assert.IsTrue(rule.Levels.Contains(LogLevel.Info));
            Assert.IsTrue(rule.Levels.Contains(LogLevel.Warn));
        }

        [TestMethod]
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

            Assert.AreEqual(1, c.LoggingRules.Count);
            var rule = c.LoggingRules[0];
            Assert.AreEqual(6, rule.Levels.Count);
            Assert.IsTrue(rule.Levels.Contains(LogLevel.Trace));
            Assert.IsTrue(rule.Levels.Contains(LogLevel.Debug));
            Assert.IsTrue(rule.Levels.Contains(LogLevel.Info));
            Assert.IsTrue(rule.Levels.Contains(LogLevel.Warn));
            Assert.IsTrue(rule.Levels.Contains(LogLevel.Error));
            Assert.IsTrue(rule.Levels.Contains(LogLevel.Fatal));
        }

        [TestMethod]
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

            Assert.AreEqual(1, c.LoggingRules.Count);
            var rule = c.LoggingRules[0];
            Assert.AreEqual(3, rule.Levels.Count);
            Assert.IsTrue(rule.Levels.Contains(LogLevel.Trace));
            Assert.IsTrue(rule.Levels.Contains(LogLevel.Info));
            Assert.IsTrue(rule.Levels.Contains(LogLevel.Warn));
        }

        [TestMethod]
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

            Assert.AreEqual(1, c.LoggingRules.Count);
            var rule = c.LoggingRules[0];
            Assert.AreEqual(3, rule.Targets.Count);
            Assert.AreSame(c.FindTargetByName("d1"), rule.Targets[0]);
            Assert.AreSame(c.FindTargetByName("d2"), rule.Targets[1]);
            Assert.AreSame(c.FindTargetByName("d3"), rule.Targets[2]);
        }

        [TestMethod]
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

            Assert.AreEqual(1, c.LoggingRules.Count);
            var rule = c.LoggingRules[0];
            Assert.AreEqual(2, rule.ChildRules.Count);
            Assert.AreEqual("Foo*", rule.ChildRules[0].LoggerNamePattern);
            Assert.AreEqual("Bar*", rule.ChildRules[1].LoggerNamePattern);
        }

        [TestMethod]
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

            Assert.AreEqual(1, c.LoggingRules.Count);
            var rule = c.LoggingRules[0];
            Assert.AreEqual(2, rule.Filters.Count);
            var conditionBasedFilter = rule.Filters[0] as ConditionBasedFilter;
            Assert.IsNotNull(conditionBasedFilter);
            Assert.AreEqual("starts-with(message, 'x')", conditionBasedFilter.Condition.ToString());
            Assert.AreEqual(FilterResult.Ignore, conditionBasedFilter.Action);

            conditionBasedFilter = rule.Filters[1] as ConditionBasedFilter;
            Assert.IsNotNull(conditionBasedFilter);
            Assert.AreEqual("starts-with(message, 'z')", conditionBasedFilter.Condition.ToString());
            Assert.AreEqual(FilterResult.Ignore, conditionBasedFilter.Action);
        }
    }
}
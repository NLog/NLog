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

#region

using System;
using System.Collections.Generic;
using System.Linq;
using NLog.Config;
using NLog.Targets;
using Xunit;

#endregion

namespace NLog.UnitTests.Config
{
    public class ConfigApiTests
    {
        [Fact]
        public void AddTarget_testname()
        {
            var config = new LoggingConfiguration();
            config.AddTarget("name1", new FileTarget { Name = "File" });
            var allTargets = config.AllTargets;
            Assert.NotNull(allTargets);
            Assert.Single(allTargets);

            //maybe confusing, but the name of the target is not changed, only the one of the key.
            Assert.Equal("File", allTargets.First().Name);
            Assert.NotNull(config.FindTargetByName<FileTarget>("name1"));

            config.RemoveTarget("name1");
            allTargets = config.AllTargets;
            Assert.Empty(allTargets);
        }

        [Fact]
        public void AddTarget_WithName_NullNameParam()
        {
            var config = new LoggingConfiguration();
            Exception ex = Assert.Throws<ArgumentNullException>(() => config.AddTarget(name: null, target: new FileTarget { Name = "name1" }));
        }      
        
        [Fact]
        public void AddTarget_WithName_EmptyPameParam()
        {
            var config = new LoggingConfiguration();
            Exception ex = Assert.Throws<ArgumentException>(() => config.AddTarget(name: "", target: new FileTarget { Name = "name1" }));
        }

        [Fact]
        public void AddTarget_WithName_NullTargetParam()
        {
            var config = new LoggingConfiguration();
            Exception ex = Assert.Throws<ArgumentNullException>(() => config.AddTarget(name: "Name1", target: null));
        }

        [Fact]
        public void AddTarget_TargetOnly_NullParam()
        {
            var config = new LoggingConfiguration();
            Exception ex = Assert.Throws<ArgumentNullException>(() => config.AddTarget(target: null));
        }

        [Fact]
        public void AddTarget_testname_param()
        {
            var config = new LoggingConfiguration();
            config.AddTarget("name1", new FileTarget { Name = "name2" });
            var allTargets = config.AllTargets;
            Assert.NotNull(allTargets);
            Assert.Single(allTargets);

            //maybe confusing, but the name of the target is not changed, only the one of the key.
            Assert.Equal("name2", allTargets.First().Name);
            Assert.NotNull(config.FindTargetByName<FileTarget>("name1"));
        }

        [Fact]
        public void AddTarget_testname_fromtarget()
        {
            var config = new LoggingConfiguration();
            config.AddTarget(new FileTarget { Name = "name2" });
            var allTargets = config.AllTargets;
            Assert.NotNull(allTargets);
            Assert.Single(allTargets);
            Assert.Equal("name2", allTargets.First().Name);
            Assert.NotNull(config.FindTargetByName<FileTarget>("name2"));
        }

        [Fact]
        public void AddRule_min_max()
        {
            var config = new LoggingConfiguration();
            config.AddTarget(new FileTarget { Name = "File" });
            config.AddRule(LogLevel.Info, LogLevel.Error, "File", "*a");
            Assert.NotNull(config.LoggingRules);
            Assert.Equal(1, config.LoggingRules.Count);
            var rule1 = config.LoggingRules.FirstOrDefault();
            Assert.NotNull(rule1);
            Assert.False(rule1.Final);
            Assert.Equal("*a", rule1.LoggerNamePattern);
            Assert.False(rule1.IsLoggingEnabledForLevel(LogLevel.Fatal));
            Assert.True(rule1.IsLoggingEnabledForLevel(LogLevel.Error));
            Assert.True(rule1.IsLoggingEnabledForLevel(LogLevel.Warn));
            Assert.True(rule1.IsLoggingEnabledForLevel(LogLevel.Info));
            Assert.False(rule1.IsLoggingEnabledForLevel(LogLevel.Debug));
            Assert.False(rule1.IsLoggingEnabledForLevel(LogLevel.Trace));
            Assert.False(rule1.IsLoggingEnabledForLevel(LogLevel.Off));
        }

        [Fact]
        public void AddRule_all()
        {
            var config = new LoggingConfiguration();
            config.AddTarget(new FileTarget { Name = "File" });
            config.AddRuleForAllLevels("File", "*a");
            Assert.NotNull(config.LoggingRules);
            Assert.Equal(1, config.LoggingRules.Count);
            var rule1 = config.LoggingRules.FirstOrDefault();
            Assert.NotNull(rule1);
            Assert.False(rule1.Final);
            Assert.Equal("*a", rule1.LoggerNamePattern);
            Assert.True(rule1.IsLoggingEnabledForLevel(LogLevel.Fatal));
            Assert.True(rule1.IsLoggingEnabledForLevel(LogLevel.Error));
            Assert.True(rule1.IsLoggingEnabledForLevel(LogLevel.Warn));
            Assert.True(rule1.IsLoggingEnabledForLevel(LogLevel.Info));
            Assert.True(rule1.IsLoggingEnabledForLevel(LogLevel.Debug));
            Assert.True(rule1.IsLoggingEnabledForLevel(LogLevel.Trace));
            Assert.False(rule1.IsLoggingEnabledForLevel(LogLevel.Off));
        }

        [Fact]
        public void AddRule_onelevel()
        {
            var config = new LoggingConfiguration();
            config.AddTarget(new FileTarget { Name = "File" });
            config.AddRuleForOneLevel(LogLevel.Error, "File", "*a");
            Assert.NotNull(config.LoggingRules);
            Assert.Equal(1, config.LoggingRules.Count);
            var rule1 = config.LoggingRules.FirstOrDefault();
            Assert.NotNull(rule1);
            Assert.False(rule1.Final);
            Assert.Equal("*a", rule1.LoggerNamePattern);
            Assert.False(rule1.IsLoggingEnabledForLevel(LogLevel.Fatal));
            Assert.True(rule1.IsLoggingEnabledForLevel(LogLevel.Error));
            Assert.False(rule1.IsLoggingEnabledForLevel(LogLevel.Warn));
            Assert.False(rule1.IsLoggingEnabledForLevel(LogLevel.Info));
            Assert.False(rule1.IsLoggingEnabledForLevel(LogLevel.Debug));
            Assert.False(rule1.IsLoggingEnabledForLevel(LogLevel.Trace));
            Assert.False(rule1.IsLoggingEnabledForLevel(LogLevel.Off));
        }

        [Fact]
        public void AddRule_with_target()
        {
            var config = new LoggingConfiguration();
            var fileTarget = new FileTarget { Name = "File" };
            config.AddRuleForOneLevel(LogLevel.Error, fileTarget, "*a");
            Assert.NotNull(config.LoggingRules);
            Assert.Equal(1, config.LoggingRules.Count);
            config.AddTarget(new FileTarget { Name = "File" });
            var allTargets = config.AllTargets;
            Assert.NotNull(allTargets);
            Assert.Single(allTargets);
            Assert.Equal("File", allTargets.First().Name);
            Assert.NotNull(config.FindTargetByName<FileTarget>("File"));
        }

        [Fact]
        public void AddRule_missingtarget()
        {
            var config = new LoggingConfiguration();

            Assert.Throws<NLogConfigurationException>(() => config.AddRuleForOneLevel(LogLevel.Error, "File", "*a"));
        }

        [Fact]
        public void CheckAllTargets()
        {
            var config = new LoggingConfiguration();
            var fileTarget = new FileTarget { Name = "File", FileName = "file" };
            config.AddRuleForOneLevel(LogLevel.Error, fileTarget, "*a");

            config.AddTarget(fileTarget);

            Assert.Single(config.AllTargets);
            Assert.Equal(fileTarget, config.AllTargets[0]);

            config.InitializeAll();

            Assert.Single(config.AllTargets);
            Assert.Equal(fileTarget, config.AllTargets[0]);
        }

        [Fact]
        public void LogRuleToStringTest_min()
        {
            var target = new FileTarget { Name = "file1" };
            var loggingRule = new LoggingRule("*", LogLevel.Error, target);
            var s = loggingRule.ToString();
            Assert.Equal("logNamePattern: (:All) levels: [ Error Fatal ] appendTo: [ file1 ]", s);
        }

        [Fact]
        public void LogRuleToStringTest_minAndMax()
        {
            var target = new FileTarget { Name = "file1" };
            var loggingRule = new LoggingRule("*", LogLevel.Debug, LogLevel.Error, target);
            var s = loggingRule.ToString();
            Assert.Equal("logNamePattern: (:All) levels: [ Debug Info Warn Error ] appendTo: [ file1 ]", s);
        }

        [Fact]
        public void LogRuleToStringTest_none()
        {
            var target = new FileTarget { Name = "file1" };
            var loggingRule = new LoggingRule("*", target);
            var s = loggingRule.ToString();
            Assert.Equal("logNamePattern: (:All) levels: [ ] appendTo: [ file1 ]", s);
        }

        [Fact]
        public void LogRuleToStringTest_empty()
        {
            var target = new FileTarget { Name = "file1" };
            var loggingRule = new LoggingRule("", target);
            var s = loggingRule.ToString();
            Assert.Equal("logNamePattern: (:Equals) levels: [ ] appendTo: [ file1 ]", s);
        }

        [Fact]
        public void LogRuleToStringTest_filter()
        {
            var target = new FileTarget { Name = "file1" };
            var loggingRule = new LoggingRule("namespace.comp1", target);
            var s = loggingRule.ToString();
            Assert.Equal("logNamePattern: (namespace.comp1:Equals) levels: [ ] appendTo: [ file1 ]", s);
        }

        [Fact]
        public void LogRuleToStringTest_multiple_targets()
        {
            var target = new FileTarget { Name = "file1" };
            var target2 = new FileTarget { Name = "file2" };
            var loggingRule = new LoggingRule("namespace.comp1", target);
            loggingRule.Targets.Add(target2);
            var s = loggingRule.ToString();
            Assert.Equal("logNamePattern: (namespace.comp1:Equals) levels: [ ] appendTo: [ file1 file2 ]", s);
        }

        [Fact]
        public void LogRuleSetLoggingLevels_enables()
        {
            var rule = new LoggingRule();
            rule.SetLoggingLevels(LogLevel.Warn, LogLevel.Fatal);
            Assert.Equal(rule.Levels, new[] { LogLevel.Warn, LogLevel.Error, LogLevel.Fatal });
        }

        [Fact]
        public void LogRuleSetLoggingLevels_disables()
        {
            var rule = new LoggingRule();
            rule.EnableLoggingForLevels(LogLevel.MinLevel, LogLevel.MaxLevel);

            rule.SetLoggingLevels(LogLevel.Warn, LogLevel.Fatal);
            Assert.Equal(rule.Levels, new[] { LogLevel.Warn, LogLevel.Error, LogLevel.Fatal });
        }


        [Fact]
        public void LogRuleSetLoggingLevels_off()
        {
            var rule = new LoggingRule();
            rule.EnableLoggingForLevels(LogLevel.MinLevel, LogLevel.MaxLevel);

            rule.SetLoggingLevels(LogLevel.Off, LogLevel.Off);
            Assert.Equal(rule.Levels, new LogLevel[0]);
        }

        [Fact]
        public void LogRuleDisableLoggingLevels()
        {
            var rule = new LoggingRule();
            rule.EnableLoggingForLevels(LogLevel.MinLevel, LogLevel.MaxLevel);

            rule.DisableLoggingForLevels(LogLevel.Warn, LogLevel.Fatal);
            Assert.Equal(rule.Levels, new[] { LogLevel.Trace, LogLevel.Debug, LogLevel.Info });
        }

        [Fact]
        public void ConfigLogRuleWithName()
        {
            var config = new LoggingConfiguration();
            var rule = new LoggingRule("hello");
            config.LoggingRules.Add(rule);
            var ruleLookup = config.FindRuleByName("hello");
            Assert.Same(rule, ruleLookup);
            Assert.True(config.RemoveRuleByName("hello"));
            ruleLookup = config.FindRuleByName("hello");
            Assert.Null(ruleLookup);
            Assert.False(config.RemoveRuleByName("hello"));
        }

        [Fact]
        public void FindRuleByName_AfterRename_FindNewOneAndDontFindOld()
        {
            // Arrange
            var config = new LoggingConfiguration();
            var rule = new LoggingRule("hello");
            config.LoggingRules.Add(rule);

            // Act
            var foundRule1 = config.FindRuleByName("hello");
            foundRule1.RuleName = "world";
            var foundRule2 = config.FindRuleByName("hello");
            var foundRule3 = config.FindRuleByName("world");

            // Assert
            Assert.Null(foundRule2);
            Assert.NotNull(foundRule1);
            Assert.Same(foundRule1, foundRule3);
        }

        [Fact]
        public void LoggerNameMatcher_None()
        {
            var matcher = LoggerNameMatcher.Create(null);
            Assert.Equal("logNamePattern: (:None)", matcher.ToString());
        }

        [Fact]
        public void LoggerNameMatcher_All()
        {
            var matcher = LoggerNameMatcher.Create("*");
            Assert.Equal("logNamePattern: (:All)", matcher.ToString());
        }

        [Fact]
        public void LoggerNameMatcher_Empty()
        {
            var matcher = LoggerNameMatcher.Create("");
            Assert.Equal("logNamePattern: (:Equals)", matcher.ToString());
        }

        [Fact]
        public void LoggerNameMatcher_Equals()
        {
            var matcher = LoggerNameMatcher.Create("abc");
            Assert.Equal("logNamePattern: (abc:Equals)", matcher.ToString());
        }

        [Fact]
        public void LoggerNameMatcher_StartsWith()
        {
            var matcher = LoggerNameMatcher.Create("abc*");
            Assert.Equal("logNamePattern: (abc:StartsWith)", matcher.ToString());
        }

        [Fact]
        public void LoggerNameMatcher_EndsWith()
        {
            var matcher = LoggerNameMatcher.Create("*abc");
            Assert.Equal("logNamePattern: (abc:EndsWith)", matcher.ToString());
        }

        [Fact]
        public void LoggerNameMatcher_Contains()
        {
            var matcher = LoggerNameMatcher.Create("*abc*");
            Assert.Equal("logNamePattern: (abc:Contains)", matcher.ToString());
        }

        [Fact]
        public void LoggerNameMatcher_MultiplePattern_StarInternal()
        {
            var matcher = LoggerNameMatcher.Create("a*bc");
            Assert.Equal("logNamePattern: (^a.*bc$:MultiplePattern)", matcher.ToString());
        }

        [Fact]
        public void LoggerNameMatcher_MultiplePattern_QuestionMark()
        {
            var matcher = LoggerNameMatcher.Create("a?bc");
            Assert.Equal("logNamePattern: (^a.bc$:MultiplePattern)", matcher.ToString());
        }

        [Fact]
        public void LoggerNameMatcher_MultiplePattern_EscapedChars()
        {
            var matcher = LoggerNameMatcher.Create("a?b.c.foo.bar");
            Assert.Equal("logNamePattern: (^a.b\\.c\\.foo\\.bar$:MultiplePattern)", matcher.ToString());
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("foobar", false)]
        public void LoggerNameMatcher_Matches_None(string name, bool result)
        {
            LoggerNameMatcher_Matches("None", null, name, result);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("foobar", false)]
        [InlineData("A", false)]
        [InlineData("a", true)]
        public void LoggerNameMatcher_Matches_Equals(string name, bool result)
        {
            LoggerNameMatcher_Matches("Equals", "a", name, result);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("Foo", false)]
        [InlineData("Foobar", false)]
        [InlineData("foo", true)]
        [InlineData("foobar", true)]
        public void LoggerNameMatcher_Matches_StartsWith(string name, bool result)
        {
            LoggerNameMatcher_Matches("StartsWith", "foo*", name, result);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("Bar", false)]
        [InlineData("fooBar", false)]
        [InlineData("bar", true)]
        [InlineData("foobar", true)]
        public void LoggerNameMatcher_Matches_EndsWith(string name, bool result)
        {
            LoggerNameMatcher_Matches("EndsWith", "*bar", name, result);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("Bar", false)]
        [InlineData("fooBar", false)]
        [InlineData("Barbaz", false)]
        [InlineData("fooBarbaz", false)]
        [InlineData("bar", true)]
        [InlineData("foobar", true)]
        [InlineData("barbaz", true)]
        [InlineData("foobarbaz", true)]
        public void LoggerNameMatcher_Matches_Contains(string name, bool result)
        {
            LoggerNameMatcher_Matches("Contains", "*bar*", name, result);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("Server[123].connection[2].reader", false)]
        [InlineData("server[123].connection[2].reader", true)]
        [InlineData("server[123].connection[2].", true)]
        [InlineData("server[123].connection[2]", false)]
        [InlineData("server[123].connection[25].reader", false)]
        [InlineData("server[].connection[2].reader", true)]
        public void LoggerNameMatcher_Matches_MultiplePattern(string name, bool result)
        {
            LoggerNameMatcher_Matches("MultiplePattern", "server[*].connection[?].*", name, result);
        }

        [Theory]
        [InlineData("MultiplePattern", "server[*].connection[??].*", "server[].connection[2].reader", false)]
        [InlineData("MultiplePattern", "server[*].connection[??].*", "server[].connection[25].reader", true)]
        [InlineData("MultiplePattern", "server[*].connection[??].*", "server[].connection[254].reader", false)]
        public void LoggerNameMatcher_Matches(string matcherType, string pattern, string name, bool result)
        {
            var matcher = LoggerNameMatcher.Create(pattern);
            Assert.Contains(":" + matcherType, matcher.ToString());
            Assert.Equal(result, matcher.NameMatches(name));
        }

    }
}
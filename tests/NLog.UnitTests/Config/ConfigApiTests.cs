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
            config.AddTarget("name1", new FileTarget {Name = "File"});
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
            Exception ex = Assert.Throws<ArgumentException>(() => config.AddTarget(name: null, target: new FileTarget { Name = "name1" }));
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
            config.AddTarget("name1", new FileTarget {Name = "name2"});
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
            config.AddTarget(new FileTarget {Name = "name2"});
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
            config.AddTarget(new FileTarget {Name = "File"});
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
            config.AddTarget(new FileTarget {Name = "File"});
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
            config.AddTarget(new FileTarget {Name = "File"});
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
            var fileTarget = new FileTarget {Name = "File"};
            config.AddRuleForOneLevel(LogLevel.Error, fileTarget, "*a");
            Assert.NotNull(config.LoggingRules);
            Assert.Equal(1, config.LoggingRules.Count);
            config.AddTarget(new FileTarget {Name = "File"});
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
            var fileTarget = new FileTarget {Name = "File", FileName = "file"};
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
            var target = new FileTarget {Name = "file1"};
            var loggingRule = new LoggingRule("*", LogLevel.Error, target);
            var s = loggingRule.ToString();
            Assert.Equal("logNamePattern: (:All) levels: [ Error Fatal ] appendTo: [ file1 ]", s);
        }

        [Fact]
        public void LogRuleToStringTest_minAndMax()
        {
            var target = new FileTarget {Name = "file1"};
            var loggingRule = new LoggingRule("*", LogLevel.Debug, LogLevel.Error, target);
            var s = loggingRule.ToString();
            Assert.Equal("logNamePattern: (:All) levels: [ Debug Info Warn Error ] appendTo: [ file1 ]", s);
        }

        [Fact]
        public void LogRuleToStringTest_none()
        {
            var target = new FileTarget {Name = "file1"};
            var loggingRule = new LoggingRule("*", target);
            var s = loggingRule.ToString();
            Assert.Equal("logNamePattern: (:All) levels: [ ] appendTo: [ file1 ]", s);
        }

        [Fact]
        public void LogRuleToStringTest_filter()
        {
            var target = new FileTarget {Name = "file1"};
            var loggingRule = new LoggingRule("namespace.comp1", target);
            var s = loggingRule.ToString();
            Assert.Equal("logNamePattern: (namespace.comp1:Equals) levels: [ ] appendTo: [ file1 ]", s);
        }

        [Fact]
        public void LogRuleToStringTest_multiple_targets()
        {
            var target = new FileTarget {Name = "file1"};
            var target2 = new FileTarget {Name = "file2"};
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
        public void LogRuleDisableLoggingLevels()
        {
            var rule = new LoggingRule();
            rule.EnableLoggingForLevels(LogLevel.MinLevel, LogLevel.MaxLevel);

            rule.DisableLoggingForLevels(LogLevel.Warn, LogLevel.Fatal);
            Assert.Equal(rule.Levels, new[] { LogLevel.Trace, LogLevel.Debug, LogLevel.Info });
        }
    }
}
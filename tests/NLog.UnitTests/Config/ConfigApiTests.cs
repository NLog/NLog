// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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
            Assert.Equal(1, allTargets.Count);

            //maybe confusing, but the name of the target is not changed, only the one of the key.
            Assert.Equal("File", allTargets.First().Name);
            Assert.NotNull(config.FindTargetByName<FileTarget>("name1"));

            config.RemoveTarget("name1");
            allTargets = config.AllTargets;
            Assert.Equal(0, allTargets.Count);
        }

        [Fact]
        public void AddTarget_testname_param()
        {
            var config = new LoggingConfiguration();
            config.AddTarget("name1", new FileTarget { Name = "name2" });
            var allTargets = config.AllTargets;
            Assert.NotNull(allTargets);
            Assert.Equal(1, allTargets.Count);

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
            Assert.Equal(1, allTargets.Count);
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
            Assert.Equal(false, rule1.Final);
            Assert.Equal("*a", rule1.LoggerNamePattern);
            Assert.Equal(false, rule1.IsLoggingEnabledForLevel(LogLevel.Fatal));
            Assert.Equal(true, rule1.IsLoggingEnabledForLevel(LogLevel.Error));
            Assert.Equal(true, rule1.IsLoggingEnabledForLevel(LogLevel.Warn));
            Assert.Equal(true, rule1.IsLoggingEnabledForLevel(LogLevel.Info));
            Assert.Equal(false, rule1.IsLoggingEnabledForLevel(LogLevel.Debug));
            Assert.Equal(false, rule1.IsLoggingEnabledForLevel(LogLevel.Trace));
            Assert.Equal(false, rule1.IsLoggingEnabledForLevel(LogLevel.Off));
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
            Assert.Equal(false, rule1.Final);
            Assert.Equal("*a", rule1.LoggerNamePattern);
            Assert.Equal(true, rule1.IsLoggingEnabledForLevel(LogLevel.Fatal));
            Assert.Equal(true, rule1.IsLoggingEnabledForLevel(LogLevel.Error));
            Assert.Equal(true, rule1.IsLoggingEnabledForLevel(LogLevel.Warn));
            Assert.Equal(true, rule1.IsLoggingEnabledForLevel(LogLevel.Info));
            Assert.Equal(true, rule1.IsLoggingEnabledForLevel(LogLevel.Debug));
            Assert.Equal(true, rule1.IsLoggingEnabledForLevel(LogLevel.Trace));
            Assert.Equal(false, rule1.IsLoggingEnabledForLevel(LogLevel.Off));
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
            Assert.Equal(false, rule1.Final);
            Assert.Equal("*a", rule1.LoggerNamePattern);
            Assert.Equal(false, rule1.IsLoggingEnabledForLevel(LogLevel.Fatal));
            Assert.Equal(true, rule1.IsLoggingEnabledForLevel(LogLevel.Error));
            Assert.Equal(false, rule1.IsLoggingEnabledForLevel(LogLevel.Warn));
            Assert.Equal(false, rule1.IsLoggingEnabledForLevel(LogLevel.Info));
            Assert.Equal(false, rule1.IsLoggingEnabledForLevel(LogLevel.Debug));
            Assert.Equal(false, rule1.IsLoggingEnabledForLevel(LogLevel.Trace));
            Assert.Equal(false, rule1.IsLoggingEnabledForLevel(LogLevel.Off));
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
            Assert.Equal(1, allTargets.Count);
            Assert.Equal("File", allTargets.First().Name);
            Assert.NotNull(config.FindTargetByName<FileTarget>("File"));
        }


        [Fact]
        public void AddRule_missingtarget()
        {
            var config = new LoggingConfiguration();

            Assert.Throws <NLogConfigurationException>(() => config.AddRuleForOneLevel(LogLevel.Error, "File", "*a"));

        }
    }
}
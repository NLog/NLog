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
            config.AddTarget("name1", new DatabaseTarget());
            var allTargets = config.AllTargets;
            Assert.NotNull(allTargets);
            Assert.Equal(1, allTargets.Count);

            //maybe confusing, but the name of the target is not changed, only the one of the key.
            Assert.Equal("Database", allTargets.First().Name);
            Assert.NotNull(config.FindTargetByName<DatabaseTarget>("name1"));

            config.RemoveTarget("name1");
            allTargets = config.AllTargets;
            Assert.Equal(0, allTargets.Count);
        }

        [Fact]
        public void AddTarget_testname_fromTarget()
        {
            var config = new LoggingConfiguration();
            config.AddTarget("name1", new DatabaseTarget {Name = "name2"});
            var allTargets = config.AllTargets;
            Assert.NotNull(allTargets);
            Assert.Equal(1, allTargets.Count);

            //maybe confusing, but the name of the target is not changed, only the one of the key.
            Assert.Equal("name2", allTargets.First().Name);
            Assert.NotNull(config.FindTargetByName<DatabaseTarget>("name1"));
        }

        [Fact]
        public void AddTarget_testname_fromTarget2()
        {
            var config = new LoggingConfiguration();
            config.AddTarget(new DatabaseTarget {Name = "name2"});
            var allTargets = config.AllTargets;
            Assert.NotNull(allTargets);
            Assert.Equal(1, allTargets.Count);
            Assert.Equal("name2", allTargets.First().Name);
            Assert.NotNull(config.FindTargetByName<DatabaseTarget>("name2"));
        }

        [Fact]
        public void AddTarget_testname_fromTargetAttr()
        {
            var config = new LoggingConfiguration();
            config.AddTarget(new DatabaseTarget());
            var allTargets = config.AllTargets;
            Assert.NotNull(allTargets);
            Assert.Equal(1, allTargets.Count);
            Assert.Equal("Database", allTargets.First().Name);
            Assert.NotNull(config.FindTargetByName<DatabaseTarget>("Database"));
        }
    }
}
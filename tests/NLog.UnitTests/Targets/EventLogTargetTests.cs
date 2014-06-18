#if !SILVERLIGHT && !MONO

namespace NLog.UnitTests.Targets
{
    using System.Diagnostics;
    using NLog.Config;
    using NLog.Targets;
    using System;
    using System.Linq;
    using Xunit;

    public class EventLogTargetTests : NLogTestBase
    {
        [Fact]
        public void WriteEventLogEntry()
        {
            var target = new EventLogTarget();
            //The Log to write to is intentionally lower case!!
            target.Log = "application";  

            SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Debug);
            var logger = LogManager.GetLogger("WriteEventLogEntry");
            var el = new EventLog(target.Log);

            var latestEntryTime  = el.Entries.Cast<EventLogEntry>().Max(n => n.TimeWritten);


            var testValue = Guid.NewGuid();
            logger.Debug(testValue.ToString());
            
            var entryExists = (from entry in el.Entries.Cast<EventLogEntry>()
                                where entry.TimeWritten >= latestEntryTime
                                && entry.Message.Contains(testValue.ToString())
                                select entry).Any();

            Assert.True(entryExists);
        }
    }
}

#endif
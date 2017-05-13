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

#if  !MONO

namespace NLog.UnitTests.Targets
{
    using System.Diagnostics;
    using NLog.Config;
    using NLog.Targets;
    using System;
    using System.Linq;
    using Xunit;
    using System.Collections.Generic;
    using System.Diagnostics.Eventing.Reader;
    using NLog.Layouts;
    using Xunit.Extensions;

    public class EventLogTargetTests : NLogTestBase
    {
        [Fact]
        public void MaxMessageLengthShouldBe16384_WhenNotSpecifyAnyOption()
        {
            const int expectedMaxMessageLength = 16384;
            LoggingConfiguration c = CreateConfigurationFromString(@"
            <nlog ThrowExceptions='true'>
                <targets>
                    <target type='EventLog' name='eventLog1' layout='${message}' />
                </targets>
                <rules>
                      <logger name='*' writeTo='eventLog1'>
                      </logger>
                </rules>
            </nlog>");

            var eventLog1 = c.FindTargetByName<EventLogTarget>("eventLog1");
            Assert.Equal(expectedMaxMessageLength, eventLog1.MaxMessageLength);
        }

        [Fact]
        public void MaxMessageLengthShouldBeAsSpecifiedOption()
        {
            const int expectedMaxMessageLength = 1000;
            LoggingConfiguration c = CreateConfigurationFromString(string.Format(@"
            <nlog ThrowExceptions='true'>
                <targets>
                    <target type='EventLog' name='eventLog1' layout='${{message}}' maxmessagelength='{0}' />
                </targets>
                <rules>
                      <logger name='*' writeTo='eventLog1'>
                      </logger>
                    </rules>
            </nlog>", expectedMaxMessageLength));

            var eventLog1 = c.FindTargetByName<EventLogTarget>("eventLog1");
            Assert.Equal(expectedMaxMessageLength, eventLog1.MaxMessageLength);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ConfigurationShouldThrowException_WhenMaxMessageLengthIsNegativeOrZero(int maxMessageLength)
        {
            string configrationText = string.Format(@"
            <nlog ThrowExceptions='true'>
                <targets>
                    <target type='EventLog' name='eventLog1' layout='${{message}}' maxmessagelength='{0}' />
                </targets>
                <rules>
                      <logger name='*' writeTo='eventLog1'>
                      </logger>
                    </rules>
            </nlog>", maxMessageLength);

            NLogConfigurationException ex = Assert.Throws<NLogConfigurationException>(() => CreateConfigurationFromString(configrationText));
            Assert.Equal("MaxMessageLength cannot be zero or negative.", ex.InnerException.InnerException.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ShouldThrowException_WhenMaxMessageLengthSetNegativeOrZero(int maxMessageLength)
        {
            ArgumentException ex = Assert.Throws<ArgumentException>(() =>
            {
                var target = new EventLogTarget();
                target.MaxMessageLength = maxMessageLength;
            });

            Assert.Equal("MaxMessageLength cannot be zero or negative.", ex.Message);
        }


        private void AssertMessageAndLogLevelForTruncatedMessages(LogLevel loglevel, EventLogEntryType expectedEventLogEntryType, string expectedMessage, Layout entryTypeLayout)
        {
            const int expectedEntryCount = 1;
            var eventRecords = Write(loglevel, expectedEventLogEntryType, expectedMessage, entryTypeLayout, EventLogTargetOverflowAction.Truncate).ToList();

            Assert.Equal(expectedEntryCount, eventRecords.Count);
            AssertWrittenMessage(eventRecords, expectedMessage);
        }

        [Fact]
        public void TruncatedMessagesShouldBeWrittenAtInformationLevel_WhenNLogLevelIsTrace()
        {
            AssertMessageAndLogLevelForTruncatedMessages(LogLevel.Trace, EventLogEntryType.Information, "TruncatedMessagesShouldBeWrittenAtInformationLevel_WhenNLogLevelIsTrace", null);
        }

        [Fact]
        public void TruncatedMessagesShouldBeWrittenAtInformationLevel_WhenNLogLevelIsDebug()
        {
            AssertMessageAndLogLevelForTruncatedMessages(LogLevel.Debug, EventLogEntryType.Information, "TruncatedMessagesShouldBeWrittenAtInformationLevel_WhenNLogLevelIsDebug", null);
        }

        [Fact]
        public void TruncatedMessagesShouldBeWrittenAtInformationLevel_WhenNLogLevelIsInfo()
        {
            AssertMessageAndLogLevelForTruncatedMessages(LogLevel.Info, EventLogEntryType.Information, "TruncatedMessagesShouldBeWrittenAtInformationLevel_WhenNLogLevelIsInfo", null);
        }

        [Fact]
        public void TruncatedMessagesShouldBeWrittenAtWarningLevel_WhenNLogLevelIsWarn()
        {
            AssertMessageAndLogLevelForTruncatedMessages(LogLevel.Warn, EventLogEntryType.Warning, "TruncatedMessagesShouldBeWrittenAtWarningLevel_WhenNLogLevelIsWarn", null);
        }

        [Fact]
        public void TruncatedMessagesShouldBeWrittenAtErrorLevel_WhenNLogLevelIsError()
        {
            AssertMessageAndLogLevelForTruncatedMessages(LogLevel.Error, EventLogEntryType.Error, "TruncatedMessagesShouldBeWrittenAtErrorLevel_WhenNLogLevelIsError", null);
        }

        [Fact]
        public void TruncatedMessagesShouldBeWrittenAtErrorLevel_WhenNLogLevelIsFatal()
        {
            AssertMessageAndLogLevelForTruncatedMessages(LogLevel.Fatal, EventLogEntryType.Error, "TruncatedMessagesShouldBeWrittenAtErrorLevel_WhenNLogLevelIsFatal", null);
        }

        [Fact]
        public void TruncatedMessagesShouldBeWrittenAtSuccessAuditLevel_WhenEntryTypeLayoutSpecifiedAsSuccessAudit()
        {
            AssertMessageAndLogLevelForTruncatedMessages(LogLevel.Warn, EventLogEntryType.SuccessAudit, "TruncatedMessagesShouldBeWrittenAtSuccessAuditLevel_WhenEntryTypeLayoutSpecifiedAsSuccessAudit", new SimpleLayout("SuccessAudit"));
        }

        [Fact]
        public void TruncatedMessagesShouldBeWrittenAtSuccessAuditLevel_WhenEntryTypeLayoutSpecifiedAsSuccessAudit_Uppercase()
        {
            AssertMessageAndLogLevelForTruncatedMessages(LogLevel.Warn, EventLogEntryType.SuccessAudit, "TruncatedMessagesShouldBeWrittenAtSuccessAuditLevel_WhenEntryTypeLayoutSpecifiedAsSuccessAudit_Uppercase", new SimpleLayout("SUCCESSAUDIT"));
        }

        [Fact]
        public void TruncatedMessagesShouldBeWrittenAtFailureAuditLevel_WhenEntryTypeLayoutSpecifiedAsFailureAudit()
        {
            AssertMessageAndLogLevelForTruncatedMessages(LogLevel.Debug, EventLogEntryType.FailureAudit, "TruncatedMessagesShouldBeWrittenAtFailureAuditLevel_WhenEntryTypeLayoutSpecifiedAsFailureAudit", new SimpleLayout("FailureAudit"));
        }

        [Fact]
        public void TruncatedMessagesShouldBeWrittenAtErrorLevel_WhenEntryTypeLayoutSpecifiedAsError()
        {
            AssertMessageAndLogLevelForTruncatedMessages(LogLevel.Debug, EventLogEntryType.Error, "TruncatedMessagesShouldBeWrittenAtErrorLevel_WhenEntryTypeLayoutSpecifiedAsError", new SimpleLayout("error"));
        }

        [Fact]
        public void TruncatedMessagesShouldBeWrittenAtSpecifiedNLogLevel_WhenWrongEntryTypeLayoutSupplied()
        {
            AssertMessageAndLogLevelForTruncatedMessages(LogLevel.Warn, EventLogEntryType.Warning, "TruncatedMessagesShouldBeWrittenAtSpecifiedNLogLevel_WhenWrongEntryTypeLayoutSupplied", new SimpleLayout("fallback to auto determined"));
        }



        private void AssertMessageCountAndLogLevelForSplittedMessages(LogLevel loglevel, EventLogEntryType expectedEventLogEntryType, Layout entryTypeLayout)
        {
            const int maxMessageLength = 16384;
            const int expectedEntryCount = 2;
            string messagePart1 = string.Join("", Enumerable.Repeat("l", maxMessageLength));
            string messagePart2 = "this part must be splitted";
            string testMessage = messagePart1 + messagePart2;
            var entries = Write(loglevel, expectedEventLogEntryType, testMessage, entryTypeLayout, EventLogTargetOverflowAction.Split, maxMessageLength).ToList();

            Assert.Equal(expectedEntryCount, entries.Count);
        }

        [Fact]
        public void SplittedMessagesShouldBeWrittenAtInformationLevel_WhenNLogLevelIsTrace()
        {
            AssertMessageCountAndLogLevelForSplittedMessages(LogLevel.Trace, EventLogEntryType.Information, null);
        }

        [Fact]
        public void SplittedMessagesShouldBeWrittenAtInformationLevel_WhenNLogLevelIsDebug()
        {
            AssertMessageCountAndLogLevelForSplittedMessages(LogLevel.Debug, EventLogEntryType.Information, null);
        }

        [Fact]
        public void SplittedMessagesShouldBeWrittenAtInformationLevel_WhenNLogLevelIsInfo()
        {
            AssertMessageCountAndLogLevelForSplittedMessages(LogLevel.Info, EventLogEntryType.Information, null);
        }

        [Fact]
        public void SplittedMessagesShouldBeWrittenAtWarningLevel_WhenNLogLevelIsWarn()
        {
            AssertMessageCountAndLogLevelForSplittedMessages(LogLevel.Warn, EventLogEntryType.Warning, null);
        }

        [Fact]
        public void SplittedMessagesShouldBeWrittenAtErrorLevel_WhenNLogLevelIsError()
        {
            AssertMessageCountAndLogLevelForSplittedMessages(LogLevel.Error, EventLogEntryType.Error, null);
        }

        [Fact]
        public void SplittedMessagesShouldBeWrittenAtErrorLevel_WhenNLogLevelIsFatal()
        {
            AssertMessageCountAndLogLevelForSplittedMessages(LogLevel.Fatal, EventLogEntryType.Error, null);
        }

        [Fact]
        public void SplittedMessagesShouldBeWrittenAtSuccessAuditLevel_WhenEntryTypeLayoutSpecifiedAsSuccessAudit()
        {
            AssertMessageCountAndLogLevelForSplittedMessages(LogLevel.Debug, EventLogEntryType.SuccessAudit, new SimpleLayout("SuccessAudit"));
        }

        [Fact]
        public void SplittedMessagesShouldBeWrittenAtFailureLevel_WhenEntryTypeLayoutSpecifiedAsFailureAudit()
        {
            AssertMessageCountAndLogLevelForSplittedMessages(LogLevel.Debug, EventLogEntryType.FailureAudit, new SimpleLayout("FailureAudit"));
        }

        [Fact]
        public void SplittedMessagesShouldBeWrittenAtErrorLevel_WhenEntryTypeLayoutSpecifiedAsError()
        {
            AssertMessageCountAndLogLevelForSplittedMessages(LogLevel.Debug, EventLogEntryType.Error, new SimpleLayout("error"));
        }


        [Fact]
        public void SplittedMessagesShouldBeWrittenAtSpecifiedNLogLevel_WhenWrongEntryTypeLayoutSupplied()
        {
            AssertMessageCountAndLogLevelForSplittedMessages(LogLevel.Info, EventLogEntryType.Information, new SimpleLayout("wrong entry type level"));
        }


        [Fact]
        public void WriteEventLogEntryLargerThanMaxMessageLengthWithOverflowTruncate_TruncatesTheMessage()
        {
            const int maxMessageLength = 16384;
            const int expectedEntryCount = 1;
            string expectedMessage = string.Join("", Enumerable.Repeat("t", maxMessageLength));
            string expectedToTruncateMessage = " this part will be truncated";
            string testMessage = expectedMessage + expectedToTruncateMessage;

            var entries = Write(LogLevel.Info, EventLogEntryType.Information, testMessage, null, EventLogTargetOverflowAction.Truncate, maxMessageLength).ToList();

            Assert.Equal(expectedEntryCount, entries.Count);
            AssertWrittenMessage(entries, expectedMessage);
        }

        [Fact]
        public void WriteEventLogEntryEqualToMaxMessageLengthWithOverflowTruncate_TheMessageIsNotTruncated()
        {
            const int maxMessageLength = 16384;
            const int expectedEntryCount = 1;
            string expectedMessage = string.Join("", Enumerable.Repeat("t", maxMessageLength));
            var entries = Write(LogLevel.Info, EventLogEntryType.Information, expectedMessage, null, EventLogTargetOverflowAction.Truncate, maxMessageLength).ToList();

            Assert.Equal(expectedEntryCount, entries.Count);
            AssertWrittenMessage(entries, expectedMessage);
        }

        [Fact]
        public void WriteEventLogEntryLargerThanMaxMessageLengthWithOverflowSplitEntries_TheMessageShouldBeSplitted()
        {
            const int maxMessageLength = 16384;
            const int expectedEntryCount = 5;
            string messagePart1 = string.Join("", Enumerable.Repeat("a", maxMessageLength));
            string messagePart2 = string.Join("", Enumerable.Repeat("b", maxMessageLength));
            string messagePart3 = string.Join("", Enumerable.Repeat("c", maxMessageLength));
            string messagePart4 = string.Join("", Enumerable.Repeat("d", maxMessageLength));
            string messagePart5 = "this part must be splitted too";
            string testMessage = messagePart1 + messagePart2 + messagePart3 + messagePart4 + messagePart5;

            var entries = Write(LogLevel.Info, EventLogEntryType.Information, testMessage, null, EventLogTargetOverflowAction.Split, maxMessageLength).ToList();

            Assert.Equal(expectedEntryCount, entries.Count);

            AssertWrittenMessage(entries, messagePart1);
            AssertWrittenMessage(entries, messagePart2);
            AssertWrittenMessage(entries, messagePart3);
            AssertWrittenMessage(entries, messagePart4);
            AssertWrittenMessage(entries, messagePart5);
        }

        [Fact]
        public void WriteEventLogEntryEqual2MaxMessageLengthWithOverflowSplitEntries_TheMessageShouldBeSplittedInTwoChunk()
        {
            const int maxMessageLength = 16384;
            const int expectedEntryCount = 2;
            string messagePart1 = string.Join("", Enumerable.Repeat("a", maxMessageLength));
            string messagePart2 = string.Join("", Enumerable.Repeat("b", maxMessageLength));
            string testMessage = messagePart1 + messagePart2;

            var entries = Write(LogLevel.Info, EventLogEntryType.Information, testMessage, null, EventLogTargetOverflowAction.Split, maxMessageLength).ToList();

            Assert.Equal(expectedEntryCount, entries.Count);

            AssertWrittenMessage(entries, messagePart1);
            AssertWrittenMessage(entries, messagePart2);
        }


        [Fact]
        public void WriteEventLogEntryEqualToMaxMessageLengthWithOverflowSplitEntries_TheMessageIsNotSplit()
        {
            const int maxMessageLength = 16384;
            const int expectedEntryCount = 1;
            string expectedMessage = string.Join("", Enumerable.Repeat("a", maxMessageLength));
            var entries = Write(LogLevel.Info, EventLogEntryType.Information, expectedMessage, null, EventLogTargetOverflowAction.Split, maxMessageLength).ToList();

            Assert.Equal(expectedEntryCount, entries.Count);
            AssertWrittenMessage(entries, expectedMessage);
        }

        [Fact]
        public void WriteEventLogEntryEqualToMaxMessageLengthWithOverflowDiscard_TheMessageIsWritten()
        {
            const int maxMessageLength = 16384;
            const int expectedEntryCount = 1;
            string expectedMessage = string.Join("", Enumerable.Repeat("a", maxMessageLength));
            var entries = Write(LogLevel.Info, EventLogEntryType.Information, expectedMessage, null, EventLogTargetOverflowAction.Discard, maxMessageLength).ToList();

            Assert.Equal(expectedEntryCount, entries.Count);
            AssertWrittenMessage(entries, expectedMessage);
        }

        [Fact]
        public void WriteEventLogEntryLargerThanMaxMessageLengthWithOverflowDiscard_TheMessageIsNotWritten()
        {
            const int maxMessageLength = 16384;
            string messagePart1 = string.Join("", Enumerable.Repeat("a", maxMessageLength));
            string messagePart2 = "b";
            string testMessage = messagePart1 + messagePart2;
            bool wasWritten = Write(LogLevel.Info, EventLogEntryType.Information, testMessage, null, EventLogTargetOverflowAction.Discard, maxMessageLength).Any();

            Assert.False(wasWritten);
        }


        [Fact]
        public void WriteEventLogEntryWithDynamicSource()
        {
            const int maxMessageLength = 10;
            string expectedMessage = string.Join("", Enumerable.Repeat("a", maxMessageLength));

            var target = CreateEventLogTarget(null, "NLog.UnitTests" + Guid.NewGuid().ToString("N"), EventLogTargetOverflowAction.Split, maxMessageLength);
            target.Layout = new SimpleLayout("${message}");
            target.Source = new SimpleLayout("${event-properties:item=DynamicSource}");
            SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);

            var logger = LogManager.GetLogger("WriteEventLogEntry");

            var sourceName = "NLog.UnitTests" + Guid.NewGuid().ToString("N");
            var logEvent = CreateLogEventWithDynamicSource(expectedMessage, LogLevel.Trace, "DynamicSource", sourceName);

            logger.Log(logEvent);

            var eventLog = new EventLog(target.Log);
            var entries = GetEventRecords(eventLog.Log).ToList();

            entries = entries.Where(a => a.ProviderName == sourceName).ToList();
            Assert.Equal(1, entries.Count);
            AssertWrittenMessage(entries, expectedMessage);

            sourceName = "NLog.UnitTests" + Guid.NewGuid().ToString("N");
            expectedMessage = string.Join("", Enumerable.Repeat("b", maxMessageLength));

            logEvent = CreateLogEventWithDynamicSource(expectedMessage, LogLevel.Trace, "DynamicSource", sourceName);
            logger.Log(logEvent);

            entries = GetEventRecords(eventLog.Log).ToList();
            entries = entries.Where(a => a.ProviderName == sourceName).ToList();
            Assert.Equal(1, entries.Count);
            AssertWrittenMessage(entries, expectedMessage);
        }

        [Fact]
        public void LogEntryWithStaticEventIdAndCategoryInTargetLayout()
        {
            var rnd = new Random();
            int eventId = rnd.Next(1, short.MaxValue);
            int category = rnd.Next(1, short.MaxValue);
            var target = CreateEventLogTarget(null, "NLog.UnitTests" + Guid.NewGuid().ToString("N"), EventLogTargetOverflowAction.Truncate, 5000);
            target.EventId = new SimpleLayout(eventId.ToString());
            target.Category = new SimpleLayout(category.ToString());
            SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);
            var logger = LogManager.GetLogger("WriteEventLogEntry");
            logger.Log(LogLevel.Error, "Simple Test Message");
            var eventLog = new EventLog(target.Log);
            var entries = GetEventRecords(eventLog.Log).ToList();
            var expectedProviderName = target.GetFixedSource();
            var filtered = entries.Where(entry =>
                                         entry.ProviderName == expectedProviderName &&
                                         HasEntryType(entry, EventLogEntryType.Error)
                                        );
            Assert.Equal(1, filtered.Count());
            var record = filtered.First();
            Assert.Equal(eventId, record.Id);
            Assert.Equal(category, record.Task);
        }

        [Fact]
        public void LogEntryWithDynamicEventIdAndCategory()
        {
            var rnd = new Random();
            int eventId = rnd.Next(1, short.MaxValue);
            int category = rnd.Next(1, short.MaxValue);
            var target = CreateEventLogTarget(null, "NLog.UnitTests" + Guid.NewGuid().ToString("N"), EventLogTargetOverflowAction.Truncate, 5000);
            target.EventId = new SimpleLayout("${event-properties:EventId}");
            target.Category = new SimpleLayout("${event-properties:Category}");
            SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);
            var logger = LogManager.GetLogger("WriteEventLogEntry");
            LogEventInfo theEvent = new LogEventInfo(LogLevel.Error, "TestLoggerName", "Simple Message");
            theEvent.Properties["EventId"] = eventId;
            theEvent.Properties["Category"] = category;
            logger.Log(theEvent);
            var eventLog = new EventLog(target.Log);
            var entries = GetEventRecords(eventLog.Log).ToList();
            var expectedProviderName = target.GetFixedSource();
            var filtered = entries.Where(entry =>
                                         entry.ProviderName == expectedProviderName &&
                                         HasEntryType(entry, EventLogEntryType.Error)
                                        );
            Assert.Equal(1, filtered.Count());
            var record = filtered.First();
            Assert.Equal(eventId, record.Id);
            Assert.Equal(category, record.Task);
        }

        private static IEnumerable<EventRecord> Write(LogLevel logLevel, EventLogEntryType expectedEventLogEntryType, string logMessage, Layout entryType = null, EventLogTargetOverflowAction overflowAction = EventLogTargetOverflowAction.Truncate, int maxMessageLength = 16384)
        {
            var target = CreateEventLogTarget(entryType, "NLog.UnitTests" + Guid.NewGuid().ToString("N"), overflowAction, maxMessageLength);
            SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);

            var logger = LogManager.GetLogger("WriteEventLogEntry");
            logger.Log(logLevel, logMessage);

            var eventLog = new EventLog(target.Log);

            var entries = GetEventRecords(eventLog.Log).ToList();

            var expectedSource = target.GetFixedSource();

            var filteredEntries = entries.Where(entry =>
                                            entry.ProviderName == expectedSource &&
                                            HasEntryType(entry, expectedEventLogEntryType)
                                            );
            if (overflowAction == EventLogTargetOverflowAction.Discard && logMessage.Length > maxMessageLength)
            {
                Assert.False(filteredEntries.Any(), string.Format("No message is expected. But {0} message(s) found entry of type '{1}' from source '{2}'.", filteredEntries.Count(), expectedEventLogEntryType, expectedSource));
            }
            else
            {
                Assert.True(filteredEntries.Any(), string.Format("Failed to find entry of type '{0}' from source '{1}'", expectedEventLogEntryType, expectedSource));
            }

            return filteredEntries;
        }

        private void AssertWrittenMessage(IEnumerable<EventRecord> eventLogs, string expectedMessage)
        {
            var messages = eventLogs.Where(entry => entry.Properties.Any(prop => Convert.ToString(prop.Value) == expectedMessage));
            Assert.True(messages.Any(), string.Format("Event records has not the expected message: '{0}'", expectedMessage));
        }

        private static EventLogTarget CreateEventLogTarget(Layout entryType, string sourceName, EventLogTargetOverflowAction overflowAction, int maxMessageLength)
        {
            var target = new EventLogTarget();
            //The Log to write to is intentionally lower case!!
            target.Log = "application";
            // set the source explicitly to prevent random AppDomain name being used as the source name
            target.Source = sourceName;
            //Be able to check message length and content, the Layout is intentionally only ${message}.
            target.Layout = new SimpleLayout("${message}");
            if (entryType != null)
            {
                //set only when not default
                target.EntryType = entryType;
            }

            target.OnOverflow = overflowAction;
            target.MaxMessageLength = maxMessageLength;

            return target;
        }

        private LogEventInfo CreateLogEventWithDynamicSource(string message, LogLevel level, string propertyKey, string proertyValue)
        {
            var logEvent = new LogEventInfo();
            logEvent.Message = message;
            logEvent.Level = level;
            logEvent.Properties[propertyKey] = proertyValue;

            return logEvent;
        }

        private static IEnumerable<EventRecord> GetEventRecords(string logName)
        {
            var query = new EventLogQuery(logName, PathType.LogName) { ReverseDirection = true };
            using (var reader = new EventLogReader(query))
                for (var eventInstance = reader.ReadEvent(); eventInstance != null; eventInstance = reader.ReadEvent())
                    yield return eventInstance;
        }

        private static bool HasEntryType(EventRecord eventRecord, EventLogEntryType entryType)
        {
            var keywords = (StandardEventKeywords)(eventRecord.Keywords ?? 0);
            var level = (StandardEventLevel)(eventRecord.Level ?? 0);
            bool isClassicEvent = keywords.HasFlag(StandardEventKeywords.EventLogClassic);
            switch (entryType)
            {
                case EventLogEntryType.Error:
                    return isClassicEvent && level == StandardEventLevel.Error;
                case EventLogEntryType.Warning:
                    return isClassicEvent && level == StandardEventLevel.Warning;
                case EventLogEntryType.Information:
                    return isClassicEvent && level == StandardEventLevel.Informational;
                case EventLogEntryType.SuccessAudit:
                    return keywords.HasFlag(StandardEventKeywords.AuditSuccess);
                case EventLogEntryType.FailureAudit:
                    return keywords.HasFlag(StandardEventKeywords.AuditFailure);
            }
            return false;
        }
    }
}

#endif
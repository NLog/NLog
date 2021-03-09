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

#if  !MONO && !NETSTANDARD

namespace NLog.UnitTests.Targets
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Eventing.Reader;
    using System.Linq;
    using System.Diagnostics;
    using NLog.Config;
    using NLog.Layouts;
    using NLog.Targets;
    using Xunit;

    public class EventLogTargetTests : NLogTestBase
    {
        private const int MaxMessageLength = EventLogTarget.EventLogMaxMessageLength;

        [Fact]
        public void MaxMessageLengthShouldBe16384_WhenNotSpecifyAnyOption()
        {
            LoggingConfiguration c = XmlLoggingConfiguration.CreateFromXmlString(@"
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
            Assert.Equal(MaxMessageLength, eventLog1.MaxMessageLength);
        }

        [Fact]
        public void MaxMessageLengthShouldBeAsSpecifiedOption()
        {
            const int expectedMaxMessageLength = 1000;
            LoggingConfiguration c = XmlLoggingConfiguration.CreateFromXmlString($@"
            <nlog ThrowExceptions='true'>
                <targets>
                    <target type='EventLog' name='eventLog1' layout='${{message}}' maxmessagelength='{
                    expectedMaxMessageLength
                }' />
                </targets>
                <rules>
                      <logger name='*' writeTo='eventLog1'>
                      </logger>
                    </rules>
            </nlog>");

            var eventLog1 = c.FindTargetByName<EventLogTarget>("eventLog1");
            Assert.Equal(expectedMaxMessageLength, eventLog1.MaxMessageLength);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ConfigurationShouldThrowException_WhenMaxMessageLengthIsNegativeOrZero(int maxMessageLength)
        {
            string configrationText = $@"
            <nlog ThrowExceptions='true'>
                <targets>
                    <target type='EventLog' name='eventLog1' layout='${{message}}' maxmessagelength='{
                    maxMessageLength
                }' />
                </targets>
                <rules>
                      <logger name='*' writeTo='eventLog1'>
                      </logger>
                    </rules>
            </nlog>";

            NLogConfigurationException ex = Assert.Throws<NLogConfigurationException>(() => XmlLoggingConfiguration.CreateFromXmlString(configrationText));
            Assert.Equal("MaxMessageLength cannot be zero or negative.", ex.InnerException.InnerException.Message);
        }

        [Theory]
        [InlineData(0)] // Is multiple of 64, but less than the min value of 64
        [InlineData(65)] // Isn't multiple of 64
        [InlineData(4194304)] // Is multiple of 64, but bigger than the max value of 4194240
        public void Configuration_ShouldThrowException_WhenMaxKilobytesIsInvalid(long? maxKilobytes)
        {
            string configrationText = $@"
            <nlog ThrowExceptions='true'>
                <targets>
                    <target type='EventLog' name='eventLog1' layout='${{message}}' maxKilobytes='{maxKilobytes}' />
                </targets>
                <rules>
                      <logger name='*' writeTo='eventLog1' />
                </rules>
            </nlog>";

            NLogConfigurationException ex = Assert.Throws<NLogConfigurationException>(() => XmlLoggingConfiguration.CreateFromXmlString(configrationText));
            Assert.Equal("MaxKilobytes must be a multiple of 64, and between 64 and 4194240", ex.InnerException.InnerException.Message);
        }

        [Theory]
        [InlineData(0)] // Is multiple of 64, but less than the min value of 64
        [InlineData(65)] // Isn't multiple of 64
        [InlineData(4194304)] // Is multiple of 64, but bigger than the max value of 4194240
        public void MaxKilobytes_ShouldThrowException_WhenMaxKilobytesIsInvalid(long? maxKilobytes)
        {
            ArgumentException ex = Assert.Throws<ArgumentException>(() =>
            {
                var target = new EventLogTarget();
                target.MaxKilobytes = maxKilobytes;
            });

            Assert.Equal("MaxKilobytes must be a multiple of 64, and between 64 and 4194240", ex.Message);
        }

        [Theory]
        // 'null' case is omitted, as it isn't a valid value for Int64 XML property.
        [InlineData(64)] // Min value
        [InlineData(4194240)] // Max value
        [InlineData(16384)] // Acceptable value
        public void ConfigurationMaxKilobytes_ShouldBeAsSpecified_WhenMaxKilobytesIsValid(long? maxKilobytes)
        {
            var expectedMaxKilobytes = maxKilobytes;

            string configrationText = $@"
            <nlog ThrowExceptions='true'>
                <targets>
                    <target type='EventLog' name='eventLog1' layout='${{message}}' maxKilobytes='{maxKilobytes}' />
                </targets>
                <rules>
                      <logger name='*' writeTo='eventLog1' />
                </rules>
            </nlog>";
            LoggingConfiguration configuration = XmlLoggingConfiguration.CreateFromXmlString(configrationText);

            var eventLog1 = configuration.FindTargetByName<EventLogTarget>("eventLog1");
            Assert.Equal(expectedMaxKilobytes, eventLog1.MaxKilobytes);
        }

        [Theory]
        [InlineData(null)] // A possible value, that should not change anything
        [InlineData(64)] // Min value
        [InlineData(4194240)] // Max value
        [InlineData(16384)] // Acceptable value
        public void MaxKilobytes_ShouldBeAsSpecified_WhenValueIsValid(long? maxKilobytes)
        {
            var expectedMaxKilobytes = maxKilobytes;

            var target = new EventLogTarget();
            target.MaxKilobytes = maxKilobytes;

            Assert.Equal(expectedMaxKilobytes, target.MaxKilobytes);
        }

        [Theory]
        [InlineData(32768, 16384, 32768)] // Should set MaxKilobytes when value is set and valid
        [InlineData(16384, 32768, 32768)] // Should not change MaxKilobytes when initial MaximumKilobytes is bigger
        [InlineData(null, EventLogMock.EventLogDefaultMaxKilobytes, EventLogMock.EventLogDefaultMaxKilobytes)]      // Should not change MaxKilobytes when the value is null
        public void ShouldSetMaxKilobytes_WhenNeeded(long? newValue, long initialValue, long expectedValue)
        {
            string targetLog = "application"; // The Log to write to is intentionally lower case!!
            var eventLogMock = new EventLogMock(
                deleteEventSourceFunction: (source, machineName) => { },
                sourceExistsFunction: (source, machineName) => false,
                logNameFromSourceNameFunction: (source, machineName) => targetLog,
                createEventSourceFunction: (sourceData) => { })
            { MaximumKilobytes = initialValue };
            var target = new EventLogTarget(eventLogMock, null)
            {
                Log = targetLog,
                Source = "NLog.UnitTests" + Guid.NewGuid().ToString("N"), // set the source explicitly to prevent random AppDomain name being used as the source name
                Layout = new SimpleLayout("${message}"), // Be able to check message length and content, the Layout is intentionally only ${message}.
                OnOverflow = EventLogTargetOverflowAction.Truncate,
                MaxMessageLength = MaxMessageLength,
                MaxKilobytes = newValue,
            };
            eventLogMock.AssociateNewEventLog(target.Log, target.MachineName, target.GetFixedSource());

            target.Install(new InstallationContext());

            Assert.Equal(expectedValue, eventLogMock.MaximumKilobytes);
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

        [Theory]
        [InlineData(0, EventLogEntryType.Information, "AtInformationLevel_WhenNLogLevelIsTrace", null)]
        [InlineData(1, EventLogEntryType.Information, "AtInformationLevel_WhenNLogLevelIsDebug", null)]
        [InlineData(2, EventLogEntryType.Information, "AtInformationLevel_WhenNLogLevelIsInfo", null)]
        [InlineData(3, EventLogEntryType.Warning, "AtWarningLevel_WhenNLogLevelIsWarn", null)]
        [InlineData(4, EventLogEntryType.Error, "AtErrorLevel_WhenNLogLevelIsError", null)]
        [InlineData(5, EventLogEntryType.Error, "AtErrorLevel_WhenNLogLevelIsFatal", null)]
        [InlineData(3, EventLogEntryType.SuccessAudit, "AtSuccessAuditLevel_WhenEntryTypeLayoutSpecifiedAsSuccessAudit", "SuccessAudit")]
        [InlineData(3, EventLogEntryType.SuccessAudit, "AtSuccessAuditLevel_WhenEntryTypeLayoutSpecifiedAsSuccessAudit_Uppercase", "SUCCESSAUDIT")]
        [InlineData(1, EventLogEntryType.FailureAudit, "AtFailureAuditLevel_WhenEntryTypeLayoutSpecifiedAsFailureAudit_Space", "FailureAudit ")]
        [InlineData(1, EventLogEntryType.Error, "AtErrorLevel_WhenEntryTypeLayoutSpecifiedAsErrorLowerCase", "error")]
        [InlineData(3, EventLogEntryType.Warning, "AtSpecifiedNLogLevel_WhenWrongEntryTypeLayoutSupplied", "fallback to auto determined")]
        public void TruncatedMessagesShouldBeWrittenAtCorrenpondingNLogLevel(int logLevelOrdinal, EventLogEntryType expectedEventLogEntryType, string expectedMessage, string layoutString)
        {
            LogLevel logLevel = LogLevel.FromOrdinal(logLevelOrdinal);
            Layout entryTypeLayout = layoutString != null ? new SimpleLayout(layoutString) : null;

            var eventRecords = WriteWithMock(logLevel, expectedEventLogEntryType, expectedMessage, entryTypeLayout).ToList();
            Assert.Single(eventRecords);
            AssertWrittenMessage(eventRecords, expectedMessage);
        }

        [Theory]
        [InlineData(0, EventLogEntryType.Information, null)] // AtInformationLevel_WhenNLogLevelIsTrace
        [InlineData(1, EventLogEntryType.Information, null)] // AtInformationLevel_WhenNLogLevelIsDebug
        [InlineData(2, EventLogEntryType.Information, null)] // AtInformationLevel_WhenNLogLevelIsInfo
        [InlineData(3, EventLogEntryType.Warning, null)] // AtWarningLevel_WhenNLogLevelIsWarn
        [InlineData(4, EventLogEntryType.Error, null)] // AtErrorLevel_WhenNLogLevelIsError
        [InlineData(5, EventLogEntryType.Error, null)] // AtErrorLevel_WhenNLogLevelIsFatal
        [InlineData(1, EventLogEntryType.SuccessAudit, "SuccessAudit")] // AtSuccessAuditLevel_WhenEntryTypeLayoutSpecifiedAsSuccessAudit
        [InlineData(1, EventLogEntryType.FailureAudit, "FailureAudit")] // AtFailureLevel_WhenEntryTypeLayoutSpecifiedAsFailureAudit
        [InlineData(1, EventLogEntryType.Error, "error")] // AtErrorLevel_WhenEntryTypeLayoutSpecifiedAsError
        [InlineData(2, EventLogEntryType.Information, "wrong entry type level")] // AtSpecifiedNLogLevel_WhenWrongEntryTypeLayoutSupplied
        public void SplitMessagesShouldBeWrittenAtCorrenpondingNLogLevel(int logLevelOrdinal, EventLogEntryType expectedEventLogEntryType, string layoutString)
        {
            LogLevel logLevel = LogLevel.FromOrdinal(logLevelOrdinal);
            Layout entryTypeLayout = layoutString != null ? new SimpleLayout(layoutString) : null;

            const int expectedEntryCount = 2;
            string messagePart1 = string.Join("", Enumerable.Repeat("l", MaxMessageLength));
            string messagePart2 = "this part must be split";
            string testMessage = messagePart1 + messagePart2;
            var entries = WriteWithMock(logLevel, expectedEventLogEntryType, testMessage, entryTypeLayout, EventLogTargetOverflowAction.Split).ToList();
            Assert.Equal(expectedEntryCount, entries.Count);
        }

        [Fact]
        public void WriteEventLogEntryLargerThanMaxMessageLengthWithOverflowTruncate_TruncatesTheMessage()
        {
            string expectedMessage = string.Join("", Enumerable.Repeat("t", MaxMessageLength));
            string expectedToTruncateMessage = " this part will be truncated";
            string testMessage = expectedMessage + expectedToTruncateMessage;

            var entries = WriteWithMock(LogLevel.Info, EventLogEntryType.Information, testMessage).ToList();

            Assert.Single(entries);
            AssertWrittenMessage(entries, expectedMessage);
        }

        [Fact]
        public void WriteEventLogEntryEqualToMaxMessageLengthWithOverflowTruncate_TheMessageIsNotTruncated()
        {
            string expectedMessage = string.Join("", Enumerable.Repeat("t", MaxMessageLength));
            var entries = WriteWithMock(LogLevel.Info, EventLogEntryType.Information, expectedMessage).ToList();

            Assert.Single(entries);
            AssertWrittenMessage(entries, expectedMessage);
        }

        [Fact]
        public void WriteEventLogEntryLargerThanMaxMessageLengthWithOverflowSplitEntries_TheMessageShouldBeSplit()
        {
            const int expectedEntryCount = 5;
            string messagePart1 = string.Join("", Enumerable.Repeat("a", MaxMessageLength));
            string messagePart2 = string.Join("", Enumerable.Repeat("b", MaxMessageLength));
            string messagePart3 = string.Join("", Enumerable.Repeat("c", MaxMessageLength));
            string messagePart4 = string.Join("", Enumerable.Repeat("d", MaxMessageLength));
            string messagePart5 = "this part must be split too";
            string testMessage = messagePart1 + messagePart2 + messagePart3 + messagePart4 + messagePart5;

            var entries = WriteWithMock(LogLevel.Info, EventLogEntryType.Information, testMessage, null, EventLogTargetOverflowAction.Split).ToList();

            Assert.Equal(expectedEntryCount, entries.Count);

            AssertWrittenMessage(entries, messagePart1);
            AssertWrittenMessage(entries, messagePart2);
            AssertWrittenMessage(entries, messagePart3);
            AssertWrittenMessage(entries, messagePart4);
            AssertWrittenMessage(entries, messagePart5);
        }

        [Fact]
        public void WriteEventLogEntryEqualToMaxMessageLengthWithOverflowSplitEntries_TheMessageShouldBeSplitInTwoChunks()
        {
            const int expectedEntryCount = 2;
            string messagePart1 = string.Join("", Enumerable.Repeat("a", MaxMessageLength));
            string messagePart2 = string.Join("", Enumerable.Repeat("b", MaxMessageLength));
            string testMessage = messagePart1 + messagePart2;

            var entries = WriteWithMock(LogLevel.Info, EventLogEntryType.Information, testMessage, null, EventLogTargetOverflowAction.Split).ToList();

            Assert.Equal(expectedEntryCount, entries.Count);

            AssertWrittenMessage(entries, messagePart1);
            AssertWrittenMessage(entries, messagePart2);
        }


        [Fact]
        public void WriteEventLogEntryEqualToMaxMessageLengthWithOverflowSplitEntries_TheMessageIsNotSplit()
        {
            string expectedMessage = string.Join("", Enumerable.Repeat("a", MaxMessageLength));
            var entries = WriteWithMock(LogLevel.Info, EventLogEntryType.Information, expectedMessage, null, EventLogTargetOverflowAction.Split).ToList();

            Assert.Single(entries);
            AssertWrittenMessage(entries, expectedMessage);
        }

        [Fact]
        public void WriteEventLogEntryEqualToMaxMessageLengthWithOverflowDiscard_TheMessageIsWritten()
        {
            string expectedMessage = string.Join("", Enumerable.Repeat("a", MaxMessageLength));
            var entries = WriteWithMock(LogLevel.Info, EventLogEntryType.Information, expectedMessage, null, EventLogTargetOverflowAction.Discard).ToList();

            Assert.Single(entries);
            AssertWrittenMessage(entries, expectedMessage);
        }

        [Fact]
        public void WriteEventLogEntryLargerThanMaxMessageLengthWithOverflowDiscard_TheMessageIsNotWritten()
        {
            string messagePart1 = string.Join("", Enumerable.Repeat("a", MaxMessageLength));
            string messagePart2 = "b";
            string testMessage = messagePart1 + messagePart2;
            bool wasWritten = WriteWithMock(LogLevel.Info, EventLogEntryType.Information, testMessage, null, EventLogTargetOverflowAction.Discard).Any();
            Assert.False(wasWritten);
        }

        [Fact]
        public void WriteEventLogEntry_WithoutSource_WillBeDiscarded()
        {
            // Arrange
            var eventLogMock = new EventLogMock(
                deleteEventSourceFunction: (source, machineName) => { },
                sourceExistsFunction: (source, machineName) => true,
                logNameFromSourceNameFunction: (source, machineName) => string.Empty,
                createEventSourceFunction: (sourceData) => { });
            var target = new EventLogTarget(eventLogMock, null);
            target.Source = "${event-properties:item=DynamicSource}";

            var logFactory = new LogFactory();
            var logConfig = new LoggingConfiguration(logFactory);
            logConfig.AddRuleForAllLevels(target);
            logFactory.Configuration = logConfig;

            // Act
            var logger = logFactory.GetLogger("EventLogCorrectLog");
            logger.Info("Hello");

            // Assert
            Assert.Empty(eventLogMock.WrittenEntries);
        }

        [Fact]
        public void WriteEventLogEntry_WillRecreate_WhenWrongLogName()
        {
            // Arrange
            string sourceName = "NLog.UnitTests" + Guid.NewGuid().ToString("N");
            string deletedSourceName = string.Empty;
            string createdLogName = string.Empty;

            var eventLogMock = new EventLogMock(
                deleteEventSourceFunction: (source, machineName) => deletedSourceName = source,
                sourceExistsFunction: (source, machineName) => true,
                logNameFromSourceNameFunction: (source, machineName) => "FaultyLog",
                createEventSourceFunction: (sourceData) => createdLogName = sourceData.LogName);
            var target = new EventLogTarget(eventLogMock, null);
            target.Log = "CorrectLog";
            target.Source = sourceName;
            target.Layout = "${message}";

            var logFactory = new LogFactory();
            var logConfig = new LoggingConfiguration(logFactory);
            logConfig.AddRuleForAllLevels(target);
            logFactory.Configuration = logConfig;

            // Act
            var logger = logFactory.GetLogger("EventLogCorrectLog");
            logger.Info("Hello");

            // Assert
            Assert.Equal(sourceName, deletedSourceName);
            Assert.Equal(target.Log, createdLogName);
            Assert.Single(eventLogMock.WrittenEntries);
            Assert.Equal(target.Log, eventLogMock.WrittenEntries[0].Log);
            Assert.Equal(sourceName, eventLogMock.WrittenEntries[0].Source);
            Assert.Equal("Hello", eventLogMock.WrittenEntries[0].Message);
        }

        [Fact]
        public void WriteEventLogEntry_WillComplain_WhenWrongLogName()
        {
            // Arrange
            string sourceName = "NLog.UnitTests" + Guid.NewGuid().ToString("N");
            string deletedSourceName = string.Empty;
            string createdLogName = string.Empty;
            var eventLogMock = new EventLogMock(
                deleteEventSourceFunction: (source, machineName) => deletedSourceName = source,
                sourceExistsFunction: (source, machineName) => true,
                logNameFromSourceNameFunction: (source, machineName) => "FaultyLog",
                createEventSourceFunction: (sourceData) => createdLogName = sourceData.LogName);
            var target = new EventLogTarget(eventLogMock, null);
            target.Log = "CorrectLog";
            target.Source = "${event-properties:item=DynamicSource}";
            target.Layout = "${message}";

            var logFactory = new LogFactory();
            var logConfig = new LoggingConfiguration(logFactory);
            logConfig.AddRuleForAllLevels(target);
            logFactory.Configuration = logConfig;

            // Act
            var logger = logFactory.GetLogger("EventLogCorrectLog");
            logger.Info("Hello {DynamicSource:l}", sourceName);

            // Assert
            Assert.Equal(string.Empty, deletedSourceName);
            Assert.Equal(string.Empty, createdLogName);
            Assert.Equal(target.Log, eventLogMock.WrittenEntries[0].Log);
            Assert.Equal(sourceName, eventLogMock.WrittenEntries[0].Source);
            Assert.Equal($"Hello {sourceName}", eventLogMock.WrittenEntries[0].Message);
        }

        [Fact]
        public void WriteEventLogEntryWithDynamicSource()
        {
            const int maxMessageLength = 10;
            string expectedMessage = string.Join("", Enumerable.Repeat("a", maxMessageLength));

            var target = CreateEventLogTarget("NLog.UnitTests" + Guid.NewGuid().ToString("N"), EventLogTargetOverflowAction.Split, maxMessageLength);
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
            Assert.Single(entries);
            AssertWrittenMessage(entries, expectedMessage);

            sourceName = "NLog.UnitTests" + Guid.NewGuid().ToString("N");
            expectedMessage = string.Join("", Enumerable.Repeat("b", maxMessageLength));

            logEvent = CreateLogEventWithDynamicSource(expectedMessage, LogLevel.Trace, "DynamicSource", sourceName);
            logger.Log(logEvent);

            entries = GetEventRecords(eventLog.Log).ToList();
            entries = entries.Where(a => a.ProviderName == sourceName).ToList();
            Assert.Single(entries);
            AssertWrittenMessage(entries, expectedMessage);
        }

        [Fact]
        public void LogEntryWithStaticEventIdAndCategoryInTargetLayout()
        {
            var rnd = new Random();
            int eventId = rnd.Next(1, short.MaxValue);
            int category = rnd.Next(1, short.MaxValue);
            var target = CreateEventLogTarget("NLog.UnitTests" + Guid.NewGuid().ToString("N"), EventLogTargetOverflowAction.Truncate, 5000);
            target.EventId = eventId;
            target.Category = (short)category;
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
            Assert.Single(filtered);
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
            var target = CreateEventLogTarget("NLog.UnitTests" + Guid.NewGuid().ToString("N"), EventLogTargetOverflowAction.Truncate, 5000);
            target.EventId = "${event-properties:EventId}";
            target.Category = "${event-properties:Category}";
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
            Assert.Single(filtered);
            var record = filtered.First();
            Assert.Equal(eventId, record.Id);
            Assert.Equal(category, record.Task);
        }

        private static IEnumerable<EventLogMock.EventRecord> WriteWithMock(LogLevel logLevel, EventLogEntryType expectedEventLogEntryType,
            string logMessage, Layout entryType = null, EventLogTargetOverflowAction overflowAction = EventLogTargetOverflowAction.Truncate, int maxMessageLength = MaxMessageLength)
        {
            var sourceName = "NLog.UnitTests" + Guid.NewGuid().ToString("N");

            var eventLogMock = new EventLogMock(
                deleteEventSourceFunction: (source, machineName) => { },
                sourceExistsFunction: (source, machineName) => false,
                logNameFromSourceNameFunction: (source, machineName) => string.Empty,
                createEventSourceFunction: (sourceData) => { });
            var target = new EventLogTarget(eventLogMock, null);
            InitializeEventLogTarget(target, sourceName, overflowAction, maxMessageLength, entryType);

            SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);

            var logger = LogManager.GetLogger("WriteEventLogEntry");
            logger.Log(logLevel, logMessage);

            var entries = eventLogMock.WrittenEntries;

            var expectedSource = target.GetFixedSource();

            var filteredEntries = entries.Where(entry =>
                                            entry.Source == expectedSource &&
                                            entry.EntryType == expectedEventLogEntryType
                                            );
            if (overflowAction == EventLogTargetOverflowAction.Discard && logMessage.Length > maxMessageLength)
            {
                Assert.False(filteredEntries.Any(),
                    $"No message is expected. But {filteredEntries.Count()} message(s) found entry of type '{expectedEventLogEntryType}' from source '{expectedSource}'.");
            }
            else
            {
                Assert.True(filteredEntries.Any(),
                    $"Failed to find entry of type '{expectedEventLogEntryType}' from source '{expectedSource}'");
            }

            return filteredEntries;
        }

        private void AssertWrittenMessage(IEnumerable<EventRecord> eventLogs, string expectedMessage)
        {
            var messages = eventLogs.Where(entry => entry.Properties.Any(prop => Convert.ToString(prop.Value) == expectedMessage));
            Assert.True(messages.Any(), $"Event records has not the expected message: '{expectedMessage}'");
        }

        private void AssertWrittenMessage(IEnumerable<EventLogMock.EventRecord> eventLogs, string expectedMessage)
        {
            var messages = eventLogs.Where(entry => entry.Message == expectedMessage);
            Assert.True(messages.Any(), $"Event records has not the expected message: '{expectedMessage}'");
        }

        private static EventLogTarget CreateEventLogTarget(string sourceName, EventLogTargetOverflowAction overflowAction, int maxMessageLength, Layout entryType = null)
        {
            return InitializeEventLogTarget(new EventLogTarget(), sourceName, overflowAction, maxMessageLength, entryType);
        }

        private static EventLogTarget InitializeEventLogTarget(EventLogTarget target, string sourceName, EventLogTargetOverflowAction overflowAction, int maxMessageLength, Layout entryType)
        {
            target.Name = "eventlog";
            target.Log = "application"; // The Log to write to is intentionally lower case!!
            target.Source = sourceName; // set the source explicitly to prevent random AppDomain name being used as the source name
            target.Layout = new SimpleLayout("${message}"); //Be able to check message length and content, the Layout is intentionally only ${message}.
            target.OnOverflow = overflowAction;
            target.MaxMessageLength = maxMessageLength;

            if (entryType != null)
            {
                target.EntryType = new Layout<EventLogEntryType>(entryType);
            }

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

        private class EventLogMock : EventLogTarget.IEventLogWrapper
        {
            public const int EventLogDefaultMaxKilobytes = 512;

            public EventLogMock(
                Action<string, string> deleteEventSourceFunction,
                Func<string, string, bool> sourceExistsFunction,
                Func<string, string, string> logNameFromSourceNameFunction,
                Action<EventSourceCreationData> createEventSourceFunction)
            {
                DeleteEventSourceFunction = deleteEventSourceFunction ?? throw new ArgumentNullException(nameof(deleteEventSourceFunction));
                SourceExistsFunction = sourceExistsFunction ?? throw new ArgumentNullException(nameof(sourceExistsFunction));
                LogNameFromSourceNameFunction = logNameFromSourceNameFunction ?? throw new ArgumentNullException(nameof(logNameFromSourceNameFunction));
                CreateEventSourceFunction = createEventSourceFunction ?? throw new ArgumentNullException(nameof(createEventSourceFunction));
            }

            private Action<string, string> DeleteEventSourceFunction { get; }
            private Func<string, string, bool> SourceExistsFunction { get; }
            private Func<string, string, string> LogNameFromSourceNameFunction { get; }
            private Action<EventSourceCreationData> CreateEventSourceFunction { get; }

            public class EventRecord
            {
                public string Message { get; set; }
                public EventLogEntryType EntryType { get; set; }
                public string Log { get; set; }
                public string Source { get; set; }
                public string MachineName { get; set; }
                public int EventId { get; set; }
                public short Category { get; set; }
            }

            internal List<EventRecord> WrittenEntries { get; } = new List<EventRecord>();

            /// <inheritdoc />
            public string Source { get; set; }

            /// <inheritdoc />
            public string Log { get; set; }

            /// <inheritdoc />
            public string MachineName { get; set; }

            /// <inheritdoc />
            public long MaximumKilobytes { get; set; } = EventLogDefaultMaxKilobytes;

            /// <inheritdoc />
            public void WriteEntry(string message, EventLogEntryType entryType, int eventId, short category)
            {
                if (!IsEventLogAssociated)
                    throw new InvalidOperationException("Missing initialization using AssociateNewEventLog");

                WrittenEntries.Add(new EventRecord()
                {
                    Message = message,
                    EntryType = entryType,
                    EventId = eventId,
                    Category = category,
                    Source = Source,
                    Log = Log,
                    MachineName = MachineName,
                });
            }

            /// <inheritdoc />
            public bool IsEventLogAssociated { get; private set; }

            /// <inheritdoc />
            public void AssociateNewEventLog(string logName, string machineName, string source)
            {
                Log = logName;
                MachineName = machineName;
                Source = source;

                if (!IsEventLogAssociated)
                {
                    IsEventLogAssociated = true;
                }
            }

            /// <inheritdoc />
            public void DeleteEventSource(string source, string machineName) => DeleteEventSourceFunction(source, machineName);

            /// <inheritdoc />
            public bool SourceExists(string source, string machineName) => SourceExistsFunction(source, machineName);

            /// <inheritdoc />
            public string LogNameFromSourceName(string source, string machineName) => LogNameFromSourceNameFunction(source, machineName);

            /// <inheritdoc />
            public void CreateEventSource(EventSourceCreationData sourceData) => CreateEventSourceFunction(sourceData);
        }
    }
}

#endif
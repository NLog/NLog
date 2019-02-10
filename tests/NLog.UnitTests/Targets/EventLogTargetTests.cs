// 
// Copyright (c) 2004-2019 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

using System.Globalization;
using System.Reflection;
using System.Security.Principal;

#if  !MONO && !NETSTANDARD

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
        [InlineData(1, EventLogEntryType.FailureAudit, "AtFailureAuditLevel_WhenEntryTypeLayoutSpecifiedAsFailureAudit", "FailureAudit")]
        [InlineData(1, EventLogEntryType.Error, "AtErrorLevel_WhenEntryTypeLayoutSpecifiedAsError", "error")]
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
            using (new NoThrowNLogExceptions())
            {
                string messagePart1 = string.Join("", Enumerable.Repeat("a", MaxMessageLength));
                string messagePart2 = "b";
                string testMessage = messagePart1 + messagePart2;
                bool wasWritten = WriteWithMock(LogLevel.Info, EventLogEntryType.Information, testMessage, null, EventLogTargetOverflowAction.Discard).Any();

                Assert.False(wasWritten);
            }
        }


        [Fact]
        public void WriteEventLogEntryWithDynamicSource()
        {
            const int maxMessageLength = 10;
            string expectedMessage = string.Join("", Enumerable.Repeat("a", maxMessageLength));

            var target = CreateEventLogTarget<EventLogTarget>("NLog.UnitTests" + Guid.NewGuid().ToString("N"), EventLogTargetOverflowAction.Split, maxMessageLength);
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
            var target = CreateEventLogTarget<EventLogTarget>("NLog.UnitTests" + Guid.NewGuid().ToString("N"), EventLogTargetOverflowAction.Truncate, 5000);
            target.EventId = eventId;
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
            var target = CreateEventLogTarget<EventLogTarget>("NLog.UnitTests" + Guid.NewGuid().ToString("N"), EventLogTargetOverflowAction.Truncate, 5000);
            target.EventId = "${event-properties:EventId}";
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
            Assert.Single(filtered);
            var record = filtered.First();
            Assert.Equal(eventId, record.Id);
            Assert.Equal(category, record.Task);
        }

        private static IEnumerable<EventRecord> WriteWithMock(LogLevel logLevel, EventLogEntryType expectedEventLogEntryType,
            string logMessage, Layout entryType = null, EventLogTargetOverflowAction overflowAction = EventLogTargetOverflowAction.Truncate, int maxMessageLength = MaxMessageLength)
        {
            var target = CreateEventLogTarget<EventLogTargetMock>("NLog.UnitTests" + Guid.NewGuid().ToString("N"), overflowAction, maxMessageLength, entryType);
            SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);

            var logger = LogManager.GetLogger("WriteEventLogEntry");
            logger.Log(logLevel, logMessage);

            var entries = target.CapturedEvents;

            var expectedSource = target.GetFixedSource();

            var filteredEntries = entries.Where(entry =>
                                            entry.ProviderName == expectedSource &&
                                            HasEntryType(entry, expectedEventLogEntryType)
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

        private class EventRecordMock : EventRecord
        {
            /// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Eventing.Reader.EventRecord" /> class.</summary>
            public EventRecordMock(int id, string logName, string providerName, EventLogEntryType type, string message, short category)
            {
                Id = id;
                LogName = logName;
                ProviderName = providerName;

                if (type == EventLogEntryType.FailureAudit)
                {
                    Keywords = (long)StandardEventKeywords.AuditFailure;
                }
                else if (type == EventLogEntryType.SuccessAudit)
                {
                    Keywords = (long)StandardEventKeywords.AuditSuccess;
                }
                else
                {
                    Keywords = (long)StandardEventKeywords.EventLogClassic;
                    if (type == EventLogEntryType.Error)
                        Level = (byte)StandardEventLevel.Error;
                    else if (type == EventLogEntryType.Warning)
                        Level = (byte)StandardEventLevel.Warning;
                    else if (type == EventLogEntryType.Information)
                        Level = (byte)StandardEventLevel.Informational;
                }

                var eventProperty = CreateEventProperty(message);
                Properties = new List<EventProperty> { eventProperty };
            }

            /// <summary>
            /// EventProperty ctor is internal
            /// </summary>
            /// <param name="message"></param>
            /// <returns></returns>
            private static EventProperty CreateEventProperty(string message)
            {
                BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
                CultureInfo culture = null; // use InvariantCulture or other if you prefer
                object instantiatedType =
                    Activator.CreateInstance(typeof(EventProperty), flags, null, new object[] { message }, culture);

                return (EventProperty)instantiatedType;
            }

            #region Overrides of EventRecord

            /// <summary>Gets the event message in the current locale.</summary>
            /// <returns>Returns a string that contains the event message in the current locale.</returns>
            public override string FormatDescription()
            {
                throw new NotImplementedException();
            }

            /// <summary>Gets the event message, replacing variables in the message with the specified values.</summary>
            /// <returns>Returns a string that contains the event message in the current locale.</returns>
            /// <param name="values">The values used to replace variables in the event message. Variables are represented by %n, where n is a number.</param>
            public override string FormatDescription(IEnumerable<object> values)
            {
                throw new NotImplementedException();
            }

            /// <summary>Gets the XML representation of the event. All of the event properties are represented in the event XML. The XML conforms to the event schema.</summary>
            /// <returns>Returns a string that contains the XML representation of the event.</returns>
            public override string ToXml()
            {
                throw new NotImplementedException();
            }

            /// <summary>Gets the identifier for this event. All events with this identifier value represent the same type of event.</summary>
            /// <returns>Returns an integer value. This value can be null.</returns>
            public override int Id { get; }

            /// <summary>Gets the version number for the event.</summary>
            /// <returns>Returns a byte value. This value can be null.</returns>
            public override byte? Version { get; }

            /// <summary>Gets the level of the event. The level signifies the severity of the event. For the name of the level, get the value of the <see cref="P:System.Diagnostics.Eventing.Reader.EventRecord.LevelDisplayName" /> property.</summary>
            /// <returns>Returns a byte value. This value can be null.</returns>
            public override byte? Level { get; }

            /// <summary>Gets a task identifier for a portion of an application or a component that publishes an event. A task is a 16-bit value with 16 top values reserved. This type allows any value between 0x0000 and 0xffef to be used. To obtain the task name, get the value of the <see cref="P:System.Diagnostics.Eventing.Reader.EventRecord.TaskDisplayName" /> property.</summary>
            /// <returns>Returns an integer value. This value can be null.</returns>
            public override int? Task { get; }

            /// <summary>Gets the opcode of the event. The opcode defines a numeric value that identifies the activity or a point within an activity that the application was performing when it raised the event. For the name of the opcode, get the value of the <see cref="P:System.Diagnostics.Eventing.Reader.EventRecord.OpcodeDisplayName" /> property.</summary>
            /// <returns>Returns a short value. This value can be null.</returns>
            public override short? Opcode { get; }

            /// <summary>Gets the keyword mask of the event. Get the value of the <see cref="P:System.Diagnostics.Eventing.Reader.EventRecord.KeywordsDisplayNames" /> property to get the name of the keywords used in this mask.</summary>
            /// <returns>Returns a long value. This value can be null.</returns>
            public override long? Keywords { get; }

            /// <summary>Gets the event record identifier of the event in the log.</summary>
            /// <returns>Returns a long value. This value can be null.</returns>
            public override long? RecordId { get; }

            /// <summary>Gets the name of the event provider that published this event.</summary>
            /// <returns>Returns a string that contains the name of the event provider that published this event.</returns>
            public override string ProviderName { get; }

            /// <summary>Gets the globally unique identifier (GUID) of the event provider that published this event.</summary>
            /// <returns>Returns a GUID value. This value can be null.</returns>
            public override Guid? ProviderId { get; }

            /// <summary>Gets the name of the event log where this event is logged.</summary>
            /// <returns>Returns a string that contains a name of the event log that contains this event.</returns>
            public override string LogName { get; }

            /// <summary>Gets the process identifier for the event provider that logged this event.</summary>
            /// <returns>Returns an integer value. This value can be null.</returns>
            public override int? ProcessId { get; }

            /// <summary>Gets the thread identifier for the thread that the event provider is running in.</summary>
            /// <returns>Returns an integer value. This value can be null.</returns>
            public override int? ThreadId { get; }

            /// <summary>Gets the name of the computer on which this event was logged.</summary>
            /// <returns>Returns a string that contains the name of the computer on which this event was logged.</returns>
            public override string MachineName { get; }

            /// <summary>Gets the security descriptor of the user whose context is used to publish the event.</summary>
            /// <returns>Returns a <see cref="T:System.Security.Principal.SecurityIdentifier" /> value.</returns>
            public override SecurityIdentifier UserId { get; }

            /// <summary>Gets the time, in <see cref="T:System.DateTime" /> format, that the event was created.</summary>
            /// <returns>Returns a <see cref="T:System.DateTime" /> value. The value can be null.</returns>
            public override DateTime? TimeCreated { get; }

            /// <summary>Gets the globally unique identifier (GUID) for the activity in process for which the event is involved. This allows consumers to group related activities.</summary>
            /// <returns>Returns a GUID value.</returns>
            public override Guid? ActivityId { get; }

            /// <summary>Gets a globally unique identifier (GUID) for a related activity in a process for which an event is involved.</summary>
            /// <returns>Returns a GUID value. This value can be null.</returns>
            public override Guid? RelatedActivityId { get; }

            /// <summary>Gets qualifier numbers that are used for event identification.</summary>
            /// <returns>Returns an integer value. This value can be null.</returns>
            public override int? Qualifiers { get; }

            /// <summary>Gets the display name of the level for this event.</summary>
            /// <returns>Returns a string that contains the display name of the level for this event.</returns>
            public override string LevelDisplayName { get; }

            /// <summary>Gets the display name of the opcode for this event.</summary>
            /// <returns>Returns a string that contains the display name of the opcode for this event.</returns>
            public override string OpcodeDisplayName { get; }

            /// <summary>Gets the display name of the task for the event.</summary>
            /// <returns>Returns a string that contains the display name of the task for the event.</returns>
            public override string TaskDisplayName { get; }

            /// <summary>Gets the display names of the keywords used in the keyword mask for this event. </summary>
            /// <returns>Returns an enumerable collection of strings that contain the display names of the keywords used in the keyword mask for this event.</returns>
            public override IEnumerable<string> KeywordsDisplayNames { get; }

            /// <summary>Gets a placeholder (bookmark) that corresponds to this event. This can be used as a placeholder in a stream of events.</summary>
            /// <returns>Returns a <see cref="T:System.Diagnostics.Eventing.Reader.EventBookmark" /> object.</returns>
            public override EventBookmark Bookmark { get; }

            /// <summary>Gets the user-supplied properties of the event.</summary>
            /// <returns>Returns a list of <see cref="T:System.Diagnostics.Eventing.Reader.EventProperty" /> objects.</returns>
            public override IList<EventProperty> Properties { get; }

            #endregion
        }

        private class EventLogTargetMock : EventLogTarget
        {
            public List<EventRecordMock> CapturedEvents { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="EventLogTarget"/> class.
            /// </summary>
            public EventLogTargetMock()
            {
                CapturedEvents = new List<EventRecordMock>();
            }

            #region Overrides of EventLogTarget

            internal override void WriteEntry(LogEventInfo logEventInfo, string message, EventLogEntryType entryType, int eventId, short category)
            {
                var source = RenderSource(logEventInfo);

                CapturedEvents.Add(new EventRecordMock(eventId, Log, source, entryType, message, category));
            }

            #endregion
        }

        private void AssertWrittenMessage(IEnumerable<EventRecord> eventLogs, string expectedMessage)
        {
            var messages = eventLogs.Where(entry => entry.Properties.Any(prop => Convert.ToString(prop.Value) == expectedMessage));
            Assert.True(messages.Any(), $"Event records has not the expected message: '{expectedMessage}'");
        }

        private static TEventLogTarget CreateEventLogTarget<TEventLogTarget>(
            string sourceName, EventLogTargetOverflowAction overflowAction, int maxMessageLength, Layout entryType = null)
            where TEventLogTarget : EventLogTarget, new()
        {
            var target = new TEventLogTarget
            {
                Log = "application", // The Log to write to is intentionally lower case!!
                Source = sourceName, // set the source explicitly to prevent random AppDomain name being used as the source name
                Layout = new SimpleLayout("${message}"), //Be able to check message length and content, the Layout is intentionally only ${message}.
                OnOverflow = overflowAction,
                MaxMessageLength = maxMessageLength,
            };

            if (entryType != null)
            {
                target.EntryType = entryType;
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

            internal List<EventRecordMock> WrittenEntries { get; } = new List<EventRecordMock>();

            /// <inheritdoc />
            public string Source { get; set; }

            /// <inheritdoc />
            public string Log { get; set; }

            /// <inheritdoc />
            public string MachineName { get; set; }

            /// <inheritdoc />
            public long MaximumKilobytes { get; set; } = EventLogDefaultMaxKilobytes;

            /// <inheritdoc />
            public void WriteEntry(string message, EventLogEntryType entryType, int eventId, short category) =>
                WrittenEntries.Add(new EventRecordMock(eventId, Log, Source, entryType, message, category));

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
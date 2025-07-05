//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

#if NETFRAMEWORK || WindowsEventLogPackage

namespace NLog.Targets
{
    using System;
    using System.Diagnostics;
    using NLog.Common;
    using NLog.Config;
    using NLog.Layouts;

    /// <summary>
    /// Writes log message to the Event Log.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/nlog/nlog/wiki/EventLog-target">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/nlog/nlog/wiki/EventLog-target">Documentation on NLog Wiki</seealso>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="https://github.com/NLog/NLog/wiki/Configuration-file">configuration file</a>,
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/EventLog/NLog.config" />
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/EventLog/Simple/Example.cs" />
    /// </example>
    [Target("EventLog")]
    public class EventLogTarget : TargetWithLayout, IInstallable
    {
        /// <summary>
        /// Max size in characters (limitation of the EventLog API).
        /// </summary>
        /// <seealso href="https://docs.microsoft.com/en-gb/windows/win32/api/winbase/nf-winbase-reporteventw"/>
        internal const int EventLogMaxMessageLength = 30000;

        private readonly IEventLogWrapper _eventLogWrapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogTarget"/> class.
        /// </summary>
        public EventLogTarget()
        {
            Source = AppDomain.CurrentDomain.FriendlyName;
            _eventLogWrapper = new EventLogWrapper();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogTarget"/> class.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        public EventLogTarget(string name)
            : this()
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogTarget"/> class.
        /// </summary>
        internal EventLogTarget(IEventLogWrapper eventLogWrapper, string sourceName)
        {
            _eventLogWrapper = eventLogWrapper;
            Source = string.IsNullOrEmpty(sourceName) ? AppDomain.CurrentDomain.FriendlyName : sourceName;
        }

        /// <summary>
        /// Gets or sets the name of the machine on which Event Log service is running.
        /// </summary>
        /// <remarks>Default: <see cref="String.Empty"/></remarks>
        /// <docgen category='Event Log Options' order='10' />
        public Layout MachineName { get; set; } = Layout.Empty;

        /// <summary>
        /// Gets or sets the layout that renders event ID.
        /// </summary>
        /// <remarks>Default: <code>${event-properties:item=EventId}</code></remarks>
        /// <docgen category='Event Log Options' order='10' />
        public Layout<int> EventId { get; set; } = "${event-properties:item=EventId}";

        /// <summary>
        /// Gets or sets the layout that renders event Category.
        /// </summary>
        /// <remarks>Default: <see langword="null"/></remarks>
        /// <docgen category='Event Log Options' order='10' />
        public Layout<short>? Category { get; set; }

        /// <summary>
        /// Optional entry type. When not set, or when not convertible to <see cref="EventLogEntryType"/> then determined by <see cref="NLog.LogLevel"/>
        /// </summary>
        /// <remarks>Default: <see langword="null"/></remarks>
        /// <docgen category='Event Log Options' order='10' />
        public Layout<EventLogEntryType>? EntryType { get; set; }

        /// <summary>
        /// Gets or sets the value to be used as the event Source.
        /// </summary>
        /// <remarks>
        /// <b>[Required]</b> Default: <see cref="AppDomain.FriendlyName"/>
        /// </remarks>
        /// <docgen category='Event Log Options' order='10' />
        public Layout Source { get; set; } = Layout.Empty;

        /// <summary>
        /// Gets or sets the name of the Event Log to write to. This can be System, Application or any user-defined name.
        /// </summary>
        /// <remarks><b>[Required]</b> Default: <c>Application</c></remarks>
        /// <docgen category='Event Log Options' order='10' />
        public Layout Log { get; set; } = "Application";

        /// <summary>
        /// Gets or sets the message length limit to write to the Event Log.
        /// </summary>
        /// <remarks>Default: <see langword="30000"/></remarks>
        /// <docgen category='Event Log Options' order='10' />
        public Layout<int> MaxMessageLength { get; set; } = EventLogMaxMessageLength;

        /// <summary>
        /// Gets or sets the maximum Event log size in kilobytes.
        /// </summary>
        /// <remarks>
        /// <value>MaxKilobytes</value> cannot be less than 64 or greater than 4194240 or not a multiple of 64.
        /// If <c>null</c>, the value will not be specified while creating the Event log.
        /// </remarks>
        /// <docgen category='Event Log Options' order='10' />
        public Layout<long>? MaxKilobytes { get; set; }

        /// <summary>
        /// Gets or sets the action to take if the message is larger than the <see cref="MaxMessageLength"/> option.
        /// </summary>
        /// <remarks>Default: <see cref="EventLogTargetOverflowAction.Truncate"/></remarks>
        /// <docgen category='Event Log Options' order='100' />
        public EventLogTargetOverflowAction OnOverflow { get; set; } = EventLogTargetOverflowAction.Truncate;

        /// <summary>
        /// Performs installation which requires administrative permissions.
        /// </summary>
        /// <param name="installationContext">The installation context.</param>
        public void Install(InstallationContext installationContext)
        {
            // always throw error to keep backwards compatible behavior.
            CreateEventSourceIfNeeded(GetFixedSource(), true);
        }

        /// <summary>
        /// Performs uninstallation which requires administrative permissions.
        /// </summary>
        /// <param name="installationContext">The installation context.</param>
        public void Uninstall(InstallationContext installationContext)
        {
            var fixedSource = GetFixedSource();
            if (string.IsNullOrEmpty(fixedSource))
            {
                InternalLogger.Debug("{0}: Skipping removing of event source because it contains layout renderers", this);
            }
            else
            {
                _eventLogWrapper.DeleteEventSource(fixedSource, ".");
            }
        }

        /// <summary>
        /// Determines whether the item is installed.
        /// </summary>
        /// <param name="installationContext">The installation context.</param>
        /// <returns>
        /// Value indicating whether the item is installed or null if it is not possible to determine.
        /// </returns>
        public bool? IsInstalled(InstallationContext installationContext)
        {
            var fixedSource = GetFixedSource();
            if (string.IsNullOrEmpty(fixedSource))
            {
                InternalLogger.Debug("{0}: Unclear if event source exists because it contains layout renderers", this);
            }
            else
            {
                return _eventLogWrapper.SourceExists(fixedSource, ".");
            }
            return null;
        }

        /// <inheritdoc/>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            if (Source is null || ReferenceEquals(Source, Layout.Empty))
                throw new NLogConfigurationException("EventLogTarget Source-property must be assigned. Source is needed for EventLog writing.");

            var maxKilobytes = MaxKilobytes?.IsFixed == true ? MaxKilobytes.FixedValue : 0;
            if (maxKilobytes > 0 && (maxKilobytes < 64 || maxKilobytes > 4194240 || (maxKilobytes % 64 != 0))) // Event log API restrictions
                throw new NLogConfigurationException("EventLogTarget MaxKilobytes must be a multiple of 64, and between 64 and 4194240");

            CreateEventSourceIfNeeded(GetFixedSource(), false);
        }

        /// <inheritdoc/>
        protected override void Write(LogEventInfo logEvent)
        {
            string message = RenderLogEvent(Layout, logEvent);

            EventLogEntryType entryType = GetEntryType(logEvent);

            int eventId = RenderLogEvent(EventId, logEvent, defaultValue: 0);
            var category = RenderLogEvent(Category, logEvent, defaultValue: default(short));

            var eventLogSource = RenderLogEvent(Source, logEvent);
            if (string.IsNullOrEmpty(eventLogSource))
            {
                InternalLogger.Warn("{0}: WriteEntry discarded because Source rendered as empty string", this);
                return;
            }

            var eventLogName = RenderLogEvent(Log, logEvent);
            if (string.IsNullOrEmpty(eventLogName))
            {
                InternalLogger.Warn("{0}: WriteEntry discarded because Log rendered as empty string", this);
                return;
            }

            var eventLogMachine = RenderLogEvent(MachineName, logEvent);
            if (string.IsNullOrEmpty(eventLogMachine))
                eventLogMachine = ".";

            // limitation of EventLog API
            var maxMessageLength = RenderLogEvent(MaxMessageLength, logEvent, EventLogMaxMessageLength);
            if (maxMessageLength > 0 && message.Length > maxMessageLength)
            {
                if (OnOverflow == EventLogTargetOverflowAction.Truncate)
                {
                    message = message.Substring(0, maxMessageLength);
                    WriteEntry(eventLogSource, eventLogName, eventLogMachine, message, entryType, eventId, category);
                }
                else if (OnOverflow == EventLogTargetOverflowAction.Split)
                {
                    for (int offset = 0; offset < message.Length; offset += maxMessageLength)
                    {
                        string chunk = message.Substring(offset, Math.Min(maxMessageLength, (message.Length - offset)));
                        WriteEntry(eventLogSource, eventLogName, eventLogMachine, chunk, entryType, eventId, category);
                    }
                }
                else if (OnOverflow == EventLogTargetOverflowAction.Discard)
                {
                    // message should not be written
                    InternalLogger.Debug("{0}: WriteEntry discarded because too big message size: {1}", this, message.Length);
                }
            }
            else
            {
                WriteEntry(eventLogSource, eventLogName, eventLogMachine, message, entryType, eventId, category);
            }
        }

        private void WriteEntry(string eventLogSource, string eventLogName, string eventLogMachine, string message, EventLogEntryType entryType, int eventId, short category)
        {
            var isCacheUpToDate = _eventLogWrapper.IsEventLogAssociated &&
                                  _eventLogWrapper.Source == eventLogSource &&
                                  string.Equals(_eventLogWrapper.Log, eventLogName, StringComparison.OrdinalIgnoreCase) &&
                                  string.Equals(_eventLogWrapper.MachineName, eventLogMachine, StringComparison.OrdinalIgnoreCase);

            if (!isCacheUpToDate)
            {
                InternalLogger.Debug("{0}: Refresh EventLog Source {1} and Log {2}", this, eventLogSource, eventLogName);

                _eventLogWrapper.AssociateNewEventLog(eventLogName, eventLogMachine, eventLogSource);
                try
                {
                    if (!_eventLogWrapper.SourceExists(eventLogSource, eventLogMachine))
                    {
                        InternalLogger.Warn("{0}: Source {1} does not exist", this, eventLogSource);
                    }
                    else
                    {
                        var currentLogName = _eventLogWrapper.LogNameFromSourceName(eventLogSource, eventLogMachine);
                        if (!currentLogName.Equals(eventLogName, StringComparison.OrdinalIgnoreCase))
                        {
                            InternalLogger.Debug("{0}: Source {1} should be mapped to Log {2}, but EventLog.LogNameFromSourceName returns {3}", this, eventLogSource, eventLogName, currentLogName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (LogManager.ThrowExceptions)
                        throw;

                    InternalLogger.Warn(ex, "{0}: Exception thrown when checking if Source {1} and Log {2} are valid", this, eventLogSource, eventLogName);
                }
            }

            _eventLogWrapper.WriteEntry(message, entryType, eventId, category);
        }

        /// <summary>
        /// Get the entry type for logging the message.
        /// </summary>
        /// <param name="logEvent">The logging event - for rendering the <see cref="EntryType"/></param>
        private EventLogEntryType GetEntryType(LogEventInfo logEvent)
        {
            var eventLogEntryType = RenderLogEvent(EntryType, logEvent, (EventLogEntryType)0);
            if (eventLogEntryType != (EventLogEntryType)0)
            {
                return eventLogEntryType;
            }

            // determine auto
            if (logEvent.Level >= LogLevel.Error)
            {
                return EventLogEntryType.Error;
            }
            if (logEvent.Level >= LogLevel.Warn)
            {
                return EventLogEntryType.Warning;
            }
            return EventLogEntryType.Information;
        }


        /// <summary>
        /// Get the source, if and only if the source is fixed.
        /// </summary>
        /// <returns><c>null</c> when not <see cref="SimpleLayout.IsFixedText"/></returns>
        /// <remarks>Internal for unit tests</remarks>
        internal string GetFixedSource()
        {
            if (Source is SimpleLayout simpleLayout && simpleLayout.IsFixedText && Log is SimpleLayout logNameLayout && logNameLayout.IsFixedText)
            {
                if (MachineName is null || (MachineName is SimpleLayout machineLayout && machineLayout.IsFixedText && (".".Equals(machineLayout.FixedText) || string.IsNullOrEmpty(machineLayout.FixedText))))
                {
                    return simpleLayout.FixedText ?? string.Empty;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// (re-)create an event source, if it isn't there. Works only with fixed source names.
        /// </summary>
        /// <param name="fixedSource">The source name. If source is not fixed (see <see cref="SimpleLayout.IsFixedText"/>, then pass <c>null</c> or <see cref="string.Empty"/>.</param>
        /// <param name="alwaysThrowError">always throw an Exception when there is an error</param>
        private void CreateEventSourceIfNeeded(string fixedSource, bool alwaysThrowError)
        {
            if (string.IsNullOrEmpty(fixedSource))
            {
                InternalLogger.Debug("{0}: Skipping creation of event source because it contains layout renderers", this);
                // we can only create event sources if the source is fixed (no layout)
                return;
            }

            var eventLogName = RenderLogEvent(Log, LogEventInfo.CreateNullEvent());
            var eventLogMachine = RenderLogEvent(MachineName, LogEventInfo.CreateNullEvent());
            if (string.IsNullOrEmpty(eventLogMachine))
                eventLogMachine = ".";
            var maxKilobytes = RenderLogEvent(MaxKilobytes, LogEventInfo.CreateNullEvent(), 0);

            // if we throw anywhere, we remain non-operational
            try
            {
                if (_eventLogWrapper.SourceExists(fixedSource, eventLogMachine))
                {
                    string currentLogName = _eventLogWrapper.LogNameFromSourceName(fixedSource, eventLogMachine);
                    if (!currentLogName.Equals(eventLogName, StringComparison.OrdinalIgnoreCase))
                    {
                        InternalLogger.Debug("{0}: Updating source {1} to use log {2}, instead of {3} (Computer restart is needed)", this, fixedSource, eventLogName, currentLogName);

                        // re-create the association between Log and Source
                        _eventLogWrapper.DeleteEventSource(fixedSource, eventLogMachine);

                        var eventSourceCreationData = new EventSourceCreationData(fixedSource, eventLogName)
                        {
                            MachineName = eventLogMachine
                        };
                        _eventLogWrapper.CreateEventSource(eventSourceCreationData);
                    }
                }
                else
                {
                    InternalLogger.Debug("{0}: Creating source {1} to use log {2}", this, fixedSource, eventLogName);
                    var eventSourceCreationData = new EventSourceCreationData(fixedSource, eventLogName)
                    {
                        MachineName = eventLogMachine
                    };
                    _eventLogWrapper.CreateEventSource(eventSourceCreationData);
                }

                _eventLogWrapper.AssociateNewEventLog(eventLogName, eventLogMachine, fixedSource);

                if (maxKilobytes > 0 && _eventLogWrapper.MaximumKilobytes < maxKilobytes)
                {
                    _eventLogWrapper.MaximumKilobytes = maxKilobytes;
                }
            }
            catch (Exception exception)
            {
                InternalLogger.Error(exception, "{0}: Error when connecting to EventLog. Source={1} in Log={2}", this, fixedSource, eventLogName);
                if (alwaysThrowError || LogManager.ThrowExceptions)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// A wrapper for Windows event log.
        /// </summary>
        internal interface IEventLogWrapper
        {
            #region Instance methods

            /// <summary>
            /// A wrapper for the property <see cref="EventLog.Source"/>.
            /// </summary>
            string Source { get; }

            /// <summary>
            /// A wrapper for the property <see cref="EventLog.Log"/>.
            /// </summary>
            string Log { get; }

            /// <summary>
            /// A wrapper for the property <see cref="EventLog.MachineName"/>.
            /// </summary>
            string MachineName { get; }

            /// <summary>
            /// A wrapper for the property <see cref="EventLog.MaximumKilobytes"/>.
            /// </summary>
            long MaximumKilobytes { get; set; }

            /// <summary>
            /// Indicates whether an event log instance is associated.
            /// </summary>
            bool IsEventLogAssociated { get; }

            /// <summary>
            /// A wrapper for the method <see cref="EventLog.WriteEntry(string, EventLogEntryType, int, short)"/>.
            /// </summary>
            void WriteEntry(string message, EventLogEntryType entryType, int eventId, short category);

            #endregion

            #region "Static" methods

            /// <summary>
            /// Creates a new association with an instance of the event log.
            /// </summary>
            void AssociateNewEventLog(string logName, string machineName, string source);

            /// <summary>
            /// A wrapper for the static method <see cref="EventLog.DeleteEventSource(string, string)"/>.
            /// </summary>
            void DeleteEventSource(string source, string machineName);

            /// <summary>
            /// A wrapper for the static method <see cref="EventLog.SourceExists(string, string)"/>.
            /// </summary>
            bool SourceExists(string source, string machineName);

            /// <summary>
            /// A wrapper for the static method <see cref="EventLog.LogNameFromSourceName(string, string)"/>.
            /// </summary>
            string LogNameFromSourceName(string source, string machineName);

            /// <summary>
            /// A wrapper for the static method <see cref="EventLog.CreateEventSource(EventSourceCreationData)"/>.
            /// </summary>
            void CreateEventSource(EventSourceCreationData sourceData);

            #endregion
        }

        /// <summary>
        /// The implementation of <see cref="IEventLogWrapper"/>, that uses Windows <see cref="EventLog"/>.
        /// </summary>
        private sealed class EventLogWrapper : IEventLogWrapper, IDisposable
        {
            private EventLog? _windowsEventLog;

            public string Source { get; private set; } = string.Empty;

            public string Log { get; private set; } = string.Empty;

            public string MachineName { get; private set; } = string.Empty;

            public long MaximumKilobytes
            {
                get => _windowsEventLog?.MaximumKilobytes ?? 0;
                set
                {
                    if (_windowsEventLog != null)
                        _windowsEventLog.MaximumKilobytes = value;
                }
            }
            public bool IsEventLogAssociated => _windowsEventLog != null;

            public void WriteEntry(string message, EventLogEntryType entryType, int eventId, short category) =>
                _windowsEventLog?.WriteEntry(message, entryType, eventId, category);

            /// <summary>
            /// Creates a new association with an instance of Windows <see cref="EventLog"/>.
            /// </summary>
            public void AssociateNewEventLog(string logName, string machineName, string source)
            {
                var windowsEventLog = _windowsEventLog;
                _windowsEventLog = new EventLog(logName, machineName, source);
                Source = source;
                Log = logName;
                MachineName = machineName;
                windowsEventLog?.Dispose();
            }

            public void DeleteEventSource(string source, string machineName) =>
                EventLog.DeleteEventSource(source, machineName);

            public bool SourceExists(string source, string machineName) =>
                EventLog.SourceExists(source, machineName);

            public string LogNameFromSourceName(string source, string machineName) =>
                EventLog.LogNameFromSourceName(source, machineName);

            public void CreateEventSource(EventSourceCreationData sourceData) =>
                EventLog.CreateEventSource(sourceData);

            public void Dispose()
            {
                _windowsEventLog?.Dispose();
                _windowsEventLog = null;
            }
        }
    }
}

#endif

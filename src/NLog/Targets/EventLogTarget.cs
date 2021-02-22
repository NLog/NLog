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

#if !NETSTANDARD || WindowsEventLogPackage

namespace NLog.Targets
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using Internal.Fakeables;
    using NLog.Common;
    using NLog.Config;
    using NLog.Layouts;

    /// <summary>
    /// Writes log message to the Event Log.
    /// </summary>
    /// <seealso href="https://github.com/nlog/nlog/wiki/EventLog-target">Documentation on NLog Wiki</seealso>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/EventLog/NLog.config" />
    /// <p>
    /// This assumes just one target and a single rule. More configuration
    /// options are described <a href="config.html">here</a>.
    /// </p>
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
        internal const int EventLogMaxMessageLength = 16384;

        private readonly IEventLogWrapper _eventLogWrapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogTarget"/> class.
        /// </summary>
        public EventLogTarget()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogTarget"/> class.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        public EventLogTarget(string name)
            : this(null, null)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogTarget"/> class.
        /// </summary>
        /// <param name="appDomain"><see cref="IAppDomain"/>.<see cref="IAppDomain.FriendlyName"/> to be used as Source.</param>
        [Obsolete("This constructor should not be used. Marked obsolete on NLog 4.6")]
        public EventLogTarget(IAppDomain appDomain)
            : this(null, appDomain)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogTarget"/> class.
        /// </summary>
        internal EventLogTarget(IEventLogWrapper eventLogWrapper, IAppDomain appDomain)
        {
            _eventLogWrapper = eventLogWrapper ?? new EventLogWrapper();
            appDomain = appDomain ?? LogFactory.CurrentAppDomain;

            Source = appDomain.FriendlyName;
            Log = "Application";
            MachineName = ".";
            MaxMessageLength = EventLogMaxMessageLength;
        }

        /// <summary>
        /// Gets or sets the name of the machine on which Event Log service is running.
        /// </summary>
        /// <docgen category='Event Log Options' order='10' />
        [DefaultValue(".")]
        public string MachineName { get; set; }

        /// <summary>
        /// Gets or sets the layout that renders event ID.
        /// </summary>
        /// <docgen category='Event Log Options' order='10' />
        public Layout<int> EventId { get; set; }

        /// <summary>
        /// Gets or sets the layout that renders event Category.
        /// </summary>
        /// <docgen category='Event Log Options' order='10' />
        public Layout<short> Category { get; set; }

        /// <summary>
        /// Optional entry type. When not set, or when not convertible to <see cref="EventLogEntryType"/> then determined by <see cref="NLog.LogLevel"/>
        /// </summary>
        /// <docgen category='Event Log Options' order='10' />
        public Layout<EventLogEntryType> EntryType { get; set; }

        /// <summary>
        /// Gets or sets the value to be used as the event Source.
        /// </summary>
        /// <remarks>
        /// By default this is the friendly name of the current AppDomain.
        /// </remarks>
        /// <docgen category='Event Log Options' order='10' />
        public Layout Source { get; set; }

        /// <summary>
        /// Gets or sets the name of the Event Log to write to. This can be System, Application or any user-defined name.
        /// </summary>
        /// <docgen category='Event Log Options' order='10' />
        [DefaultValue("Application")]
        public string Log { get; set; }

        /// <summary>
        /// Gets or sets the message length limit to write to the Event Log.
        /// </summary>
        /// <remarks><value>MaxMessageLength</value> cannot be zero or negative</remarks>
        /// <docgen category='Event Log Options' order='10' />
        [DefaultValue(EventLogMaxMessageLength)]
        public int MaxMessageLength
        {
            get => _maxMessageLength;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("MaxMessageLength cannot be zero or negative.");

                _maxMessageLength = value;
            }
        }
        private int _maxMessageLength;

        /// <summary>
        /// Gets or sets the maximum Event log size in kilobytes.
        /// </summary>
        /// <remarks>
        /// <value>MaxKilobytes</value> cannot be less than 64 or greater than 4194240 or not a multiple of 64.
        /// If <c>null</c>, the value will not be specified while creating the Event log.
        /// </remarks>
        /// <docgen category='Event Log Options' order='10' />
        [DefaultValue(null)]
        public long? MaxKilobytes
        {
            get => _maxKilobytes;
            set
            {
                if (value != null && (value < 64 || value > 4194240 || (value % 64 != 0))) // Event log API restrictions
                    throw new ArgumentException("MaxKilobytes must be a multiple of 64, and between 64 and 4194240");

                _maxKilobytes = value;
            }
        }
        private long? _maxKilobytes;

        /// <summary>
        /// Gets or sets the action to take if the message is larger than the <see cref="MaxMessageLength"/> option.
        /// </summary>
        /// <docgen category='Event Log Overflow Action' order='10' />
        [DefaultValue(EventLogTargetOverflowAction.Truncate)]
        public EventLogTargetOverflowAction OnOverflow { get; set; }

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
                _eventLogWrapper.DeleteEventSource(fixedSource, MachineName);
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

            if (!string.IsNullOrEmpty(fixedSource))
            {
                return _eventLogWrapper.SourceExists(fixedSource, MachineName);
            }
            InternalLogger.Debug("{0}: Unclear if event source exists because it contains layout renderers", this);
            return null; //unclear!
        }

        /// <summary>
        /// Initializes the target.
        /// </summary>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            CreateEventSourceIfNeeded(GetFixedSource(), false);
        }

        /// <summary>
        /// Writes the specified logging event to the event log.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
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

            // limitation of EventLog API
            if (message.Length > MaxMessageLength)
            {
                if (OnOverflow == EventLogTargetOverflowAction.Truncate)
                {
                    message = message.Substring(0, MaxMessageLength);
                    WriteEntry(eventLogSource, message, entryType, eventId, category);
                }
                else if (OnOverflow == EventLogTargetOverflowAction.Split)
                {
                    for (int offset = 0; offset < message.Length; offset += MaxMessageLength)
                    {
                        string chunk = message.Substring(offset, Math.Min(MaxMessageLength, (message.Length - offset)));
                        WriteEntry(eventLogSource, chunk, entryType, eventId, category);
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
                WriteEntry(eventLogSource, message, entryType, eventId, category);
            }
        }

        private void WriteEntry(string eventLogSource, string message, EventLogEntryType entryType, int eventId, short category)
        {
            var isCacheUpToDate = _eventLogWrapper.IsEventLogAssociated &&
                                  _eventLogWrapper.Log == Log &&
                                  _eventLogWrapper.MachineName == MachineName &&
                                  _eventLogWrapper.Source == eventLogSource;

            if (!isCacheUpToDate)
            {
                InternalLogger.Debug("{0}: Refresh EventLog Source {1} and Log {2}", this, eventLogSource, Log);

                _eventLogWrapper.AssociateNewEventLog(Log, MachineName, eventLogSource);
                try
                {
                    if (!_eventLogWrapper.SourceExists(eventLogSource, MachineName))
                    {
                        InternalLogger.Warn("{0}: Source {1} does not exist", this, eventLogSource);
                    }
                    else
                    {
                        var currentLogName = _eventLogWrapper.LogNameFromSourceName(eventLogSource, MachineName);
                        if (!currentLogName.Equals(Log, StringComparison.OrdinalIgnoreCase))
                        {
                            InternalLogger.Debug("{0}: Source {1} should be mapped to Log {2}, but EventLog.LogNameFromSourceName returns {3}", this, eventLogSource, Log, currentLogName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (LogManager.ThrowExceptions)
                        throw;

                    InternalLogger.Warn(ex, "{0}: Exception thrown when checking if Source {1} and LogName {2} are valid", this, eventLogSource, Log);
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
            if (Source is SimpleLayout simpleLayout && simpleLayout.IsFixedText)
            {
                return simpleLayout.FixedText;
            }
            return null;
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

            // if we throw anywhere, we remain non-operational
            try
            {
                if (_eventLogWrapper.SourceExists(fixedSource, MachineName))
                {
                    string currentLogName = _eventLogWrapper.LogNameFromSourceName(fixedSource, MachineName);
                    if (!currentLogName.Equals(Log, StringComparison.OrdinalIgnoreCase))
                    {
                        InternalLogger.Debug("{0}: Updating source {1} to use log {2}, instead of {3} (Computer restart is needed)", this, fixedSource, Log, currentLogName);

                        // re-create the association between Log and Source
                        _eventLogWrapper.DeleteEventSource(fixedSource, MachineName);

                        var eventSourceCreationData = new EventSourceCreationData(fixedSource, Log)
                        {
                            MachineName = MachineName
                        };
                        _eventLogWrapper.CreateEventSource(eventSourceCreationData);
                    }
                }
                else
                {
                    InternalLogger.Debug("{0}: Creating source {1} to use log {2}", this, fixedSource, Log);
                    var eventSourceCreationData = new EventSourceCreationData(fixedSource, Log)
                    {
                        MachineName = MachineName
                    };
                    _eventLogWrapper.CreateEventSource(eventSourceCreationData);
                }

                _eventLogWrapper.AssociateNewEventLog(Log, MachineName, fixedSource);

                if (MaxKilobytes.HasValue && _eventLogWrapper.MaximumKilobytes < MaxKilobytes)
                {
                    _eventLogWrapper.MaximumKilobytes = MaxKilobytes.Value;
                }
            }
            catch (Exception exception)
            {
                InternalLogger.Error(exception, "{0}: Error when connecting to EventLog. Source={1} in Log={2}", this, fixedSource, Log);
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
            private EventLog _windowsEventLog;

            #region Instance methods

            /// <inheritdoc />
            public string Source { get; private set; }

            /// <inheritdoc />
            public string Log { get; private set; }

            /// <inheritdoc />
            public string MachineName { get; private set; }

            /// <inheritdoc />
            public long MaximumKilobytes
            {
                get => _windowsEventLog.MaximumKilobytes;
                set => _windowsEventLog.MaximumKilobytes = value;
            }

            /// <inheritdoc />
            public bool IsEventLogAssociated => _windowsEventLog != null;

            /// <inheritdoc />
            public void WriteEntry(string message, EventLogEntryType entryType, int eventId, short category) =>
                _windowsEventLog.WriteEntry(message, entryType, eventId, category);

            #endregion

            #region "Static" methods

            /// <inheritdoc />
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
                if (windowsEventLog != null)
                    windowsEventLog.Dispose();
            }

            /// <inheritdoc />
            public void DeleteEventSource(string source, string machineName) =>
                EventLog.DeleteEventSource(source, machineName);

            /// <inheritdoc />
            public bool SourceExists(string source, string machineName) =>
                EventLog.SourceExists(source, machineName);

            /// <inheritdoc />
            public string LogNameFromSourceName(string source, string machineName) =>
                EventLog.LogNameFromSourceName(source, machineName);

            /// <inheritdoc />
            public void CreateEventSource(EventSourceCreationData sourceData) =>
                EventLog.CreateEventSource(sourceData);

            public void Dispose()
            {
                _windowsEventLog?.Dispose();
                _windowsEventLog = null;
            }

            #endregion
        }
    }
}

#endif

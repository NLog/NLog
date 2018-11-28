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

#if !SILVERLIGHT && !__IOS__ && !__ANDROID__ && (!NETSTANDARD || WindowsEventLogPackage)

namespace NLog.Targets
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using Internal.Fakeables;
    using NLog.Common;
    using NLog.Config;
    using NLog.Internal;
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
        private IEventLogInstanceWrapper eventLogInstanceWrapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogTarget"/> class.
        /// </summary>
        public EventLogTarget()
            : this(LogFactory.CurrentAppDomain)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogTarget"/> class.
        /// </summary>
        public EventLogTarget(IAppDomain appDomain)
        {
            Source = appDomain.FriendlyName;
            Log = "Application";
            MachineName = ".";
            MaxMessageLength = 16384;
            OptimizeBufferReuse = GetType() == typeof(EventLogTarget);  // Class not sealed, reduce breaking changes
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogTarget"/> class.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        public EventLogTarget(string name)
            : this(LogFactory.CurrentAppDomain)
        {
            Name = name;
        }

        /// <summary>
        /// Gets or sets the provider of Windows event log static methods to be utilized by <see cref="EventLogTarget"/>.
        /// </summary>
        /// <remarks>Introduced to improve the testability of the class.</remarks>
        protected internal IEventLogStaticWrapper EventLogStaticMethods { get; set; } = EventLogStaticWrapper.Instance;

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
        public Layout EventId { get; set; }

        /// <summary>
        /// Gets or sets the layout that renders event Category.
        /// </summary>
        /// <docgen category='Event Log Options' order='10' />
        public Layout Category { get; set; }

        /// <summary>
        /// Optional entrytype. When not set, or when not convertable to <see cref="EventLogEntryType"/> then determined by <see cref="NLog.LogLevel"/>
        /// </summary>
        /// <docgen category='Event Log Options' order='10' />
        public Layout EntryType { get; set; }

        /// <summary>
        /// Gets or sets the value to be used as the event Source.
        /// </summary>
        /// <remarks>
        /// By default this is the friendly name of the current AppDomain.
        /// </remarks>
        /// <docgen category='Event Log Options' order='10' />
        public Layout Source { get; set; }

        /// <summary>
        /// Gets or sets the name of the Event Log to write to. This can be System, Application or 
        /// any user-defined name.
        /// </summary>
        /// <docgen category='Event Log Options' order='10' />
        [DefaultValue("Application")]
        public string Log { get; set; }

        /// <summary>
        /// Gets or sets the message length limit to write to the Event Log.
        /// </summary>
        /// <remarks><value>MaxMessageLength</value> cannot be zero or negative</remarks>
        /// <docgen category='Event Log Options' order='10' />
        [DefaultValue(16384)]
        public int MaxMessageLength
        {
            get => maxMessageLength;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("MaxMessageLength cannot be zero or negative.");

                maxMessageLength = value;
            }
        }
        private int maxMessageLength;

        /// <summary>
        /// Gets or sets the maximum Event log size in kilobytes.
        /// </summary>
        /// <remarks>
        /// <value>MaxKilobytes</value> cannot be less than 64 or greater than 4194240 or not a multiple of 64.
        /// If <c>null</c>, the value will not be specified while creating the Event log. The default value of 512 will be set, as specified by Eventlog API.
        /// </remarks>
        /// <docgen category='Event Log Options' order='10' />
        [DefaultValue(null)]
        public long? MaxKilobytes
        {
            get => maxKilobytes;
            set
            {   //Event log API restriction
                if (value != null && (value < 64 || value > 4194240 || (value % 64 != 0)))
                    throw new ArgumentException("MaxKilobytes must be a multiple of 64, and between 64 and 4194240");
                maxKilobytes = value;
            }
        }
        private long? maxKilobytes;

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
                InternalLogger.Debug("EventLogTarget(Name={0}): Skipping removing of event source because it contains layout renderers", Name);
            }
            else
            {
                EventLogStaticMethods.DeleteEventSource(fixedSource, MachineName);
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
                return EventLogStaticMethods.SourceExists(fixedSource, MachineName);
            }
            InternalLogger.Debug("EventLogTarget(Name={0}): Unclear if event source exists because it contains layout renderers", Name);
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

            int eventId = 0;
            string renderEventId = RenderLogEvent(EventId, logEvent);
            if (!string.IsNullOrEmpty(renderEventId) && !int.TryParse(renderEventId, out eventId))
            {
                InternalLogger.Warn("EventLogTarget(Name={0}): WriteEntry failed to parse EventId={1}", Name, renderEventId);
            }

            short category = 0;
            string renderCategory = RenderLogEvent(Category, logEvent);
            if (!string.IsNullOrEmpty(renderCategory) && !short.TryParse(renderCategory, out category))
            {
                InternalLogger.Warn("EventLogTarget(Name={0}): WriteEntry failed to parse Category={1}", Name, renderCategory);
            }

            // limitation of EventLog API
            if (message.Length > MaxMessageLength)
            {
                if (OnOverflow == EventLogTargetOverflowAction.Truncate)
                {
                    message = message.Substring(0, MaxMessageLength);
                    WriteEntry(logEvent, message, entryType, eventId, category);
                }
                else if (OnOverflow == EventLogTargetOverflowAction.Split)
                {
                    for (int offset = 0; offset < message.Length; offset += MaxMessageLength)
                    {
                        string chunk = message.Substring(offset, Math.Min(MaxMessageLength, (message.Length - offset)));
                        WriteEntry(logEvent, chunk, entryType, eventId, category);
                    }
                }
                else if (OnOverflow == EventLogTargetOverflowAction.Discard)
                {
                    //message will not be written
                    return;
                }
            }
            else
            {
                WriteEntry(logEvent, message, entryType, eventId, category);
            }
        }

        internal virtual void WriteEntry(LogEventInfo logEventInfo, string message, EventLogEntryType entryType, int eventId, short category)
        {
            IEventLogInstanceWrapper eventLog = GetEventLog(logEventInfo);
            eventLog.WriteEntry(message, entryType, eventId, category);
        }

        /// <summary>
        /// Get the entry type for logging the message.
        /// </summary>
        /// <param name="logEvent">The logging event - for rendering the <see cref="EntryType"/></param>
        /// <returns></returns>
        private EventLogEntryType GetEntryType(LogEventInfo logEvent)
        {
            string renderEntryType = RenderLogEvent(EntryType, logEvent);
            if (!string.IsNullOrEmpty(renderEntryType))
            {
                //try parse, if fail,  determine auto
                if (ConversionHelpers.TryParseEnum(renderEntryType, out EventLogEntryType eventLogEntryType))
                {
                    return eventLogEntryType;
                }
                InternalLogger.Warn("EventLogTarget(Name={0}): WriteEntry failed to parse EntryType={1}", Name, renderEntryType);
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
            if (Source == null)
            {
                return null;
            }
            var simpleLayout = Source as SimpleLayout;
            if (simpleLayout != null && simpleLayout.IsFixedText)
            {
                return simpleLayout.FixedText;
            }
            return null;
        }

        /// <summary>
        /// Get the eventlog to write to.
        /// </summary>
        /// <param name="logEvent">Event if the source needs to be rendered.</param>
        /// <returns></returns>
        private IEventLogInstanceWrapper GetEventLog(LogEventInfo logEvent)
        {
            var renderedSource = RenderSource(logEvent);
            var isCacheUpToDate = eventLogInstanceWrapper != null && renderedSource == eventLogInstanceWrapper.Source &&
                                  eventLogInstanceWrapper.Log == Log && eventLogInstanceWrapper.MachineName == MachineName;

            if (!isCacheUpToDate)
            {
                eventLogInstanceWrapper = EventLogStaticMethods.CreateEventLogInstanceWrapper(Log, MachineName, renderedSource);
            }

            return eventLogInstanceWrapper;
        }

        internal string RenderSource(LogEventInfo logEvent)
        {
            return Source != null ? RenderLogEvent(Source, logEvent) : null;
        }

        /// <summary>
        /// (re-)create an event source, if it isn't there. Works only with fixed sourcenames.
        /// </summary>
        /// <param name="fixedSource">sourcename. If source is not fixed (see <see cref="SimpleLayout.IsFixedText"/>, then pass <c>null</c> or emptystring.</param>
        /// <param name="alwaysThrowError">always throw an Exception when there is an error</param>
        private void CreateEventSourceIfNeeded(string fixedSource, bool alwaysThrowError)
        {
            if (string.IsNullOrEmpty(fixedSource))
            {
                InternalLogger.Debug("EventLogTarget(Name={0}): Skipping creation of event source because it contains layout renderers", Name);
                // we can only create event sources if the source is fixed (no layout)
                return;
            }

            // if we throw anywhere, we remain non-operational
            try
            {
                if (EventLogStaticMethods.SourceExists(fixedSource, MachineName))
                {
                    string currentLogName = EventLogStaticMethods.LogNameFromSourceName(fixedSource, MachineName);
                    if (!currentLogName.Equals(Log, StringComparison.CurrentCultureIgnoreCase))
                    {
                        // re-create the association between Log and Source
                        EventLogStaticMethods.DeleteEventSource(fixedSource, MachineName);

                        var eventSourceCreationData = new EventSourceCreationData(fixedSource, Log)
                        {
                            MachineName = MachineName
                        };
                        EventLogStaticMethods.CreateEventSource(eventSourceCreationData);
                    }
                }
                else
                {
                    var eventSourceCreationData = new EventSourceCreationData(fixedSource, Log)
                    {
                        MachineName = MachineName
                    };
                    EventLogStaticMethods.CreateEventSource(eventSourceCreationData);
                }

                if (MaxKilobytes.HasValue && GetEventLog(null).MaximumKilobytes < MaxKilobytes)
                {
                    GetEventLog(null).MaximumKilobytes = MaxKilobytes.Value;
                }
            }
            catch (Exception exception)
            {
                InternalLogger.Error(exception, "EventLogTarget(Name={0}): Error when connecting to EventLog.", Name);
                if (alwaysThrowError || LogManager.ThrowExceptions)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// A wrapper for Windows event log static methods.
        /// </summary>
        protected internal interface IEventLogStaticWrapper
        {
            /// <summary>
            /// Creates an event log instance wrapper, that implements <see cref="IEventLogInstanceWrapper"/> interface.
            /// </summary>
            /// <returns>A wrapped <see cref="IEventLogInstanceWrapper"/> instance.</returns>
            IEventLogInstanceWrapper CreateEventLogInstanceWrapper(string logName, string machineName, string source);

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
        }

        /// <summary>
        /// A wrapper for Windows event log instance methods and properties.
        /// </summary>
        protected internal interface IEventLogInstanceWrapper
        {
            /// <summary>
            /// A wrapper for the property <see cref="EventLog.Source"/>.
            /// </summary>
            string Source { get; }

            /// <summary>
            /// A wrapper for the property <see cref="EventLog.Log"/>.
            /// </summary>
            string Log { get; set; }

            /// <summary>
            /// A wrapper for the property <see cref="EventLog.MachineName"/>.
            /// </summary>
            string MachineName { get; set; }

            /// <summary>
            /// A wrapper for the property <see cref="EventLog.MaximumKilobytes"/>.
            /// </summary>
            long MaximumKilobytes { get; set; }

            /// <summary>
            /// A wrapper for the method <see cref="EventLog.WriteEntry(string, EventLogEntryType, int, short)"/>.
            /// </summary>
            void WriteEntry(string message, EventLogEntryType entryType, int eventId, short category);
        }

        /// <summary>
        /// The implementation of <see cref="IEventLogStaticWrapper"/>, that uses Windows event log through <see cref="EventLog"/> static methods.
        /// </summary>
        /// <remarks>
        /// <c>System.Lazy{T}</c> is not present in .net35, so using a nested constructor for the singleton implementation.
        /// </remarks>
        protected internal sealed class EventLogStaticWrapper : IEventLogStaticWrapper
        {
            #region Singleton implementation

            private EventLogStaticWrapper() { }

            /// <summary>
            /// Gets the singleton instance of <see cref="EventLogStaticWrapper"/>.
            /// </summary>
            public static EventLogStaticWrapper Instance => Nested.Instance;

            // ReSharper disable once ClassNeverInstantiated.Local
            private class Nested
            {
                // Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
                static Nested()
                {
                }

                internal static readonly EventLogStaticWrapper Instance = new EventLogStaticWrapper();
            }

            #endregion Singleton implementation

            /// <inheritdoc />
            /// <summary>
            /// Creates a Windows event log instance of type <see cref="EventLog"/> wrapped in the implementation of the <see cref="IEventLogInstanceWrapper"/> interface.
            /// </summary>
            public IEventLogInstanceWrapper CreateEventLogInstanceWrapper(string logName, string machineName, string source) =>
                new EventLogInstanceWrapper(new EventLog(logName, machineName, source));

            /// <inheritdoc />
            public void DeleteEventSource(string source, string machineName) => EventLog.DeleteEventSource(source, machineName);

            /// <inheritdoc />
            public bool SourceExists(string source, string machineName) => EventLog.SourceExists(source, machineName);

            /// <inheritdoc />
            public string LogNameFromSourceName(string source, string machineName) => EventLog.LogNameFromSourceName(source, machineName);

            /// <inheritdoc />
            public void CreateEventSource(EventSourceCreationData sourceData) => EventLog.CreateEventSource(sourceData);
        }

        /// <summary>
        /// The implementation of <see cref="IEventLogInstanceWrapper"/>, that uses Windows event log through an <see cref="EventLog"/> instance.
        /// </summary>
        protected internal class EventLogInstanceWrapper : IEventLogInstanceWrapper
        {
            private readonly EventLog _windowsEventLog;

            /// <summary>
            /// Initializes the <see cref="EventLogInstanceWrapper"/>.
            /// </summary>
            /// <param name="windowsEventLog"></param>
            protected internal EventLogInstanceWrapper(EventLog windowsEventLog) => _windowsEventLog = windowsEventLog;

            /// <inheritdoc />
            public string Source
            {
                get => _windowsEventLog.Source;
                set => _windowsEventLog.Source = value;
            }

            /// <inheritdoc />
            public string Log
            {
                get => _windowsEventLog.Log;
                set => _windowsEventLog.Log = value;
            }

            /// <inheritdoc />
            public string MachineName
            {
                get => _windowsEventLog.MachineName;
                set => _windowsEventLog.MachineName = value;
            }

            /// <inheritdoc />
            public long MaximumKilobytes
            {
                get => _windowsEventLog.MaximumKilobytes;
                set => _windowsEventLog.MaximumKilobytes = value;
            }

            /// <inheritdoc />
            public void WriteEntry(string message, EventLogEntryType entryType, int eventId, short category) =>
                _windowsEventLog.WriteEntry(message, entryType, eventId, category);
        }
    }
}

#endif

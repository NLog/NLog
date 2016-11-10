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

#if !SILVERLIGHT && !__IOS__ && !__ANDROID__

namespace NLog.Targets
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Security;
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
        private EventLog eventLogInstance;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogTarget"/> class.
        /// </summary>
        public EventLogTarget()
            : this(AppDomainWrapper.CurrentDomain)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogTarget"/> class.
        /// </summary>
        public EventLogTarget(IAppDomain appDomain)
        {
            this.Source = appDomain.FriendlyName;
            this.Log = "Application";
            this.MachineName = ".";
            this.MaxMessageLength = 16384;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogTarget"/> class.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        public EventLogTarget(string name) : this(AppDomainWrapper.CurrentDomain)
        {
            this.Name = name;
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
        public Layout EventId { get; set; }

        /// <summary>
        /// Gets or sets the layout that renders event Category.
        /// </summary>
        /// <docgen category='Event Log Options' order='10' />
        public Layout Category { get; set; }

        /// <summary>
        /// Optional entrytype. When not set, or when not convertable to <see cref="LogLevel"/> then determined by <see cref="NLog.LogLevel"/>
        /// </summary>
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

        private int maxMessageLength;
        /// <summary>
        /// Gets or sets the message length limit to write to the Event Log.
        /// </summary>
        /// <remarks><value>MaxMessageLength</value> cannot be zero or negative</remarks>
        [DefaultValue(16384)]
        public int MaxMessageLength
        {
            get { return this.maxMessageLength; }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("MaxMessageLength cannot be zero or negative.");

                this.maxMessageLength = value;
            }
        }

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
            var fixedSource = GetFixedSource();

            //always throw error to keep backwardscomp behavior.
            CreateEventSourceIfNeeded(fixedSource, true);
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
                InternalLogger.Debug("Skipping removing of event source because it contains layout renderers");
            }
            else
            {
                EventLog.DeleteEventSource(fixedSource, this.MachineName);
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
                return EventLog.SourceExists(fixedSource, this.MachineName);
            }
            InternalLogger.Debug("Unclear if event source exists because it contains layout renderers");
            return null; //unclear! 
        }

        /// <summary>
        /// Initializes the target.
        /// </summary>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            var fixedSource = GetFixedSource();

            if (string.IsNullOrEmpty(fixedSource))
            {
                InternalLogger.Debug("Skipping creation of event source because it contains layout renderers");
            }
            else
            {
                var currentSourceName = EventLog.LogNameFromSourceName(fixedSource, this.MachineName);
                if (!currentSourceName.Equals(this.Log, StringComparison.CurrentCultureIgnoreCase))
                {
                    this.CreateEventSourceIfNeeded(fixedSource, false);
                }
            }
        }

        /// <summary>
        /// Writes the specified logging event to the event log. 
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            string message = this.Layout.Render(logEvent);

            EventLogEntryType entryType = GetEntryType(logEvent);

            int eventId = 0;

            if (this.EventId != null)
            {
                eventId = Convert.ToInt32(this.EventId.Render(logEvent), CultureInfo.InvariantCulture);
            }

            short category = 0;

            if (this.Category != null)
            {
                category = Convert.ToInt16(this.Category.Render(logEvent), CultureInfo.InvariantCulture);
            }

            EventLog eventLog = GetEventLog(logEvent);

            // limitation of EventLog API
            if (message.Length > this.MaxMessageLength)
            {
                if (OnOverflow == EventLogTargetOverflowAction.Truncate)
                {
                    message = message.Substring(0, this.MaxMessageLength);
                    eventLog.WriteEntry(message, entryType, eventId, category);
                }
                else if (OnOverflow == EventLogTargetOverflowAction.Split)
                {
                    for (int offset = 0; offset < message.Length; offset += this.MaxMessageLength)
                    {
                        string chunk = message.Substring(offset, Math.Min(this.MaxMessageLength, (message.Length - offset)));
                        eventLog.WriteEntry(chunk, entryType, eventId, category);
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
                eventLog.WriteEntry(message, entryType, eventId, category);
            }
        }

        /// <summary>
        /// Get the entry type for logging the message.
        /// </summary>
        /// <param name="logEvent">The logging event - for rendering the <see cref="EntryType"/></param>
        /// <returns></returns>
        private EventLogEntryType GetEntryType(LogEventInfo logEvent)
        {
            if (this.EntryType != null)
            {
                //try parse, if fail,  determine auto

                var value = this.EntryType.Render(logEvent);

                EventLogEntryType eventLogEntryType;
                if (EnumHelpers.TryParse(value, true, out eventLogEntryType))
                {
                    return eventLogEntryType;
                }
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
            if (this.Source == null)
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
        private EventLog GetEventLog(LogEventInfo logEvent)
        {
            var renderedSource = this.Source != null ? this.Source.Render(logEvent) : null;
            var isCacheUpToDate = eventLogInstance != null && renderedSource == eventLogInstance.Source &&
                                   eventLogInstance.Log == this.Log && eventLogInstance.MachineName == this.MachineName;

            if (!isCacheUpToDate)
            {
                eventLogInstance = new EventLog(this.Log, this.MachineName, renderedSource);
            }
            return eventLogInstance;
        }

        /// <summary>
        /// (re-)create a event source, if it isn't there. Works only with fixed sourcenames.
        /// </summary>
        /// <param name="fixedSource">sourcenaam. If source is not fixed (see <see cref="SimpleLayout.IsFixedText"/>, then pass <c>null</c> or emptystring.</param>
        /// <param name="alwaysThrowError">always throw an Exception when there is an error</param>
        private void CreateEventSourceIfNeeded(string fixedSource, bool alwaysThrowError)
        {

            if (string.IsNullOrEmpty(fixedSource))
            {
                InternalLogger.Debug("Skipping creation of event source because it contains layout renderers");
                //we can only create event sources if the source is fixed (no layout)
                return;

            }

            // if we throw anywhere, we remain non-operational
            try
            {
                if (EventLog.SourceExists(fixedSource, this.MachineName))
                {
                    string currentLogName = EventLog.LogNameFromSourceName(fixedSource, this.MachineName);
                    if (!currentLogName.Equals(this.Log, StringComparison.CurrentCultureIgnoreCase))
                    {
                        // re-create the association between Log and Source
                        EventLog.DeleteEventSource(fixedSource, this.MachineName);
                        var eventSourceCreationData = new EventSourceCreationData(fixedSource, this.Log)
                        {
                            MachineName = this.MachineName
                        };

                        EventLog.CreateEventSource(eventSourceCreationData);
                    }
                }
                else
                {
                    var eventSourceCreationData = new EventSourceCreationData(fixedSource, this.Log)
                    {
                        MachineName = this.MachineName
                    };

                    EventLog.CreateEventSource(eventSourceCreationData);
                }
            }
            catch (Exception exception)
            {
                InternalLogger.Error(exception, "Error when connecting to EventLog.");
                if (alwaysThrowError || exception.MustBeRethrown())
                {
                    throw;
                }

            }
        }
    }
}

#endif
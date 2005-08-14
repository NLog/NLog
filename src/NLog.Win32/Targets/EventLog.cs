using System;
using System.Diagnostics;
using System.Text;

using NLog.Internal;
using NLog.Config;

namespace NLog.Win32.Targets
{
    /// <summary>
    /// Writes log message to the Event Log.
    /// </summary>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <xml src="examples/targets/EventLog/EventLogTarget.nlog" />
    /// <p>
    /// This assumes just one target and a single rule. More configuration
    /// options are described <a href="config.html">here</a>.
    /// </p>
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <cs src="examples/targets/EventLog/EventLogTarget.cs" />
    /// </example>
    /// <remarks>
    /// Currently there's no way to pass EventID or Category 
    /// event log parameters. This issue may be addressed
    /// in future releases.
    /// </remarks>
    [Target("EventLog")]
	public class EventLogTarget : Target
	{
        private string _machineName = ".";
        private string _sourceName;
        private string _logName = "Application";
        private bool _needEventLogSourceUpdate = true;
        private bool _operational = false;

        /// <summary>
        /// Creates a new instance of <see cref="EventLogTarget"/> and 
        /// </summary>
        public EventLogTarget()
        {
            _sourceName = AppDomain.CurrentDomain.FriendlyName;
        }

        /// <summary>
        /// Machine name on which Event Log service is running.
        /// </summary>
        [System.ComponentModel.DefaultValue(".")]
        public string MachineName
        {
            get { return _machineName; }
            set 
            {
                _machineName = value; 
                _needEventLogSourceUpdate = true;
            }
        }

        /// <summary>
        /// The value to be used as the event Source.
        /// </summary>
        /// <remarks>
        /// By default this is the friendly name of the current AppDomain.
        /// </remarks>
        public string Source
        {
            get { return _sourceName; }
            set 
            {
                _sourceName = value;
                _needEventLogSourceUpdate = true;
            }
        }

        /// <summary>
        /// Name of the Event Log to write to. This can be System, Application or 
        /// any user-defined name.
        /// </summary>
        [System.ComponentModel.DefaultValue("Application")]
        public string Log
        {
            get { return _logName; }
            set { _logName = value; }
        }

        private void UpdateEventLogSource()
        {
            if (!_needEventLogSourceUpdate)
                return;

            lock (this)
            {
                _operational = false;

                // if we throw anywhere, we remain non-operational

                try
                {
                    if (!_needEventLogSourceUpdate)
                        return;

                    if (EventLog.SourceExists(_sourceName, _machineName))
                    {
                        string currentLogName = EventLog.LogNameFromSourceName(_sourceName, _machineName);
                        if (currentLogName != _logName)
                        {
                            // re-create the association between Log and Source
                            EventLog.DeleteEventSource(_sourceName, _machineName);
                            EventLog.CreateEventSource(_sourceName, _logName, _machineName);
                        }
                        else
                        {
                            // ok, Source registered and associated with the correct Log
                        }
                    }
                    else
                    {
                        // source doesn't exist, register it.
                        EventLog.CreateEventSource(_sourceName, _logName, _machineName);
                    }
                    // mark the configuration as operational
                    _operational = true;
                }
                catch (Exception ex)
                {
                    InternalLogger.Error("Error when connecting to EventLog: {0}", ex);
                }
                finally
                {
                    _needEventLogSourceUpdate = false;
                }
            }
        }

        /// <summary>
        /// Writes the specified logging event to the event log. 
        /// </summary>
        /// <param name="ev">The logging event.</param>
        protected override void Append(LogEventInfo ev)
        {
            UpdateEventLogSource();
            if (!_operational)
                return;
            
            string message = CompiledLayout.GetFormattedMessage(ev);
            if (message.Length > 16384)
            {
                // limitation of EventLog API
                message = message.Substring(0, 16384);
            }

            EventLogEntryType entryType;

            if (ev.Level >= LogLevel.Error)
            {
                entryType = EventLogEntryType.Error;
            } 
            else if (ev.Level >= LogLevel.Warn)
            {
                entryType = EventLogEntryType.Warning;
            }
            else
            {
                entryType = EventLogEntryType.Information;
            }

            EventLog.WriteEntry(_sourceName, message, entryType);
        }
	}
}
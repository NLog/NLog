// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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

#if !NETCF

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
    [SupportedRuntime(OS=RuntimeOS.WindowsNT)]
    [NotSupportedRuntime(Framework=RuntimeFramework.DotNetCompactFramework)]
    public class EventLogTarget : Target
	{
        private string _machineName = ".";
        private string _sourceName;
        private string _logName = "Application";
        private bool _needEventLogSourceUpdate;
        private bool _operational;

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
        /// <param name="logEvent">The logging event.</param>
        protected internal override void Write(LogEventInfo logEvent)
        {
            UpdateEventLogSource();
            if (!_operational)
                return;
            
            string message = CompiledLayout.GetFormattedMessage(logEvent);
            if (message.Length > 16384)
            {
                // limitation of EventLog API
                message = message.Substring(0, 16384);
            }

            EventLogEntryType entryType;

            if (logEvent.Level >= LogLevel.Error)
            {
                entryType = EventLogEntryType.Error;
            } 
            else if (logEvent.Level >= LogLevel.Warn)
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

#endif
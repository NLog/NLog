// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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

namespace NLog.Targets
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
#if WCF_SUPPORTED
    using System.ServiceModel;
    using System.ServiceModel.Channels;
#endif
    using System.Threading;
#if SILVERLIGHT
    using System.Windows;
    using System.Windows.Threading;
#endif
    using NLog.Common;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Layouts;
    using NLog.LogReceiverService;

    /// <summary>
    /// Sends log messages to a NLog Receiver Service (using WCF or Web Services).
    /// </summary>
    /// <seealso href="http://nlog-project.org/wiki/LogReceiverService_target">Documentation on NLog Wiki</seealso>
    [Target("LogReceiverService")]
    public class LogReceiverWebServiceTarget : Target
    {
        private LogEventInfoBuffer buffer = new LogEventInfoBuffer(10000, false, 10000);
        private bool inCall;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogReceiverWebServiceTarget"/> class.
        /// </summary>
        public LogReceiverWebServiceTarget()
        {
            this.Parameters = new List<MethodCallParameter>();
        }

        /// <summary>
        /// Gets or sets the endpoint address.
        /// </summary>
        /// <value>The endpoint address.</value>
        /// <docgen category='Connection Options' order='10' />
        [RequiredParameter]
        public string EndpointAddress { get; set; }

#if WCF_SUPPORTED
        /// <summary>
        /// Gets or sets the name of the endpoint configuration in WCF configuration file.
        /// </summary>
        /// <value>The name of the endpoint configuration.</value>
        /// <docgen category='Connection Options' order='10' />
        public string EndpointConfigurationName { get; set; }

#if !SILVERLIGHT2
        /// <summary>
        /// Gets or sets a value indicating whether to use binary message encoding.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        public bool UseBinaryEncoding { get; set; }
#endif
#endif

        /// <summary>
        /// Gets or sets the client ID.
        /// </summary>
        /// <value>The client ID.</value>
        /// <docgen category='Payload Options' order='10' />
        public Layout ClientId { get; set; }

        /// <summary>
        /// Gets the list of parameters.
        /// </summary>
        /// <value>The parameters.</value>
        /// <docgen category='Payload Options' order='10' />
        [ArrayParameter(typeof(MethodCallParameter), "parameter")]
        public IList<MethodCallParameter> Parameters { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include per-event properties in the payload sent to the server.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        public bool IncludeEventProperties { get; set; }

        /// <summary>
        /// Called when log events are being sent (test hook).
        /// </summary>
        /// <param name="events">The events.</param>
        /// <param name="asyncContinuations">The async continuations.</param>
        /// <returns>True if events should be sent, false to stop processing them.</returns>
        protected internal virtual bool OnSend(NLogEvents events, IEnumerable<AsyncLogEventInfo> asyncContinuations)
        {
            return true;
        }

        /// <summary>
        /// Writes logging event to the log target. Must be overridden in inheriting
        /// classes.
        /// </summary>
        /// <param name="logEvent">Logging event to be written out.</param>
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            this.Write(new[] { logEvent });
        }

        /// <summary>
        /// Writes an array of logging events to the log target. By default it iterates on all
        /// events and passes them to "Append" method. Inheriting classes can use this method to
        /// optimize batch writes.
        /// </summary>
        /// <param name="logEvents">Logging events to be written out.</param>
        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            // if web service call is being processed, buffer new events and return
            // lock is being held here
            if (this.inCall)
            {
                foreach (var ev in logEvents)
                {
                    this.buffer.Append(ev);
                }

                return;
            }

            var networkLogEvents = this.TranslateLogEvents(logEvents);
            this.Send(networkLogEvents, logEvents);
        }

        private static int GetStringOrdinal(NLogEvents context, Dictionary<string, int> stringTable, string value)
        {
            int stringIndex;

            if (!stringTable.TryGetValue(value, out stringIndex))
            {
                stringIndex = context.Strings.Count;
                stringTable.Add(value, stringIndex);
                context.Strings.Add(value);
            }

            return stringIndex;
        }

        private NLogEvents TranslateLogEvents(AsyncLogEventInfo[] logEvents)
        {
            string clientID = string.Empty;
            if (this.ClientId != null)
            {
                clientID = this.ClientId.Render(logEvents[0].LogEvent);
            }

            var networkLogEvents = new NLogEvents
            {
                ClientName = clientID,
                LayoutNames = new StringCollection(),
                Strings = new StringCollection(),
                BaseTimeUtc = logEvents[0].LogEvent.TimeStamp.ToUniversalTime().Ticks
            };

            var stringTable = new Dictionary<string, int>();

            for (int i = 0; i < this.Parameters.Count; ++i)
            {
                networkLogEvents.LayoutNames.Add(this.Parameters[i].Name);
            }

            if (this.IncludeEventProperties)
            {
                for (int i = 0; i < logEvents.Length; ++i)
                {
                    var ev = logEvents[i].LogEvent;

                    // add all event-level property names in 'LayoutNames' collection.
                    foreach (var prop in ev.Properties)
                    {
                        string propName = prop.Key as string;
                        if (propName != null)
                        {
                            if (!networkLogEvents.LayoutNames.Contains(propName))
                            {
                                networkLogEvents.LayoutNames.Add(propName);
                            }
                        }
                    }
                }
            }

            networkLogEvents.Events = new NLogEvent[logEvents.Length];
            for (int i = 0; i < logEvents.Length; ++i)
            {
                networkLogEvents.Events[i] = this.TranslateEvent(logEvents[i].LogEvent, networkLogEvents, stringTable);
            }

            return networkLogEvents;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Client is disposed asynchronously.")]
        private void Send(NLogEvents events, IEnumerable<AsyncLogEventInfo> asyncContinuations)
        {
            if (!this.OnSend(events, asyncContinuations))
            {
                return;
            }

#if WCF_SUPPORTED
            WcfLogReceiverClient client;

            if (string.IsNullOrEmpty(this.EndpointConfigurationName))
            {
                // endpoint not specified - use BasicHttpBinding
                Binding binding;

#if !SILVERLIGHT2
                if (this.UseBinaryEncoding)
                {
                    binding = new CustomBinding(new BinaryMessageEncodingBindingElement(), new HttpTransportBindingElement());
                }
                else
#endif
                {
                    binding = new BasicHttpBinding();
                 }

                client = new WcfLogReceiverClient(binding, new EndpointAddress(this.EndpointAddress));
            }
            else
            {
                client = new WcfLogReceiverClient(this.EndpointConfigurationName, new EndpointAddress(this.EndpointAddress));
            }

            client.ProcessLogMessagesCompleted += (sender, e) =>
                {
                    // report error to the callers
                    foreach (var ev in asyncContinuations)
                    {
                        ev.Continuation(e.Error);
                    }

                    // send any buffered events
                    this.SendBufferedEvents();
                };

            this.inCall = true;
#if SILVERLIGHT
            if (!Deployment.Current.Dispatcher.CheckAccess())
            {
                Deployment.Current.Dispatcher.BeginInvoke(() => client.ProcessLogMessagesAsync(events));
            }
            else
            {
                client.ProcessLogMessagesAsync(events);
            }
#else
            client.ProcessLogMessagesAsync(events);
#endif
#else
            var client = new SoapLogReceiverClient(this.EndpointAddress);
            this.inCall = true;
            client.BeginProcessLogMessages(
                events,
                result =>
                    {
                        Exception exception = null;

                        try
                        {
                            client.EndProcessLogMessages(result);
                        }
                        catch (Exception ex)
                        {
                            if (ex.MustBeRethrown())
                            {
                                throw;
                            }

                            exception = ex;
                        }

                        // report error to the callers
                        foreach (var ev in asyncContinuations)
                        {
                            ev.Continuation(exception);
                        }

                        // send any buffered events
                        this.SendBufferedEvents();
                    },
                null);
#endif
        }

        private void SendBufferedEvents()
        {
            lock (this.SyncRoot)
            {
                // clear inCall flag
                AsyncLogEventInfo[] bufferedEvents = this.buffer.GetEventsAndClear();
                if (bufferedEvents.Length > 0)
                {
                    var networkLogEvents = this.TranslateLogEvents(bufferedEvents);
                    this.Send(networkLogEvents, bufferedEvents);
                }
                else
                {
                    // nothing in the buffer, clear in-call flag
                    this.inCall = false;
                }
            }
        }

        private NLogEvent TranslateEvent(LogEventInfo eventInfo, NLogEvents context, Dictionary<string, int> stringTable)
        {
            var nlogEvent = new NLogEvent();
            nlogEvent.Id = eventInfo.SequenceID;
            nlogEvent.MessageOrdinal = GetStringOrdinal(context, stringTable, eventInfo.FormattedMessage);
            nlogEvent.LevelOrdinal = eventInfo.Level.Ordinal;
            nlogEvent.LoggerOrdinal = GetStringOrdinal(context, stringTable, eventInfo.LoggerName);
            nlogEvent.TimeDelta = eventInfo.TimeStamp.ToUniversalTime().Ticks - context.BaseTimeUtc;

            for (int i = 0; i < this.Parameters.Count; ++i)
            {
                var param = this.Parameters[i];
                var value = param.Layout.Render(eventInfo);
                int stringIndex = GetStringOrdinal(context, stringTable, value);

                nlogEvent.ValueIndexes.Add(stringIndex);
            }

            // layout names beyond Parameters.Count are per-event property names.
            for (int i = this.Parameters.Count; i < context.LayoutNames.Count; ++i)
            {
                string value;
                object propertyValue;
                
                if (eventInfo.Properties.TryGetValue(context.LayoutNames[i], out propertyValue))
                {
                    value = Convert.ToString(propertyValue, CultureInfo.InvariantCulture);
                }
                else
                {
                    value = string.Empty;
                }

                int stringIndex = GetStringOrdinal(context, stringTable, value);
                nlogEvent.ValueIndexes.Add(stringIndex);
            }

            return nlogEvent;
        }
    }
}

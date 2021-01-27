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

namespace NLog.Targets
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using NLog.Common;
    using NLog.Config;
    using NLog.Layouts;
    using NLog.LogReceiverService;
    using LogEventInfoBuffer = Wcf.LogEventInfoBuffer;

    /// <summary>
    /// Sends log messages to a NLog Receiver Service (using WCF or Web Services).
    /// </summary>
    /// <seealso href="https://github.com/nlog/nlog/wiki/LogReceiverService-target">Documentation on NLog Wiki</seealso>
    [Target("LogReceiverService")]
    public class LogReceiverWebServiceTarget : Target
    {
        private readonly LogEventInfoBuffer buffer = new LogEventInfoBuffer(10000, false, 10000);
        private bool inCall;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogReceiverWebServiceTarget"/> class.
        /// </summary>
        public LogReceiverWebServiceTarget()
        {
            Parameters = new List<MethodCallParameter>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogReceiverWebServiceTarget"/> class.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        public LogReceiverWebServiceTarget(string name) : this()
        {
            Name = name;
        }

        /// <summary>
        /// Gets or sets the endpoint address.
        /// </summary>
        /// <value>The endpoint address.</value>
        /// <docgen category='Connection Options' order='10' />
        [RequiredParameter]
        public virtual string EndpointAddress { get; set; }

        /// <summary>
        /// Gets or sets the name of the endpoint configuration in WCF configuration file.
        /// </summary>
        /// <value>The name of the endpoint configuration.</value>
        /// <docgen category='Connection Options' order='10' />
        public string EndpointConfigurationName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use binary message encoding.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        public bool UseBinaryEncoding { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use a WCF service contract that is one way (fire and forget) or two way (request-reply)
        /// </summary>
        /// <docgen category='Connection Options' order='10' />
        public bool UseOneWayContract { get; set; }

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
            Write((IList<AsyncLogEventInfo>)new[] { logEvent });
        }

        /// <summary>
        /// Writes an array of logging events to the log target. By default it iterates on all
        /// events and passes them to "Append" method. Inheriting classes can use this method to
        /// optimize batch writes.
        /// </summary>
        /// <param name="logEvents">Logging events to be written out.</param>
        protected override void Write(IList<AsyncLogEventInfo> logEvents)
        {
            // if web service call is being processed, buffer new events and return
            // lock is being held here
            if (inCall)
            {
                for (int i = 0; i < logEvents.Count; ++i)
                {
                    PrecalculateVolatileLayouts(logEvents[i].LogEvent);
                    buffer.Append(logEvents[i]);
                }
                return;
            }

            // Make clone as the input IList will be reused on next call
            AsyncLogEventInfo[] logEventsArray = new AsyncLogEventInfo[logEvents.Count];
            logEvents.CopyTo(logEventsArray, 0);

            var networkLogEvents = TranslateLogEvents(logEvents);
            Send(networkLogEvents, logEvents, null);
        }

        /// <summary>
        /// Flush any pending log messages asynchronously (in case of asynchronous targets).
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        protected override void FlushAsync(AsyncContinuation asyncContinuation)
        {
            SendBufferedEvents(asyncContinuation);
        }

        /// <summary>
        /// Add value to the <see cref="NLogEvents.Strings"/>, returns ordinal in <see cref="NLogEvents.Strings"/>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="stringTable">lookup so only unique items will be added to <see cref="NLogEvents.Strings"/></param>
        /// <param name="value">value to add</param>
        /// <returns></returns>
        private static int AddValueAndGetStringOrdinal(NLogEvents context, Dictionary<string, int> stringTable, string value)
        {

            if (value == null || !stringTable.TryGetValue(value, out var stringIndex))
            {
                stringIndex = context.Strings.Count;
                if (value != null)
                {
                    //don't add null to the string table, that would crash
                    stringTable.Add(value, stringIndex);
                }
                context.Strings.Add(value);
            }

            return stringIndex;
        }

        private NLogEvents TranslateLogEvents(IList<AsyncLogEventInfo> logEvents)
        {
            if (logEvents.Count == 0 && !LogManager.ThrowExceptions)
            {
                InternalLogger.Error("{0}: LogEvents array is empty, sending empty event...", this);
                return new NLogEvents();
            }

            string clientID = string.Empty;
            if (ClientId != null)
            {
                clientID = ClientId.Render(logEvents[0].LogEvent);
            }

            var networkLogEvents = new NLogEvents
            {
                ClientName = clientID,
                LayoutNames = new StringCollection(),
                Strings = new StringCollection(),
                BaseTimeUtc = logEvents[0].LogEvent.TimeStamp.ToUniversalTime().Ticks
            };

            var stringTable = new Dictionary<string, int>();

            for (int i = 0; i < Parameters.Count; ++i)
            {
                networkLogEvents.LayoutNames.Add(Parameters[i].Name);
            }

            if (IncludeEventProperties)
            {
                AddEventProperties(logEvents, networkLogEvents);
            }

            networkLogEvents.Events = new NLogEvent[logEvents.Count];
            for (int i = 0; i < logEvents.Count; ++i)
            {
                AsyncLogEventInfo ev = logEvents[i];
                networkLogEvents.Events[i] = TranslateEvent(ev.LogEvent, networkLogEvents, stringTable);
            }

            return networkLogEvents;
        }

        private static void AddEventProperties(IList<AsyncLogEventInfo> logEvents, NLogEvents networkLogEvents)
        {
            for (int i = 0; i < logEvents.Count; ++i)
            {
                var ev = logEvents[i].LogEvent;

                if (ev.HasProperties)
                {
                    // add all event-level property names in 'LayoutNames' collection.
                    foreach (var prop in ev.Properties)
                    {
                        if (prop.Key is string propName && !networkLogEvents.LayoutNames.Contains(propName))
                        {
                            networkLogEvents.LayoutNames.Add(propName);
                        }
                    }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Client is disposed asynchronously.")]
        private void Send(NLogEvents events, IList<AsyncLogEventInfo> asyncContinuations, AsyncContinuation flushContinuations)
        {
            if (!OnSend(events, asyncContinuations))
            {
                if (flushContinuations != null)
                    flushContinuations(null);
                return;
            }

            var client = CreateLogReceiver();
            client.ProcessLogMessagesCompleted += (sender, e) =>
            {
                if (e.Error != null)
                    InternalLogger.Error(e.Error, "{0}: Error while sending", this);

                // report error to the callers
                for (int i = 0; i < asyncContinuations.Count; ++i)
                {
                    asyncContinuations[i].Continuation(e.Error);
                }

                if (flushContinuations != null)
                    flushContinuations(e.Error);

                // send any buffered events
                SendBufferedEvents(null);
            };

            inCall = true;
            client.ProcessLogMessagesAsync(events);
        }

        /// <summary>
        /// Creating a new instance of IWcfLogReceiverClient
        /// 
        /// Inheritors can override this method and provide their own 
        /// service configuration - binding and endpoint address
        /// </summary>
        /// <returns></returns>
        /// <remarks>virtual is used by endusers</remarks>
        protected virtual IWcfLogReceiverClient CreateLogReceiver()
        {
            WcfLogReceiverClient client;

            if (string.IsNullOrEmpty(EndpointConfigurationName))
            {
                // endpoint not specified - use BasicHttpBinding
                Binding binding;

                if (UseBinaryEncoding)
                {
                    binding = new CustomBinding(new BinaryMessageEncodingBindingElement(), new HttpTransportBindingElement());
                }
                else
                {
                    binding = new BasicHttpBinding();
                }

                client = new WcfLogReceiverClient(UseOneWayContract, binding, new EndpointAddress(EndpointAddress));
            }
            else
            {
                client = new WcfLogReceiverClient(UseOneWayContract, EndpointConfigurationName, new EndpointAddress(EndpointAddress));
            }

            client.ProcessLogMessagesCompleted += ClientOnProcessLogMessagesCompleted;

            return client;
        }

        private void ClientOnProcessLogMessagesCompleted(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs)
        {
            var client = sender as IWcfLogReceiverClient;
            if (client != null && client.State == CommunicationState.Opened)
            {
                try
                {
                    client.Close();
                }
                catch
                {
                    client.Abort();
                }
            }
        }

        private void SendBufferedEvents(AsyncContinuation flushContinuation)
        {
            try
            {
                lock (SyncRoot)
                {
                    // clear inCall flag
                    AsyncLogEventInfo[] bufferedEvents = buffer.GetEventsAndClear();
                    if (bufferedEvents.Length > 0)
                    {
                        var networkLogEvents = TranslateLogEvents(bufferedEvents);
                        Send(networkLogEvents, bufferedEvents, flushContinuation);
                    }
                    else
                    {
                        // nothing in the buffer, clear in-call flag
                        inCall = false;
                        if (flushContinuation != null)
                            flushContinuation(null);
                    }
                }
            }
            catch (Exception exception)
            {
                if (flushContinuation != null)
                {
                    InternalLogger.Error(exception, "{0}: Error in flush async", this);
                    if (LogManager.ThrowExceptions)
                        throw;

                    flushContinuation(exception);
                }
                else
                {
                    // Throwing exceptions here will crash the entire application (.NET 2.0 behavior)
                    InternalLogger.Error(exception, "{0}: Error in send async", this);
                }
            }
        }

        internal NLogEvent TranslateEvent(LogEventInfo eventInfo, NLogEvents context, Dictionary<string, int> stringTable)
        {
            var nlogEvent = new NLogEvent();
            nlogEvent.Id = eventInfo.SequenceID;
            nlogEvent.MessageOrdinal = AddValueAndGetStringOrdinal(context, stringTable, eventInfo.FormattedMessage);
            nlogEvent.LevelOrdinal = eventInfo.Level.Ordinal;
            nlogEvent.LoggerOrdinal = AddValueAndGetStringOrdinal(context, stringTable, eventInfo.LoggerName);
            nlogEvent.TimeDelta = eventInfo.TimeStamp.ToUniversalTime().Ticks - context.BaseTimeUtc;

            for (int i = 0; i < Parameters.Count; ++i)
            {
                var param = Parameters[i];
                var value = param.Layout.Render(eventInfo);
                int stringIndex = AddValueAndGetStringOrdinal(context, stringTable, value);

                nlogEvent.ValueIndexes.Add(stringIndex);
            }

            // layout names beyond Parameters.Count are per-event property names.
            for (int i = Parameters.Count; i < context.LayoutNames.Count; ++i)
            {
                string value;
                object propertyValue;

                if (eventInfo.HasProperties && eventInfo.Properties.TryGetValue(context.LayoutNames[i], out propertyValue))
                {
                    value = Convert.ToString(propertyValue, CultureInfo.InvariantCulture);
                }
                else
                {
                    value = string.Empty;
                }

                int stringIndex = AddValueAndGetStringOrdinal(context, stringTable, value);
                nlogEvent.ValueIndexes.Add(stringIndex);
            }

            if (eventInfo.Exception != null)
            {
                nlogEvent.ValueIndexes.Add(AddValueAndGetStringOrdinal(context, stringTable, eventInfo.Exception.ToString()));
            }

            return nlogEvent;
        }
    }
}
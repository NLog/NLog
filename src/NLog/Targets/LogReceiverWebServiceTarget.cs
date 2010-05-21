// 
// Copyright (c) 2004-2010 Jaroslaw Kowalski <jaak@jkowalski.net>
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
#if WCF_SUPPORTED
    using System.ServiceModel;
#endif

    using NLog.Config;
    using NLog.Internal;
    using NLog.LogReceiverService;

    /// <summary>
    /// Sends log messages to a NLog Receiver Service (using WCF or Web Services).
    /// </summary>
    [Target("LogReceiverService")]
    public sealed class LogReceiverWebServiceTarget : Target
    {
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
#endif

        /// <summary>
        /// Gets or sets the client ID.
        /// </summary>
        /// <value>The client ID.</value>
        /// <docgen category='Payload Options' order='10' />
        public string ClientID { get; set; }

        /// <summary>
        /// Gets the list of parameters.
        /// </summary>
        /// <value>The parameters.</value>
        /// <docgen category='Payload Options' order='10' />
        [ArrayParameter(typeof(MethodCallParameter), "parameter")]
        public IList<MethodCallParameter> Parameters { get; private set; }

        /// <summary>
        /// Writes logging event to the log target. Must be overridden in inheriting
        /// classes.
        /// </summary>
        /// <param name="logEvent">Logging event to be written out.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            throw new NotSupportedException("This target must be invoked asynchronously.");
        }

        /// <summary>
        /// Writes logging event to the log target. Must be overridden in inheriting
        /// classes.
        /// </summary>
        /// <param name="logEvent">Logging event to be written out.</param>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        protected override void Write(LogEventInfo logEvent, AsyncContinuation asyncContinuation)
        {
            this.Write(new[] { logEvent }, new[] { asyncContinuation });
        }

        /// <summary>
        /// Writes an array of logging events to the log target. By default it iterates on all
        /// events and passes them to "Append" method. Inheriting classes can use this method to
        /// optimize batch writes.
        /// </summary>
        /// <param name="logEvents">Logging events to be written out.</param>
        /// <param name="asyncContinuations">The asynchronous continuations.</param>
        protected override void Write(LogEventInfo[] logEvents, AsyncContinuation[] asyncContinuations)
        {
            var networkLogEvents = this.TranslateLogEvents(logEvents);

            this.Send(networkLogEvents, asyncContinuations);
        }

        private NLogEvents TranslateLogEvents(LogEventInfo[] logEvents)
        {
            var networkLogEvents = new NLogEvents
            {
                ClientName = this.ClientID,
                LayoutNames = new ListOfStrings(),
                LoggerNames = new ListOfStrings(),
                BaseTimeUtc = logEvents[0].TimeStamp.ToUniversalTime().Ticks
            };

            for (int i = 0; i < this.Parameters.Count; ++i)
            {
                networkLogEvents.LayoutNames.Add(this.Parameters[i].Name);
            }

            networkLogEvents.Events = new NLogEvent[logEvents.Length];
            for (int i = 0; i < logEvents.Length; ++i)
            {
                networkLogEvents.Events[i] = this.TranslateEvent(logEvents[i], networkLogEvents);
            }

            return networkLogEvents;
        }

        private void Send(NLogEvents events, AsyncContinuation[] asyncContinuations)
        {
#if WCF_SUPPORTED
            WcfLogReceiverClient client;

            if (string.IsNullOrEmpty(this.EndpointConfigurationName))
            {
                // endpoint not specified - use BasicHttpBinding
                var binding = new BasicHttpBinding();
                client = new WcfLogReceiverClient(binding, new EndpointAddress(this.EndpointAddress));
            }
            else
            {
                client = new WcfLogReceiverClient(this.EndpointConfigurationName, new EndpointAddress(this.EndpointAddress));
            }

            client.ProcessLogMessagesCompleted += (sender, e) =>
                {
                    // report error to the callers
                    foreach (var cont in asyncContinuations)
                    {
                        cont(e.Error);
                    }
                };

            client.ProcessLogMessagesAsync(events);
#endif
        }

        private NLogEvent TranslateEvent(LogEventInfo eventInfo, NLogEvents context)
        {
            var nlogEvent = new NLogEvent();
            nlogEvent.Id = eventInfo.SequenceID;
            nlogEvent.Values = new ListOfStrings();
            nlogEvent.LevelOrdinal = eventInfo.Level.Ordinal;
            int loggerOrdinal = context.LoggerNames.IndexOf(eventInfo.LoggerName);
            if (loggerOrdinal < 0)
            {
                loggerOrdinal = context.LoggerNames.Count;
                context.LoggerNames.Add(eventInfo.LoggerName);
            }

            nlogEvent.LoggerOrdinal = loggerOrdinal;
            nlogEvent.TimeDelta = eventInfo.TimeStamp.ToUniversalTime().Ticks - context.BaseTimeUtc;
            for (int i = 0; i < this.Parameters.Count; ++i)
            {
                var param = this.Parameters[i];
                nlogEvent.Values.Add(param.Layout.Render(eventInfo));
            }

            return nlogEvent;
        }
    }
}

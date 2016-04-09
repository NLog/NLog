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

#if WCF_SUPPORTED && !SILVERLIGHT

namespace NLog.LogReceiverService
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Base implementation of a log receiver server which forwards received logs through <see cref="LogManager"/> or a given <see cref="LogFactory"/>.
    /// </summary>
    public abstract class BaseLogReceiverForwardingService
    {
        private readonly LogFactory logFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseLogReceiverForwardingService"/> class.
        /// </summary>
        protected BaseLogReceiverForwardingService()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseLogReceiverForwardingService"/> class.
        /// </summary>
        /// <param name="logFactory">The log factory.</param>
        protected BaseLogReceiverForwardingService(LogFactory logFactory)
        {
            this.logFactory = logFactory;
        }

        /// <summary>
        /// Processes the log messages.
        /// </summary>
        /// <param name="events">The events to process.</param>
        public void ProcessLogMessages(NLogEvents events)
        {
            var baseTimeUtc = new DateTime(events.BaseTimeUtc, DateTimeKind.Utc);
            var logEvents = new LogEventInfo[events.Events.Length];

            // convert transport representation of log events into workable LogEventInfo[]
            for (int j = 0; j < events.Events.Length; ++j)
            {
                var ev = events.Events[j];
                LogLevel level = LogLevel.FromOrdinal(ev.LevelOrdinal);
                string loggerName = events.Strings[ev.LoggerOrdinal];

                var logEventInfo = new LogEventInfo();
                logEventInfo.Level = level;
                logEventInfo.LoggerName = loggerName;
                logEventInfo.TimeStamp = baseTimeUtc.AddTicks(ev.TimeDelta).ToLocalTime();
                logEventInfo.Message = events.Strings[ev.MessageOrdinal];
                logEventInfo.Properties.Add("ClientName", events.ClientName);
                for (int i = 0; i < events.LayoutNames.Count; ++i)
                {
                    logEventInfo.Properties.Add(events.LayoutNames[i], events.Strings[ev.ValueIndexes[i]]);
                }

                logEvents[j] = logEventInfo;
            }

            this.ProcessLogMessages(logEvents);
        }

        /// <summary>
        /// Processes the log messages.
        /// </summary>
        /// <param name="logEvents">The log events.</param>
        protected virtual void ProcessLogMessages(LogEventInfo[] logEvents)
        {
            ILogger logger = null;
            string lastLoggerName = string.Empty;

            foreach (var ev in logEvents)
            {
                if (ev.LoggerName != lastLoggerName)
                {
                    if (this.logFactory != null)
                    {
                        logger = this.logFactory.GetLogger(ev.LoggerName);
                    }
                    else
                    {
                        logger = LogManager.GetLogger(ev.LoggerName);
                    }

                    lastLoggerName = ev.LoggerName;
                }

                logger.Log(ev);
            }
        }
    }
}

#endif
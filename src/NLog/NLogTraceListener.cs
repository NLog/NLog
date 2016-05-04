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

#if !SILVERLIGHT && !__IOS__&& !__ANDROID__

namespace NLog
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Text;
    using System.Xml;
    using NLog.Internal;
    using NLog.Time;

    /// <summary>
    /// TraceListener which routes all messages through NLog.
    /// </summary>
    public class NLogTraceListener : TraceListener
    {
        private static readonly Assembly systemAssembly = typeof(Trace).Assembly;
        private LogFactory logFactory;
        private LogLevel defaultLogLevel = LogLevel.Debug;
        private bool attributesLoaded;
        private bool autoLoggerName;
        private LogLevel forceLogLevel;
        private bool disableFlush;

        /// <summary>
        /// Initializes a new instance of the <see cref="NLogTraceListener"/> class.
        /// </summary>
        public NLogTraceListener()
        {
        }

        /// <summary>
        /// Gets or sets the log factory to use when outputting messages (null - use LogManager).
        /// </summary>
        public LogFactory LogFactory
        {
            get
            {
                this.InitAttributes();
                return this.logFactory;
            }

            set
            {
                this.attributesLoaded = true;
                this.logFactory = value;
            }
        }

        /// <summary>
        /// Gets or sets the default log level.
        /// </summary>
        public LogLevel DefaultLogLevel
        {
            get
            {
                this.InitAttributes();
                return this.defaultLogLevel;
            }

            set
            {
                this.attributesLoaded = true;
                this.defaultLogLevel = value;
            }
        }

        /// <summary>
        /// Gets or sets the log which should be always used regardless of source level.
        /// </summary>
        public LogLevel ForceLogLevel
        {
            get
            {
                this.InitAttributes();
                return this.forceLogLevel;
            }

            set
            {
                this.attributesLoaded = true;
                this.forceLogLevel = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether flush calls from trace sources should be ignored.
        /// </summary>
        public bool DisableFlush
        {
            get
            {
                this.InitAttributes();
                return this.disableFlush;
            }

            set
            {
                this.attributesLoaded = true;
                this.disableFlush = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the trace listener is thread safe.
        /// </summary>
        /// <value></value>
        /// <returns>true if the trace listener is thread safe; otherwise, false. The default is false.</returns>
        public override bool IsThreadSafe
        {
            get { return true; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use auto logger name detected from the stack trace.
        /// </summary>
        public bool AutoLoggerName
        {
            get
            {
                this.InitAttributes();
                return this.autoLoggerName;
            }

            set
            {
                this.attributesLoaded = true;
                this.autoLoggerName = value;
            }
        }

        /// <summary>
        /// When overridden in a derived class, writes the specified message to the listener you create in the derived class.
        /// </summary>
        /// <param name="message">A message to write.</param>
        public override void Write(string message)
        {
            this.ProcessLogEventInfo(this.DefaultLogLevel, null, message, null, null, TraceEventType.Resume, null);
        }

        /// <summary>
        /// When overridden in a derived class, writes a message to the listener you create in the derived class, followed by a line terminator.
        /// </summary>
        /// <param name="message">A message to write.</param>
        public override void WriteLine(string message)
        {
            this.ProcessLogEventInfo(this.DefaultLogLevel, null, message, null, null, TraceEventType.Resume, null);
        }

        /// <summary>
        /// When overridden in a derived class, closes the output stream so it no longer receives tracing or debugging output.
        /// </summary>
        public override void Close()
        {
        }

        /// <summary>
        /// Emits an error message.
        /// </summary>
        /// <param name="message">A message to emit.</param>
        public override void Fail(string message)
        {
            this.ProcessLogEventInfo(LogLevel.Error, null, message, null, null, TraceEventType.Error, null);
        }

        /// <summary>
        /// Emits an error message and a detailed error message.
        /// </summary>
        /// <param name="message">A message to emit.</param>
        /// <param name="detailMessage">A detailed message to emit.</param>
        public override void Fail(string message, string detailMessage)
        {
            this.ProcessLogEventInfo(LogLevel.Error, null, message + " " + detailMessage, null, null, TraceEventType.Error, null);
        }

        /// <summary>
        /// Flushes the output buffer.
        /// </summary>
        public override void Flush()
        {
            if (!this.DisableFlush)
            {
                if (this.LogFactory != null)
                {
                    this.LogFactory.Flush();
                }
                else
                {
                    LogManager.Flush();
                }
            }
        }

        /// <summary>
        /// Writes trace information, a data object and event information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache"/> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType"/> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="data">The trace data to emit.</param>
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            this.TraceData(eventCache, source, eventType, id, new object[] { data });
        }

        /// <summary>
        /// Writes trace information, an array of data objects and event information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache"/> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType"/> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="data">An array of objects to emit as data.</param>
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < data.Length; ++i)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }

                sb.Append("{");
                sb.Append(i);
                sb.Append("}");
            }

            this.ProcessLogEventInfo(TranslateLogLevel(eventType), source, sb.ToString(), data, id, eventType, null);
        }

        /// <summary>
        /// Writes trace and event information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache"/> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType"/> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            this.ProcessLogEventInfo(TranslateLogLevel(eventType), source, string.Empty, null, id, eventType, null);
        }

        /// <summary>
        /// Writes trace information, a formatted array of objects and event information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache"/> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType"/> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="format">A format string that contains zero or more format items, which correspond to objects in the <paramref name="args"/> array.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            this.ProcessLogEventInfo(TranslateLogLevel(eventType), source, format, args, id, eventType, null);
        }

        /// <summary>
        /// Writes trace information, a message, and event information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache"/> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType"/> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="message">A message to write.</param>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            this.ProcessLogEventInfo(TranslateLogLevel(eventType), source, message, null, id, eventType, null);
        }

        /// <summary>
        /// Writes trace information, a message, a related activity identity and event information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache"/> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="message">A message to write.</param>
        /// <param name="relatedActivityId">A <see cref="T:System.Guid"/>  object identifying a related activity.</param>
        public override void TraceTransfer(TraceEventCache eventCache, string source, int id, string message, Guid relatedActivityId)
        {
            this.ProcessLogEventInfo(LogLevel.Debug, source, message, null, id, TraceEventType.Transfer, relatedActivityId);
        }

        /// <summary>
        /// Gets the custom attributes supported by the trace listener.
        /// </summary>
        /// <returns>
        /// A string array naming the custom attributes supported by the trace listener, or null if there are no custom attributes.
        /// </returns>
        protected override string[] GetSupportedAttributes()
        {
            return new[] { "defaultLogLevel", "autoLoggerName", "forceLogLevel", "disableFlush" };
        }

        /// <summary>
        /// Translates the event type to level from <see cref="TraceEventType"/>.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <returns>Translated log level.</returns>
        private static LogLevel TranslateLogLevel(TraceEventType eventType)
        {
            switch (eventType)
            {
                case TraceEventType.Verbose:
                    return LogLevel.Trace;

                case TraceEventType.Information:
                    return LogLevel.Info;

                case TraceEventType.Warning:
                    return LogLevel.Warn;

                case TraceEventType.Error:
                    return LogLevel.Error;

                case TraceEventType.Critical:
                    return LogLevel.Fatal;

                default:
                    return LogLevel.Debug;
            }
        }

        /// <summary>
        /// Process the log event
        /// <param name="logLevel">The log level.</param>
        /// <param name="loggerName">The name of the logger.</param>
        /// <param name="message">The log message.</param>
        /// <param name="arguments">The log parameters.</param>
        /// <param name="eventId">The event id.</param>
        /// <param name="eventType">The event type.</param>
        /// <param name="relatedActiviyId">The related activity id.</param>
        /// </summary>
        protected virtual void ProcessLogEventInfo(LogLevel logLevel, string loggerName, [Localizable(false)] string message, object[] arguments, int? eventId, TraceEventType? eventType, Guid? relatedActiviyId)
        {
            var ev = new LogEventInfo();

            ev.LoggerName = (loggerName ?? this.Name) ?? string.Empty;
            
            if (this.AutoLoggerName)
            {
                var stack = new StackTrace();
                int userFrameIndex = -1;
                MethodBase userMethod = null;

                for (int i = 0; i < stack.FrameCount; ++i)
                {
                    var frame = stack.GetFrame(i);
                    var method = frame.GetMethod();

                    if (method.DeclaringType == this.GetType())
                    {
                        // skip all methods of this type
                        continue;
                    }

                    if (method.DeclaringType.Assembly == systemAssembly)
                    {
                        // skip all methods from System.dll
                        continue;
                    }

                    userFrameIndex = i;
                    userMethod = method;
                    break;
                }

                if (userFrameIndex >= 0)
                {
                    ev.SetStackTrace(stack, userFrameIndex);
                    if (userMethod.DeclaringType != null)
                    {
                        ev.LoggerName = userMethod.DeclaringType.FullName;
                    }
                }
            }

            if (eventType.HasValue)
            {
                ev.Properties.Add("EventType", eventType.Value);
            }

            if (relatedActiviyId.HasValue)
            {
                ev.Properties.Add("RelatedActivityID", relatedActiviyId.Value);
            }

            ev.TimeStamp = TimeSource.Current.Time;
            ev.Message = message;
            ev.Parameters = arguments;
            ev.Level = this.forceLogLevel ?? logLevel;

            if (eventId.HasValue)
            {
                ev.Properties.Add("EventID", eventId.Value);
            }

            ILogger logger;
            if (this.LogFactory != null)
            {
                logger = this.LogFactory.GetLogger(ev.LoggerName);
            }
            else
            {
                logger = LogManager.GetLogger(ev.LoggerName);
            }

            logger.Log(ev);
        }

        private void InitAttributes()
        {
            if (!this.attributesLoaded)
            {
                this.attributesLoaded = true;
                foreach (DictionaryEntry de in this.Attributes)
                {
                    var key = (string)de.Key;
                    var value = (string)de.Value;

                    switch (key.ToUpperInvariant())
                    {
                        case "DEFAULTLOGLEVEL":
                            this.defaultLogLevel = LogLevel.FromString(value);
                            break;

                        case "FORCELOGLEVEL":
                            this.forceLogLevel = LogLevel.FromString(value);
                            break;

                        case "AUTOLOGGERNAME":
                            this.AutoLoggerName = XmlConvert.ToBoolean(value);
                            break;

                        case "DISABLEFLUSH":
                            this.disableFlush = Boolean.Parse(value);
                            break;
                    }
                }
            }
        }
    }
}

#endif
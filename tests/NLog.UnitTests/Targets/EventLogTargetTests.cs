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

using System.Collections.Generic;
using System.Threading;
using NLog.Layouts;

#if !SILVERLIGHT && !MONO

namespace NLog.UnitTests.Targets
{
    using System.Diagnostics;
    using NLog.Config;
    using NLog.Targets;
    using System;
    using System.Linq;
    using Xunit;

    public class EventLogTargetTests : NLogTestBase
    {
        [Fact]
        public void WriteEventLogEntryTrace()
        {
            WriteEventLogEntry2(LogLevel.Trace, EventLogEntryType.Information);
        }

        [Fact]
        public void WriteEventLogEntryDebug()
        {
            WriteEventLogEntry2(LogLevel.Debug, EventLogEntryType.Information);
        }

        [Fact]
        public void WriteEventLogEntryInfo()
        {
            WriteEventLogEntry2(LogLevel.Info, EventLogEntryType.Information);
        }
        [Fact]
        public void WriteEventLogEntryWarn()
        {
            WriteEventLogEntry2(LogLevel.Warn, EventLogEntryType.Warning);
        }

        [Fact]
        public void WriteEventLogEntryError()
        {
            WriteEventLogEntry2(LogLevel.Error, EventLogEntryType.Error);
        }
        [Fact]
        public void WriteEventLogEntryFatal()
        {
            WriteEventLogEntry2(LogLevel.Fatal, EventLogEntryType.Error);
        }


        [Fact]
        public void WriteEventLogEntryFatalCustomEntryType()
        {
            WriteEventLogEntry2(LogLevel.Warn, EventLogEntryType.SuccessAudit, new SimpleLayout("SuccessAudit"));
        }

        [Fact]
        public void WriteEventLogEntryFatalCustomEntryTyp_caps()
        {
            WriteEventLogEntry2(LogLevel.Warn, EventLogEntryType.SuccessAudit, new SimpleLayout("SUCCESSAUDIT"));
        }

        [Fact]
        public void WriteEventLogEntryFatalCustomEntryTyp_fallback()
        {
            WriteEventLogEntry2(LogLevel.Warn, EventLogEntryType.Warning, new SimpleLayout("falllback to auto determined"));
        }

        [Fact]
        public void WriteEventLogEntryFatalCustomEntryTyp_error()
        {
            WriteEventLogEntry2(LogLevel.Debug, EventLogEntryType.Error, new SimpleLayout("error"));
        }
        private static void WriteEventLogEntry2(LogLevel logLevel, EventLogEntryType eventLogEntryType, Layout entryType = null)
        {
            var target = new EventLogTarget();
            //The Log to write to is intentionally lower case!!
            target.Log = "application";
            // set the source explicitly to prevent random AppDomain name being used as the source name
            target.Source = "NLog.UnitTests";
            if (entryType != null)
            {
                //set only when not default
                target.EntryType = entryType;
            }
            SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);
            var logger = LogManager.GetLogger("WriteEventLogEntry");
            var el = new EventLog(target.Log);

            var entriesWritten = new List<EventLogEntry>();
            EntryWrittenEventHandler onEntryWritten = (s, e) => entriesWritten.Add(e.Entry);
            el.EnableRaisingEvents = true;
            el.EntryWritten += onEntryWritten;

            var testValue = Guid.NewGuid().ToString();
            try
            {
                logger.Log(logLevel, testValue);
                // introduce small delay to catch the event on another worker thread
                Thread.Sleep(200);
            }
            finally
            {
                el.EnableRaisingEvents = false;
                el.EntryWritten -= onEntryWritten;
            }

            //debug-> error
            EntryExists(entriesWritten, testValue, target.Source, eventLogEntryType);
        }

        private static void EntryExists(IEnumerable<EventLogEntry> entries, string expectedValue, string expectedSource, EventLogEntryType expectedEntryType)
        {
            var entryExists = entries
                .Any(entry => 
                    entry.Source == expectedSource && 
                    entry.EntryType == expectedEntryType && 
                    entry.Message.Contains(expectedValue));

            Assert.True(entryExists, string.Format(
                "Failed to find entry of type '{1}' from source '{0}' containing text '{2}' in the message", 
                expectedSource, expectedEntryType, expectedValue));
        }

    }
}

#endif
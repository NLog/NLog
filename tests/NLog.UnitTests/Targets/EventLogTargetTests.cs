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
        public void WriteEventLogEntry()
        {
            var target = new EventLogTarget();
            //The Log to write to is intentionally lower case!!
            target.Log = "application";  

            SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Debug);
            var logger = LogManager.GetLogger("WriteEventLogEntry");
            var el = new EventLog(target.Log);

            var latestEntryTime  = el.Entries.Cast<EventLogEntry>().Max(n => n.TimeWritten);


            var testValue = Guid.NewGuid();
            logger.Debug(testValue.ToString());
            
            var entryExists = (from entry in el.Entries.Cast<EventLogEntry>()
                                where entry.TimeWritten >= latestEntryTime
                                && entry.Message.Contains(testValue.ToString())
                                select entry).Any();

            Assert.True(entryExists);
        }
    }
}

#endif
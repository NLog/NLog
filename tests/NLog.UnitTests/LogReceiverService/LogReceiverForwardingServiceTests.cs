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

#if WCF_SUPPORTED && !SILVERLIGHT && !NET_CF

namespace NLog.UnitTests.LogReceiverService
{
    using System;
    using NUnit.Framework;

#if !NUNIT
    using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
    using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
    using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
    using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif
    using NLog.LogReceiverService;

    [TestFixture]
    public class LogReceiverForwardingServiceTests : NLogTestBase
    {
        [Test]
        public void ToLogEventInfoTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${message} ${event-context:foo} ${event-context:bar} ${event-context:baz}' />
                    <target name='debug2' type='Debug' layout='${message} ${event-context:foo} ${event-context:bar} ${event-context:baz}' />
                    <target name='debug3' type='Debug' layout='${message} ${event-context:foo} ${event-context:bar} ${event-context:baz}' />
                </targets>
                <rules>
                    <logger name='logger1' minlevel='Trace' writeTo='debug1' />
                    <logger name='logger2' minlevel='Trace' writeTo='debug2' />
                    <logger name='logger3' minlevel='Trace' writeTo='debug3' />
                </rules>
            </nlog>");

            var service = new LogReceiverForwardingService();
            var events = new NLogEvents
            {
                BaseTimeUtc = new DateTime(2010, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).Ticks,
                ClientName = "foo",
                LayoutNames = new StringCollection { "foo", "bar", "baz" },
                Strings = new StringCollection { "logger1", "logger2", "logger3", "zzz", "message1" },
                Events =
                    new[]
                    {
                        new NLogEvent
                        {
                            Id = 1,
                            LevelOrdinal = 2,
                            LoggerOrdinal = 0,
                            TimeDelta = 30000000,
                            MessageOrdinal = 4,
                            Values = "0|1|2"
                        },
                        new NLogEvent
                        {
                            Id = 2,
                            LevelOrdinal = 3,
                            LoggerOrdinal = 2,
                            MessageOrdinal = 4,
                            TimeDelta = 30050000,
                            Values = "0|1|3",
                        }
                    }
            };

            service.ProcessLogMessages(events);
            this.AssertDebugCounter("debug1", 1);
            this.AssertDebugCounter("debug2", 0);
            this.AssertDebugCounter("debug3", 1);
            this.AssertDebugLastMessage("debug1", "message1 logger1 logger2 logger3");
            this.AssertDebugLastMessage("debug3", "message1 logger1 logger2 zzz");
        }
    }
}

#endif
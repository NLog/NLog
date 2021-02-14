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

namespace NLog.Wcf.Tests
{
    using System;
    using NLog.LogReceiverService;
    using Xunit;

    public class LogReceiverForwardingServiceTests
    {
        [Fact]
        public void ToLogEventInfoTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog throwExceptions='true'>
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
            </nlog>").LogFactory;
            var debug1 = logFactory.Configuration.FindTargetByName<NLog.Targets.DebugTarget>("debug1");
            var debug2 = logFactory.Configuration.FindTargetByName<NLog.Targets.DebugTarget>("debug2");
            var debug3 = logFactory.Configuration.FindTargetByName<NLog.Targets.DebugTarget>("debug3");

            var service = new LogReceiverForwardingService(logFactory);
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

            Assert.Equal(1, debug1.Counter);
            Assert.Equal(0, debug2.Counter);
            Assert.Equal(1, debug3.Counter);
            Assert.Equal("message1 logger1 logger2 logger3", debug1.LastMessage);
            Assert.Equal("message1 logger1 logger2 zzz", debug3.LastMessage);

            logFactory.Shutdown();
        }
    }
}
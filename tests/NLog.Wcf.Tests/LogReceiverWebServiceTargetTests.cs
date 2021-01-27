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
    using System.Collections.Generic;
    using NLog.Common;
    using NLog.LogReceiverService;
    using NLog.Targets;
    using Xunit;
    using NLog.Targets.Wrappers;
    using System.Threading;

    public class LogReceiverWebServiceTargetTests
    {
        public LogReceiverWebServiceTargetTests()
        {
            LogManager.ThrowExceptions = true;
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("message1")]
        public void TranslateEventAndBack(string message)
        {
            // Arrange
            var service = new LogReceiverWebServiceTarget { IncludeEventProperties = true };

            var logEvent = new LogEventInfo(LogLevel.Debug, "logger1", message);

            var nLogEvents = new NLogEvents
            {
                Strings = new StringCollection(),
                LayoutNames = new StringCollection(),
                BaseTimeUtc = DateTime.UtcNow.Ticks,
                ClientName = "client1",
                Events = new NLogEvent[0]

            };
            var dict2 = new Dictionary<string, int>();

            // Act
            var translateEvent = service.TranslateEvent(logEvent, nLogEvents, dict2);
            var result = translateEvent.ToEventInfo(nLogEvents, "");

            // Assert
            Assert.Equal("logger1", result.LoggerName);
            Assert.Equal(message, result.Message);
        }

        [Fact]
        public void LogReceiverWebServiceTargetSingleEventTest()
        {
            var target = new MyLogReceiverWebServiceTarget();
            target.EndpointAddress = "http://notimportant:9999/";
            target.Parameters.Add(new MethodCallParameter("message", "${message}"));
            target.Parameters.Add(new MethodCallParameter("lvl", "${level}"));

            var logger = new LogFactory().Setup().LoadConfiguration(cfg =>
            {
                cfg.Configuration.AddRuleForAllLevels(target);
            }).GetLogger("loggerName");

            logger.Info("message text");

            var payload = target.LastPayload;
            Assert.Equal(2, payload.LayoutNames.Count);
            Assert.Equal("message", payload.LayoutNames[0]);
            Assert.Equal("lvl", payload.LayoutNames[1]);
            Assert.Equal(3, payload.Strings.Count);
            Assert.Single(payload.Events);
            Assert.Equal("message text", payload.Strings[payload.Events[0].ValueIndexes[0]]);
            Assert.Equal("Info", payload.Strings[payload.Events[0].ValueIndexes[1]]);
            Assert.Equal("loggerName", payload.Strings[payload.Events[0].LoggerOrdinal]);
        }

        [Fact]
        public void LogReceiverWebServiceTargetMultipleEventTest()
        {
            var target = new MyLogReceiverWebServiceTarget();
            target.EndpointAddress = "http://notimportant:9999/";
            target.Parameters.Add(new MethodCallParameter("message", "${message}"));
            target.Parameters.Add(new MethodCallParameter("lvl", "${level}"));

            new LogFactory().Setup().LoadConfiguration(cfg =>
            {
                cfg.Configuration.AddRuleForAllLevels(target);
            });

            var exceptions = new List<Exception>();

            var events = new[]
            {
                LogEventInfo.Create(LogLevel.Info, "logger1", "message1").WithContinuation(exceptions.Add),
                LogEventInfo.Create(LogLevel.Debug, "logger2", "message2").WithContinuation(exceptions.Add),
                LogEventInfo.Create(LogLevel.Fatal, "logger1", "message2").WithContinuation(exceptions.Add),
            };

            target.WriteAsyncLogEvents(events);

            // with multiple events, we should get string caching
            var payload = target.LastPayload;
            Assert.Equal(2, payload.LayoutNames.Count);
            Assert.Equal("message", payload.LayoutNames[0]);
            Assert.Equal("lvl", payload.LayoutNames[1]);

            // 7 strings instead of 9 since 'logger1' and 'message2' are being reused
            Assert.Equal(7, payload.Strings.Count);

            Assert.Equal(3, payload.Events.Length);
            Assert.Equal("message1", payload.Strings[payload.Events[0].ValueIndexes[0]]);
            Assert.Equal("message2", payload.Strings[payload.Events[1].ValueIndexes[0]]);
            Assert.Equal("message2", payload.Strings[payload.Events[2].ValueIndexes[0]]);

            Assert.Equal("Info", payload.Strings[payload.Events[0].ValueIndexes[1]]);
            Assert.Equal("Debug", payload.Strings[payload.Events[1].ValueIndexes[1]]);
            Assert.Equal("Fatal", payload.Strings[payload.Events[2].ValueIndexes[1]]);

            Assert.Equal("logger1", payload.Strings[payload.Events[0].LoggerOrdinal]);
            Assert.Equal("logger2", payload.Strings[payload.Events[1].LoggerOrdinal]);
            Assert.Equal("logger1", payload.Strings[payload.Events[2].LoggerOrdinal]);

            Assert.Equal(payload.Events[0].LoggerOrdinal, payload.Events[2].LoggerOrdinal);
        }

        [Fact]
        public void LogReceiverWebServiceTargetMultipleEventWithPerEventPropertiesTest()
        {
            var target = new MyLogReceiverWebServiceTarget();
            target.IncludeEventProperties = true;
            target.EndpointAddress = "http://notimportant:9999/";
            target.Parameters.Add(new MethodCallParameter("message", "${message}"));
            target.Parameters.Add(new MethodCallParameter("lvl", "${level}"));

            new LogFactory().Setup().LoadConfiguration(cfg =>
            {
                cfg.Configuration.AddRuleForAllLevels(target);
            });

            var exceptions = new List<Exception>();

            var events = new[]
            {
                LogEventInfo.Create(LogLevel.Info, "logger1", "message1").WithContinuation(exceptions.Add),
                LogEventInfo.Create(LogLevel.Debug, "logger2", "message2").WithContinuation(exceptions.Add),
                LogEventInfo.Create(LogLevel.Fatal, "logger1", "message2").WithContinuation(exceptions.Add),
            };

            events[0].LogEvent.Properties["prop1"] = "value1";
            events[1].LogEvent.Properties["prop1"] = "value2";
            events[2].LogEvent.Properties["prop1"] = "value3";
            events[0].LogEvent.Properties["prop2"] = "value2a";

            target.WriteAsyncLogEvents(events);

            // with multiple events, we should get string caching
            var payload = target.LastPayload;

            // 4 layout names - 2 from Parameters, 2 from unique properties in events
            Assert.Equal(4, payload.LayoutNames.Count);
            Assert.Equal("message", payload.LayoutNames[0]);
            Assert.Equal("lvl", payload.LayoutNames[1]);
            Assert.Equal("prop1", payload.LayoutNames[2]);
            Assert.Equal("prop2", payload.LayoutNames[3]);

            Assert.Equal(12, payload.Strings.Count);

            Assert.Equal(3, payload.Events.Length);
            Assert.Equal("message1", payload.Strings[payload.Events[0].ValueIndexes[0]]);
            Assert.Equal("message2", payload.Strings[payload.Events[1].ValueIndexes[0]]);
            Assert.Equal("message2", payload.Strings[payload.Events[2].ValueIndexes[0]]);

            Assert.Equal("Info", payload.Strings[payload.Events[0].ValueIndexes[1]]);
            Assert.Equal("Debug", payload.Strings[payload.Events[1].ValueIndexes[1]]);
            Assert.Equal("Fatal", payload.Strings[payload.Events[2].ValueIndexes[1]]);

            Assert.Equal("value1", payload.Strings[payload.Events[0].ValueIndexes[2]]);
            Assert.Equal("value2", payload.Strings[payload.Events[1].ValueIndexes[2]]);
            Assert.Equal("value3", payload.Strings[payload.Events[2].ValueIndexes[2]]);

            Assert.Equal("value2a", payload.Strings[payload.Events[0].ValueIndexes[3]]);
            Assert.Equal("", payload.Strings[payload.Events[1].ValueIndexes[3]]);
            Assert.Equal("", payload.Strings[payload.Events[2].ValueIndexes[3]]);

            Assert.Equal("logger1", payload.Strings[payload.Events[0].LoggerOrdinal]);
            Assert.Equal("logger2", payload.Strings[payload.Events[1].LoggerOrdinal]);
            Assert.Equal("logger1", payload.Strings[payload.Events[2].LoggerOrdinal]);

            Assert.Equal(payload.Events[0].LoggerOrdinal, payload.Events[2].LoggerOrdinal);
        }

        [Fact]
        public void NoEmptyEventLists()
        {
            var target = new MyLogReceiverWebServiceTarget();
            target.EndpointAddress = "http://notimportant:9999/";

            var logger = new LogFactory().Setup().LoadConfiguration(cfg =>
            {
                var asyncTarget = new AsyncTargetWrapper(target)
                {
                    Name = "NoEmptyEventLists_wrapper"
                };
                cfg.Configuration.AddRuleForAllLevels(asyncTarget);
            }).GetLogger("logger1");

            try
            {
                logger.Info("message1");
                Thread.Sleep(1000);
                Assert.Equal(1, target.SendCount);
            }
            finally
            {
                logger.Factory.Shutdown();
            }
        }

        public class MyLogReceiverWebServiceTarget : LogReceiverWebServiceTarget
        {
            public NLogEvents LastPayload;
            public int SendCount;

            public MyLogReceiverWebServiceTarget() : base()
            {
            }

            public MyLogReceiverWebServiceTarget(string name) : base(name)
            {
            }

            protected internal override bool OnSend(NLogEvents events, IEnumerable<AsyncLogEventInfo> asyncContinuations)
            {
                LastPayload = events;
                ++SendCount;

                foreach (var ac in asyncContinuations)
                {
                    ac.Continuation(null);
                }

                return false;
            }
        }
    }
}

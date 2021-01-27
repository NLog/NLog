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
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System;
    using System.IO;
    using Xunit;
    using System.Data;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Xml;
    using System.Xml.Serialization;
    using NLog.Config;
    using NLog.Layouts;
    using NLog.LogReceiverService;

    public class LogReceiverServiceTests
    {
        private const string logRecieverUrl = "http://localhost:8080/logrecievertest";

        public LogReceiverServiceTests()
        {
            LogManager.ThrowExceptions = true;
        }

        [Fact]
        public void ToLogEventInfoTest()
        {
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

            var converted = events.ToEventInfo();

            Assert.Equal(2, converted.Count);
            Assert.Equal("message1", converted[0].FormattedMessage);
            Assert.Equal("message1", converted[1].FormattedMessage);

            Assert.Equal(new DateTime(2010, 1, 1, 0, 0, 3, 0, DateTimeKind.Utc), converted[0].TimeStamp.ToUniversalTime());
            Assert.Equal(new DateTime(2010, 1, 1, 0, 0, 3, 5, DateTimeKind.Utc), converted[1].TimeStamp.ToUniversalTime());

            Assert.Equal("logger1", converted[0].LoggerName);
            Assert.Equal("logger3", converted[1].LoggerName);

            Assert.Equal(LogLevel.Info, converted[0].Level);
            Assert.Equal(LogLevel.Warn, converted[1].Level);

            Layout fooLayout = "${event-context:foo}";
            Layout barLayout = "${event-context:bar}";
            Layout bazLayout = "${event-context:baz}";

            Assert.Equal("logger1", fooLayout.Render(converted[0]));
            Assert.Equal("logger1", fooLayout.Render(converted[1]));

            Assert.Equal("logger2", barLayout.Render(converted[0]));
            Assert.Equal("logger2", barLayout.Render(converted[1]));

            Assert.Equal("logger3", bazLayout.Render(converted[0]));
            Assert.Equal("zzz", bazLayout.Render(converted[1]));
        }

        [Fact]
        public void NoLayoutsTest()
        {
            var events = new NLogEvents
            {
                BaseTimeUtc = new DateTime(2010, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).Ticks,
                ClientName = "foo",
                LayoutNames = new StringCollection(),
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
                            Values = null,
                        },
                        new NLogEvent
                        {
                            Id = 2,
                            LevelOrdinal = 3,
                            LoggerOrdinal = 2,
                            MessageOrdinal = 4,
                            TimeDelta = 30050000,
                            Values = null,
                        }
                    }
            };

            var converted = events.ToEventInfo();

            Assert.Equal(2, converted.Count);
            Assert.Equal("message1", converted[0].FormattedMessage);
            Assert.Equal("message1", converted[1].FormattedMessage);

            Assert.Equal(new DateTime(2010, 1, 1, 0, 0, 3, 0, DateTimeKind.Utc), converted[0].TimeStamp.ToUniversalTime());
            Assert.Equal(new DateTime(2010, 1, 1, 0, 0, 3, 5, DateTimeKind.Utc), converted[1].TimeStamp.ToUniversalTime());

            Assert.Equal("logger1", converted[0].LoggerName);
            Assert.Equal("logger3", converted[1].LoggerName);

            Assert.Equal(LogLevel.Info, converted[0].Level);
            Assert.Equal(LogLevel.Warn, converted[1].Level);
        }

        /// <summary>
        /// Ensures that serialization formats of DataContractSerializer and XmlSerializer are the same
        /// on the same <see cref="NLogEvents"/> object.
        /// </summary>
        [Fact]
        public void CompareSerializationFormats()
        {
            var events = new NLogEvents
            {
                BaseTimeUtc = DateTime.UtcNow.Ticks,
                ClientName = "foo",
                LayoutNames = new StringCollection { "foo", "bar", "baz" },
                Strings = new StringCollection { "logger1", "logger2", "logger3" },
                Events =
                    new[]
                    {
                        new NLogEvent
                        {
                            Id = 1,
                            LevelOrdinal = 2,
                            LoggerOrdinal = 0,
                            TimeDelta = 34,
                            Values = "1|2|3"
                        },
                        new NLogEvent
                        {
                            Id = 2,
                            LevelOrdinal = 3,
                            LoggerOrdinal = 2,
                            TimeDelta = 345,
                            Values = "1|2|3",
                        }
                    }
            };

            var serializer1 = new XmlSerializer(typeof(NLogEvents));
            var sw1 = new StringWriter();
            using (var writer1 = XmlWriter.Create(sw1, new XmlWriterSettings { Indent = true }))
            {
                var namespaces = new XmlSerializerNamespaces();
                namespaces.Add("i", "http://www.w3.org/2001/XMLSchema-instance");

                serializer1.Serialize(writer1, events, namespaces);
            }

            var serializer2 = new DataContractSerializer(typeof(NLogEvents));
            var sw2 = new StringWriter();
            using (var writer2 = XmlWriter.Create(sw2, new XmlWriterSettings { Indent = true }))
            {
                serializer2.WriteObject(writer2, events);
            }

            var xml1 = sw1.ToString();
            var xml2 = sw2.ToString();

            Assert.Equal(xml1, xml2);
        }

#if !NETSTANDARD
#if MONO
        [Fact(Skip="Not working under MONO - not sure if unit test is wrong, or the code")]
#else
        [Fact]
#endif
        public void RealTestLogReciever_two_way()
        {
            RealTestLogReciever(false, false);
        }

#if MONO
        [Fact(Skip="Not working under MONO - not sure if unit test is wrong, or the code")]
#else
        [Fact]
#endif
        public void RealTestLogReciever_one_way()
        {
            RealTestLogReciever(true, false);
        }

        [Fact(Skip = "unit test should listen to non-http for this")]
        public void RealTestLogReciever_two_way_binary()
        {
            RealTestLogReciever(false, true);
        }

        [Fact(Skip = "unit test should listen to non-http for this")]
        public void RealTestLogReciever_one_way_binary()
        {
            RealTestLogReciever(true, true);
        }

        private void RealTestLogReciever(bool useOneWayContract, bool binaryEncode)
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml($@"
          <nlog throwExceptions='true' autoLoadExtensions='true'>
                <targets>
                   <target type='LogReceiverService'
                          name='s1'
               
                          endpointAddress='{logRecieverUrl}'
                          useOneWayContract='{useOneWayContract.ToString().ToLower()}'
                          useBinaryEncoding='{binaryEncode.ToString().ToLower()}'
                  
                          includeEventProperties='false'>
                  <!--  <parameter name='key1' layout='testparam1'  type='String'/> -->
               </target>

                   
                </targets>
                <rules>
                    <logger name='logger1' minlevel='Trace' writeTo='s1' />
              
                </rules>
            </nlog>").LogFactory;

            ExecLogRecieverAndCheck(ExecLogging1, CheckReceived1, 2, logFactory);
        }

        /// <summary>
        /// Create WCF service, logs and listen to the events
        /// </summary>
        /// <param name="logFunc">function for logging the messages</param>
        /// <param name="logCheckFunc">function for checking the received messages</param>
        /// <param name="messageCount">message count for wait for listen and checking</param>
        private void ExecLogRecieverAndCheck(Action<Logger> logFunc, Action<List<NLogEvents>> logCheckFunc, int messageCount, LogFactory logFactory)
        {
            Uri baseAddress = new Uri(logRecieverUrl);

            // Create the ServiceHost.
            var countdownEvent = new CountdownEvent(messageCount);
            var logReceiverMock = new LogReceiverMock(countdownEvent);

            using (ServiceHost host = new ServiceHost(logReceiverMock, baseAddress))
            {
                var behaviour = host.Description.Behaviors.Find<ServiceBehaviorAttribute>();
                behaviour.InstanceContextMode = InstanceContextMode.Single;

                // Enable metadata publishing.
                ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                
                smb.HttpGetEnabled = true;
#if !MONO
                smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
#endif
                host.Description.Behaviors.Add(smb);

                // Open the ServiceHost to start listening for messages. Since
                // no endpoints are explicitly configured, the runtime will create
                // one endpoint per base address for each service contract implemented
                // by the service.
                host.Open();

                //wait for 2 events

              

                var logger1 = logFactory.GetLogger("logger1");
                logFunc(logger1);

                countdownEvent.Wait(20000);
               
                var received = logReceiverMock.ReceivedEvents;

                Assert.Equal(messageCount, received.Count);

                logCheckFunc(received);

                logFactory.Shutdown();

                // Close the ServiceHost.
                host.Close();
            }
        }

        private static void CheckReceived1(List<NLogEvents> received)
        {
            //in some case the messages aren't retrieved in the right order when invoked in the same sec.
            //more important is that both are retrieved with the correct info
            Assert.Equal(2, received.Count);

            var logmessages = new HashSet<string> { received[0].ToEventInfo().First().Message, received[1].ToEventInfo().First().Message };

            Assert.True(logmessages.Contains("test 1"), "message 1 is missing");
            Assert.True(logmessages.Contains("test 2"), "message 2 is missing");
        }

        private static void ExecLogging1(Logger logger)
        {
            logger.Info("test 1");

            //we wait 10 ms, because after a cold boot, the messages are arrived in the same moment and the order can change.
            Thread.Sleep(10);
            logger.Info(new InvalidConstraintException("boo"), "test 2");
        }

        public class LogReceiverMock : ILogReceiverServer, ILogReceiverOneWayServer
        {
            public CountdownEvent CountdownEvent { get; }

            /// <inheritdoc />
            public LogReceiverMock(CountdownEvent countdownEvent)
            {
                CountdownEvent = countdownEvent;
            }

            public List<NLogEvents> ReceivedEvents { get; } = new List<NLogEvents>();

            /// <summary>
            /// Processes the log messages.
            /// </summary>
            /// <param name="events">The events.</param>
            public void ProcessLogMessages(NLogEvents events)
            {
                if (CountdownEvent == null)
                {
                    throw new Exception("test not prepared well");
                }

                ReceivedEvents.Add(events);

                CountdownEvent.Signal();
            }
        }
#endif // !NETSTANDARD
    }
}

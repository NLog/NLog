//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.Targets.Network
{
    using System;
    using NLog.Layouts;
    using Xunit;

    public class GelfLayoutTests
    {
        private static readonly string HostName = ResolveHostname();

        public GelfLayoutTests()
        {
            LogManager.ThrowExceptions = true;
            LogManager.Setup().SetupExtensions(ext =>
            {
                ext.RegisterLayout<GelfLayout>();
            });
        }

        [Fact]
        public void CanRenderGelf()
        {
            var gelfLayout = new GelfLayout();

            var memTarget = new NLog.Targets.MemoryTarget() { Layout = gelfLayout };
            using (var logFactory = new LogFactory().Setup().LoadConfiguration(cfg => cfg.ForLogger().WriteTo(memTarget)).LogFactory)
            {
                var loggerName = "TestLogger";
                var dateTime = DateTime.Now;
                var message = "hello, gelf :)";
                var logLevel = LogLevel.Info;

                var logEvent = new LogEventInfo
                {
                    LoggerName = loggerName,
                    Level = logLevel,
                    Message = message,
                    TimeStamp = dateTime,
                };

                logFactory.GetLogger(loggerName).Log(logEvent);
                Assert.Single(memTarget.Logs);
                var renderedGelf = memTarget.Logs[0];

                var expectedDateTime = GelfLayout.ToUnixTimeStamp(dateTime);
                var expectedGelf = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{{"+ "\"version\":\"1.1\","
                        + "\"host\":\"{0}\","
                        + "\"short_message\":\"{1}\","
                        + "\"timestamp\":{2},"
                        + "\"level\":{3},"
                        + "\"_logLevel\":\"{4}\","
                        + "\"_logger\":\"{5}\""
                        + "}}",
                    HostName,
                    message,
                    expectedDateTime,
                    (int)GelfLayout.ToSyslogSeverity(logLevel),
                    logLevel,
                    loggerName);

                Assert.Equal(expectedGelf, renderedGelf);
            }
        }

        [Fact]
        public void CanRenderGelfFacility()
        {
            var gelfLayout = new GelfLayout();
            gelfLayout.GelfFields.Add(new TargetPropertyWithContext("_LoggerName", "${logger}"));

            var memTarget = new NLog.Targets.MemoryTarget() { Layout = gelfLayout };
            using (var logFactory = new LogFactory().Setup().LoadConfiguration(cfg => cfg.ForLogger().WriteTo(memTarget)).LogFactory)
            {
                var loggerName = "TestLogger";
                var facility = "TestFacility";
                var dateTime = DateTime.Now;
                var message = "hello, gelf :)";
                var logLevel = LogLevel.Info;

                gelfLayout.GelfFacility = facility;

                var logEvent = new LogEventInfo
                {
                    LoggerName = loggerName,
                    Level = logLevel,
                    Message = message,
                    TimeStamp = dateTime,
                };

                logFactory.GetLogger(loggerName).Log(logEvent);
                Assert.Single(memTarget.Logs);
                var renderedGelf = memTarget.Logs[0];

                var expectedDateTime = GelfLayout.ToUnixTimeStamp(dateTime);
                var expectedGelf = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{{"+ "\"version\":\"1.1\","
                        + "\"host\":\"{0}\","
                        + "\"short_message\":\"{1}\","
                        + "\"timestamp\":{2},"
                        + "\"level\":{3},"
                        + "\"facility\":\"{4}\","
                        + "\"file\":\"TestLogger\","
                        + "\"_LoggerName\":\"{5}\""
                        + "}}",
                    HostName,
                    message,
                    expectedDateTime,
                    (int)GelfLayout.ToSyslogSeverity(logLevel),
                    facility,
                    loggerName);

                Assert.Equal(expectedGelf, renderedGelf);
            }
        }

        [Fact]
        public void CanRenderGelfAdditionalFieldsCustomMessage()
        {
            var gelfLayout = new GelfLayout();
            gelfLayout.GelfFields.Add(new TargetPropertyWithContext("ThreadId", "${threadid}") {  PropertyType = typeof(int) });
            gelfLayout.GelfFullMessage = "${hostname}|${message}";
            gelfLayout.GelfShortMessage = "short|${message}";

            var memTarget = new NLog.Targets.MemoryTarget() { Layout = gelfLayout };
            using (var logFactory = new LogFactory().Setup().LoadConfiguration(cfg => cfg.ForLogger().WriteTo(memTarget).WithAsync()).LogFactory)
            {
                var loggerName = "TestLogger";
                var dateTime = DateTime.Now;
                var message = "hello, gelf :)";
                var logLevel = LogLevel.Info;

                var logEvent = new LogEventInfo
                {
                    LoggerName = loggerName,
                    Level = logLevel,
                    Message = message,
                    TimeStamp = dateTime,
                };

                logFactory.GetLogger(loggerName).Log(logEvent);
                logFactory.Flush();
                Assert.Single(memTarget.Logs);
                var renderedGelf = memTarget.Logs[0];

                int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
                var expectedDateTime = GelfLayout.ToUnixTimeStamp(dateTime);
                var expectedGelf = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{{" + "\"version\":\"1.1\","
                        + "\"host\":\"{0}\","
                        + "\"short_message\":\"short|{1}\","
                        + "\"full_message\":\"{0}|{1}\","
                        + "\"timestamp\":{2},"
                        + "\"level\":{3},"
                        + "\"_ThreadId\":{4}"
                        + "}}",
                    HostName,
                    message,
                    expectedDateTime,
                    (int)GelfLayout.ToSyslogSeverity(logLevel),
                    threadId);
                Assert.Equal(expectedGelf, renderedGelf);
            }
        }

        [Fact]
        public void CanRenderEventProperties()
        {
            var gelfLayout = new GelfLayout();
            gelfLayout.GelfFields.Add(new TargetPropertyWithContext(" ThreadId ", "${threadid}") { PropertyType = typeof(int) });
            gelfLayout.IncludeEventProperties = true;
            gelfLayout.IncludeProperties.Add("RequestId");

            var memTarget = new NLog.Targets.MemoryTarget() { Layout = gelfLayout };
            using (var logFactory = new LogFactory().Setup().LoadConfiguration(cfg => cfg.ForLogger().WriteTo(memTarget).WithAsync()).LogFactory)
            {
                var loggerName = "TestLogger";
                var dateTime = DateTime.Now;
                var message = "hello, {world} from {RequestId} :)";
                var logLevel = LogLevel.Info;
                var requestId = Guid.NewGuid();

                var logEvent = new LogEventInfo
                {
                    TimeStamp = dateTime,
                    LoggerName = loggerName,
                    Level = logLevel,
                    Message = message,
                    Parameters = new object[] { "Gelf", requestId },
                };

                logFactory.GetLogger(loggerName).Log(logEvent);
                logFactory.Flush();
                Assert.Single(memTarget.Logs);
                var renderedGelf = memTarget.Logs[0];

                int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
                var expectedDateTime = GelfLayout.ToUnixTimeStamp(dateTime);
                var expectedGelf = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{{" + "\"version\":\"1.1\","
                        + "\"host\":\"{0}\","
                        + "\"short_message\":\"hello, Gelf from {5} :)\","
                        + "\"timestamp\":{2},"
                        + "\"level\":{3},"
                        + "\"_ThreadId\":{4},"
                        + "\"_RequestId\":\"{5}\""
                        + "}}",
                    HostName,
                    message,
                    expectedDateTime,
                    (int)GelfLayout.ToSyslogSeverity(logLevel),
                    threadId,
                    requestId);
                Assert.Equal(expectedGelf, renderedGelf);
            }
        }

        [Fact]
        public void CanRenderScopeContext()
        {
            var gelfLayout = new GelfLayout();
            gelfLayout.GelfFields.Add(new TargetPropertyWithContext(" ThreadId ", "${threadid}") { PropertyType = typeof(int) });
            gelfLayout.IncludeEventProperties = true;
            gelfLayout.IncludeScopeProperties = true;
            gelfLayout.ExcludeProperties.Add("World");

            var memTarget = new NLog.Targets.MemoryTarget() { Layout = gelfLayout };
            using (var logFactory = new LogFactory().Setup().LoadConfiguration(cfg => cfg.ForLogger().WriteTo(memTarget).WithAsync()).LogFactory)
            {
                var loggerName = "TestLogger";
                var dateTime = DateTime.Now;
                var message = "hello, {world} :)";
                var logLevel = LogLevel.Info;

                var logEvent = new LogEventInfo
                {
                    TimeStamp = dateTime,
                    LoggerName = loggerName,
                    Level = logLevel,
                    Message = message,
                    Parameters = new object[] { "Gelf" },
                };

                var requestId = Guid.NewGuid();
                using (logFactory.GetLogger(loggerName).PushScopeProperty("RequestId", requestId))
                {
                    logFactory.GetLogger(loggerName).Log(logEvent);
                }
                logFactory.Flush();
                Assert.Single(memTarget.Logs);
                var renderedGelf = memTarget.Logs[0];

                int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
                var expectedDateTime = GelfLayout.ToUnixTimeStamp(dateTime);
                var expectedGelf = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{{" + "\"version\":\"1.1\","
                        + "\"host\":\"{0}\","
                        + "\"short_message\":\"hello, Gelf :)\","
                        + "\"timestamp\":{2},"
                        + "\"level\":{3},"
                        + "\"_ThreadId\":{4},"
                        + "\"_RequestId\":\"{5}\""
                        + "}}",
                    HostName,
                    message,
                    expectedDateTime,
                    (int)GelfLayout.ToSyslogSeverity(logLevel),
                    threadId,
                    requestId);
                Assert.Equal(expectedGelf, renderedGelf);
            }
        }

        static string ResolveHostname()
        {
            return Environment.GetEnvironmentVariable("HOSTNAME")
            ?? Environment.GetEnvironmentVariable("COMPUTERNAME")
            ?? Environment.GetEnvironmentVariable("MACHINENAME")
            ?? Environment.MachineName;
        }
    }
}

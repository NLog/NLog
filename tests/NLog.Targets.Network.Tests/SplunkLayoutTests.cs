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

    public class SplunkLayoutTests
    {
        private static readonly string HostName = ResolveHostname();

        public SplunkLayoutTests()
        {
            LogManager.ThrowExceptions = true;
            LogManager.Setup().SetupExtensions(ext =>
            {
                ext.RegisterLayout<SplunkLayout>();
            });
        }

        [Fact]
        public void CanRenderSplunk()
        {
            var splunkLayout = new SplunkLayout();

            var memTarget = new NLog.Targets.MemoryTarget() { Layout = splunkLayout };
            using (var logFactory = new LogFactory().Setup().LoadConfiguration(cfg => cfg.ForLogger().WriteTo(memTarget)).LogFactory)
            {
                var loggerName = "TestLogger";
                var dateTime = DateTime.Now;
                var message = "hello, splunk :)";
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
                var renderedSplunk = memTarget.Logs[0];

                var expectedDateTime = SplunkLayout.ToUnixTimeStamp(dateTime);
                var processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                var expectedSplunk = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{{" + "\"time\":{0},"
                        + "\"host\":\"{1}\","
                        + "\"source\":\"{2}\","
                        + "\"event\":{{"
                        + "\"level\":\"{3}\","
                        + "\"message\":\"{4}\","
                        + "\"logger\":\"{5}\""
                        + "}}}}",
                    expectedDateTime,
                    HostName,
                    processName,
                    logLevel,
                    message,
                    loggerName);

                Assert.Equal(expectedSplunk, renderedSplunk);
            }
        }

        [Fact]
        public void CanRenderSplunkSourceType()
        {
            var splunkLayout = new SplunkLayout();

            var memTarget = new NLog.Targets.MemoryTarget() { Layout = splunkLayout };
            using (var logFactory = new LogFactory().Setup().LoadConfiguration(cfg => cfg.ForLogger().WriteTo(memTarget)).LogFactory)
            {
                var loggerName = "TestLogger";
                var facility = "TestFacility";
                var dateTime = DateTime.Now;
                var message = "hello, splunk :)";
                var logLevel = LogLevel.Info;

                splunkLayout.SplunkSourceType = facility;

                var logEvent = new LogEventInfo
                {
                    LoggerName = loggerName,
                    Level = logLevel,
                    Message = message,
                    TimeStamp = dateTime,
                };

                logFactory.GetLogger(loggerName).Log(logEvent);
                Assert.Single(memTarget.Logs);
                var renderedSplunk = memTarget.Logs[0];

                var expectedDateTime = SplunkLayout.ToUnixTimeStamp(dateTime);
                var processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                var expectedSplunk = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{{" + "\"time\":{0},"
                        + "\"host\":\"{1}\","
                        + "\"source\":\"{2}\","
                        + "\"sourcetype\":\"{3}\","
                        + "\"event\":{{"
                        + "\"level\":\"{4}\","
                        + "\"message\":\"{5}\","
                        + "\"logger\":\"{6}\""
                        + "}}}}",
                    expectedDateTime,
                    HostName,
                    processName,
                    facility,
                    logLevel,
                    message,
                    loggerName);

                Assert.Equal(expectedSplunk, renderedSplunk);
            }
        }

        [Fact]
        public void CanRenderSplunkAdditionalEventCustomMessage()
        {
            var splunkLayout = new SplunkLayout();
            splunkLayout.SplunkFields.Add(new JsonAttribute("threadid", "${threadid}") { Encode = false });

            var memTarget = new NLog.Targets.MemoryTarget() { Layout = splunkLayout };
            using (var logFactory = new LogFactory().Setup().LoadConfiguration(cfg => cfg.ForLogger().WriteTo(memTarget).WithAsync()).LogFactory)
            {
                var loggerName = "TestLogger";
                var dateTime = DateTime.Now;
                var message = "hello, splunk :)";
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
                var renderedSplunk = memTarget.Logs[0];

                int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
                var expectedDateTime = SplunkLayout.ToUnixTimeStamp(dateTime);
                var processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                var expectedSplunk = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{{" + "\"time\":{0},"
                        + "\"host\":\"{1}\","
                        + "\"source\":\"{2}\","
                        + "\"event\":{{"
                        + "\"threadid\":{3}"
                        + "}}}}",
                    expectedDateTime,
                    HostName,
                    processName,
                    threadId);

                Assert.Equal(expectedSplunk, renderedSplunk);
            }
        }

        [Fact]
        public void CanRenderEventProperties()
        {
            var splunkLayout = new SplunkLayout();
            splunkLayout.SplunkFields.Add(new JsonAttribute("mt", "${message:raw=true}"));
            splunkLayout.IncludeEventProperties = true;

            var memTarget = new NLog.Targets.MemoryTarget() { Layout = splunkLayout };
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
                    Parameters = new object[] { "Splunk", requestId },
                };

                logFactory.GetLogger(loggerName).Log(logEvent);
                logFactory.Flush();
                Assert.Single(memTarget.Logs);
                var renderedSplunk = memTarget.Logs[0];

                var expectedDateTime = SplunkLayout.ToUnixTimeStamp(dateTime);
                var processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                var expectedSplunk = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{{" + "\"time\":{0},"
                        + "\"host\":\"{1}\","
                        + "\"source\":\"{2}\","
                        + "\"event\":{{"
                        + "\"mt\":\"{3}\","
                        + "\"world\":\"Splunk\","
                        + "\"RequestId\":\"{4}\""
                        + "}}}}",
                    expectedDateTime,
                    HostName,
                    processName,
                    message,
                    requestId);

                Assert.Equal(expectedSplunk, renderedSplunk);
            }
        }

        [Fact]
        public void CanRenderScopeContext()
        {
            var splunkLayout = new SplunkLayout();
            splunkLayout.SplunkFields.Add(new JsonAttribute("mt", "${message:raw=true}"));
            splunkLayout.IncludeEventProperties = true;
            splunkLayout.IncludeScopeProperties = true;
            splunkLayout.ExcludeProperties.Add("World");

            var memTarget = new NLog.Targets.MemoryTarget() { Layout = splunkLayout };
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
                    Parameters = new object[] { "Splunk" },
                };

                var requestId = Guid.NewGuid();
                using (logFactory.GetLogger(loggerName).PushScopeProperty("RequestId", requestId))
                {
                    logFactory.GetLogger(loggerName).Log(logEvent);
                }
                logFactory.Flush();
                Assert.Single(memTarget.Logs);
                var renderedSplunk = memTarget.Logs[0];

                var expectedDateTime = SplunkLayout.ToUnixTimeStamp(dateTime);
                var processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                var expectedSplunk = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{{" + "\"time\":{0},"
                        + "\"host\":\"{1}\","
                        + "\"source\":\"{2}\","
                        + "\"event\":{{"
                        + "\"mt\":\"{3}\","
                        + "\"RequestId\":\"{4}\""
                        + "}}}}",
                    expectedDateTime,
                    HostName,
                    processName,
                    message,
                    requestId);

                Assert.Equal(expectedSplunk, renderedSplunk);
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

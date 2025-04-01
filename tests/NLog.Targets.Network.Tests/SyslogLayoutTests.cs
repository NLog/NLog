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

    public class SyslogLayoutTests
    {
        private static string HostName = ResolveHostname();
        private static string ProcessName = ResolveProcessName();
        private static int ProcessId = ResolveProcessId();

        public SyslogLayoutTests()
        {
            LogManager.ThrowExceptions = true;
            LogManager.Setup().SetupExtensions(ext =>
            {
                ext.RegisterLayout<SyslogLayout>();
            });
        }

        [Fact]
        public void SyslogLayout_Rfc3164()
        {
            var memTarget = new NLog.Targets.MemoryTarget() { Layout = new SyslogLayout() };
            using (var logFactory = new LogFactory().Setup().LoadConfiguration(cfg => cfg.ForLogger().WriteTo(memTarget)).LogFactory)
            {
                var logger = logFactory.GetCurrentClassLogger();
                var logEvent = new LogEventInfo(LogLevel.Info, null, "Hello World");
                logger.Log(logEvent);

                var dateFormat = logEvent.TimeStamp.Day < 10 ? "{0:MMM  d HH:mm:ss}" : "{0:MMM dd HH:mm:ss}";
                var timestamp = string.Format(System.Globalization.CultureInfo.InvariantCulture, dateFormat, logEvent.TimeStamp);

                Assert.Single(memTarget.Logs);
                Assert.Equal($"<134>{timestamp} {HostName} {ProcessName}[{ProcessId}]: {logEvent.Message}", memTarget.Logs[0]);
            }
        }

        [Fact]
        public void SyslogLayout_Rfc3164_Newline()
        {
            var memTarget = new NLog.Targets.MemoryTarget() { Layout = new SyslogLayout() };
            using (var logFactory = new LogFactory().Setup().LoadConfiguration(cfg => cfg.ForLogger().WriteTo(memTarget)).LogFactory)
            {
                var logger = logFactory.GetCurrentClassLogger();
                var logEvent = new LogEventInfo(LogLevel.Info, null, "Hello World\r\nGoodbye World");
                logger.Log(logEvent);

                var dateFormat = logEvent.TimeStamp.Day < 10 ? "{0:MMM  d HH:mm:ss}" : "{0:MMM dd HH:mm:ss}";
                var timestamp = string.Format(System.Globalization.CultureInfo.InvariantCulture, dateFormat, logEvent.TimeStamp);

                Assert.Single(memTarget.Logs);
                Assert.Equal($"<134>{timestamp} {HostName} {ProcessName}[{ProcessId}]: Hello World Goodbye World", memTarget.Logs[0]);
            }
        }

        [Theory]
        [InlineData("Trace", "<143>")]
        [InlineData("Debug", "<143>")]
        [InlineData("Info", "<142>")]
        [InlineData("Warn", "<140>")]
        [InlineData("Error", "<139>")]
        [InlineData("Fatal", "<136>")]
        public void SyslogLayout_Rfc3164_LogLevels(string logLevel, string priValue)
        {
            var memTarget = new NLog.Targets.MemoryTarget() { Layout = new SyslogLayout() { SyslogFacility = SyslogFacility.Local1 } };
            using (var logFactory = new LogFactory().Setup().LoadConfiguration(cfg => cfg.ForLogger().WriteTo(memTarget)).LogFactory)
            {
                var logger = logFactory.GetCurrentClassLogger();
                var logEvent = new LogEventInfo(LogLevel.FromString(logLevel), null, "Hello World");
                logger.Log(logEvent);

                var dateFormat = logEvent.TimeStamp.Day < 10 ? "{0:MMM  d HH:mm:ss}" : "{0:MMM dd HH:mm:ss}";
                var timestamp = string.Format(System.Globalization.CultureInfo.InvariantCulture, dateFormat, logEvent.TimeStamp);

                Assert.Single(memTarget.Logs);
                Assert.Equal($"{priValue}{timestamp} {HostName} {ProcessName}[{ProcessId}]: {logEvent.Message}", memTarget.Logs[0]);
            }
        }

        [Fact]
        public void SyslogLayout_Rfc3164_EscapeNames()
        {
            var memTarget = new NLog.Targets.MemoryTarget() { Layout = new SyslogLayout() { SyslogHostName = " Hello World ", SyslogAppName = " Destroy World ", SyslogProcessId = " Goodbye World " } };
            using (var logFactory = new LogFactory().Setup().LoadConfiguration(cfg => cfg.ForLogger().WriteTo(memTarget)).LogFactory)
            {
                var logger = logFactory.GetCurrentClassLogger();
                var logEvent = new LogEventInfo(LogLevel.Info, null, "Hello World");
                logger.Log(logEvent);

                var dateFormat = logEvent.TimeStamp.Day < 10 ? "{0:MMM  d HH:mm:ss}" : "{0:MMM dd HH:mm:ss}";
                var timestamp = string.Format(System.Globalization.CultureInfo.InvariantCulture, dateFormat, logEvent.TimeStamp);

                Assert.Single(memTarget.Logs);
                Assert.Equal($"<134>{timestamp} Hello_World Destroy_World[Goodbye_World]: {logEvent.Message}", memTarget.Logs[0]);
            }
        }

        [Fact]
        public void SyslogLayout_Rfc5424()
        {
            var memTarget = new NLog.Targets.MemoryTarget() { Layout = new SyslogLayout() { Rfc5424 = true } };
            using (var logFactory = new LogFactory().Setup().LoadConfiguration(cfg => cfg.ForLogger().WriteTo(memTarget)).LogFactory)
            {
                var logger = logFactory.GetCurrentClassLogger();
                var logEvent = LogEventInfo.Create(LogLevel.Info, null, "Hello World");
                logger.Log(logEvent);

                Assert.Single(memTarget.Logs);
                Assert.Equal($"<134>1 {logEvent.TimeStamp:o} {HostName} {ProcessName} {ProcessId} - {logEvent.Message}", memTarget.Logs[0]);
            }
        }

        [Fact]
        public void SyslogLayout_Rfc5424_Newline()
        {
            var memTarget = new NLog.Targets.MemoryTarget() { Layout = new SyslogLayout() { Rfc5424 = true } };
            using (var logFactory = new LogFactory().Setup().LoadConfiguration(cfg => cfg.ForLogger().WriteTo(memTarget)).LogFactory)
            {
                var logger = logFactory.GetCurrentClassLogger();
                var logEvent = LogEventInfo.Create(LogLevel.Info, null, "Hello World\r\nGoodbye World");
                logger.Log(logEvent);

                Assert.Single(memTarget.Logs);
                Assert.Equal($"<134>1 {logEvent.TimeStamp:o} {HostName} {ProcessName} {ProcessId} - Hello World\r\nGoodbye World", memTarget.Logs[0]);
            }
        }

        [Theory]
        [InlineData("Trace", "<143>")]
        [InlineData("Debug", "<143>")]
        [InlineData("Info", "<142>")]
        [InlineData("Warn", "<140>")]
        [InlineData("Error", "<139>")]
        [InlineData("Fatal", "<136>")]
        public void SyslogLayout_Rfc5424_LogLevels(string logLevel, string priValue)
        {
            var memTarget = new NLog.Targets.MemoryTarget() { Layout = new SyslogLayout() { Rfc5424 = true, SyslogFacility = SyslogFacility.Local1 } };
            using (var logFactory = new LogFactory().Setup().LoadConfiguration(cfg => cfg.ForLogger().WriteTo(memTarget)).LogFactory)
            {
                var logger = logFactory.GetCurrentClassLogger();
                var logEvent = LogEventInfo.Create(LogLevel.FromString(logLevel), null, "Hello World");
                logger.Log(logEvent);

                Assert.Single(memTarget.Logs);
                Assert.Equal($"{priValue}1 {logEvent.TimeStamp:o} {HostName} {ProcessName} {ProcessId} - {logEvent.Message}", memTarget.Logs[0]);
            }
        }

        [Fact]
        public void SyslogLayout_Rfc5424_EscapeNames()
        {
            var memTarget = new NLog.Targets.MemoryTarget() { Layout = new SyslogLayout() { Rfc5424 = true } };
            using (var logFactory = new LogFactory().Setup().LoadConfiguration(cfg => cfg.ForLogger().WriteTo(memTarget)).LogFactory)
            {
                var logger = logFactory.GetCurrentClassLogger();
                var logEvent = LogEventInfo.Create(LogLevel.Info, null, "Hello World");
                logger.Log(logEvent);

                Assert.Single(memTarget.Logs);
                Assert.Equal($"<134>1 {logEvent.TimeStamp:o} {HostName} {ProcessName} {ProcessId} - {logEvent.Message}", memTarget.Logs[0]);
            }
        }

        [Fact]
        public void SyslogLayout_Rfc5424_StructuredData()
        {
            var syslogLayout = new SyslogLayout() { Rfc5424 = true };
            syslogLayout.StructuredDataParams.Add(new TargetPropertyWithContext(" Hello World ", " Goodbye World "));
            syslogLayout.IncludeEventProperties = true;

            var memTarget = new NLog.Targets.MemoryTarget() { Layout = syslogLayout };
            using (var logFactory = new LogFactory().Setup().LoadConfiguration(cfg => cfg.ForLogger().WriteTo(memTarget)).LogFactory)
            {
                var guid = Guid.NewGuid();

                var logger = logFactory.GetCurrentClassLogger();
                var logEvent = LogEventInfo.Create(LogLevel.Info, null, null, "Hello {World} with {CorrelationKey}", new object[] { "\nEarth\n", guid });
                logger.Log(logEvent);

                Assert.Single(memTarget.Logs);
                Assert.Equal($"<134>1 {logEvent.TimeStamp:o} {HostName} {ProcessName} {ProcessId} - [meta World=\" Earth \" CorrelationKey=\"{guid}\" Hello_World=\" Goodbye World \"] {logEvent.FormattedMessage}", memTarget.Logs[0]);
            }
        }

        static string ResolveHostname()
        {
            return Environment.GetEnvironmentVariable("HOSTNAME")
            ?? Environment.GetEnvironmentVariable("COMPUTERNAME")
            ?? Environment.GetEnvironmentVariable("MACHINENAME")
            ?? Environment.MachineName;
        }

        static string ResolveProcessName() => System.Diagnostics.Process.GetCurrentProcess().ProcessName;

        static int ResolveProcessId() => System.Diagnostics.Process.GetCurrentProcess().Id;
    }
}

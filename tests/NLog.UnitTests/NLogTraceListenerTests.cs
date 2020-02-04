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

#define DEBUG

using NLog.Config;

namespace NLog.UnitTests
{
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Diagnostics;
    using Xunit;

    public class NLogTraceListenerTests : NLogTestBase, IDisposable
    {
        private readonly CultureInfo previousCultureInfo;

        public NLogTraceListenerTests()
        {
            previousCultureInfo = Thread.CurrentThread.CurrentCulture;
            // set the culture info with the decimal separator (comma) different from InvariantCulture separator (point)
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
        }
        
        public void Dispose()
        {
            // restore previous culture info
            Thread.CurrentThread.CurrentCulture = previousCultureInfo;
        }

#if !NETSTANDARD1_5
        [Fact]
        public void TraceWriteTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${logger} ${level} ${message}' /></targets>
                    <rules>
                        <logger name='*' minlevel='Debug' writeTo='debug' />
                    </rules>
                </nlog>");

            Trace.Listeners.Clear();
            Trace.Listeners.Add(new NLogTraceListener { Name = "Logger1" });

            Trace.Write("Hello");
            AssertDebugLastMessage("debug", "Logger1 Debug Hello");

            Trace.Write("Hello", "Cat1");
            AssertDebugLastMessage("debug", "Logger1 Debug Cat1: Hello");

            Trace.Write(3.1415);
            AssertDebugLastMessage("debug", $"Logger1 Debug {3.1415}");

            Trace.Write(3.1415, "Cat2");
            AssertDebugLastMessage("debug", $"Logger1 Debug Cat2: {3.1415}");
        }

        [Fact]
        public void TraceWriteLineTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${logger} ${level} ${message}' /></targets>
                    <rules>
                        <logger name='*' minlevel='Debug' writeTo='debug' />
                    </rules>
                </nlog>");

            Trace.Listeners.Clear();
            Trace.Listeners.Add(new NLogTraceListener { Name = "Logger1" });

            Trace.WriteLine("Hello");
            AssertDebugLastMessage("debug", "Logger1 Debug Hello");

            Trace.WriteLine("Hello", "Cat1");
            AssertDebugLastMessage("debug", "Logger1 Debug Cat1: Hello");

            Trace.WriteLine(3.1415);
            AssertDebugLastMessage("debug", $"Logger1 Debug {3.1415}");

            Trace.WriteLine(3.1415, "Cat2");
            AssertDebugLastMessage("debug", $"Logger1 Debug Cat2: {3.1415}");
        }

        [Fact]
        public void TraceWriteNonDefaultLevelTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${logger} ${level} ${message}' /></targets>
                    <rules>
                        <logger name='*' minlevel='Trace' writeTo='debug' />
                    </rules>
                </nlog>");

            Trace.Listeners.Clear();
            Trace.Listeners.Add(new NLogTraceListener { Name = "Logger1", DefaultLogLevel = LogLevel.Trace });

            Trace.Write("Hello");
            AssertDebugLastMessage("debug", "Logger1 Trace Hello");
        }

        [Fact]
        public void TraceConfiguration()
        {
            var listener = new NLogTraceListener();
            listener.Attributes.Add("defaultLogLevel", "Warn");
            listener.Attributes.Add("forceLogLevel", "Error");
            listener.Attributes.Add("autoLoggerName", "1");
            listener.Attributes.Add("DISABLEFLUSH", "true"); 

            Assert.Equal(LogLevel.Warn, listener.DefaultLogLevel);
            Assert.Equal(LogLevel.Error, listener.ForceLogLevel);
            Assert.True(listener.AutoLoggerName);
            Assert.True(listener.DisableFlush);
        }

        [Fact]
        public void TraceFailTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${logger} ${level} ${message}' /></targets>
                    <rules>
                        <logger name='*' minlevel='Debug' writeTo='debug' />
                    </rules>
                </nlog>");

            Trace.Listeners.Clear();
            Trace.Listeners.Add(new NLogTraceListener { Name = "Logger1" });

            Trace.Fail("Message");
            AssertDebugLastMessage("debug", "Logger1 Error Message");

            Trace.Fail("Message", "Detailed Message");
            AssertDebugLastMessage("debug", "Logger1 Error Message Detailed Message");
        }

        [Fact]
        public void AutoLoggerNameTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${logger} ${level} ${message}' /></targets>
                    <rules>
                        <logger name='*' minlevel='Debug' writeTo='debug' />
                    </rules>
                </nlog>");

            Trace.Listeners.Clear();
            Trace.Listeners.Add(new NLogTraceListener { Name = "Logger1", AutoLoggerName = true });

            Trace.Write("Hello");
            AssertDebugLastMessage("debug", GetType().FullName + " Debug Hello");
        }

        [Fact]
        public void TraceDataTests()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${logger} ${level} ${message} ${event-context:EventID}' /></targets>
                    <rules>
                        <logger name='*' minlevel='Trace' writeTo='debug' />
                    </rules>
                </nlog>");

            TraceSource ts = CreateTraceSource();
            ts.Listeners.Add(new NLogTraceListener { Name = "Logger1", DefaultLogLevel = LogLevel.Trace });

            ts.TraceData(TraceEventType.Critical, 123, 42);
            AssertDebugLastMessage("debug", "MySource1 Fatal 42 123");

            ts.TraceData(TraceEventType.Critical, 145, 42, 3.14, "foo");
            AssertDebugLastMessage("debug", $"MySource1 Fatal 42, {3.14.ToString(CultureInfo.CurrentCulture)}, foo 145");
        }

#if MONO
        [Fact(Skip="Not working under MONO - not sure if unit test is wrong, or the code")]
#else
        [Fact]
#endif
        public void LogInformationTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${logger} ${level} ${message} ${event-context:EventID}' /></targets>
                    <rules>
                        <logger name='*' minlevel='Trace' writeTo='debug' />
                    </rules>
                </nlog>");

            TraceSource ts = CreateTraceSource();
            ts.Listeners.Add(new NLogTraceListener { Name = "Logger1", DefaultLogLevel = LogLevel.Trace });
            
            ts.TraceInformation("Quick brown fox");
            AssertDebugLastMessage("debug", "MySource1 Info Quick brown fox 0");

            ts.TraceInformation("Mary had {0} lamb", "a little");
            AssertDebugLastMessage("debug", "MySource1 Info Mary had a little lamb 0");
        }

        [Fact]
        public void TraceEventTests()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${logger} ${level} ${message} ${event-context:EventID}' /></targets>
                    <rules>
                        <logger name='*' minlevel='Trace' writeTo='debug' />
                    </rules>
                </nlog>");

            TraceSource ts = CreateTraceSource();
            ts.Listeners.Add(new NLogTraceListener { Name = "Logger1", DefaultLogLevel = LogLevel.Trace });

            ts.TraceEvent(TraceEventType.Information, 123, "Quick brown {0} jumps over the lazy {1}.", "fox", "dog");
            AssertDebugLastMessage("debug", "MySource1 Info Quick brown fox jumps over the lazy dog. 123");

            ts.TraceEvent(TraceEventType.Information, 123);
            AssertDebugLastMessage("debug", "MySource1 Info  123");

            ts.TraceEvent(TraceEventType.Verbose, 145, "Bar");
            AssertDebugLastMessage("debug", "MySource1 Trace Bar 145");

            ts.TraceEvent(TraceEventType.Error, 145, "Foo");
            AssertDebugLastMessage("debug", "MySource1 Error Foo 145");

            ts.TraceEvent(TraceEventType.Suspend, 145, "Bar");
            AssertDebugLastMessage("debug", "MySource1 Debug Bar 145");

            ts.TraceEvent(TraceEventType.Resume, 145, "Foo");
            AssertDebugLastMessage("debug", "MySource1 Debug Foo 145");

            ts.TraceEvent(TraceEventType.Warning, 145, "Bar");
            AssertDebugLastMessage("debug", "MySource1 Warn Bar 145");

            ts.TraceEvent(TraceEventType.Critical, 145, "Foo");
            AssertDebugLastMessage("debug", "MySource1 Fatal Foo 145");
        }

#if MONO
        [Fact(Skip="Not working under MONO - not sure if unit test is wrong, or the code")]
#else
        [Fact]
#endif
        public void ForceLogLevelTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${logger} ${level} ${message} ${event-context:EventID}' /></targets>
                    <rules>
                        <logger name='*' minlevel='Trace' writeTo='debug' />
                    </rules>
                </nlog>");

            TraceSource ts = CreateTraceSource();
            ts.Listeners.Add(new NLogTraceListener { Name = "Logger1", DefaultLogLevel = LogLevel.Trace, ForceLogLevel = LogLevel.Warn });

            // force all logs to be Warn, DefaultLogLevel has no effect on TraceSource
            ts.TraceInformation("Quick brown fox");
            AssertDebugLastMessage("debug", "MySource1 Warn Quick brown fox 0");

            ts.TraceInformation("Mary had {0} lamb", "a little");
            AssertDebugLastMessage("debug", "MySource1 Warn Mary had a little lamb 0");
        }

        [Fact]
        public void FilterTraceTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${logger} ${level} ${message} ${event-context:EventID}' /></targets>
                    <rules>
                        <logger name='*' minlevel='Trace' writeTo='debug' />
                    </rules>
                </nlog>");

            TraceSource ts = CreateTraceSource();
            ts.Listeners.Add(new NLogTraceListener { Name = "Logger1", DefaultLogLevel = LogLevel.Trace, ForceLogLevel = LogLevel.Warn, Filter = new EventTypeFilter(SourceLevels.Error) });

            // force all logs to be Warn, DefaultLogLevel has no effect on TraceSource
            ts.TraceEvent(TraceEventType.Error, 0, "Quick brown fox");
            AssertDebugLastMessage("debug", "MySource1 Warn Quick brown fox 0");

            ts.TraceInformation("Mary had {0} lamb", "a little");
            AssertDebugLastMessage("debug", "MySource1 Warn Quick brown fox 0");
        }
#endif

        [Fact]
        public void TraceTargetWriteLineTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
                <nlog>
                    <targets>
                        <target name='trace' type='Trace' layout='${logger} ${level} ${message}' rawWrite='true' />
                    </targets>
                    <rules>
                        <logger name='*' minlevel='Trace' writeTo='trace' />
                    </rules>
                </nlog>");

            var logger = LogManager.GetLogger("MySource1");
            var sw = new System.IO.StringWriter();

            try
            {
                Trace.Listeners.Clear();
                Trace.Listeners.Add(new TextWriterTraceListener(sw));
                foreach (var logLevel in LogLevel.AllLevels)
                {
                    if (logLevel == LogLevel.Off)
                        continue;
                    logger.Log(logLevel, "Quick brown fox");
                    Trace.Flush();
                    Assert.Equal($"MySource1 {logLevel} Quick brown fox" + Environment.NewLine, sw.GetStringBuilder().ToString());
                    sw.GetStringBuilder().Length = 0;
                }
                
                Trace.Flush();
            }
            finally
            {
                Trace.Listeners.Clear();
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TraceTargetEnableTraceFailTest(bool enableTraceFail)
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString($@"
                <nlog>
                    <targets>
                        <target name='trace' type='Trace' layout='${{logger}} ${{level}} ${{message}}' enableTraceFail='{enableTraceFail}' />
                    </targets>
                    <rules>
                        <logger name='*' minlevel='Trace' writeTo='trace' />
                    </rules>
                </nlog>");

            var logger = LogManager.GetLogger(nameof(TraceTargetEnableTraceFailTest));
            var sw = new System.IO.StringWriter();

            try
            {
                Trace.Listeners.Clear();
                Trace.Listeners.Add(new TextWriterTraceListener(sw));
                foreach (var logLevel in LogLevel.AllLevels)
                {
                    if (logLevel == LogLevel.Off)
                        continue;
                    logger.Log(logLevel, "Quick brown fox");
                    Trace.Flush();

                    if (logLevel == LogLevel.Fatal)
                    {
                        if (enableTraceFail)
                            Assert.Equal($"Fail: {logger.Name} Fatal Quick brown fox" + Environment.NewLine, sw.GetStringBuilder().ToString());
                        else
                            Assert.NotEqual($"Fail: {logger.Name} Fatal Quick brown fox" + Environment.NewLine, sw.GetStringBuilder().ToString());
                    }
                    
                    Assert.Contains($"{logger.Name} {logLevel} Quick brown fox" + Environment.NewLine, sw.GetStringBuilder().ToString());
                    sw.GetStringBuilder().Length = 0;
                }
            }
            finally
            {
                Trace.Listeners.Clear();
            }
        }

        private static TraceSource CreateTraceSource()
        {
            var ts = new TraceSource("MySource1", SourceLevels.All);
#if MONO
            // for some reason needed on Mono
            ts.Switch = new SourceSwitch("MySource1", "Verbose");
            ts.Switch.Level = SourceLevels.All;
#endif
            return ts;
        }
    }
}


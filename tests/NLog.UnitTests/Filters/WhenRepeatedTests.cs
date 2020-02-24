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

using NLog.Config;

namespace NLog.UnitTests.Filters
{
    using System;
    using Xunit;

    public class WhenRepeatedTests : NLogTestBase
    {
        [Fact]
        public void WhenRepeatedIgnoreTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='debug' type='Debug' layout='${message}' />
                </targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug'>
                   <filters defaultAction='log'>
                        <whenRepeated layout='${message}' action='Ignore' />
                    </filters>
                    </logger>
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Debug("a");
            AssertDebugCounter("debug", 1);
            logger.Debug("zzz");
            AssertDebugCounter("debug", 2);
            logger.Debug("zzz");
            AssertDebugCounter("debug", 2);
        }

        [Fact]
        public void WhenRepeatedIgnoreDualTargetTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='debug' type='Debug' layout='${message}' />
                    <target name='debug2' type='Debug' layout='${message}' />
                </targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug,debug2'>
                   <filters defaultAction='log'>
                        <whenRepeated layout='${message}' action='Ignore' />
                    </filters>
                    </logger>
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Debug("a");
            AssertDebugCounter("debug", 1);
            AssertDebugCounter("debug2", 1);
            logger.Debug("zzz");
            AssertDebugCounter("debug", 2);
            AssertDebugCounter("debug2", 2);
            logger.Debug("zzz");
            AssertDebugCounter("debug", 2);
            AssertDebugCounter("debug2", 2);
        }

        [Fact]
        public void WhenRepeatedLogAfterTimeoutTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug'>
                   <filters defaultAction='log'>
                        <whenRepeated layout='${message}' action='Ignore' includeFirst='True' />
                    </filters>
                    </logger>
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Debug("a");
            AssertDebugCounter("debug", 0);
            logger.Debug("zzz");
            AssertDebugCounter("debug", 0);
            logger.Debug("zzz");
            AssertDebugCounter("debug", 0);
        }

        [Fact]
        public void WhenRepeatedTimeoutIgnoreTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug'>
                   <filters defaultAction='log'>
                        <whenRepeated layout='${message}' action='Ignore' timeoutSeconds='10' />
                    </filters>
                    </logger>
                </rules>
            </nlog>");

            var defaultTimeSource = Time.TimeSource.Current;

            try
            {
                var timeSource = new TimeSourceTests.ShiftedTimeSource(DateTimeKind.Local);

                Time.TimeSource.Current = timeSource;

                ILogger logger = LogManager.GetLogger("A");
                logger.Debug("a");
                AssertDebugCounter("debug", 1);
                logger.Debug("zzz");
                AssertDebugCounter("debug", 2);
                logger.Debug("zzz");
                AssertDebugCounter("debug", 2);

                timeSource.AddToLocalTime(TimeSpan.FromSeconds(5));
                logger.Debug("zzz");
                AssertDebugCounter("debug", 2);

                logger.Debug("b");
                AssertDebugCounter("debug", 3);

                timeSource.AddToLocalTime(TimeSpan.FromSeconds(10));
                logger.Debug("zzz");
                AssertDebugCounter("debug", 4);
            }
            finally
            {
                Time.TimeSource.Current = defaultTimeSource; // restore default time source
            }
        }

        [Fact]
        public void WhenRepeatedTimeoutLogAfterTimeoutTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug'>
                   <filters defaultAction='log'>
                        <whenRepeated layout='${message}' action='Ignore' includeFirst='True' timeoutSeconds='10' />
                    </filters>
                    </logger>
                </rules>
            </nlog>");

            var defaultTimeSource = Time.TimeSource.Current;

            try
            {
                var timeSource = new TimeSourceTests.ShiftedTimeSource(DateTimeKind.Local);

                Time.TimeSource.Current = timeSource;

                ILogger logger = LogManager.GetLogger("A");
                logger.Debug("a");
                AssertDebugCounter("debug", 0);
                logger.Debug("zzz");
                AssertDebugCounter("debug", 0);
                logger.Debug("zzz");
                AssertDebugCounter("debug", 0);

                timeSource.AddToLocalTime(TimeSpan.FromSeconds(5));
                logger.Debug("zzz");
                AssertDebugCounter("debug", 0);

                logger.Debug("b");
                AssertDebugCounter("debug", 0);

                timeSource.AddToLocalTime(TimeSpan.FromSeconds(10));
                logger.Debug("zzz");
                AssertDebugCounter("debug", 1);
            }
            finally
            {
                Time.TimeSource.Current = defaultTimeSource; // restore default time source
            }
        }

        [Fact]
        public void WhenRepeatedDefaultFilterCountIgnoreTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug'>
                   <filters defaultAction='log'>
                        <whenRepeated layout='${message}' action='Ignore' defaultFilterCacheSize='5' timeoutSeconds='10' />
                    </filters>
                    </logger>
                </rules>
            </nlog>");

            var defaultTimeSource = Time.TimeSource.Current;

            try
            {
                var timeSource = new TimeSourceTests.ShiftedTimeSource(DateTimeKind.Local);

                Time.TimeSource.Current = timeSource;

                ILogger logger = LogManager.GetLogger("A");
                logger.Debug("a");
                AssertDebugCounter("debug", 1);
                logger.Debug("zzz");
                AssertDebugCounter("debug", 2);
                logger.Debug("zzz");
                AssertDebugCounter("debug", 2);
                logger.Debug("b");
                AssertDebugCounter("debug", 3);
                logger.Debug("c");
                AssertDebugCounter("debug", 4);
                logger.Debug("d");
                AssertDebugCounter("debug", 5);

                timeSource.AddToLocalTime(TimeSpan.FromSeconds(5));

                logger.Debug("e");
                AssertDebugCounter("debug", 6);
                logger.Debug("f");
                AssertDebugCounter("debug", 7);
                logger.Debug("zzz");
                AssertDebugCounter("debug", 7);

                timeSource.AddToLocalTime(TimeSpan.FromSeconds(10));

                for (int i = 0; i < 10; ++i)
                {
                    char charCount = (char)('g' + i);
                    logger.Debug(charCount.ToString());
                    AssertDebugCounter("debug", 8 + i);
                }

                logger.Debug("zzz");
                AssertDebugCounter("debug", 18);
            }
            finally
            {
                Time.TimeSource.Current = defaultTimeSource; // restore default time source
            }
        }

        [Fact]
        public void WhenRepeatedMaxCacheSizeIgnoreTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug'>
                   <filters defaultAction='log'>
                        <whenRepeated layout='${message}' action='Ignore' maxFilterCacheSize='5' defaultFilterCacheSize='5' timeoutSeconds='10' />
                    </filters>
                    </logger>
                </rules>
            </nlog>");

            var defaultTimeSource = Time.TimeSource.Current;

            try
            {
                var timeSource = new TimeSourceTests.ShiftedTimeSource(DateTimeKind.Local);

                Time.TimeSource.Current = timeSource;

                ILogger logger = LogManager.GetLogger("A");
                logger.Debug("a");
                AssertDebugCounter("debug", 1);
                logger.Debug("zzz");
                AssertDebugCounter("debug", 2);
                logger.Debug("zzz");
                AssertDebugCounter("debug", 2);
                logger.Debug("b");
                AssertDebugCounter("debug", 3);
                logger.Debug("c");
                AssertDebugCounter("debug", 4);
                logger.Debug("d");
                AssertDebugCounter("debug", 5);

                timeSource.AddToLocalTime(TimeSpan.FromSeconds(5));

                logger.Debug("e");
                AssertDebugCounter("debug", 6);
                logger.Debug("f");
                AssertDebugCounter("debug", 7);
                logger.Debug("zzz");
                AssertDebugCounter("debug", 7);

                timeSource.AddToLocalTime(TimeSpan.FromSeconds(10));

                for (int i = 0; i < 10; ++i)
                {
                    char charCount = (char)('g' + i);
                    logger.Debug(charCount.ToString());
                    AssertDebugCounter("debug", 8 + i);
                }

                logger.Debug("zzz");
                AssertDebugCounter("debug", 18);
            }
            finally
            {
                Time.TimeSource.Current = defaultTimeSource; // restore default time source
            }
        }


        [Fact]
        public void WhenRepeatedLevelIgnoreTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug'>
                   <filters defaultAction='log'>
                        <whenRepeated layout='${message}' action='Ignore' />
                    </filters>
                    </logger>
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Debug("a");
            AssertDebugCounter("debug", 1);
            logger.Debug("zzz");
            AssertDebugCounter("debug", 2);
            logger.Debug("zzz");
            AssertDebugCounter("debug", 2);
            logger.Error("zzz");
            AssertDebugCounter("debug", 3);
            logger.Error("zzz");
            AssertDebugCounter("debug", 3);
            logger.Debug("zzz");
            AssertDebugCounter("debug", 3);
            logger.Fatal("zzz");
            AssertDebugCounter("debug", 4);
        }

        [Fact]
        public void WhenRepeatedMaxLengthIgnoreTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug'>
                   <filters defaultAction='log'>
                        <whenRepeated layout='${message}' action='Ignore' maxLength='16' optimizeBufferDefaultLength='16' />
                    </filters>
                    </logger>
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Debug("a");
            AssertDebugCounter("debug", 1);
            logger.Debug("zzzzzzzzzzzzzzzz");
            AssertDebugCounter("debug", 2);
            logger.Debug("zzzzzzzzzzzzzzzz");
            AssertDebugCounter("debug", 2);
            logger.Debug("zzzzzzzzzzzzzzzzzzzz");
            AssertDebugCounter("debug", 2);
            logger.Debug("b");
            AssertDebugCounter("debug", 3);
        }

        [Fact]
        public void WhenRepeatedFilterCountPropertyNameIgnoreTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message}${event-properties:item=hits}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug'>
                   <filters defaultAction='log'>
                        <whenRepeated layout='${message}' action='Ignore' timeoutSeconds='5' filterCountPropertyName='hits' />
                    </filters>
                    </logger>
                </rules>
            </nlog>");

            var defaultTimeSource = Time.TimeSource.Current;

            try
            {
                var timeSource = new TimeSourceTests.ShiftedTimeSource(DateTimeKind.Local);

                Time.TimeSource.Current = timeSource;

                ILogger logger = LogManager.GetLogger("A");
                logger.Debug("a");
                AssertDebugCounter("debug", 1);
                logger.Debug("zzz");
                AssertDebugCounter("debug", 2);
                AssertDebugLastMessage("debug", "zzz");
                logger.Debug("zzz");
                AssertDebugCounter("debug", 2);

                timeSource.AddToLocalTime(TimeSpan.FromSeconds(3));
                logger.Debug("zzz");
                AssertDebugCounter("debug", 2);

                logger.Debug("b");
                AssertDebugCounter("debug", 3);

                timeSource.AddToLocalTime(TimeSpan.FromSeconds(3));
                logger.Debug("zzz");
                AssertDebugCounter("debug", 4);
                AssertDebugLastMessage("debug", "zzz2");
                logger.Debug("zzz");
                AssertDebugCounter("debug", 4);

                timeSource.AddToLocalTime(TimeSpan.FromSeconds(12));
                logger.Debug("zzz");
                AssertDebugCounter("debug", 5);
                AssertDebugLastMessage("debug", "zzz");
            }
            finally
            {
                Time.TimeSource.Current = defaultTimeSource; // restore default time source
            }
        }

        [Fact]
        public void WhenRepeatedFilterCountAppendFormatIgnoreTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message}${event-properties:item=hits}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug'>
                   <filters defaultAction='log'>
                        <whenRepeated layout='${message}' action='Ignore' timeoutSeconds='5' filterCountMessageAppendFormat=' (Hits: {0})' />
                    </filters>
                    </logger>
                </rules>
            </nlog>");

            var defaultTimeSource = Time.TimeSource.Current;

            try
            {
                var timeSource = new TimeSourceTests.ShiftedTimeSource(DateTimeKind.Local);

                Time.TimeSource.Current = timeSource;

                ILogger logger = LogManager.GetLogger("A");
                logger.Debug("a");
                AssertDebugCounter("debug", 1);
                logger.Debug("zzz");
                AssertDebugCounter("debug", 2);
                AssertDebugLastMessage("debug", "zzz");
                logger.Debug("zzz");
                AssertDebugCounter("debug", 2);

                timeSource.AddToLocalTime(TimeSpan.FromSeconds(3));
                logger.Debug("zzz");
                AssertDebugCounter("debug", 2);

                logger.Debug("b");
                AssertDebugCounter("debug", 3);

                timeSource.AddToLocalTime(TimeSpan.FromSeconds(3));
                logger.Debug("zzz");
                AssertDebugCounter("debug", 4);
                AssertDebugLastMessage("debug", "zzz (Hits: 2)");
                logger.Debug("zzz");
                AssertDebugCounter("debug", 4);

                timeSource.AddToLocalTime(TimeSpan.FromSeconds(12));
                logger.Debug("zzz");
                AssertDebugCounter("debug", 5);
                AssertDebugLastMessage("debug", "zzz");
            }
            finally
            {
                Time.TimeSource.Current = defaultTimeSource; // restore default time source
            }
        }

    }
}
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

namespace NLog.UnitTests.Layouts
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using NLog.Layouts;
    using Xunit;

    public class CsvLayoutTests : NLogTestBase
    {
        [Fact]
        public void EndToEndTest()
        {
            string tempFile = string.Empty;

            try
            {
                tempFile = Path.GetTempFileName();
                LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                  <target name='f' type='File' fileName='" + tempFile + @"'>
                    <layout type='CSVLayout'>
                      <column name='level' layout='${level}' />
                      <column name='message' layout='${message}' />
                      <column name='counter' layout='${counter}' />
                      <delimiter>Comma</delimiter>
                    </layout>
                  </target>
                </targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='f' />
                </rules>
            </nlog>");

                ILogger logger = LogManager.GetLogger("A");
                logger.Debug("msg");
                logger.Info("msg2");
                logger.Warn("Message with, a comma");

                using (StreamReader sr = File.OpenText(tempFile))
                {
                    Assert.Equal("level,message,counter", sr.ReadLine());
                    Assert.Equal("Debug,msg,1", sr.ReadLine());
                    Assert.Equal("Info,msg2,2", sr.ReadLine());
                    Assert.Equal("Warn,\"Message with, a comma\",3", sr.ReadLine());
                }
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        /// <summary>
        /// Custom header overwrites file headers
        /// 
        /// Note: maybe changed with an option in the future
        /// </summary>
        [Fact]
        public void CustomHeaderTest()
        {
            string tempFile = string.Empty;

            try
            {
                tempFile = Path.GetTempFileName();
                LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                  <target name='f' type='File' fileName='" + tempFile + @"'>
                    <layout type='CSVLayout'>
                      <header>headertest</header>
                      <column name='level' layout='${level}' quoting='Nothing' />
                      <column name='message' layout='${message}' />
                      <column name='counter' layout='${counter}' />
                      <delimiter>Comma</delimiter>
                    </layout>
                  </target>
                </targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='f' />
                </rules>
            </nlog>");

                ILogger logger = LogManager.GetLogger("A");
                logger.Debug("msg");
                logger.Info("msg2");
                logger.Warn("Message with, a comma");

                using (StreamReader sr = File.OpenText(tempFile))
                {
                    Assert.Equal("headertest", sr.ReadLine());
                    //   Assert.Equal("level,message,counter", sr.ReadLine());
                    Assert.Equal("Debug,msg,1", sr.ReadLine());
                    Assert.Equal("Info,msg2,2", sr.ReadLine());
                    Assert.Equal("Warn,\"Message with, a comma\",3", sr.ReadLine());
                }
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        [Fact]
        public void NoHeadersTest()
        {
            string tempFile = string.Empty;

            try
            {
                tempFile = Path.GetTempFileName();
                LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                  <target name='f' type='File' fileName='" + tempFile + @"'>
                    <layout type='CSVLayout' withHeader='false'>
                      <delimiter>Comma</delimiter>
                      <column name='level' layout='${level}' />
                      <column name='message' layout='${message}' />
                      <column name='counter' layout='${counter}' />
                    </layout>
                  </target>
                </targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='f' />
                </rules>
            </nlog>");

                ILogger logger = LogManager.GetLogger("A");
                logger.Debug("msg");
                logger.Info("msg2");
                logger.Warn("Message with, a comma");

                using (StreamReader sr = File.OpenText(tempFile))
                {
                    Assert.Equal("Debug,msg,1", sr.ReadLine());
                    Assert.Equal("Info,msg2,2", sr.ReadLine());
                    Assert.Equal("Warn,\"Message with, a comma\",3", sr.ReadLine());
                }
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        [Fact]
        public void CsvLayoutRenderingNoQuoting()
        {
            var delimiters = new Dictionary<CsvColumnDelimiterMode, string>
            {
                { CsvColumnDelimiterMode.Auto, CultureInfo.CurrentCulture.TextInfo.ListSeparator },
                { CsvColumnDelimiterMode.Comma, "," },
                { CsvColumnDelimiterMode.Semicolon, ";" },
                { CsvColumnDelimiterMode.Space, " " },
                { CsvColumnDelimiterMode.Tab, "\t" },
                { CsvColumnDelimiterMode.Pipe, "|" },
                { CsvColumnDelimiterMode.Custom, "zzz" },
            };

            foreach (var delim in delimiters)
            {
                var csvLayout = new CsvLayout()
                {
                    Quoting = CsvQuotingMode.Nothing,
                    Columns =
                        {
                            new CsvColumn("date", "${longdate}"),
                            new CsvColumn("level", "${level}"),
                            new CsvColumn("message;text", "${message}"),
                        },
                    Delimiter = delim.Key,
                    CustomColumnDelimiter = "zzz",
                };

                var ev = new LogEventInfo();
                ev.TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56);
                ev.Level = LogLevel.Info;
                ev.Message = string.Concat(csvLayout.QuoteChar, "hello, world", csvLayout.QuoteChar);

                string sep = delim.Value;
                Assert.Equal("2010-01-01 12:34:56.0000" + sep + "Info" + sep + "\"hello, world\"", csvLayout.Render(ev));
                Assert.Equal("date" + sep + "level" + sep + "message;text", csvLayout.Header.Render(ev));
            }
        }

        [Fact]
        public void CsvLayoutRenderingFullQuoting()
        {
            var delimiters = new Dictionary<CsvColumnDelimiterMode, string>
            {
                { CsvColumnDelimiterMode.Auto, CultureInfo.CurrentCulture.TextInfo.ListSeparator },
                { CsvColumnDelimiterMode.Comma, "," },
                { CsvColumnDelimiterMode.Semicolon, ";" },
                { CsvColumnDelimiterMode.Space, " " },
                { CsvColumnDelimiterMode.Tab, "\t" },
                { CsvColumnDelimiterMode.Pipe, "|" },
                { CsvColumnDelimiterMode.Custom, "zzz" },
            };

            foreach (var delim in delimiters)
            {
                var csvLayout = new CsvLayout()
                {
                    Quoting = CsvQuotingMode.All,
                    Columns =
                        {
                            new CsvColumn("date", "${longdate}"),
                            new CsvColumn("level", "${level}"),
                            new CsvColumn("message;text", "${message}"),
                        },
                    QuoteChar = "'",
                    Delimiter = delim.Key,
                    CustomColumnDelimiter = "zzz",
                };

                var ev = new LogEventInfo();
                ev.TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56);
                ev.Level = LogLevel.Info;
                ev.Message = string.Concat(csvLayout.QuoteChar, "hello, world", csvLayout.QuoteChar);

                string sep = delim.Value;
                Assert.Equal("'2010-01-01 12:34:56.0000'" + sep + "'Info'" + sep + "'''hello, world'''", csvLayout.Render(ev));
                Assert.Equal("'date'" + sep + "'level'" + sep + "'message;text'", csvLayout.Header.Render(ev));
            }
        }

        [Fact]
        public void CsvLayoutRenderingAutoQuoting()
        {
            var csvLayout = new CsvLayout()
            {
                Quoting = CsvQuotingMode.Auto,
                Columns =
                    {
                        new CsvColumn("date", "${longdate}"),
                        new CsvColumn("level", "${level}"),
                        new CsvColumn("message;text", "${message}"),
                    },
                QuoteChar = "'",
                Delimiter = CsvColumnDelimiterMode.Semicolon,
            };

            // no quoting
            Assert.Equal(
                "2010-01-01 12:34:56.0000;Info;hello, world",
                csvLayout.Render(new LogEventInfo
                {
                    TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56),
                    Level = LogLevel.Info,
                    Message = "hello, world"
                }));

            // multi-line string - requires quoting
            Assert.Equal(
                "2010-01-01 12:34:56.0000;Info;'hello\rworld'",
                csvLayout.Render(new LogEventInfo
                {
                    TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56),
                    Level = LogLevel.Info,
                    Message = "hello\rworld"
                }));

            // multi-line string - requires quoting
            Assert.Equal(
                "2010-01-01 12:34:56.0000;Info;'hello\nworld'",
                csvLayout.Render(new LogEventInfo
                {
                    TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56),
                    Level = LogLevel.Info,
                    Message = "hello\nworld"
                }));

            // quote character used in string, will be quoted and doubled
            Assert.Equal(
                "2010-01-01 12:34:56.0000;Info;'hello''world'",
                csvLayout.Render(new LogEventInfo
                {
                    TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56),
                    Level = LogLevel.Info,
                    Message = "hello'world"
                }));

            Assert.Equal("date;level;'message;text'", csvLayout.Header.Render(LogEventInfo.CreateNullEvent()));
        }

        [Fact]
        public void CsvLayoutCachingTest()
        {
            var csvLayout = new CsvLayout()
            {
                Quoting = CsvQuotingMode.Auto,
                Columns =
                    {
                        new CsvColumn("date", "${longdate}"),
                        new CsvColumn("level", "${level}"),
                        new CsvColumn("message", "${message}"),
                        new CsvColumn("threadid", "${threadid}"),
                    },
                QuoteChar = "'",
                Delimiter = CsvColumnDelimiterMode.Semicolon,
            };

            var e1 = new LogEventInfo
            {
                TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56),
                Level = LogLevel.Info,
                Message = "hello, world"
            };

            var e2 = new LogEventInfo
            {
                TimeStamp = new DateTime(2010, 01, 01, 12, 34, 57),
                Level = LogLevel.Info,
                Message = "hello, world"
            };

            var r11 = csvLayout.Render(e1);
            var r12 = csvLayout.Render(e1);
            var r21 = csvLayout.Render(e2);
            var r22 = csvLayout.Render(e2);

            var h11 = csvLayout.Header.Render(e1);
            var h12 = csvLayout.Header.Render(e1);
            var h21 = csvLayout.Header.Render(e2);
            var h22 = csvLayout.Header.Render(e2);

            Assert.Same(r11, r12);
            Assert.Same(r21, r22);

            Assert.NotSame(r11, r21);
            Assert.NotSame(r12, r22);

            Assert.Equal(h11, h21);
            Assert.Same(h11, h12);
            Assert.Same(h21, h22);
        }
    }
}

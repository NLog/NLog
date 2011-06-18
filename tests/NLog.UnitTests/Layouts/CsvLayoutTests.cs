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

namespace NLog.UnitTests.Layouts
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using NUnit.Framework;

#if !NUNIT
    using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
    using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
    using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
    using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif
    using NLog.Layouts;

    [TestFixture]
    public class CsvLayoutTests : NLogTestBase
    {
#if !SILVERLIGHT
        [Test]
        public void EndToEndTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets>
                  <target name='f' type='File' fileName='CSVLayoutEndToEnd1.txt'>
                    <layout type='CSVLayout'>
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

            Logger logger = LogManager.GetLogger("A");
            logger.Debug("msg");
            logger.Info("msg2");
            logger.Warn("Message with, a comma");

            using (StreamReader sr = File.OpenText("CSVLayoutEndToEnd1.txt"))
            {
                Assert.AreEqual("level,message,counter", sr.ReadLine());
                Assert.AreEqual("Debug,msg,1", sr.ReadLine());
                Assert.AreEqual("Info,msg2,2", sr.ReadLine());
                Assert.AreEqual("Warn,\"Message with, a comma\",3", sr.ReadLine());
            }
        }

        [Test]
        public void NoHeadersTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets>
                  <target name='f' type='File' fileName='CSVLayoutEndToEnd2.txt'>
                    <layout type='CSVLayout' withHeader='false'>
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

            Logger logger = LogManager.GetLogger("A");
            logger.Debug("msg");
            logger.Info("msg2");
            logger.Warn("Message with, a comma");

            using (StreamReader sr = File.OpenText("CSVLayoutEndToEnd2.txt"))
            {
                Assert.AreEqual("Debug,msg,1", sr.ReadLine());
                Assert.AreEqual("Info,msg2,2", sr.ReadLine());
                Assert.AreEqual("Warn,\"Message with, a comma\",3", sr.ReadLine());
            }
        }
#endif

        [Test]
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
                ev.Message = "hello, world";

                string sep = delim.Value;
                Assert.AreEqual("2010-01-01 12:34:56.0000" + sep + "Info" + sep + "hello, world", csvLayout.Render(ev));
                Assert.AreEqual("date" + sep + "level" + sep + "message;text", csvLayout.Header.Render(ev));
            }
        }

        [Test]
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
                ev.Message = "hello, world";

                string sep = delim.Value;
                Assert.AreEqual("'2010-01-01 12:34:56.0000'" + sep + "'Info'" + sep + "'hello, world'", csvLayout.Render(ev));
                Assert.AreEqual("'date'" + sep + "'level'" + sep + "'message;text'", csvLayout.Header.Render(ev));
            }
        }

        [Test]
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
            Assert.AreEqual(
                "2010-01-01 12:34:56.0000;Info;hello, world",
                csvLayout.Render(new LogEventInfo
                {
                    TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56),
                    Level = LogLevel.Info,
                    Message = "hello, world"
                }));

            // multi-line string - requires quoting
            Assert.AreEqual(
                "2010-01-01 12:34:56.0000;Info;'hello\rworld'",
                csvLayout.Render(new LogEventInfo
                {
                    TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56),
                    Level = LogLevel.Info,
                    Message = "hello\rworld"
                }));

            // multi-line string - requires quoting
            Assert.AreEqual(
                "2010-01-01 12:34:56.0000;Info;'hello\nworld'",
                csvLayout.Render(new LogEventInfo
                {
                    TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56),
                    Level = LogLevel.Info,
                    Message = "hello\nworld"
                }));

            // quote character used in string, will be quoted and doubled
            Assert.AreEqual(
                "2010-01-01 12:34:56.0000;Info;'hello''world'",
                csvLayout.Render(new LogEventInfo
                {
                    TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56),
                    Level = LogLevel.Info,
                    Message = "hello'world"
                }));

            Assert.AreEqual("date;level;'message;text'", csvLayout.Header.Render(LogEventInfo.CreateNullEvent()));
        }

        [Test]
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

            Assert.AreSame(r11, r12);
            Assert.AreSame(r21, r22);

            Assert.AreNotSame(r11, r21);
            Assert.AreNotSame(r12, r22);

            Assert.AreSame(h11, h12);
            Assert.AreSame(h21, h22);

            Assert.AreNotSame(h11, h21);
            Assert.AreNotSame(h12, h22);
        }
    }
}

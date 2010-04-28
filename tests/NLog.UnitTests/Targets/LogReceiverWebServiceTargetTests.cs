// 
// Copyright (c) 2004-2010 Jaroslaw Kowalski <jaak@jkowalski.net>
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

using System;
using System.IO;

#if !NET_CF && !SILVERLIGHT
using System.Runtime.Serialization;
using System.Xml.Serialization;
#endif

using System.Xml;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NLog.Config;
using NLog.LogReceiverService;
using NLog.Targets;

namespace NLog.UnitTests.Targets
{
    [TestClass]
    public class LogReceiverWebServiceTargetTests : NLogTestBase
    {
        /// <summary>
        /// The test 1.
        /// </summary>
        [TestMethod]
        public void Test1()
        {
            var logger = LogManager.GetLogger("Aaa");
            var target = new LogReceiverWebServiceTarget();
            target.EndpointAddress = "http://notimportant:9999/";
            target.Parameters.Add(new MethodCallParameter("message", "${message}"));
            target.Parameters.Add(new MethodCallParameter("date", "${longdate}"));

            SimpleConfigurator.ConfigureForTargetLogging(target);
            logger.Info("aa");
        }

#if !SILVERLIGHT && !NET_CF && !NET2_0
        [TestMethod]
        public void CompareSerializationFormats()
        {
            var events = new NLogEvents
            {
                BaseTimeUtc = DateTime.UtcNow.Ticks,
                ClientName = "foo",
                LayoutNames = new ListOfStrings { "foo", "bar", "baz" },
                LoggerNames = new ListOfStrings { "logger1", "logger2", "logger3" },
                Events =
                    new[]
                    {
                        new NLogEvent
                        {
                            Id = 1,
                            LevelOrdinal = 2,
                            LoggerOrdinal = 0,
                            TimeDelta = 34,
                            Values = new ListOfStrings() { "value1", "value2", "value3", }
                        },
                        new NLogEvent
                        {
                            Id = 2,
                            LevelOrdinal = 3,
                            LoggerOrdinal = 2,
                            TimeDelta = 345,
                            Values = new ListOfStrings() { "xvalue1", "xvalue2", "xvalue3", }
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

            Assert.AreEqual(xml1, xml2);
        }
#endif
    }
}
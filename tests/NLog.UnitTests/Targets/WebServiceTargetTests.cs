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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
#if !NETSTANDARD
using System.Collections.Concurrent;
using System.Web.Http;
using System.Web.Http.Dependencies;
using Microsoft.Owin.Hosting;
using Owin;
#endif
using NLog.Targets;
using NLog.Config;
using Xunit;

namespace NLog.UnitTests.Targets
{
    public class WebServiceTargetTests : NLogTestBase
    {
        public WebServiceTargetTests()
        {
            LogManager.ThrowExceptions = true;
        }

        [Fact]
        public void WebserviceTest_httppost_utf8_default_no_bom()
        {
            WebserviceTest_httppost_utf8("", false);
        }

        [Fact]
        public void WebserviceTest_httppost_utf8_with_bom()
        {
            WebserviceTest_httppost_utf8("includeBOM='true'", true);
        }

        [Fact]
        public void WebserviceTest_httppost_utf8_no_boml()
        {
            WebserviceTest_httppost_utf8("includeBOM='false'", false);
        }

        private void WebserviceTest_httppost_utf8(string bomAttr, bool includeBom)
        {
            var logFactory = new LogFactory().Setup()
                                             .SetupExtensions(ext => ext.RegisterAssembly(typeof(WebServiceTarget).Assembly))
                                             .LoadConfigurationFromXml(@"
                <nlog>
<targets>
    <target type='WebService'
            name='webservice'
            url='http://localhost:57953/Home/Foo2'
            protocol='HttpPost'
          " + bomAttr + @"
            encoding='UTF-8'
            methodName='Foo'>
        <parameter name='empty' type='System.String' layout=''/> <!-- work around so the guid is decoded properly -->
        <parameter name='guid' type='System.String' layout='${guid}'/>
        <parameter name='m' type='System.String' layout='${message}'/>
        <parameter name='date' type='System.String' layout='${longdate}'/>
        <parameter name='logger' type='System.String' layout='${logger}'/>
        <parameter name='level' type='System.String' layout='${level}'/>
    </target>
</targets>
                </nlog>").LogFactory;

            var target = logFactory.Configuration.FindTargetByName("webservice") as WebServiceTarget;
            Assert.NotNull(target);

            Assert.Equal(6, target.Parameters.Count);

            Assert.Equal("utf-8", target.Encoding.WebName);

            //async call with mockup stream
            var webRequest = System.Net.WebRequest.Create("http://www.test.com");
            var httpWebRequest = (HttpWebRequest)webRequest;
            var streamMock = new StreamMock();

            //event for async testing
            var counterEvent = new ManualResetEvent(false);

            var parameterValues = new object[] { "", "336cec87129942eeabab3d8babceead7", "Debg", "2014-06-26 23:15:14.6348", "TestClient.Program", "Debug" };
            target.DoInvoke(parameterValues, c => counterEvent.Set(), httpWebRequest,
                (request,callback) =>
                {
                    var t = new Task(() => { });
                    callback(t);
                    return t;
                },
                (request,result) => streamMock);

            counterEvent.WaitOne(10000);

            var bytes = streamMock.bytes;
            var url = streamMock.stringed;

            const string expectedUrl = "empty=&guid=336cec87129942eeabab3d8babceead7&m=Debg&date=2014-06-26+23%3a15%3a14.6348&logger=TestClient.Program&level=Debug";
            Assert.Equal(expectedUrl, url);

            Assert.True(bytes.Length > 3);

            //not bom
            var possbleBomBytes = bytes.Take(3).ToArray();
            if (includeBom)
            {
                Assert.Equal(possbleBomBytes, System.Text.Encoding.UTF8.GetPreamble());
            }
            else
            {
                Assert.NotEqual(possbleBomBytes, System.Text.Encoding.UTF8.GetPreamble());
            }

            Assert.Equal(bytes.Length, includeBom ? 126 : 123);
        }

        /// <summary>
        /// Mock the stream
        /// </summary>
        private class StreamMock : MemoryStream
        {
            public byte[] bytes;
            public string stringed;

            /// <summary>
            /// Releases the unmanaged resources used by the <see cref="T:System.IO.MemoryStream"/> class and optionally releases the managed resources.
            /// </summary>
            /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
            protected override void Dispose(bool disposing)
            {
                //save stuff before dispose
                Flush();
                bytes = ToArray();
                stringed = StreamToString(this);
                base.Dispose(disposing);
            }

            private static string StreamToString(Stream s)
            {
                s.Position = 0;
                var sr = new StreamReader(s);
                return sr.ReadToEnd();
            }
        }

#if !NETSTANDARD
        private static string getNewWsAddress()
        {
            string WsAddress = "http://localhost:9000/";
            return WsAddress.Substring(0, WsAddress.Length - 5) + (9000 + System.Threading.Interlocked.Increment(ref _portOffset)).ToString() + "/";
        }
        private static int _portOffset;

        /// <summary>
        /// Test the Webservice with REST api - <see cref="WebServiceProtocol.HttpPost"/> (only checking for no exception)
        /// </summary>
        [Fact]
        public void WebserviceTest_restapi_httppost()
        {
            string wsAddress = getNewWsAddress();
            var logFactory = new LogFactory().Setup()
                                             .SetupExtensions(ext => ext.RegisterAssembly(typeof(WebServiceTarget).Assembly))
                                             .LoadConfigurationFromXml($@"
                <nlog throwExceptions='true'>
                    <targets>
                        <target type='WebService'
                                name='ws'
                                url='{wsAddress}{"api/logme"}'
                                protocol='HttpPost'
                                encoding='UTF-8'
                               >
                            <parameter name='param1' type='System.String' layout='${{message}}'/> 
                            <parameter name='param2' type='System.String' layout='${{level}}'/>    
                        </target>
                    </targets>
                    <rules>
                      <logger name='*' writeTo='ws' />
                    </rules>
                </nlog>").LogFactory;

            var logger = logFactory.GetCurrentClassLogger();

            var context = new LogMeController.TestContext();
            context.ResetState(2);

            var message1 = "message 1 with a post";
            var message2 = "a b c é k è ï ?";
            StartOwinTest(wsAddress, context, () =>
            {
                logger.Info(message1);
                logger.Info(message2);
            });

            Assert.Equal(0, context.CountdownEvent.CurrentCount);
            Assert.Equal(2, context.ReceivedLogsPostParam1.Count);
            CheckQueueMessage(message1, context.ReceivedLogsPostParam1);
            CheckQueueMessage(message2, context.ReceivedLogsPostParam1);
        }

        /// <summary>
        /// Test the Webservice with REST api -  <see cref="WebServiceProtocol.HttpGet"/>  (only checking for no exception)
        /// </summary>
        [Fact]
        public void WebserviceTest_restapi_httpget()
        {
            string wsAddress = getNewWsAddress();
            var logger = SetUpHttpGetWebservice(wsAddress, "api/logme").GetCurrentClassLogger();

            var context = new LogMeController.TestContext();
            context.ResetState(2);

            var message1 = "message 1 with a post";
            var message2 = "a b c é k è ï ?";
            StartOwinTest(wsAddress, context, () =>
            {
                logger.Info(message1);
                logger.Info(message2);
            });

            Assert.Equal(0, context.CountdownEvent.CurrentCount);
            Assert.Equal(2, context.ReceivedLogsGetParam1.Count);
            CheckQueueMessage(message1, context.ReceivedLogsGetParam1);
            CheckQueueMessage(message2, context.ReceivedLogsGetParam1);
        }

        /// <summary>
        /// Test the Webservice with REST api -  <see cref="WebServiceProtocol.HttpGet"/>  (only checking for no exception)
        /// </summary>
        [Fact]
        public void WebserviceTest_restapi_httpget_flush()
        {
            string wsAddress = getNewWsAddress();
            var logFactory = SetUpHttpGetWebservice(wsAddress, "api/logme");
            var logger = logFactory.GetCurrentClassLogger();

            var context = new LogMeController.TestContext();
            context.ResetState(0);

            var message1 = "message with a post";
            StartOwinTest(wsAddress, context, () =>
            {
                for (int i = 0; i < 100; ++i)
                    logger.Info(message1);

                // Make triple-flush to fully exercise the async flushing logic
                try
                {
                    LogManager.Flush(0);
                }
                catch (NLogRuntimeException)
                { }
                logFactory.Flush(); // Waits for flush (Scheduled on top of the previous flush)
                logFactory.Flush(); // Nothing to flush
            });

            Assert.Equal(100, context.ReceivedLogsGetParam1.Count);
        }

        [Fact]
        public void WebServiceTest_restapi_httpget_querystring()
        {
            string wsAddress = getNewWsAddress();
            var logger = SetUpHttpGetWebservice(wsAddress, "api/logme?paramFromConfig=valueFromConfig").GetCurrentClassLogger();

            var context = new LogMeController.TestContext();
            context.ResetState(1);

            StartOwinTest(wsAddress, context, () =>
            {
                logger.Info("another message");
            });

            Assert.Equal(0, context.CountdownEvent.CurrentCount);
            Assert.Single(context.ReceivedLogsGetParam1);
            CheckQueueMessage("another message", context.ReceivedLogsGetParam1);
        }

        private static LogFactory SetUpHttpGetWebservice(string wsAddress, string relativeUrl)
        {
            var logFactory = new LogFactory().Setup()
                                             .SetupExtensions(ext => ext.RegisterAssembly(typeof(WebServiceTarget).Assembly))
                                             .LoadConfigurationFromXml($@"
                <nlog throwExceptions='true' >
                    <targets>
                        <target type='WebService'
                                name='ws'
                                url='{wsAddress}{relativeUrl}'
                                protocol='HttpGet'
                                encoding='UTF-8'
                               >
                            <parameter name='param1' type='System.String' layout='${{message}}'/> 
                            <parameter name='param2' type='System.String' layout='${{level}}'/>
                        </target>
                    </targets>
                    <rules>
                      <logger name='*' writeTo='ws' />
                    </rules>
                </nlog>").LogFactory;
            return logFactory;
        }

        private static void CheckQueueMessage(string message1, ConcurrentBag<string> receivedLogsGetParam1)
        {
            var success = receivedLogsGetParam1.Contains(message1);
            Assert.True(success, $"message '{message1}' not found");
        }

        /// <summary>
        /// Timeout for <see cref="WebserviceTest_restapi_httppost_checkingLost"/>.
        /// 
        /// in miliseconds. 20000 = 20 sec
        /// </summary>
        const int webserviceCheckTimeoutMs = 20000;

        /// <summary>
        /// Test the Webservice with REST api - <see cref="WebServiceProtocol.HttpPost"/> (only checking for no exception)
        /// 
        /// repeats for checking 'lost messages'
        /// </summary>
        [Fact]
        public void WebserviceTest_restapi_httppost_checkingLost()
        {
            RetryingIntegrationTest(3, () =>
            {
                string wsAddress = getNewWsAddress();
                var logFactory = new LogFactory().Setup()
                                                 .SetupExtensions(ext => ext.RegisterAssembly(typeof(WebServiceTarget).Assembly))
                                                 .LoadConfigurationFromXml($@"
                    <nlog throwExceptions='true'>
                        <targets>
                            <target type='WebService'
                                    name='ws'
                                    url='{wsAddress}{"api/logme"}'
                                    protocol='HttpPost'
                                    encoding='UTF-8'
                                   >
                                <parameter name='param1' type='System.String' layout='${{message}}'/> 
                                <parameter name='param2' type='System.String' layout='${{level}}'/>
                            </target>
                        </targets>
                        <rules>
                          <logger name='*' writeTo='ws' />
                        </rules>
                    </nlog>").LogFactory;

                var logger = logFactory.GetCurrentClassLogger();

                const int messageCount = 1000;
                var createdMessages = new List<string>(messageCount);

                for (int i = 0; i < messageCount; i++)
                {
                    var message = "message " + i;
                    createdMessages.Add(message);
                }

                //reset
                var context = new LogMeController.TestContext();
                context.ResetState(messageCount);

                StartOwinTest(wsAddress, context, () =>
                {
                    foreach (var createdMessage in createdMessages)
                    {
                        logger.Info(createdMessage);
                    }
                });

                Assert.Equal(0, context.CountdownEvent.CurrentCount);
                Assert.Equal(createdMessages.Count, context.ReceivedLogsPostParam1.Count);
            });
        }

        /// <summary>
        /// Test the Webservice with REST api - <see cref="WebServiceProtocol.JsonPost"/>
        /// </summary>
        [Fact]
        public void WebserviceTest_restapi_json()
        {
            string wsAddress = getNewWsAddress();
            var logFactory = new LogFactory().Setup()
                                             .SetupExtensions(ext => ext.RegisterAssembly(typeof(WebServiceTarget).Assembly))
                                             .LoadConfigurationFromXml($@"
                <nlog throwExceptions='true'>
                    <targets>
                        <target type='WebService'
                                name='ws'
                                url='{wsAddress}{"api/logdoc/json"}'
                                protocol='JsonPost'
                                encoding='UTF-8'
                               >
                            <header name='Authorization' layout='OpenBackDoor' />
                            <parameter name='param1' ParameterType='System.String' layout='${{message}}'/> 
                            <parameter name='param2' ParameterType='System.String' layout='${{level}}'/>
                            <parameter name='param3' ParameterType='System.Boolean' layout='True'/>
                            <parameter name='param4' ParameterType='System.DateTime' layout='${{date:universalTime=true:format=o}}'/>
                        </target>
                    </targets>
                    <rules>
                      <logger name='*' writeTo='ws' />
                    </rules>
                </nlog>").LogFactory;

            var logger = logFactory.GetCurrentClassLogger();

            var txt = "message 1 with a JSON POST<hello><again\\>\"\b";   // Lets tease the JSON serializer and see it can handle valid and invalid xml chars
            var count = 101;
            var context = new LogDocController.TestContext(count, false, new Dictionary<string, string>() { { "Authorization", "OpenBackDoor" } }, txt, "info", true, DateTime.UtcNow);

            StartOwinDocTest(wsAddress, context, () =>
            {
                for (int i = 0; i < count; i++)
                    logger.Info(txt);
            });

            Assert.Equal<int>(0, context.CountdownEvent.CurrentCount);
        }


        /// <summary>
        /// Test the Webservice with REST api - <see cref="WebServiceProtocol.XmlPost"/> 
        /// </summary>
        [Fact]
        public void WebserviceTest_restapi_xml()
        {
            string wsAddress = getNewWsAddress();
            var logFactory = new LogFactory().Setup()
                                             .SetupExtensions(ext => ext.RegisterAssembly(typeof(WebServiceTarget).Assembly))
                                             .LoadConfigurationFromXml($@"
                <nlog throwExceptions='true'>
                    <targets>
                        <target type='WebService'
                                name='ws'
                                url='{wsAddress}{"api/logdoc/xml"}'
                                protocol='XmlPost'
                                XmlRoot='ComplexType'
                                encoding='UTF-8'
                               >
                            <parameter name='param1' ParameterType='System.String' layout='${{message}}'/> 
                            <parameter name='param2' ParameterType='System.String' layout='${{level}}'/>
                            <parameter name='param3' ParameterType='System.Boolean' layout='True'/>
                            <parameter name='param4' ParameterType='System.DateTime' layout='${{date:universalTime=true:format=o}}'/>
                        </target>
                    </targets>
                    <rules>
                      <logger name='*' writeTo='ws' />
                    </rules>
                </nlog>").LogFactory;

            var logger = logFactory.GetCurrentClassLogger();

            var txt = "message 1 with a XML POST<hello><again\\>\"";   // Lets tease the Xml-Serializer, and see it can handle xml-tags
            var count = 101;
            var context = new LogDocController.TestContext(count, true, null, txt, "info", true, DateTime.UtcNow);

            StartOwinDocTest(wsAddress, context, () =>
            {
                for (int i = 0; i < count; i++)
                    logger.Info(txt);
            });

            Assert.Equal<int>(0, context.CountdownEvent.CurrentCount);
        }

        /// <summary>
        /// Test the Webservice with Soap11 api - <see cref="WebServiceProtocol.Soap11"/> 
        /// </summary>
        [Fact]
        public void WebserviceTest_soap11_default_soapaction()
        {
            string wsAddress = getNewWsAddress();
            var logFactory = new LogFactory().Setup()
                                             .SetupExtensions(ext => ext.RegisterAssembly(typeof(WebServiceTarget).Assembly))
                                             .LoadConfigurationFromXml($@"
                <nlog throwExceptions='true'>
                    <targets>
                        <target type='WebService'
                                name='ws'
                                url='{wsAddress}{"api/logdoc/soap11"}'
                                protocol='Soap11'
                                namespace='http://tempuri.org/'
                                methodName ='Ping'
                                preAuthenticate='false'
                                encoding ='UTF-8'
                               >
                            <parameter name='param1' ParameterType='System.String' layout='${{message}}'/> 
                            <parameter name='param2' ParameterType='System.String' layout='${{level}}'/>
                        </target>
                    </targets>
                    <rules>
                      <logger name='*' writeTo='ws' />
                    </rules>
                </nlog>").LogFactory;

            var logger = logFactory.GetCurrentClassLogger();

            var txt = "test.message";   // Lets tease the Xml-Serializer, and see it can handle xml-tags
            var count = 1;
            var expectedHeaders = new Dictionary<string, string>
            {
                {"SOAPAction", "http://tempuri.org/Ping" }
            };
            var context = new LogDocController.TestContext(count, true, expectedHeaders, null, null, true, DateTime.UtcNow);

            StartOwinDocTest(wsAddress, context, () =>
            {
                logger.Info(txt);
            });

            Assert.Equal<int>(0, context.CountdownEvent.CurrentCount);
        }

        /// <summary>
        /// Test the Webservice with Soap11 api - <see cref="WebServiceProtocol.Soap11"/> 
        /// </summary>
        [Fact]
        public void WebserviceTest_soap11_custom_soapaction()
        {
            string wsAddress = getNewWsAddress();
            var logFactory = new LogFactory().Setup()
                                             .SetupExtensions(ext => ext.RegisterAssembly(typeof(WebServiceTarget).Assembly))
                                             .LoadConfigurationFromXml($@"
                <nlog throwExceptions='true'>
                    <targets>
                        <target type='WebService'
                                name='ws'
                                url='{wsAddress}{"api/logdoc/soap11"}'
                                protocol='Soap11'
                                namespace='http://tempuri.org/'
                                methodName ='Ping'
                                preAuthenticate='false'
                                encoding ='UTF-8'
                               >
                            <header name='SOAPAction' layout='http://tempuri.org/custom-namespace/Ping'/>
                            <parameter name='param1' ParameterType='System.String' layout='${{message}}'/> 
                            <parameter name='param2' ParameterType='System.String' layout='${{level}}'/>
                        </target>
                    </targets>
                    <rules>
                      <logger name='*' writeTo='ws' />
                    </rules>
                </nlog>").LogFactory;

            var logger = logFactory.GetCurrentClassLogger();

            var txt = "test.message";   // Lets tease the Xml-Serializer, and see it can handle xml-tags
            var count = 1;
            var expectedHeaders = new Dictionary<string, string>
            {
                {"SOAPAction", "http://tempuri.org/custom-namespace/Ping" }
            };
            var context = new LogDocController.TestContext(count, true, expectedHeaders, null, null, true, DateTime.UtcNow);

            StartOwinDocTest(wsAddress, context, () =>
            {
                logger.Info(txt);
            });

            Assert.Equal<int>(0, context.CountdownEvent.CurrentCount);
        }

        /// <summary>
        /// Test the Webservice with Soap11 api - <see cref="WebServiceProtocol.Soap11"/> 
        /// </summary>
        [Fact]
        public void WebserviceTest_soap12_default_soapaction()
        {
            string wsAddress = getNewWsAddress();
            var logFactory = new LogFactory().Setup()
                                             .SetupExtensions(ext => ext.RegisterAssembly(typeof(WebServiceTarget).Assembly))
                                             .LoadConfigurationFromXml($@"
                <nlog throwExceptions='true'>
                    <targets>
                        <target type='WebService'
                                name='ws'
                                url='{wsAddress}{"api/logdoc/soap12"}'
                                protocol='Soap12'
                                namespace='http://tempuri.org/'
                                methodName ='Ping'
                                preAuthenticate='false'
                                encoding ='UTF-8'
                               >
                            <parameter name='param1' ParameterType='System.String' layout='${{message}}'/> 
                            <parameter name='param2' ParameterType='System.String' layout='${{level}}'/>
                        </target>
                    </targets>
                    <rules>
                      <logger name='*' writeTo='ws' />
                    </rules>
                </nlog>").LogFactory;

            var logger = logFactory.GetCurrentClassLogger();

            var txt = "test.message";   // Lets tease the Xml-Serializer, and see it can handle xml-tags
            var count = 1;
            var contentType = MediaTypeHeaderValue.Parse("application/soap+xml;charset=utf-8;action=\"http://tempuri.org/Ping\"");
            var context = new LogDocController.TestContext(count, true, null, null, null, true, DateTime.UtcNow, contentType);

            StartOwinDocTest(wsAddress, context, () =>
            {
                logger.Info(txt);
            });

            Assert.Equal<int>(0, context.CountdownEvent.CurrentCount);
        }

        ///<remarks>Must be public </remarks>
        public class LogMeController : ApiController
        {
            public TestContext Context { get; set; } = new TestContext();

            /// <summary>
            /// We need a complex type for modelbinding because of content-type: "application/x-www-form-urlencoded" in <see cref="WebServiceTarget"/>
            /// </summary>
            [DataContract(Namespace = "")]
            [XmlRoot(ElementName = "ComplexType", Namespace = "")]
            public class ComplexType
            {
                [DataMember(Name = "param1")]
                [XmlElement("param1")]
                public string Param1 { get; set; }
                [DataMember(Name = "param2")]
                [XmlElement("param2")]
                public string Param2 { get; set; }
                [DataMember(Name = "param3")]
                [XmlElement("param3")]
                public bool Param3 { get; set; }
                [DataMember(Name = "param4")]
                [XmlElement("param4")]
                public DateTime Param4 { get; set; }
            }

            /// <summary>
            /// Get
            /// </summary>
            public string Get(int id)
            {

                return "value";
            }

            // GET api/values 
            public IEnumerable<string> Get(string param1 = "", string param2 = "")
            {
                Context.ReceivedLogsGetParam1.Add(param1);
                if (Context.CountdownEvent != null)
                {
                    Context.CountdownEvent.Signal();
                }

                return new string[] { "value1", "value2" };
            }

            /// <summary>
            /// Post
            /// </summary>
            public void Post([FromBody] ComplexType complexType)
            {
                //this is working. 
                if (complexType == null)
                {
                    throw new ArgumentNullException(nameof(complexType));
                }
                Context.ReceivedLogsPostParam1.Add(complexType.Param1);

                if (Context.CountdownEvent != null)
                {
                    Context.CountdownEvent.Signal();
                }
            }

            /// <summary>
            /// Put
            /// </summary>
            public void Put(int id, [FromBody]string value)
            {
            }

            /// <summary>
            /// Delete
            /// </summary>
            public void Delete(int id)
            {
            }

            public class TestContext
            {
                /// <summary>
                /// Countdown event for keeping WS alive.
                /// </summary>
                public CountdownEvent CountdownEvent;

                /// <summary>
                /// Received param1 values (get)
                /// </summary>
                public ConcurrentBag<string> ReceivedLogsGetParam1 = new ConcurrentBag<string>();
                /// <summary>
                /// Received param1 values(post)
                /// </summary>
                public ConcurrentBag<string> ReceivedLogsPostParam1 = new ConcurrentBag<string>();

                /// <summary>
                /// Reset the state for unit testing
                /// </summary>
                /// <param name="expectedMessages"></param>
                public void ResetState(int expectedMessages)
                {
                    ReceivedLogsPostParam1 = new ConcurrentBag<string>();
                    ReceivedLogsGetParam1 = new ConcurrentBag<string>();
                    if (expectedMessages > 0)
                        CountdownEvent = new CountdownEvent(expectedMessages);
                    else
                        CountdownEvent = null;
                }
            }
        }

        internal static void StartOwinTest(string url, LogMeController.TestContext testContext, Action testsFunc)
        {
            // HttpSelfHostConfiguration. So info: http://www.asp.net/web-api/overview/hosting-aspnet-web-api/use-owin-to-self-host-web-api

            // Start webservice 
            using (WebApp.Start(url, (appBuilder) =>
            {
                // Configure Web API for self-host. 
                HttpConfiguration config = new HttpConfiguration();

                config.DependencyResolver = new ControllerResolver<LogMeController>(() => new LogMeController() { Context = testContext });

                config.Routes.MapHttpRoute(
                    name: "DefaultApi",
                    routeTemplate: "api/{controller}/{id}",
                    defaults: new { id = RouteParameter.Optional }
                );

                appBuilder.UseWebApi(config);
            }))
            {
                testsFunc();

                //wait for all received message, or timeout. There is no exception on timeout, so we have to check carefully in the unit test.
                if (testContext.CountdownEvent != null)
                {
                    testContext.CountdownEvent.Wait(webserviceCheckTimeoutMs);
                    //we need some extra time for completion
                    Thread.Sleep(1000);
                }
            }
        }

        internal static void StartOwinDocTest(string url, LogDocController.TestContext testContext, Action testsFunc)
        {
            using (WebApp.Start(url, (appBuilder) =>
            {
                // Configure Web API for self-host. 
                HttpConfiguration config = new HttpConfiguration();

                config.DependencyResolver = new ControllerResolver<LogDocController>(() => new LogDocController() { Context = testContext });

                config.Routes.MapHttpRoute(
                    name: "ApiWithAction",
                    routeTemplate: "api/{controller}/{action}/{id}",
                    defaults: new { id = RouteParameter.Optional }
                );
                config.Routes.MapHttpRoute(
                    name: "DefaultApi",
                    routeTemplate: "api/{controller}/{id}",
                    defaults: new { id = RouteParameter.Optional }
                );

                if (testContext.XmlInsteadOfJson)
                {
                    // Default Xml Formatter uses DataContractSerializer, changing it to XmlSerializer
                    config.Formatters.XmlFormatter.UseXmlSerializer = true;
                }
                else
                {
                    // Use ISO 8601 / RFC 3339 Date-Format (2012-07-27T18:51:45.53403Z), instead of Microsoft JSON date format ("\/Date(ticks)\/")
                    config.Formatters.JsonFormatter.SerializerSettings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat;
                    config.Formatters.JsonFormatter.UseDataContractJsonSerializer = false;  // JSON.NET serializer instead of the ancient DataContractJsonSerializer
                }

                appBuilder.UseWebApi(config);
            }))
            {
                testsFunc();

                if (testContext.CountdownEvent != null)
                {
                    testContext.CountdownEvent.Wait(webserviceCheckTimeoutMs);
                    Thread.Sleep(1000);
                }
            }
        }

        private sealed class ControllerResolver<T> : IDependencyResolver
        {
            private readonly Func<T> _factory;

            public ControllerResolver(Func<T> factory)
            {
                _factory = factory;
            }

            public IDependencyScope BeginScope()
            {
                return this;
            }

            public void Dispose()
            {
            }

            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(T))
                {
                    return _factory.Invoke();
                }
                else
                {
                    return null;
                }
            }

            public IEnumerable<object> GetServices(Type serviceType)
            {
                if (serviceType == typeof(T))
                {
                    return new object[] { GetService(serviceType) };
                }
                else
                {
                    return new object[0];
                }
            }
        }

        ///<remarks>Must be public </remarks>
        public class LogDocController : ApiController
        {
            public TestContext Context { get; set; }

            [HttpPost]
            public void Json(LogMeController.ComplexType complexType)
            {
                if (complexType == null)
                {
                    throw new ArgumentNullException(nameof(complexType));
                }

                processRequest(complexType);
            }

            private void processRequest(LogMeController.ComplexType complexType)
            {
                if (Context != null)
                {
                    if (string.Equals(Context.ExpectedParam2, complexType.Param2, StringComparison.OrdinalIgnoreCase)
                        && Context.ExpectedParam1 == complexType.Param1
                        && Context.ExpectedParam3 == complexType.Param3
                        && Context.ExpectedParam4.Date == complexType.Param4.Date)
                    {
                        if (!ValidateHeaders())
                        {
                            return;
                        }
                        Context.CountdownEvent.Signal();
                    }
                }
            }

            [HttpPost]
            public void Xml(LogMeController.ComplexType complexType)
            {
                if (complexType == null)
                {
                    throw new ArgumentNullException(nameof(complexType));
                }

                processRequest(complexType);
            }

            [HttpPost]
            public void Soap11()
            {
                if (Context != null)
                {
                    if (ValidateHeaders())
                    {
                        Context.CountdownEvent.Signal();
                    }
                }
            }

            [HttpPost]
            public void Soap12()
            {
                if (Context?.ExpectedContentType != null && Context.ExpectedContentType.Equals(Request.Content.Headers.ContentType))
                {
                    Context.CountdownEvent.Signal();
                }
            }

            private bool ValidateHeaders()
            {
                if (Context.ExpectedHeaders?.Count > 0)
                {
                    foreach (var expectedHeader in Context.ExpectedHeaders)
                    {
                        if (Request.Headers.GetValues(expectedHeader.Key).First() != expectedHeader.Value)
                            return false;
                    }
                }

                return true;
            }

            public class TestContext
            {
                public CountdownEvent CountdownEvent { get; }

                public bool XmlInsteadOfJson { get; }

                public Dictionary<string, string> ExpectedHeaders { get; }

                public string ExpectedParam1 { get; }

                public string ExpectedParam2 { get; }

                public bool ExpectedParam3 { get; }

                public DateTime ExpectedParam4 { get; }

                public MediaTypeHeaderValue ExpectedContentType { get; }

                public TestContext(int expectedMessages, bool xmlInsteadOfJson, Dictionary<string, string> expectedHeaders, string expected1, string expected2, bool expected3, DateTime expected4, MediaTypeHeaderValue expectedContentType = null)
                {
                    CountdownEvent = new CountdownEvent(expectedMessages);
                    XmlInsteadOfJson = xmlInsteadOfJson;
                    ExpectedHeaders = expectedHeaders;
                    ExpectedParam1 = expected1;
                    ExpectedParam2 = expected2;
                    ExpectedParam3 = expected3;
                    ExpectedParam4 = expected4;
                    ExpectedContentType = expectedContentType;
                }
            }
        }

#endif
    }

}

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
using System.Threading;
using System.Threading.Tasks;
using NLog.Internal;
using NLog.Targets;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using NLog.Config;
#if !NETSTANDARD
using System.Collections.Concurrent;
using System.Web.Http;
using Owin;
using Microsoft.Owin.Hosting;
using System.Web.Http.Dependencies;
#endif
using Xunit;

namespace NLog.UnitTests.Targets
{
    public class WebServiceTargetTests : NLogTestBase
    {
        [Fact]
        public void Stream_CopyWithOffset_test()
        {
            var text = @"

Lorem ipsum dolor sit amet consectetuer tellus semper dictum urna consectetuer. Eu iaculis enim tincidunt mi pede id ut sociis non vitae. Condimentum augue Nam Vestibulum faucibus tortor et at Sed et molestie. Interdum morbi Nullam pellentesque Vestibulum pede et eget semper Pellentesque quis. Velit cursus nec dolor vitae id et urna quis ante velit. Neque urna et vitae neque Vestibulum tellus convallis dui.

Tellus nibh enim augue senectus ut augue Donec Pellentesque Sed pretium. Volutpat nunc rutrum auctor dolor pharetra malesuada elit sapien ac nec. Adipiscing et id penatibus turpis a odio risus orci Suspendisse eu. Nibh eu facilisi eu consectetuer nibh eu in Nunc Curabitur rutrum. Quisque sit lacus consectetuer eu Duis quis felis hendrerit lobortis mauris. Nam Vivamus enim Aenean rhoncus.

Nulla tellus dui orci montes Vestibulum Aenean condimentum non id vel. Euismod Nam libero odio ut ut Nunc ac dui Nulla volutpat. Quisque facilisis consequat tempus tempus Curabitur tortor id Phasellus Suspendisse In. Lorem et Phasellus wisi Fusce fringilla pretium pede sapien amet ligula. In sed id In eget tristique quam sed interdum wisi commodo. Volutpat neque nibh mauris Quisque lorem nunc porttitor Cras faucibus augue. Sociis tempus et.

Morbi Nulla justo Aenean orci Vestibulum ullamcorper tincidunt mollis et hendrerit. Enim at laoreet elit eros ut at laoreet vel velit quis. Netus sed Suspendisse sed Curabitur vel sed wisi sapien nonummy congue. Semper Sed a malesuada tristique Vivamus et est eu quis ante. Wisi cursus Suspendisse dictum pretium habitant sodales scelerisque dui tempus libero. Venenatis consequat Lorem eu.



";

            var textStream = GenerateStreamFromString(text);
            var textBytes = StreamToBytes(textStream);

            textStream.Position = 0;
            textStream.Flush();

            var resultStream = new MemoryStream();
            textStream.CopyWithOffset(resultStream, 3);
            var result = StreamToBytes(resultStream);
            var expected = textBytes.Skip(3).ToArray();
            Assert.Equal(result.Length, expected.Length);
            Assert.Equal(result, expected);


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
            var configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
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
                </nlog>");

            var target = configuration.FindTargetByName("webservice") as WebServiceTarget;
            Assert.NotNull(target);

            Assert.Equal(6, target.Parameters.Count);

            Assert.Equal("utf-8", target.Encoding.WebName);

            //async call with mockup stream
            WebRequest webRequest = WebRequest.Create("http://www.test.com");
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
                Assert.Equal(possbleBomBytes, EncodingHelpers.Utf8BOM);
            }
            else
            {
                Assert.NotEqual(possbleBomBytes, EncodingHelpers.Utf8BOM);
            }

            Assert.Equal(bytes.Length, includeBom ? 126 : 123);
        }
        #region helpers

        private Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private static byte[] StreamToBytes(Stream stream)
        {
            stream.Flush();
            stream.Position = 0;
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Mock the stream
        /// </summary>
        private class StreamMock : MemoryStream
        {
            public byte[] bytes;
            public string stringed;

            #region Overrides of MemoryStream

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

            #endregion
        }


        #endregion

#if !NETSTANDARD
        const string WsAddress = "http://localhost:9000/";

        private static string getWsAddress(int portOffset)
        {
            return WsAddress.Substring(0, WsAddress.Length - 5) + (9000 + portOffset).ToString() + "/";
        }

        /// <summary>
        /// Test the Webservice with REST api - <see cref="WebServiceProtocol.HttpPost"/> (only checking for no exception)
        /// </summary>
        [Fact]
        public void WebserviceTest_restapi_httppost()
        {
            var configuration = XmlLoggingConfiguration.CreateFromXmlString($@"
                <nlog throwExceptions='true'>
                    <targets>
                        <target type='WebService'
                                name='ws'
                                url='{WsAddress}{"api/logme"}'
                                protocol='HttpPost'
                                encoding='UTF-8'
                               >
                            <parameter name='param1' type='System.String' layout='${{message}}'/> 
                            <parameter name='param2' type='System.String' layout='${{level}}'/>
     
                        </target>
                    </targets>
                    <rules>
                      <logger name='*' writeTo='ws'>
                       
                      </logger>
                    </rules>
                </nlog>");


            LogManager.Configuration = configuration;
            var logger = LogManager.GetCurrentClassLogger();

            LogMeController.ResetState(1);



            LogMeController.ResetState(2);

            var message1 = "message 1 with a post";
            var message2 = "a b c é k è ï ?";
            StartOwinTest(() =>
            {

                logger.Info(message1);
                logger.Info(message2);
            });

            Assert.Equal(0, LogMeController.CountdownEvent.CurrentCount);
            Assert.Equal(2, LogMeController.ReceivedLogsPostParam1.Count);
            CheckQueueMessage(message1, LogMeController.ReceivedLogsPostParam1);
            CheckQueueMessage(message2, LogMeController.ReceivedLogsPostParam1);
        }

        /// <summary>
        /// Test the Webservice with REST api -  <see cref="WebServiceProtocol.HttpGet"/>  (only checking for no exception)
        /// </summary>
        [Fact]
        public void WebserviceTest_restapi_httpget()
        {
            var logger = SetUpHttpGetWebservice("api/logme");

            LogMeController.ResetState(2);

            var message1 = "message 1 with a post";
            var message2 = "a b c é k è ï ?";
            StartOwinTest(() =>
            {

                logger.Info(message1);
                logger.Info(message2);
            });


            Assert.Equal(0, LogMeController.CountdownEvent.CurrentCount);
            Assert.Equal(2, LogMeController.ReceivedLogsGetParam1.Count);
            CheckQueueMessage(message1, LogMeController.ReceivedLogsGetParam1);
            CheckQueueMessage(message2, LogMeController.ReceivedLogsGetParam1);
        }

        /// <summary>
        /// Test the Webservice with REST api -  <see cref="WebServiceProtocol.HttpGet"/>  (only checking for no exception)
        /// </summary>
        [Fact]
        public void WebserviceTest_restapi_httpget_flush()
        {
            var logger = SetUpHttpGetWebservice("api/logme");

            LogMeController.ResetState(0);

            var message1 = "message with a post";
            StartOwinTest(() =>
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
                LogManager.Flush(); // Waits for flush (Scheduled on top of the previous flush)
                LogManager.Flush(); // Nothing to flush
            });

            Assert.Equal(100, LogMeController.ReceivedLogsGetParam1.Count);
        }

        [Fact]
        public void WebServiceTest_restapi_httpget_querystring()
        {
            var logger = SetUpHttpGetWebservice("api/logme?paramFromConfig=valueFromConfig");

            LogMeController.ResetState(1);

            StartOwinTest(() =>
            {

                logger.Info("another message");
            });


            Assert.Equal(0, LogMeController.CountdownEvent.CurrentCount);
            Assert.Single(LogMeController.ReceivedLogsGetParam1);
            CheckQueueMessage("another message", LogMeController.ReceivedLogsGetParam1);
        }

        private static Logger SetUpHttpGetWebservice(string relativeUrl)
        {
            var configuration = XmlLoggingConfiguration.CreateFromXmlString($@"
                <nlog throwExceptions='true' >
                    <targets>
                        <target type='WebService'
                                name='ws'
                                url='{WsAddress}{relativeUrl}'
                                protocol='HttpGet'
                                encoding='UTF-8'
                               >
                            <parameter name='param1' type='System.String' layout='${{message}}'/> 
                            <parameter name='param2' type='System.String' layout='${{level}}'/>
     
                        </target>
                    </targets>
                    <rules>
                      <logger name='*' writeTo='ws'>
                       
                      </logger>
                    </rules>
                </nlog>");


            LogManager.Configuration = configuration;
            var logger = LogManager.GetCurrentClassLogger();
            return logger;
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
            var configuration = XmlLoggingConfiguration.CreateFromXmlString($@"
                <nlog throwExceptions='true'>
                    <targets>
                        <target type='WebService'
                                name='ws'
                                url='{WsAddress}{"api/logme"}'
                                protocol='HttpPost'
                                encoding='UTF-8'
                               >
                            <parameter name='param1' type='System.String' layout='${{message}}'/> 
                            <parameter name='param2' type='System.String' layout='${{level}}'/>
     
                        </target>
                    </targets>
                    <rules>
                      <logger name='*' writeTo='ws'>
                       
                      </logger>
                    </rules>
                </nlog>");


            LogManager.Configuration = configuration;
            var logger = LogManager.GetCurrentClassLogger();

            RetryingIntegrationTest(3, () =>
            {

                const int messageCount = 1000;
                var createdMessages = new List<string>(messageCount);

                for (int i = 0; i < messageCount; i++)
                {
                    var message = "message " + i;
                    createdMessages.Add(message);

                }

                //reset
                LogMeController.ResetState(messageCount);

                StartOwinTest(() =>
                {
                    foreach (var createdMessage in createdMessages)
                    {
                        logger.Info(createdMessage);
                    }
                });

                Assert.Equal(0, LogMeController.CountdownEvent.CurrentCount);
                Assert.Equal(createdMessages.Count, LogMeController.ReceivedLogsPostParam1.Count);
                //Assert.Equal(createdMessages, ValuesController.ReceivedLogsPostParam1);
            });
        }

        /// <summary>
        /// Test the Webservice with REST api - <see cref="WebServiceProtocol.JsonPost"/>
        /// </summary>
        [Fact]
        public void WebserviceTest_restapi_json()
        {
            var configuration = XmlLoggingConfiguration.CreateFromXmlString($@"
                <nlog throwExceptions='true'>
                    <targets>
                        <target type='WebService'
                                name='ws'
                                url='{getWsAddress(1)}{"api/logdoc/json"}'
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
                      <logger name='*' writeTo='ws'>
                       
                      </logger>
                    </rules>
                </nlog>");


            LogManager.Configuration = configuration;
            var logger = LogManager.GetCurrentClassLogger();

            var txt = "message 1 with a JSON POST<hello><again\\>\"\b";   // Lets tease the JSON serializer and see it can handle valid and invalid xml chars
            var count = 101;
            var context = new LogDocController.TestContext(1, count, false, new Dictionary<string, string>() { { "Authorization", "OpenBackDoor" } }, txt, "info", true, DateTime.UtcNow);

            StartOwinDocTest(context, () =>
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
            var configuration = XmlLoggingConfiguration.CreateFromXmlString($@"
                <nlog throwExceptions='true'>
                    <targets>
                        <target type='WebService'
                                name='ws'
                                url='{getWsAddress(1)}{"api/logdoc/xml"}'
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
                      <logger name='*' writeTo='ws'>
                       
                      </logger>
                    </rules>
                </nlog>");


            LogManager.Configuration = configuration;
            var logger = LogManager.GetCurrentClassLogger();

            var txt = "message 1 with a XML POST<hello><again\\>\"";   // Lets tease the Xml-Serializer, and see it can handle xml-tags
            var count = 101;
            var context = new LogDocController.TestContext(1, count, true, null, txt, "info", true, DateTime.UtcNow);

            StartOwinDocTest(context, () =>
            {
                for (int i = 0; i < count; i++)
                    logger.Info(txt + "\b");    // Lets tease the Xml-Serializer, and see it can remove invalid chars
            });

            Assert.Equal<int>(0, context.CountdownEvent.CurrentCount);
        }

        /// <summary>
        /// Test the Webservice with Soap11 api - <see cref="WebServiceProtocol.Soap11"/> 
        /// </summary>
        [Fact]
        public void WebserviceTest_soap11_default_soapaction()
        {
            var configuration = XmlLoggingConfiguration.CreateFromXmlString($@"
                <nlog throwExceptions='true'>
                    <targets>
                        <target type='WebService'
                                name='ws'
                                url='{getWsAddress(1)}{"api/logdoc/soap11"}'
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
                </nlog>");


            LogManager.Configuration = configuration;
            var logger = LogManager.GetCurrentClassLogger();

            var txt = "test.message";   // Lets tease the Xml-Serializer, and see it can handle xml-tags
            var count = 1;
            var expectedHeaders = new Dictionary<string, string>
            {
                {"SOAPAction", "http://tempuri.org/Ping" }
            };
            var context = new LogDocController.TestContext(1, count, true, expectedHeaders, null, null, true, DateTime.UtcNow);

            StartOwinDocTest(context, () =>
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
            var configuration = XmlLoggingConfiguration.CreateFromXmlString($@"
                <nlog throwExceptions='true'>
                    <targets>
                        <target type='WebService'
                                name='ws'
                                url='{getWsAddress(1)}{"api/logdoc/soap11"}'
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
                </nlog>");

            LogManager.Configuration = configuration;
            var logger = LogManager.GetCurrentClassLogger();

            var txt = "test.message";   // Lets tease the Xml-Serializer, and see it can handle xml-tags
            var count = 1;
            var expectedHeaders = new Dictionary<string, string>
            {
                {"SOAPAction", "http://tempuri.org/custom-namespace/Ping" }
            };
            var context = new LogDocController.TestContext(1, count, true, expectedHeaders, null, null, true, DateTime.UtcNow);

            StartOwinDocTest(context, () =>
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
            var configuration = XmlLoggingConfiguration.CreateFromXmlString($@"
                <nlog throwExceptions='true'>
                    <targets>
                        <target type='WebService'
                                name='ws'
                                url='{getWsAddress(1)}{"api/logdoc/soap12"}'
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
                </nlog>");


            LogManager.Configuration = configuration;
            var logger = LogManager.GetCurrentClassLogger();

            var txt = "test.message";   // Lets tease the Xml-Serializer, and see it can handle xml-tags
            var count = 1;
            var contentType = MediaTypeHeaderValue.Parse("application/soap+xml;charset=utf-8;action=\"http://tempuri.org/Ping\"");
            var context = new LogDocController.TestContext(1, count, true, null, null, null, true, DateTime.UtcNow, contentType);

            StartOwinDocTest(context, () =>
            {
                logger.Info(txt);
            });

            Assert.Equal<int>(0, context.CountdownEvent.CurrentCount);
        }

        /// <summary>
        /// Start/config route of WS
        /// </summary>
        private class Startup
        {
            // This code configures Web API. The Startup class is specified as a type
            // parameter in the WebApp.Start method.
            public void Configuration(IAppBuilder appBuilder)
            {
                // Configure Web API for self-host. 
                HttpConfiguration config = new HttpConfiguration();
                config.Routes.MapHttpRoute(
                    name: "DefaultApi",
                    routeTemplate: "api/{controller}/{id}",
                    defaults: new { id = RouteParameter.Optional }
                );

                appBuilder.UseWebApi(config);
            }
        }

        ///<remarks>Must be public </remarks>
        public class LogMeController : ApiController
        {
            /// <summary>
            /// Reset the state for unit testing
            /// </summary>
            /// <param name="expectedMessages"></param>
            public static void ResetState(int expectedMessages)
            {
                ReceivedLogsPostParam1 = new ConcurrentBag<string>();
                ReceivedLogsGetParam1 = new ConcurrentBag<string>();
                if (expectedMessages > 0)
                    CountdownEvent = new CountdownEvent(expectedMessages);
                else
                    CountdownEvent = null;
            }

            /// <summary>
            /// Countdown event for keeping WS alive.
            /// </summary>
            public static CountdownEvent CountdownEvent;


            /// <summary>
            /// Received param1 values (get)
            /// </summary>
            public static ConcurrentBag<string> ReceivedLogsGetParam1 = new ConcurrentBag<string>();
            /// <summary>
            /// Received param1 values(post)
            /// </summary>
            public static ConcurrentBag<string> ReceivedLogsPostParam1 = new ConcurrentBag<string>();


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

                ReceivedLogsGetParam1.Add(param1);
                if (CountdownEvent != null)
                {
                    CountdownEvent.Signal();
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
                ReceivedLogsPostParam1.Add(complexType.Param1);

                if (CountdownEvent != null)
                {
                    CountdownEvent.Signal();
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
        }

        internal static void StartOwinTest(Action testsFunc)
        {
            // HttpSelfHostConfiguration. So info: http://www.asp.net/web-api/overview/hosting-aspnet-web-api/use-owin-to-self-host-web-api

            // Start webservice 
            using (WebApp.Start<Startup>(url: WsAddress))
            {
                testsFunc();

                //wait for all received message, or timeout. There is no exception on timeout, so we have to check carefully in the unit test.
                if (LogMeController.CountdownEvent != null)
                {
                    LogMeController.CountdownEvent.Wait(webserviceCheckTimeoutMs);
                    //we need some extra time for completion
                    Thread.Sleep(1000);
                }
            }
        }


        internal static void StartOwinDocTest(LogDocController.TestContext testContext, Action testsFunc)
        {
            var stu = new StartupDoc(testContext);
            using (WebApp.Start(getWsAddress(testContext.PortOffset), stu.Configuration))
            {
                testsFunc();

                if (testContext.CountdownEvent != null)
                {
                    testContext.CountdownEvent.Wait(webserviceCheckTimeoutMs);
                    Thread.Sleep(1000);
                }
            }
        }

        private class StartupDoc
        {
            LogDocController.TestContext _testContext;

            public StartupDoc(LogDocController.TestContext testContext)
            {
                _testContext = testContext;
            }

            // This code configures Web API. The Startup class is specified as a type
            // parameter in the WebApp.Start method.
            public void Configuration(IAppBuilder appBuilder)
            {
                // Configure Web API for self-host. 
                HttpConfiguration config = new HttpConfiguration();

                config.DependencyResolver = new ControllerResolver(_testContext);

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

                if (_testContext.XmlInsteadOfJson)
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
            }

            private class ControllerResolver : IDependencyResolver, IDependencyScope
            {
                private LogDocController.TestContext _testContext;

                public ControllerResolver(LogDocController.TestContext testContext)
                {
                    _testContext = testContext;
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
                    if (serviceType == typeof(LogDocController))
                    {
                        return new LogDocController() { Context = _testContext };
                    }
                    else
                    {
                        return null;
                    }
                }

                public IEnumerable<object> GetServices(Type serviceType)
                {
                    if (serviceType == typeof(LogDocController))
                    {
                        return new object[] { new LogDocController() { Context = _testContext } };
                    }
                    else
                    {
                        return new object[0];
                    }
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

                public int PortOffset { get; }

                public bool XmlInsteadOfJson { get; }

                public Dictionary<string, string> ExpectedHeaders { get; }

                public string ExpectedParam1 { get; }

                public string ExpectedParam2 { get; }

                public bool ExpectedParam3 { get; }

                public DateTime ExpectedParam4 { get; }

                public MediaTypeHeaderValue ExpectedContentType { get; }

                public TestContext(int portOffset, int expectedMessages, bool xmlInsteadOfJson, Dictionary<string, string> expectedHeaders, string expected1, string expected2, bool expected3, DateTime expected4, MediaTypeHeaderValue expectedContentType = null)
                {
                    CountdownEvent = new CountdownEvent(expectedMessages);
                    PortOffset = portOffset;
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

﻿// 
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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NLog.Internal;
using NLog.Targets;

#if NET4_5
using System.Web.Http;
using Owin;
using Microsoft.Owin.Hosting;
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
            var configuration = CreateConfigurationFromString(@"
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

            Assert.Equal(target.Parameters.Count, 6);

            Assert.Equal(target.Encoding.WebName, "utf-8");

            //async call with mockup stream
            WebRequest webRequest = WebRequest.Create("http://www.test.com");
            var request = (HttpWebRequest)webRequest;
            var streamMock = new StreamMock();

            //event for async testing
            var counterEvent = new ManualResetEvent(false);

            var parameterValues = new object[] { "", "336cec87129942eeabab3d8babceead7", "Debg", "2014-06-26 23:15:14.6348", "TestClient.Program", "Debug" };
            target.DoInvoke(parameterValues, c => counterEvent.Set(), request,
                callback =>
                {
                    var t = new Task(() => { });
                    callback(t);
                    return t;
                },
                result => streamMock);

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
                this.Flush();
                bytes = this.ToArray();
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

#if NET4_5


        const string WsAddress = "http://localhost:9000/";

        /// <summary>
        /// Test the Webservice with REST api - <see cref="WebServiceProtocol.HttpPost"/> (only checking for no exception)
        /// </summary>
        [Fact]
        public void WebserviceTest_restapi_httppost()
        {


            var configuration = CreateConfigurationFromString(string.Format(@"
                <nlog throwExceptions='true'>
                    <targets>
                        <target type='WebService'
                                name='ws'
                                url='{0}{1}'
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
                </nlog>", WsAddress, "api/logme"));


            LogManager.Configuration = configuration;
            var logger = LogManager.GetCurrentClassLogger();

            LogMeController.ResetState(1);



            StartOwinTest(() =>
            {

                logger.Info("message 1 with a post");
            });


        }

        /// <summary>
        /// Test the Webservice with REST api -  <see cref="WebServiceProtocol.HttpGet"/>  (only checking for no exception)
        /// </summary>
        [Fact(Skip = "Not working - ProtocolViolationException - skip for fix later")]
        public void WebserviceTest_restapi_httpget()
        {


            var configuration = CreateConfigurationFromString(string.Format(@"
                <nlog throwExceptions='true' >
                    <targets>
                        <target type='WebService'
                                name='ws'
                                url='{0}{1}'
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
                </nlog>", WsAddress, "api/logme"));


            LogManager.Configuration = configuration;
            var logger = LogManager.GetCurrentClassLogger();

            LogMeController.ResetState(1);

            StartOwinTest(() =>
         {

             logger.Info("message 1 with a post");
         });


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


            var configuration = CreateConfigurationFromString(string.Format(@"
                <nlog throwExceptions='true'>
                    <targets>
                        <target type='WebService'
                                name='ws'
                                url='{0}{1}'
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
                </nlog>", WsAddress, "api/logme"));


            LogManager.Configuration = configuration;
            var logger = LogManager.GetCurrentClassLogger();



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



            Assert.Equal(LogMeController.CountdownEvent.CurrentCount, 0);
            Assert.Equal(createdMessages.Count, LogMeController.RecievedLogsPostParam1.Count);
            //Assert.Equal(createdMessages, ValuesController.RecievedLogsPostParam1);

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

        private const string LogTemplate = "Method: {0}, param1: '{1}', param2: '{2}', body: {3}";

        ///<remarks>Must be public </remarks>
        public class LogMeController : ApiController
        {
            /// <summary>
            /// Reset the state for unit testing
            /// </summary>
            /// <param name="expectedMessages"></param>
            public static void ResetState(int expectedMessages)
            {
                RecievedLogsPostParam1 = new ConcurrentBag<string>();
                RecievedLogsGetParam1 = new ConcurrentBag<string>();
                CountdownEvent = new CountdownEvent(expectedMessages);
            }

            /// <summary>
            /// Countdown event for keeping WS alive.
            /// </summary>
            public static CountdownEvent CountdownEvent = null;


            /// <summary>
            /// Recieved param1 values (get)
            /// </summary>
            public static ConcurrentBag<string> RecievedLogsGetParam1 = new ConcurrentBag<string>();
            /// <summary>
            /// Recieved param1 values(post)
            /// </summary>
            public static ConcurrentBag<string> RecievedLogsPostParam1 = new ConcurrentBag<string>();
      

            /// <summary>
            /// We need a complex type for modelbinding because of content-type: "application/x-www-form-urlencoded" in <see cref="WebServiceTarget"/>
            /// </summary>
            public class ComplexType
            {
                public string Param1 { get; set; }
                public string Param2 { get; set; }
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

                RecievedLogsGetParam1.Add(param1);
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
                    throw new ArgumentNullException("complexType");
                }
                RecievedLogsPostParam1.Add(complexType.Param1);

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

                //wait for all recieved message, or timeout. There is no exception on timeout, so we have to check carefully in the unit test.
                if (LogMeController.CountdownEvent != null)
                {

                    LogMeController.CountdownEvent.Wait(webserviceCheckTimeoutMs);
                    //we need some extra time for completion
                    Thread.Sleep(1000);
                }
            }
        }

#endif
    }

}

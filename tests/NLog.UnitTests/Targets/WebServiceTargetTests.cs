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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin.Hosting;
using NLog.Fluent;
using NLog.Internal;
using NLog.Layouts;
using NLog.Targets;
using Owin;
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
        <parameter name='machinename' type='System.String' layout='${machinename}'/>
    </target>
</targets>
                </nlog>");

            var target = configuration.FindTargetByName("webservice") as WebServiceTarget;
            Assert.NotNull(target);

            Assert.Equal(target.Parameters.Count, 7);

            Assert.Equal(target.Encoding.WebName, "utf-8");

            //async call with mockup stream
            WebRequest webRequest = WebRequest.Create("http://www.test.com");
            var request = (HttpWebRequest)webRequest;
            var streamMock = new StreamMock();

            //event for async testing
            var counterEvent = new CountdownEvent(1);

            var parameterValues = new object[] { "", "336cec87129942eeabab3d8babceead7", "Debg", "2014-06-26 23:15:14.6348", "TestClient.Program", "Debug", "DELL" };
            target.DoInvoke(parameterValues, c => counterEvent.Signal(), request,
                callback =>
                {
                    var t = new Task(() => { });
                    callback(t);
                    return t;
                },
                result => streamMock);

            counterEvent.Wait();

            var bytes = streamMock.bytes;
            var url = streamMock.stringed;

            const string expectedUrl = "empty=&guid=336cec87129942eeabab3d8babceead7&m=Debg&date=2014-06-26+23%3a15%3a14.6348&logger=TestClient.Program&level=Debug&machinename=DELL";
            Assert.Equal(expectedUrl, url);

            Assert.True(bytes.Length > 3);

            //not bom
            var possbleBomBytes = bytes.Take(3);
            if (includeBom)
            {
                Assert.Equal(possbleBomBytes, EncodingHelpers.Utf8BOM);
            }
            else
            {
                Assert.NotEqual(possbleBomBytes, EncodingHelpers.Utf8BOM);
            }

            Assert.Equal(bytes.Length, includeBom ? 143 : 140);
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


        const string ws_address = "http://localhost:9000/";
        [Fact]
        public void TestWebserviceCall_post()
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
                </nlog>", ws_address, "api/values"));


            LogManager.Configuration = configuration;
            var logger = LogManager.GetCurrentClassLogger();

          



            StartOwinTest(() =>
            {

                logger.Info("message 1 with a post");
            });


        }


        [Fact]
        public void TestWebserviceCall_get()
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
                </nlog>", ws_address, "api/values"));


            LogManager.Configuration = configuration;
            var logger = LogManager.GetCurrentClassLogger();

            //            Test Name:	TestWebserviceCall_get
            //Test FullName:	NLog.UnitTests.Targets.WebServiceTargetTests.TestWebserviceCall_get
            //Test Source:	x:\_GitHub\NLog\tests\NLog.UnitTests\Targets\WebServiceTargetTests.cs : line 293
            //Test Outcome:	Failed
            //Test Duration:	0:00:00,463

            //Result Message:	System.Net.ProtocolViolationException : Cannot send a content-body with this verb-type.
            //Result StackTrace:	
            //at System.Net.HttpWebRequest.CheckProtocol(Boolean onRequestStream)
            //   at System.Net.HttpWebRequest.BeginGetRequestStream(AsyncCallback callback, Object state)
            //   at NLog.Targets.WebServiceTarget.<>c__DisplayClass1.<DoInvoke>b__0(AsyncCallback r) in x:\_GitHub\NLog\src\NLog\Targets\WebServiceTarget.cs:line 151
            //   at NLog.Targets.WebServiceTarget.DoInvoke(Object[] parameters, AsyncContinuation continuation, HttpWebRequest request, Func`2 beginFunc, Func`2 getStreamFunc) in x:\_GitHub\NLog\src\NLog\Targets\WebServiceTarget.cs:line 218
            //   at NLog.Targets.WebServiceTarget.DoInvoke(Object[] parameters, AsyncContinuation continuation) in x:\_GitHub\NLog\src\NLog\Targets\WebServiceTarget.cs:line 154
            //   at NLog.Targets.MethodCallTargetBase.Write(AsyncLogEventInfo logEvent) in x:\_GitHub\NLog\src\NLog\Targets\MethodCallTargetBase.cs:line 79
            //   at NLog.Targets.Target.WriteAsyncLogEvent(AsyncLogEventInfo logEvent) in x:\_GitHub\NLog\src\NLog\Targets\Target.cs:line 215
            //   at NLog.LoggerImpl.WriteToTargetWithFilterChain(TargetWithFilterChain targetListHead, LogEventInfo logEvent, AsyncContinuation onException) in x:\_GitHub\NLog\src\NLog\LoggerImpl.cs:line 194
            //   at NLog.LoggerImpl.Write(Type loggerType, TargetWithFilterChain targets, LogEventInfo logEvent, LogFactory factory) in x:\_GitHub\NLog\src\NLog\LoggerImpl.cs:line 97
            //   at NLog.Logger.WriteToTargets(LogLevel level, IFormatProvider formatProvider, String message) in x:\_GitHub\NLog\src\NLog\Logger.cs:line 480
            //   at NLog.Logger.Info(String message) in x:\_GitHub\NLog\src\NLog\Logger1.cs:line 636
            //   at NLog.UnitTests.Targets.WebServiceTargetTests.<>c__DisplayClassc.<TestWebserviceCall_get>b__b() in x:\_GitHub\NLog\tests\NLog.UnitTests\Targets\WebServiceTargetTests.cs:line 326
            //   at NLog.UnitTests.Targets.WebServiceTargetTests.StartOwinTest(Action tests) in x:\_GitHub\NLog\tests\NLog.UnitTests\Targets\WebServiceTargetTests.cs:line 422
            //   at NLog.UnitTests.Targets.WebServiceTargetTests.TestWebserviceCall_get() in x:\_GitHub\NLog\tests\NLog.UnitTests\Targets\WebServiceTargetTests.cs:line 323

            StartOwinTest(() =>
            {

                logger.Info("message 1 with a post");
            });


        }

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
        public class ValuesController : ApiController
        {

            private Logger logger = LogManager.GetLogger("apiLogger");

            // GET api/values 
            public IEnumerable<string> Get(string param1 = "", string param2 = "")
            {

                logger.Info(LogTemplate, "GET", param1, param2, null);

                return new string[] { "value1", "value2" };
            }

            // GET api/values/5 
            public string Get(int id)
            {

                return "value";
            }

            // POST api/values 
            public void Post(string param1 = "", string param2 = "")
            {
                logger.Info(LogTemplate, "POST", param1, param2, null);
            }

            //// POST api/values 
            //public void Post(string param1, [FromBody]string value, string param2 = "")
            //{
            //    logger.Info(LogTemplate, "POST", param1, param2, value);
            //}

            // PUT api/values/5 
            public void Put(int id, [FromBody]string value)
            {
            }

            // DELETE api/values/5 
            public void Delete(int id)
            {
            }
        }

        private static void StartOwinTest(Action tests)
        {
            // HttpSelfHostConfiguration 
            //http://www.asp.net/web-api/overview/hosting-aspnet-web-api/use-owin-to-self-host-web-api




            // Start OWIN host 
            using (WebApp.Start<Startup>(url: ws_address))
            {


                //plain tests for testing call with httpclient
                //HttpClient client = new HttpClient();

                //{
                //    var response = client.GetAsync(ws_address + "api/values").Result;
                //    var result = response.Content.ReadAsStringAsync().Result;
                //}

                //{
                //    var response = client.PostAsync(ws_address + "api/values", new StringContent("abc")).Result;
                //    var result = response.Content.ReadAsStringAsync().Result;
                //}
                tests();
                Thread.Sleep(2000);

                //  Console.WriteLine(response);
                //   var result = response.Content.ReadAsStringAsync().Result;
                //    Console.WriteLine(result);
            }
        }
    }


}

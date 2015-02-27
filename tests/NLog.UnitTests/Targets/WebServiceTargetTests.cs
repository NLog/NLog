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
// THE


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using NLog.Layouts;
using NLog.Targets;
using Xunit;

namespace NLog.UnitTests.Targets
{
    public class WebServiceTargetTests : NLogTestBase
    {
        [Fact]
        public void WebserviceTest_httpget_utf8_default_no_BOM()
        {
            var configuration = CreateConfigurationFromString(@"
                <nlog>
                    <targets>
                        <target name='d1' type='Webservice' protocol='HttpGet'  encoding='utf-8-nobom'  >
                            <parameter layout='Layout' name='a' type='System.Type'/>
                            <parameter layout='Layout' name='b' type='System.Type'/>
                    </target>
                    </targets>
                </nlog>");

            var d1 = configuration.FindTargetByName("d1") as WebServiceTarget;
            
            Assert.Equal(d1.Parameters.Count, 2);
            var ms = d1.CreateHttpRequestUrl(new object[] { "v1", "v2" });
            var url = StreamToString(ms);

            Assert.Equal(url, "a=v1&b=v2");
            Assert.Equal(d1.Encoding.WebName, "utf-8");
            var emptyBom = new byte[] {};
            Assert.Equal(d1.Encoding.GetPreamble(), emptyBom);
            Assert.NotNull(d1);



        }

        [Fact]
        public void WebserviceTest_httpget_utf8_default_BOM()
        {
            var configuration = CreateConfigurationFromString(@"
                <nlog>
                    <targets>
                        <target name='d1' type='Webservice' protocol='HttpGet'  encoding='utf-8'  >
                            <parameter layout='Layout' name='a' type='System.Type'/>
                            <parameter layout='Layout' name='b' type='System.Type'/>
                    </target>
                    </targets>
                </nlog>");

            var d1 = configuration.FindTargetByName("d1") as WebServiceTarget;
            Assert.NotNull(d1);


            
            Assert.Equal(d1.Parameters.Count, 2);
            var ms = d1.CreateHttpRequestUrl(new object[] { "v1", "v2" });
            var url = StreamToString(ms);

            Assert.Equal(url, "a=v1&b=v2");
            Assert.Equal(d1.Encoding.WebName, "utf-8");
            var utf8BOM = new byte[] { 239, 187, 191 };
            Assert.Equal(d1.Encoding.GetPreamble(), utf8BOM);
     



        }

        [Fact]
        public void WebserviceTest_httppost_utf8_default_BOM()
        {
            var configuration = CreateConfigurationFromString(@"
                <nlog>
                    <targets>
                        <target name='d1' type='Webservice' protocol='HttpPost'  encoding='utf-8'  >
                            <parameter layout='Layout' name='a' type='System.Type'/>
                            <parameter layout='Layout' name='b' type='System.Type'/>
                    </target>
                    </targets>
                </nlog>");

            var d1 = configuration.FindTargetByName("d1") as WebServiceTarget;
            Assert.NotNull(d1);
            
            Assert.Equal(d1.Parameters.Count, 2);
            var ms = d1.CreateHttpRequestUrl(new object[] { "v1", "v2" });
            var url = StreamToString(ms);

            Assert.Equal(url, "a=v1&b=v2");
            Assert.Equal(d1.Encoding.WebName, "utf-8");
            var utf8BOM = new byte[] { 239, 187, 191 };
            Assert.Equal(d1.Encoding.GetPreamble(), utf8BOM);
     



        }


        [Fact]
        public void WebserviceTest_httppost_utf8_full()
        {
            var configuration = CreateConfigurationFromString(@"
                <nlog>
<targets>
    <target type='WebService'
            name='webservice'
            url='http://localhost:57953/Home/Foo2'
            protocol='HttpPost'
            encoding='UTF-8-nobom'
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

            var d1 = configuration.FindTargetByName("webservice") as WebServiceTarget;
            Assert.NotNull(d1);
            
            Assert.Equal(d1.Parameters.Count, 7);
            var parameterValues = new object[] { "", "336cec87129942eeabab3d8babceead7", "Debg", "2014-06-26 23:15:14.6348", "TestClient.Program", "Debug", "DELL" };
            var ms = d1.CreateHttpRequestUrl(parameterValues);
            var url = StreamToString(ms);

            var expectedUrl = "empty=&guid=336cec87129942eeabab3d8babceead7&m=Debg&date=2014-06-26+23%3a15%3a14.6348&logger=TestClient.Program&level=Debug&machinename=DELL";
            Assert.Equal(url, expectedUrl);
            Assert.Equal(d1.Encoding.WebName, "utf-8");
            var utf8BOM = new byte[] { 239, 187, 191 };
         /
            //we got BOM bytes here!
            var bytes = ms.ToArray();

            var first3 = bytes.Take(3);
            Assert.NotEqual(first3, utf8BOM);



        }

        private static string StreamToString(Stream s)
        {
            s.Position = 0;
            var sr = new StreamReader(s);
            return sr.ReadToEnd();
        }
    }
}

// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using System.Xml;
using System.Reflection;
using System.IO;
using System.Net;

using NUnit.Framework;

namespace NLog.UnitTests.Web
{
    [TestFixture]
	public class ASPTests : NLogWebTestBase
	{
        [Test]
        public void ContextTest()
        {
            //
            // simulate a POST request, with "formvariable=fv1" POST data,
            // cookie1=abcd Cookie and ?queryparam=1234 query string
            //
            // expect the web server to return a formatted text that
            // uses all layout renderers
            //

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(NLogTestBaseUrl + "context.asp?queryparam=1234");
            request.Headers.Add("Cookie: cookie1=abcd");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            string postDataString = "formvariable=fv1";
            byte[] postData = System.Text.Encoding.ASCII.GetBytes(postDataString);
            request.ContentLength = postData.Length;
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(postData, 0, postData.Length);
            }
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                Console.WriteLine(response.ContentLength);
                Console.WriteLine(response.ContentType);
                byte[] data = new byte[4000];
                int got;

                using (Stream stream = response.GetResponseStream())
                {
                    got = stream.Read(data, 0, data.Length);
                }
                string s = System.Text.Encoding.ASCII.GetString(data, 0, got);
                Assert.AreEqual("id='1234', form='fv1', cookie='abcd', servervariable='/nlogtest/context.asp' item='1234' session='sessionvalue2' app='appvalue2' message", s);
            }
        }
    }
}

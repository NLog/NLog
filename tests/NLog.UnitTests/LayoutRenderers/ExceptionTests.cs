// 
// Copyright (c) 2004-2009 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using System.Globalization;

using NLog;
using NLog.Config;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NLog.UnitTests.LayoutRenderers
{
    [TestClass]
    public class ExceptionTests : NLogTestBase
    {
        [TestMethod]
        public void Test1()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${exception}' />
                    <target name='debug2' type='Debug' layout='${exception:format=stacktrace}' />
                    <target name='debug3' type='Debug' layout='${exception:format=type}' />
                    <target name='debug4' type='Debug' layout='${exception:format=shorttype}' />
                    <target name='debug5' type='Debug' layout='${exception:format=tostring}' />
                    <target name='debug6' type='Debug' layout='${exception:format=message}' />
                    <target name='debug7' type='Debug' layout='${exception:format=method}' />
                    <target name='debug8' type='Debug' layout='${exception:format=message,shorttype:separator=*}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1,debug2,debug3,debug4,debug5,debug6,debug7,debug8' />
                </rules>
            </nlog>");

            LogManager.Configuration = new XmlLoggingConfiguration(doc.DocumentElement, null);

            try
            {
                throw new Exception("Test exception");
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().ErrorException("msg", ex);
                AssertDebugLastMessage("debug1", "Test exception");
                AssertDebugLastMessage("debug2", ex.StackTrace);
                AssertDebugLastMessage("debug3", typeof(Exception).FullName);
                AssertDebugLastMessage("debug4", typeof(Exception).Name);
                AssertDebugLastMessage("debug5", ex.ToString());
                AssertDebugLastMessage("debug6", "Test exception");
                AssertDebugLastMessage("debug7", ex.TargetSite.ToString());
                AssertDebugLastMessage("debug8", "Test exception*Exception");
            }
        }
    }
}
// 
// Copyright (c) 2004,2005 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
// 
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
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

using NLog;
using NLog.Config;

using NUnit.Framework;

namespace NLog.UnitTests.LayoutRenderers
{
    [TestFixture]
	public class ExceptionTests : NLogTestBase
	{
        [Test]
        public void ExceptionTest()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${exception:format=message}' />
                    <target name='debug2' type='Debug' layout='${exception:format=type}' />
                    <target name='debug3' type='Debug' layout='${exception:format=shorttype}' />
                    <target name='debug4' type='Debug' layout='${exception:format=tostring}' />
                    <target name='debug5' type='Debug' layout='${exception:format=stacktrace}' />
                    <target name='debug6' type='Debug' layout='${exception:format=method}' />
                </targets>
                <rules>
                    <logger name='*' minlevel='Debug' appendTo='debug1,debug2,debug3,debug4,debug5,debug6' />
                </rules>
            </nlog>");

            LogManager.Configuration = new XmlLoggingConfiguration(doc.DocumentElement, null);

            try
            {
                throw new ArgumentException("exceptionmsg");
            }
            catch (ArgumentException ex)
            {
                LogManager.GetLogger("d").DebugException("zzz", ex);
                AssertDebugLastMessage("debug1", "exceptionmsg");
                AssertDebugLastMessage("debug2", "System.ArgumentException");
                AssertDebugLastMessage("debug3", "ArgumentException");
                AssertDebugLastMessage("debug4", ex.ToString());
                AssertDebugLastMessage("debug5", ex.StackTrace);
                AssertDebugLastMessage("debug6", System.Reflection.MethodBase.GetCurrentMethod().ToString());
            }
        }
    }
}

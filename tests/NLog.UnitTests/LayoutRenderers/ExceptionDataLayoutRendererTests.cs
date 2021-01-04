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

namespace NLog.UnitTests.LayoutRenderers
{
    using System;
    using System.Collections.Generic;
    using NLog.LayoutRenderers;
    using NLog.Layouts;
    using NLog.Targets;
    using NLog.Internal;
    using Xunit;
    using NLog.Config;

    public class ExceptionDataLayoutRendererTests : NLogTestBase
    {
        private ILogger logger = LogManager.GetLogger("NLog.UnitTests.LayoutRenderer.ExceptionDataLayoutRendererTests");

        [Fact]
        public void ExceptionWithDataItemIsLoggedTest()
        {
            const string exceptionMessage = "Test exception";
            const string exceptionDataKey = "testkey";
            const string exceptionDataValue = "testvalue";

            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${exceptiondata:"+exceptionDataKey+@"}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1' />
                </rules>
            </nlog>");
            Exception ex = new ArgumentException(exceptionMessage);
            ex.Data.Add(exceptionDataKey, exceptionDataValue);

            logger.Error(ex, "msg");
            
            AssertDebugLastMessage("debug1", exceptionDataValue);
        }

        [Fact]
        public void ExceptionWithOutDataIsNotLoggedTest()
        {
            const string exceptionMessage = "Test exception";
            const string exceptionDataKey = "testkey";
            const string exceptionDataValue = "testvalue";

            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${exceptiondata:" + exceptionDataKey + @"}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1' />
                </rules>
            </nlog>");


            Exception ex = new ArgumentException(exceptionMessage);
            logger.Error(ex, "msg");

            Assert.NotEqual(GetDebugLastMessage("debug1"), exceptionDataValue);
        }

        [Fact]
        public void ExceptionUsingSpecifiedParamLogsProperlyTest()
        {
            const string exceptionMessage = "I don't like nullref exception!";
            const string exceptionDataKey = "testkey";
            const string exceptionDataValue = "testvalue";

            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${exceptiondata:item="+exceptionDataKey+@"}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1' />
                </rules>
            </nlog>");


            Exception ex = new ArgumentException(exceptionMessage);
            ex.Data.Add(exceptionDataKey, exceptionDataValue);
            logger.Error(ex);
            AssertDebugLastMessage("debug1", exceptionDataValue);
        }

        [Fact]
        public void BadDataForItemResultsInEmptyValueTest()
        {
            const string exceptionMessage = "I don't like nullref exception!";
            const string exceptionDataKey = "testkey";
            const string exceptionDataValue = "testvalue";
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${exceptiondata:item=@}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1' />
                </rules>
            </nlog>");

            Exception ex = new ArgumentException(exceptionMessage);
            ex.Data.Add(exceptionDataKey, exceptionDataValue);
            logger.Error(ex);
            AssertDebugLastMessage("debug1", "");
        }

        [Fact]
        public void NoDatkeyTest()
        {
            const string exceptionMessage = "I don't like nullref exception!";
            const string exceptionDataKey = "testkey";
            const string exceptionDataValue = "testvalue";
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${exceptiondata:}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1' />
                </rules>
            </nlog>");

            Exception ex = new ArgumentException(exceptionMessage);
            ex.Data.Add(exceptionDataKey, exceptionDataValue);
            logger.Error(ex);
            AssertDebugLastMessage("debug1", "");
        }

        [Fact]
        public void NoDatkeyUingParamTest()
        {
            const string exceptionMessage = "I don't like nullref exception!";
            const string exceptionDataKey = "testkey";
            const string exceptionDataValue = "testvalue";
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${exceptiondata:item=}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1' />
                </rules>
            </nlog>");

            Exception ex = new ArgumentException(exceptionMessage);
            ex.Data.Add(exceptionDataKey, exceptionDataValue);
            logger.Error(ex);
            AssertDebugLastMessage("debug1", "");
        }

        [Fact]
        public void BaseExceptionFlippedTest()
        {
            const string exceptionDataKey = "testkey";
            const string exceptionDataValue = "testvalue";
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${exceptiondata:item=" + exceptionDataKey + @":BaseException=true}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1' />
                </rules>
            </nlog>");

            Exception exceptionToTest;
            try
            {
                try
                {
                    try
                    {
                        var except = new ArgumentException("Inner Exception");
                        except.Data[exceptionDataKey] = exceptionDataValue;
                        throw except;
                    }
                    catch (Exception exception)
                    {
                        throw new System.ArgumentException("Wrapper1", exception);
                    }
                }
                catch (Exception exception)
                {
                    throw new ApplicationException("Wrapper2", exception);
                }
            }
            catch (Exception ex)
            {
                exceptionToTest = ex;
            }

            logger.Fatal(exceptionToTest, "msg");

            AssertDebugLastMessage("debug1", exceptionDataValue);
        }
    }
}
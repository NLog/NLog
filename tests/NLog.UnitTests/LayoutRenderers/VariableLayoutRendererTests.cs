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

#region

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

#endregion

namespace NLog.UnitTests.LayoutRenderers
{
    public class VariableLayoutRendererTests : NLogTestBase
    {
        [Fact]
        public void Var_from_xml()
        {
            CreateConfigFromXml();

            ILogger logger = LogManager.GetLogger("A");

            logger.Debug("msg");
            var lastMessage = GetDebugLastMessage("debug");
            Assert.Equal("msg and admin=realgoodpassword", lastMessage);
        }

        [Fact]
        public void Var_from_xml_and_edit()
        {
            CreateConfigFromXml();

            LogManager.Configuration.Variables["password"] = "123";
            ILogger logger = LogManager.GetLogger("A");

            logger.Debug("msg");
            var lastMessage = GetDebugLastMessage("debug");
            Assert.Equal("msg and admin=123", lastMessage);
        }

        [Fact]
        public void Var_with_layout_renderers()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true'>
    <variable name='user' value='logger=${logger}' />
    <variable name='password' value='realgoodpassword' />
            
                <targets>
                    <target name='debug' type='Debug' layout= '${message} and ${var:user}=${var:password}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            LogManager.Configuration.Variables["password"] = "123";
            ILogger logger = LogManager.GetLogger("A");

            logger.Debug("msg");
            var lastMessage = GetDebugLastMessage("debug");
            Assert.Equal("msg and logger=A=123", lastMessage);
        }


        [Fact]
        public void Var_with_other_var()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true'>
    <variable name='user' value='${var:password}=' />
    <variable name='password' value='realgoodpassword' />
            
                <targets>
                    <target name='debug' type='Debug' layout= '${message} and ${var:user}=${var:password}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            LogManager.Configuration.Variables["password"] = "123";
            ILogger logger = LogManager.GetLogger("A");

            logger.Debug("msg");
            var lastMessage = GetDebugLastMessage("debug");
            Assert.Equal("msg and 123==123", lastMessage);
        }

        [Fact]
        public void Var_from_api()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true'>
           
                <targets>
                    <target name='debug' type='Debug' layout= '${message} and ${var:user}=${var:password}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            LogManager.Configuration.Variables["user"] = "admin";
            LogManager.Configuration.Variables["password"] = "123";
            ILogger logger = LogManager.GetLogger("A");

            logger.Debug("msg");
            var lastMessage = GetDebugLastMessage("debug");
            Assert.Equal("msg and admin=123", lastMessage);
        }

        [Fact]
        public void Var_default()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true'>
    <variable name='user' value='admin' />
 
            
                <targets>
                    <target name='debug' type='Debug' layout= '${message} and ${var:user}=${var:password:default=unknown}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");

            logger.Debug("msg");
            var lastMessage = GetDebugLastMessage("debug");
            Assert.Equal("msg and admin=unknown", lastMessage);
        }

        [Fact]
        public void Var_default_after_clear()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true'>
    <variable name='user' value='admin' />
    <variable name='password' value='realgoodpassword' />
            
                <targets>
                    <target name='debug' type='Debug' layout= '${message} and ${var:user}=${var:password:default=unknown}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");

            LogManager.Configuration.Variables.Remove("password");

            logger.Debug("msg");
            var lastMessage = GetDebugLastMessage("debug");
            Assert.Equal("msg and admin=unknown", lastMessage);
        }

        [Fact]
        public void Var_default_after_set_null()
        {
            CreateConfigFromXml();

            ILogger logger = LogManager.GetLogger("A");

            LogManager.Configuration.Variables["password"] = null;

            logger.Debug("msg");
            var lastMessage = GetDebugLastMessage("debug");
            Assert.Equal("msg and admin=", lastMessage);
        }

        [Fact]
        public void Var_default_after_set_emptyString()
        {
            CreateConfigFromXml();

            ILogger logger = LogManager.GetLogger("A");

            LogManager.Configuration.Variables["password"] = "";

            logger.Debug("msg");
            var lastMessage = GetDebugLastMessage("debug");
            Assert.Equal("msg and admin=", lastMessage);
        }

        [Fact]
        public void Var_default_after_xml_emptyString()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true'>
    <variable name='user' value='admin' />
    <variable name='password' value='' />
            
                <targets>
                    <target name='debug' type='Debug' layout= '${message} and ${var:user}=${var:password}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");

            logger.Debug("msg");
            var lastMessage = GetDebugLastMessage("debug");
            Assert.Equal("msg and admin=", lastMessage);
        }

        private void CreateConfigFromXml()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true'>
    <variable name='user' value='admin' />
    <variable name='password' value='realgoodpassword' />
            
                <targets>
                    <target name='debug' type='Debug' layout= '${message} and ${var:user}=${var:password}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");
        }
    }
}
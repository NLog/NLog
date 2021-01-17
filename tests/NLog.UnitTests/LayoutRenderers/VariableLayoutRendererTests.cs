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
using System.IO;
using System.Linq;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using Xunit;

namespace NLog.UnitTests.LayoutRenderers
{
    public class VariableLayoutRendererTests : NLogTestBase
    {
        [Fact]
        public void Var_from_xml()
        {
            // Arrange
            var logFactory = CreateConfigFromXml();
            var logger = logFactory.GetLogger("A");

            // Act
            logger.Debug("msg");
            
            // Assert
            logFactory.AssertDebugLastMessage("msg and admin=realgoodpassword");
            Assert.Equal(2, logFactory.Configuration.Variables.Count);
            Assert.Equal(2, logFactory.Configuration.Variables.Keys.Count);
            Assert.Equal(2, logFactory.Configuration.Variables.Values.Count);
            Assert.True(logFactory.Configuration.Variables.ContainsKey("uSeR"));
            Assert.True(logFactory.Configuration.Variables.TryGetValue("passWORD", out var _));
        }

        [Fact]
        public void Var_from_xml_and_edit()
        {
            // Arrange
            var logFactory = CreateConfigFromXml();
            ILogger logger = logFactory.GetLogger("A");

            // Act
            logFactory.Configuration.Variables["password"] = "123";
            logger.Debug("msg");

            // Assert
            logFactory.AssertDebugLastMessage("msg and admin=123");
        }

        [Fact]
        public void Var_from_xml_and_clear()
        {
            // Arrange
            var logFactory = CreateConfigFromXml();
            ILogger logger = logFactory.GetLogger("A");

            // Act
            logFactory.Configuration.Variables.Clear();
            logger.Debug("msg");

            // Assert
            logFactory.AssertDebugLastMessage("msg and =");
        }

        [Fact]
        public void Var_with_layout_renderers()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
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

        [Theory]
        [InlineData("myJson", "${MyJson}")]
        [InlineData("myJson", "${var:myJSON}")]
        public void Var_with_layout(string variableName, string layoutStyle)
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString($@"
<nlog throwExceptions='true'>
    <variable name='{variableName}'  >
        <layout type='JsonLayout'>
            <attribute name='short date' layout='${{level}}' />
            <attribute name='message' layout='${{message}}' />
        </layout>
    </variable>
            
                <targets>
                    <target name='debug' type='Debug' layout='{layoutStyle}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");

            logger.Debug("msg");
            var lastMessage = GetDebugLastMessage("debug");
            Assert.Equal("{ \"short date\": \"Debug\", \"message\": \"msg\" }", lastMessage);
        }

        [Fact]
        public void Var_in_file_target()
        {
            string folderPath = Path.GetTempPath();
            string logFilePath = Path.Combine(folderPath, "test.log");

            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString($@"
            <nlog>
                <variable name='dir' value='{folderPath}' />
                <targets>
                    <target name='f' type='file' fileName='${{var:dir}}/test.log' layout='${{message}}' lineEnding='LF' />
                </targets>
                <rules>
                    <logger name='*' writeTo='f' />
                </rules>
            </nlog>");
            try
            {
                LogManager.GetLogger("A").Debug("msg");

                Assert.True(File.Exists(logFilePath), "Log file was not created at expected file path.");
                Assert.Equal("msg\n", File.ReadAllText(logFilePath));
            }
            finally
            {
                File.Delete(logFilePath);
            }
        }

        [Fact]
        public void Var_Layout_Target_CallSite()
        {

            var logFactory = new LogFactory().Setup()
                .LoadConfigurationFromXml(@"<nlog throwExceptions='true'>
                    <variable name='myvar' value='${callsite}' />
                    <targets>
                        <target name='debug' type='Debug' layout='${var:myvar}' />
                    </targets>
                    <rules>
                        <logger name='*' minLevel='Debug' writeTo='debug' />
                    </rules>
                </nlog>").LogFactory;

            // Act
            logFactory.GetCurrentClassLogger().Info("Hello");

            // Assert
            logFactory.AssertDebugLastMessage(GetType().ToString() + "." + nameof(Var_Layout_Target_CallSite));
        }

        [Fact]
        public void Var_Layout_Target_ThreadSafe()
        {
            int checkThreadSafe = 1;

            var logFactory = new LogFactory().Setup()
                .SetupExtensions(ext => ext.RegisterLayoutRenderer("naked-runner", (evt) =>
                {
                    var orgValue = System.Threading.Interlocked.Exchange(ref checkThreadSafe, 0);
                    if (orgValue == 0)
                        throw new InvalidOperationException("Running naked in the woods");

                    System.Threading.Thread.Sleep(10);
                    System.Threading.Interlocked.Exchange(ref checkThreadSafe, orgValue);
                    return "Running safely";
                }))
                .LoadConfigurationFromXml(@"<nlog throwExceptions='true'>
                    <variable name='myvar' value='Hello' />
                    <targets>
                        <target name='debug1' type='Memory' layout='${logger}|${message}|${var:myvar}' />
                        <target name='debug2' type='Memory' layout='${logger}|${message}|${var:myvar}' />
                    </targets>
                    <rules>
                        <logger name='d1' minLevel='Debug' writeTo='debug1' />
                        <logger name='d2' minLevel='Debug' writeTo='debug2' />
                    </rules>
                </nlog>")
                .LoadConfiguration(cfg => cfg.Configuration.Variables["myvar"] = new SimpleLayout("${naked-runner}", cfg.LogFactory.ServiceRepository.ConfigurationItemFactory)).LogFactory;

            // Act
            int expectedLogCount = 100;
            var manualEvent = new System.Threading.ManualResetEvent(false);
            Action<Logger> runnerMethod = (logger) =>
            {
                manualEvent.WaitOne();
                for (int i = 0; i < expectedLogCount / 2; ++i)
                    logger.Info("Test {0}", i);
            };

            var logger1 = logFactory.GetLogger("d1");
            var thread1 = new System.Threading.Thread((s) => runnerMethod((Logger)s)) { Name = logger1.Name, IsBackground = true };
            thread1.Start(logger1);

            var logger2 = logFactory.GetLogger("d2");
            var thread2 = new System.Threading.Thread((s) => runnerMethod((Logger)s)) { Name = logger2.Name, IsBackground = true };
            thread2.Start(logger2);

            manualEvent.Set();
            thread1.Join();
            thread2.Join();

            // Assert
            Assert.Equal(expectedLogCount, logFactory.Configuration.AllTargets.OfType<MemoryTarget>().Sum(m => m.Logs.Count));
        }

        [Fact]
        public void Var_with_other_var()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
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
            // LogManager.ReconfigExistingLoggers();
            logger.Debug("msg");
            var lastMessage = GetDebugLastMessage("debug");
            Assert.Equal("msg and 123==123", lastMessage);
        }

        [Fact]
        public void Var_from_api()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
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
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
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
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
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
            // Arrange
            var logFactory = CreateConfigFromXml();
            ILogger logger = logFactory.GetLogger("A");

            // Act
            logFactory.Configuration.Variables["password"] = null;
            logger.Debug("msg");

            // Assert
            logFactory.AssertDebugLastMessage("msg and admin=");
        }

        [Fact]
        public void Var_default_after_set_emptyString()
        {
            // Arrange
            var logFactory = CreateConfigFromXml();
            ILogger logger = logFactory.GetLogger("A");

            // Act
            logFactory.Configuration.Variables["password"] = "";
            logger.Debug("msg");

            // Assert
            logFactory.AssertDebugLastMessage("msg and admin=");
        }

        [Fact]
        public void Var_default_after_xml_emptyString()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
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

        [Fact]
        public void null_should_be_ok()
        {
            Layout l = "${var:var1}";
            var config = new LoggingConfiguration();
            config.Variables["var1"] = null;
            l.Initialize(config);
            var result = l.Render(LogEventInfo.CreateNullEvent());
            Assert.Equal("", result);
        }

        [Fact]
        public void null_should_not_use_default()
        {
            Layout l = "${var:var1:default=x}";
            var config = new LoggingConfiguration();
            config.Variables["var1"] = null;
            l.Initialize(config);
            var result = l.Render(LogEventInfo.CreateNullEvent());
            Assert.Equal("", result);
        }

        [Fact]
        public void notset_should_use_default()
        {
            Layout l = "${var:var1:default=x}";
            var config = new LoggingConfiguration();
            l.Initialize(config);
            var result = l.Render(LogEventInfo.CreateNullEvent());
            Assert.Equal("x", result);
        }
        [Fact]
        public void test_with_mockLogManager()
        {
            LogFactory logFactory = new LogFactory();
            var logConfig = new LoggingConfiguration();
            var debugTarget = new DebugTarget
            {
                Name = "t1",
                Layout = "${message}|${var:var1:default=x}"
            };
            logConfig.AddRuleForAllLevels(debugTarget);
            logConfig.Variables["var1"] = "my-mocking-manager";
            logFactory.Configuration = logConfig;

            ILogger logger = logFactory.GetLogger("A");
            logger.Debug("msg");
            Assert.Equal("msg|my-mocking-manager", debugTarget.LastMessage);
        }

        private LogFactory CreateConfigFromXml()
        {
            return new LogFactory().Setup().LoadConfigurationFromXml(@"
<nlog throwExceptions='true'>
    <variable name='user' value='admin' />
    <variable name='password' value='realgoodpassword' />
            
                <targets>
                    <target name='debug' type='Debug' layout= '${message} and ${var:user}=${var:password}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;
        }
    }
}
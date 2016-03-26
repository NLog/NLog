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


#if !SILVERLIGHT

namespace NLog.UnitTests.Targets
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Mail;
    using NLog.Internal;
    using NLog.Layouts;
    using NLog.Targets;
    using Xunit;
	  using System.IO;

    public class MailTargetTests : NLogTestBase
    {
        [Fact]
        public void SimpleEmailTest()
        {
            var mmt = new MockMailTarget
            {
                From = "foo@bar.com",
                To = "bar@foo.com",
                CC = "me@myserver.com;you@yourserver.com",
                Bcc = "foo@myserver.com;bar@yourserver.com",
                Subject = "Hello from NLog",
                SmtpServer = "server1",
                SmtpPort = 27,
                Body = "${level} ${logger} ${message}"
            };

            mmt.Initialize(null);

            var exceptions = new List<Exception>();
            mmt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger", "log message 1").WithContinuation(exceptions.Add));
            Assert.Null(exceptions[0]);

            Assert.Equal(1, mmt.CreatedMocks.Count);

            var mock = mmt.CreatedMocks[0];
            Assert.Equal(1, mock.MessagesSent.Count);
            Assert.Equal("server1", mock.Host);
            Assert.Equal(27, mock.Port);
            Assert.False(mock.EnableSsl);
            Assert.Null(mock.Credentials);

            var msg = mock.MessagesSent[0];
            Assert.Equal("Hello from NLog", msg.Subject);
            Assert.Equal("foo@bar.com", msg.From.Address);
            Assert.Equal(1, msg.To.Count);
            Assert.Equal("bar@foo.com", msg.To[0].Address);
            Assert.Equal(2, msg.CC.Count);
            Assert.Equal("me@myserver.com", msg.CC[0].Address);
            Assert.Equal("you@yourserver.com", msg.CC[1].Address);
            Assert.Equal(2, msg.Bcc.Count);
            Assert.Equal("foo@myserver.com", msg.Bcc[0].Address);
            Assert.Equal("bar@yourserver.com", msg.Bcc[1].Address);
            Assert.Equal(msg.Body, "Info MyLogger log message 1");
        }

        [Fact]
        public void MailTarget_WithNewlineInSubject_SendsMail()
        {
            var mmt = new MockMailTarget
            {
                From = "foo@bar.com",
                To = "bar@foo.com",
                CC = "me@myserver.com;you@yourserver.com",
                Bcc = "foo@myserver.com;bar@yourserver.com",
                Subject = "Hello from NLog\n",
                SmtpServer = "server1",
                SmtpPort = 27,
                Body = "${level} ${logger} ${message}"
            };

            mmt.Initialize(null);

            var exceptions = new List<Exception>();
            mmt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger", "log message 1").WithContinuation(exceptions.Add));
            Assert.Null(exceptions[0]);

            Assert.Equal(1, mmt.CreatedMocks.Count);

            var mock = mmt.CreatedMocks[0];
            Assert.Equal(1, mock.MessagesSent.Count);
            var msg = mock.MessagesSent[0];
        }

        [Fact]
        public void NtlmEmailTest()
        {
            var mmt = new MockMailTarget
            {
                From = "foo@bar.com",
                To = "bar@foo.com",
                SmtpServer = "server1",
                SmtpAuthentication = SmtpAuthenticationMode.Ntlm,
            };

            mmt.Initialize(null);

            var exceptions = new List<Exception>();
            mmt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger", "log message 1").WithContinuation(exceptions.Add));
            Assert.Null(exceptions[0]);

            Assert.Equal(1, mmt.CreatedMocks.Count);

            var mock = mmt.CreatedMocks[0];
            Assert.Equal(CredentialCache.DefaultNetworkCredentials, mock.Credentials);
        }

        [Fact]
        public void BasicAuthEmailTest()
        {
            try
            {
                var mmt = new MockMailTarget
                {
                    From = "foo@bar.com",
                    To = "bar@foo.com",
                    SmtpServer = "server1",
                    SmtpAuthentication = SmtpAuthenticationMode.Basic,
                    SmtpUserName = "${mdc:username}",
                    SmtpPassword = "${mdc:password}",
                };

                mmt.Initialize(null);

                var exceptions = new List<Exception>();
                MappedDiagnosticsContext.Set("username", "u1");
                MappedDiagnosticsContext.Set("password", "p1");
                mmt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger", "log message 1").WithContinuation(exceptions.Add));
                Assert.Null(exceptions[0]);

                Assert.Equal(1, mmt.CreatedMocks.Count);

                var mock = mmt.CreatedMocks[0];
                var credential = mock.Credentials as NetworkCredential;
                Assert.NotNull(credential);
                Assert.Equal("u1", credential.UserName);
                Assert.Equal("p1", credential.Password);
                Assert.Equal(string.Empty, credential.Domain);
            }
            finally
            {
                MappedDiagnosticsContext.Clear();
            }
        }

        [Fact]
        public void CsvLayoutTest()
        {
            var layout = new CsvLayout()
            {
                Delimiter = CsvColumnDelimiterMode.Semicolon,
                WithHeader = true,
                Columns =
                {
                    new CsvColumn("name", "${logger}"),
                    new CsvColumn("level", "${level}"),
                    new CsvColumn("message", "${message}"),
                }
            };

            var mmt = new MockMailTarget
            {
                From = "foo@bar.com",
                To = "bar@foo.com",
                SmtpServer = "server1",
                AddNewLines = true,
                Layout = layout,
            };

            layout.Initialize(null);

            mmt.Initialize(null);

            var exceptions = new List<Exception>();
            mmt.WriteAsyncLogEvents(
                new LogEventInfo(LogLevel.Info, "MyLogger1", "log message 1").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Debug, "MyLogger2", "log message 2").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Error, "MyLogger3", "log message 3").WithContinuation(exceptions.Add));
            Assert.Null(exceptions[0]);

            Assert.Equal(1, mmt.CreatedMocks.Count);

            var mock = mmt.CreatedMocks[0];
            Assert.Equal(1, mock.MessagesSent.Count);
            var msg = mock.MessagesSent[0];
            string expectedBody = "name;level;message\nMyLogger1;Info;log message 1\nMyLogger2;Debug;log message 2\nMyLogger3;Error;log message 3\n";
            Assert.Equal(expectedBody, msg.Body);
        }

        [Fact]
        public void PerMessageServer()
        {
            var mmt = new MockMailTarget
            {
                From = "foo@bar.com",
                To = "bar@foo.com",
                SmtpServer = "${logger}.mydomain.com",
                Body = "${message}",
                AddNewLines = true,
            };

            mmt.Initialize(null);

            var exceptions = new List<Exception>();
            mmt.WriteAsyncLogEvents(
                new LogEventInfo(LogLevel.Info, "MyLogger1", "log message 1").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Debug, "MyLogger2", "log message 2").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Error, "MyLogger1", "log message 3").WithContinuation(exceptions.Add));
            Assert.Null(exceptions[0]);

            // 2 messages are sent, one using MyLogger1.mydomain.com, another using MyLogger2.mydomain.com
            Assert.Equal(2, mmt.CreatedMocks.Count);

            var mock1 = mmt.CreatedMocks[0];
            Assert.Equal("MyLogger1.mydomain.com", mock1.Host);
            Assert.Equal(1, mock1.MessagesSent.Count);

            var msg1 = mock1.MessagesSent[0];
            Assert.Equal("log message 1\nlog message 3\n", msg1.Body);

            var mock2 = mmt.CreatedMocks[1];
            Assert.Equal("MyLogger2.mydomain.com", mock2.Host);
            Assert.Equal(1, mock2.MessagesSent.Count);

            var msg2 = mock2.MessagesSent[0];
            Assert.Equal("log message 2\n", msg2.Body);
        }

        [Fact]
        public void ErrorHandlingTest()
        {
            var mmt = new MockMailTarget
            {
                From = "foo@bar.com",
                To = "bar@foo.com",
                SmtpServer = "${logger}",
                Body = "${message}",
                AddNewLines = true,
            };

            mmt.Initialize(null);

            var exceptions = new List<Exception>();
            var exceptions2 = new List<Exception>();

            mmt.WriteAsyncLogEvents(
                new LogEventInfo(LogLevel.Info, "MyLogger1", "log message 1").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Debug, "ERROR", "log message 2").WithContinuation(exceptions2.Add),
                new LogEventInfo(LogLevel.Error, "MyLogger1", "log message 3").WithContinuation(exceptions.Add));
            Assert.Null(exceptions[0]);
            Assert.Null(exceptions[1]);

            Assert.NotNull(exceptions2[0]);
            Assert.Equal("Some SMTP error.", exceptions2[0].Message);

            // 2 messages are sent, one using MyLogger1.mydomain.com, another using MyLogger2.mydomain.com
            Assert.Equal(2, mmt.CreatedMocks.Count);

            var mock1 = mmt.CreatedMocks[0];
            Assert.Equal("MyLogger1", mock1.Host);
            Assert.Equal(1, mock1.MessagesSent.Count);

            var msg1 = mock1.MessagesSent[0];
            Assert.Equal("log message 1\nlog message 3\n", msg1.Body);

            var mock2 = mmt.CreatedMocks[1];
            Assert.Equal("ERROR", mock2.Host);
            Assert.Equal(1, mock2.MessagesSent.Count);

            var msg2 = mock2.MessagesSent[0];
            Assert.Equal("log message 2\n", msg2.Body);
        }

        /// <summary>
        /// Tests that it is possible to user different email address for each log message,
        /// for example by using ${logger}, ${event-context} or any other layout renderer.
        /// </summary>
        [Fact]
        public void PerMessageAddress()
        {
            var mmt = new MockMailTarget
            {
                From = "foo@bar.com",
                To = "${logger}@foo.com",
                Body = "${message}",
                SmtpServer = "server1.mydomain.com",
                AddNewLines = true,
            };

            mmt.Initialize(null);

            var exceptions = new List<Exception>();
            mmt.WriteAsyncLogEvents(
                new LogEventInfo(LogLevel.Info, "MyLogger1", "log message 1").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Debug, "MyLogger2", "log message 2").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Error, "MyLogger1", "log message 3").WithContinuation(exceptions.Add));
            Assert.Null(exceptions[0]);

            // 2 messages are sent, one using MyLogger1.mydomain.com, another using MyLogger2.mydomain.com
            Assert.Equal(2, mmt.CreatedMocks.Count);

            var mock1 = mmt.CreatedMocks[0];
            Assert.Equal(1, mock1.MessagesSent.Count);

            var msg1 = mock1.MessagesSent[0];
            Assert.Equal("MyLogger1@foo.com", msg1.To[0].Address);
            Assert.Equal("log message 1\nlog message 3\n", msg1.Body);

            var mock2 = mmt.CreatedMocks[1];
            Assert.Equal(1, mock2.MessagesSent.Count);

            var msg2 = mock2.MessagesSent[0];
            Assert.Equal("MyLogger2@foo.com", msg2.To[0].Address);
            Assert.Equal("log message 2\n", msg2.Body);
        }

        [Fact]
        public void CustomHeaderAndFooter()
        {
            var mmt = new MockMailTarget
            {
                From = "foo@bar.com",
                To = "bar@foo.com",
                SmtpServer = "server1",
                AddNewLines = true,
                Layout = "${message}",
                Header = "First event: ${logger}",
                Footer = "Last event: ${logger}",
            };

            mmt.Initialize(null);

            var exceptions = new List<Exception>();
            mmt.WriteAsyncLogEvents(
                new LogEventInfo(LogLevel.Info, "MyLogger1", "log message 1").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Debug, "MyLogger2", "log message 2").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Error, "MyLogger3", "log message 3").WithContinuation(exceptions.Add));
            Assert.Null(exceptions[0]);

            Assert.Equal(1, mmt.CreatedMocks.Count);

            var mock = mmt.CreatedMocks[0];
            Assert.Equal(1, mock.MessagesSent.Count);
            var msg = mock.MessagesSent[0];
            string expectedBody = "First event: MyLogger1\nlog message 1\nlog message 2\nlog message 3\nLast event: MyLogger3\n";
            Assert.Equal(expectedBody, msg.Body);
        }

        [Fact]
        public void DefaultSmtpClientTest()
        {
            var mailTarget = new MailTarget();
            var client = mailTarget.CreateSmtpClient();
            Assert.IsType(typeof(MySmtpClient), client);
        }

        [Fact]
        public void ReplaceNewlinesWithBreakInHtmlMail()
        {
            var mmt = new MockMailTarget
            {
                From = "foo@bar.com",
                To = "bar@foo.com",
                Subject = "Hello from NLog",
                SmtpServer = "server1",
                Body = "${level}${newline}${logger}${newline}${message}",
                Html = true,
                ReplaceNewlineWithBrTagInHtml = true
            };

            mmt.Initialize(null);

            var exceptions = new List<Exception>();
            mmt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger", "log message 1").WithContinuation(exceptions.Add));

            var messageSent = mmt.CreatedMocks[0].MessagesSent[0];
            Assert.True(messageSent.IsBodyHtml);
            var lines = messageSent.Body.Split(new[] { "<br/>" }, StringSplitOptions.RemoveEmptyEntries);
            Assert.True(lines.Length == 3);
        }

        [Fact]
        public void NoReplaceNewlinesWithBreakInHtmlMail()
        {
            var mmt = new MockMailTarget
            {
                From = "foo@bar.com",
                To = "bar@foo.com",
                Subject = "Hello from NLog",
                SmtpServer = "server1",
                Body = "${level}${newline}${logger}${newline}${message}",
                Html = true,
                ReplaceNewlineWithBrTagInHtml = false
            };

            mmt.Initialize(null);

            var exceptions = new List<Exception>();
            mmt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger", "log message 1").WithContinuation(exceptions.Add));

            var messageSent = mmt.CreatedMocks[0].MessagesSent[0];
            Assert.True(messageSent.IsBodyHtml);
            var lines = messageSent.Body.Split(new[] {Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            Assert.True(lines.Length == 3);
        }

        [Fact]
        public void MailTarget_WithPriority_SendsMailWithPrioritySet()
        {
            var mmt = new MockMailTarget
            {
                From = "foo@bar.com",
                To = "bar@foo.com",
                Subject = "Hello from NLog",
                SmtpServer = "server1",
                Priority = "high"
            };
            mmt.Initialize(null);

            mmt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger", "log message 1").WithContinuation(_ => { }));

            var messageSent = mmt.CreatedMocks[0].MessagesSent[0];
            Assert.Equal(MailPriority.High, messageSent.Priority);
        }

        [Fact]
        public void MailTarget_WithoutPriority_SendsMailWithNormalPriority()
        {
            var mmt = new MockMailTarget
            {
                From = "foo@bar.com",
                To = "bar@foo.com",
                Subject = "Hello from NLog",
                SmtpServer = "server1",
            };
            mmt.Initialize(null);

            mmt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger", "log message 1").WithContinuation(_ => { }));

            var messageSent = mmt.CreatedMocks[0].MessagesSent[0];
            Assert.Equal(MailPriority.Normal, messageSent.Priority);
        }

        [Fact]
        public void MailTarget_WithInvalidPriority_SendsMailWithNormalPriority()
        {
            var mmt = new MockMailTarget
            {
                From = "foo@bar.com",
                To = "bar@foo.com",
                Subject = "Hello from NLog",
                SmtpServer = "server1",
                Priority = "invalidPriority"
            };
            mmt.Initialize(null);

            mmt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger", "log message 1").WithContinuation(_ => { }));

            var messageSent = mmt.CreatedMocks[0].MessagesSent[0];
            Assert.Equal(MailPriority.Normal, messageSent.Priority);
        }

        [Fact]
        public void MailTarget_WithValidToAndEmptyCC_SendsMail()
        {
            var mmt = new MockMailTarget
            {
                From = "foo@bar.com",
                To = "bar@foo.com",
                CC = "",
                Subject = "Hello from NLog",
                SmtpServer = "server1",
                SmtpPort = 27,
                Body = "${level} ${logger} ${message}",
            };
            mmt.Initialize(null);

            var exceptions = new List<Exception>();
            mmt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger", "log message 1").WithContinuation(exceptions.Add));

            Assert.Null(exceptions[0]);
            Assert.Equal(1, mmt.CreatedMocks.Count);
            Assert.Equal(1, mmt.CreatedMocks[0].MessagesSent.Count);
        }

        [Fact]
        public void MailTarget_WithValidToAndEmptyBcc_SendsMail()
        {
            var mmt = new MockMailTarget
            {
                From = "foo@bar.com",
                To = "bar@foo.com",
                Bcc = "",
                Subject = "Hello from NLog",
                SmtpServer = "server1",
                SmtpPort = 27,
                Body = "${level} ${logger} ${message}",
            };
            mmt.Initialize(null);

            var exceptions = new List<Exception>();
            mmt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger", "log message 1").WithContinuation(exceptions.Add));

            Assert.Null(exceptions[0]);
            Assert.Equal(1, mmt.CreatedMocks.Count);
            Assert.Equal(1, mmt.CreatedMocks[0].MessagesSent.Count);
        }

        [Fact]
        public void MailTarget_WithEmptyTo_ThrowsNLogRuntimeException()
        {
            var mmt = new MockMailTarget
            {
                From = "foo@bar.com",
                To = "",
                Subject = "Hello from NLog",
                SmtpServer = "server1",
                SmtpPort = 27,
                Body = "${level} ${logger} ${message}",
            };
            mmt.Initialize(null);

            var exceptions = new List<Exception>();
            mmt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger", "log message 1").WithContinuation(exceptions.Add));

            Assert.NotNull(exceptions[0]);
            Assert.IsType<NLogRuntimeException>(exceptions[0]);
        }

        [Fact]
        public void MailTarget_WithEmptyFrom_ThrowsNLogRuntimeException()
        {
            var mmt = new MockMailTarget
            {
                From = "",
                To = "foo@bar.com",
                Subject = "Hello from NLog",
                SmtpServer = "server1",
                SmtpPort = 27,
                Body = "${level} ${logger} ${message}"
            };
            mmt.Initialize(null);

            var exceptions = new List<Exception>();
            mmt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger", "log message 1").WithContinuation(exceptions.Add));

            Assert.NotNull(exceptions[0]);
            Assert.IsType<NLogRuntimeException>(exceptions[0]);
        }

        [Fact]
        public void MailTarget_WithEmptySmtpServer_ThrowsNLogRuntimeException()
        {
            var mmt = new MockMailTarget
            {
                From = "bar@bar.com",
                To = "foo@bar.com",
                Subject = "Hello from NLog",
                SmtpServer = "",
                SmtpPort = 27,
                Body = "${level} ${logger} ${message}"
            };
            mmt.Initialize(null);

            var exceptions = new List<Exception>();
            mmt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger", "log message 1").WithContinuation(exceptions.Add));

            Assert.NotNull(exceptions[0]);
            Assert.IsType<NLogRuntimeException>(exceptions[0]);
        }

        [Fact]
        public void MailTargetInitialize_WithoutSpecifiedTo_ThrowsConfigException()
        {
            var mmt = new MockMailTarget
            {
                From = "foo@bar.com",
                Subject = "Hello from NLog",
                SmtpServer = "server1",
                SmtpPort = 27,
                Body = "${level} ${logger} ${message}"
            };
            Assert.Throws<NLogConfigurationException>(() => mmt.Initialize(null));
        }

        [Fact]
        public void MailTargetInitialize_WithoutSpecifiedFrom_ThrowsConfigException()
        {
            var mmt = new MockMailTarget
            {
                To = "foo@bar.com",
                Subject = "Hello from NLog",
                SmtpServer = "server1",
                SmtpPort = 27,
                Body = "${level} ${logger} ${message}"
            };
            Assert.Throws<NLogConfigurationException>(() => mmt.Initialize(null));
        }

        [Fact]
        public void MailTargetInitialize_WithoutSpecifiedSmtpServer_should_not_ThrowsConfigException()
        {
            var mmt = new MockMailTarget
            {
                From = "foo@bar.com",
                To = "bar@bar.com",
                Subject = "Hello from NLog",
                SmtpPort = 27,
                Body = "${level} ${logger} ${message}",
                UseSystemNetMailSettings = true
            };

        }

        [Fact]
        public void MailTargetInitialize_WithoutSpecifiedSmtpServer_ThrowsConfigException_if_UseSystemNetMailSettings()
        {
            var mmt = new MockMailTarget
            {
                From = "foo@bar.com",
                To = "bar@bar.com",
                Subject = "Hello from NLog",
                SmtpPort = 27,
                Body = "${level} ${logger} ${message}",
                UseSystemNetMailSettings = false
            };
            Assert.Throws<NLogConfigurationException>(() => mmt.Initialize(null));
        }



        /// <summary>
        /// Test for https://github.com/NLog/NLog/issues/690
        /// </summary>
        [Fact]
        public void MailTarget_UseSystemNetMailSettings_False_Override_ThrowsNLogRuntimeException_if_DeliveryMethodNotSpecified()
        {
            var inConfigVal = @"C:\config";
            var mmt = new MockMailTarget(inConfigVal)
            {
                From = "foo@bar.com",
                To = "bar@bar.com",
                Subject = "Hello from NLog",
                SmtpPort = 27,
                Body = "${level} ${logger} ${message}",
                PickupDirectoryLocation = @"C:\TEMP",
                UseSystemNetMailSettings = false
            };

            Assert.Throws<NLogRuntimeException>(() => mmt.ConfigureMailClient());
        }

        /// <summary>
        /// Test for https://github.com/NLog/NLog/issues/690
        /// </summary>
        [Fact]
        public void MailTarget_UseSystemNetMailSettings_False_Override_DeliveryMethod_SpecifiedDeliveryMethod()
        {
            var inConfigVal = @"C:\config";
            var mmt = new MockMailTarget(inConfigVal)
            {
                From = "foo@bar.com",
                To = "bar@bar.com",
                Subject = "Hello from NLog",
                SmtpPort = 27,
                Body = "${level} ${logger} ${message}",
                PickupDirectoryLocation = @"C:\TEMP",
                UseSystemNetMailSettings = false,
                DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory
            };
            mmt.ConfigureMailClient();
            Assert.NotEqual(mmt.PickupDirectoryLocation, inConfigVal);
        }

        [Fact]
        public void MailTarget_UseSystemNetMailSettings_True()
        {
            var inConfigVal = @"C:\config";
            var mmt = new MockMailTarget(inConfigVal)
            {
                From = "foo@bar.com",
                To = "bar@bar.com",
                Subject = "Hello from NLog",
                Body = "${level} ${logger} ${message}",
                UseSystemNetMailSettings = true
            };
            mmt.ConfigureMailClient();

            Assert.Equal(mmt.SmtpClientPickUpDirectory, inConfigVal);
        }
    
        [Fact]
        public void MailTarget_UseSystemNetMailSettings_True_WithVirtualPath()
        {
            var inConfigVal = @"~/App_Data/Mail";
            var mmt = new MockMailTarget(inConfigVal)
            {
                From = "foo@bar.com",
                To = "bar@bar.com",
                Subject = "Hello from NLog",
                Body = "${level} ${logger} ${message}",
                UseSystemNetMailSettings = false,
                PickupDirectoryLocation = inConfigVal,
                DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory
            };
            mmt.ConfigureMailClient();
            
            Assert.NotEqual(inConfigVal, mmt.SmtpClientPickUpDirectory);
            var separator = Path.DirectorySeparatorChar;
            Assert.Contains(string.Format("{0}App_Data{0}Mail", separator), mmt.SmtpClientPickUpDirectory);
        }

        [Fact]
        public void MailTarget_WithoutSubject_SendsMessageWithDefaultSubject()
        {
            var mmt = new MockMailTarget
            {
                From = "foo@bar.com",
                To = "bar@bar.com",
                SmtpServer = "server1",
                SmtpPort = 27,
                Body = "${level} ${logger} ${message}"
            };
            mmt.Initialize(null);

            var exceptions = new List<Exception>();
            mmt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger", "log message 1").WithContinuation(exceptions.Add));

            Assert.Null(exceptions[0]);
            Assert.Equal(1, mmt.CreatedMocks.Count);
            var mock = mmt.CreatedMocks[0];
            Assert.Equal(1, mock.MessagesSent.Count);

            Assert.Equal(string.Format("Message from NLog on {0}", Environment.MachineName), mock.MessagesSent[0].Subject);
        }

        public class MockSmtpClient : ISmtpClient
        {
            public MockSmtpClient()
            {
                this.MessagesSent = new List<MailMessage>();
            }

            public SmtpDeliveryMethod DeliveryMethod { get; set; }
            public string Host { get; set; }
            public int Port { get; set; }
            public int Timeout { get; set; }
            public string PickupDirectoryLocation { get; set; }


            public ICredentialsByHost Credentials { get; set; }
            public bool EnableSsl { get; set; }
            public List<MailMessage> MessagesSent { get; private set; }

            public void Send(MailMessage msg)
            {
                if (string.IsNullOrEmpty(this.Host) && string.IsNullOrEmpty(this.PickupDirectoryLocation))
                {
                    throw new InvalidOperationException("[Host/Pickup directory] is null or empty.");
                }
                this.MessagesSent.Add(msg);
                if (Host == "ERROR")
                {
                    throw new InvalidOperationException("Some SMTP error.");
                }
            }
            public void Dispose()
            {
            }
        }

        public class MockMailTarget : MailTarget
        {
            private const string RequiredPropertyIsEmptyFormat = "After the processing of the MailTarget's '{0}' property it appears to be empty. The email message will not be sent.";

            public MockSmtpClient Client;

            public MockMailTarget()
            {
                Client = new MockSmtpClient();
            }

            public MockMailTarget(string configPickUpdirectory)
            {
                Client = new MockSmtpClient
                {
                    PickupDirectoryLocation = configPickUpdirectory
                };

            }


            public List<MockSmtpClient> CreatedMocks = new List<MockSmtpClient>();

            internal override ISmtpClient CreateSmtpClient()
            {
                var client = new MockSmtpClient();

                CreatedMocks.Add(client);

                return client;
            }

            public void ConfigureMailClient()
            {
                if (UseSystemNetMailSettings) return;

                if (this.SmtpServer == null && string.IsNullOrEmpty(this.PickupDirectoryLocation))
                {
                    throw new NLogRuntimeException(string.Format(RequiredPropertyIsEmptyFormat, "SmtpServer/PickupDirectoryLocation"));
                }

                if (this.DeliveryMethod == SmtpDeliveryMethod.Network && this.SmtpServer == null)
                {
                    throw new NLogRuntimeException(string.Format(RequiredPropertyIsEmptyFormat, "SmtpServer"));
                }

                if (this.DeliveryMethod == SmtpDeliveryMethod.SpecifiedPickupDirectory && string.IsNullOrEmpty(this.PickupDirectoryLocation))
                {
                    throw new NLogRuntimeException(string.Format(RequiredPropertyIsEmptyFormat, "PickupDirectoryLocation"));
                }

                if (!string.IsNullOrEmpty(this.PickupDirectoryLocation) && this.DeliveryMethod == SmtpDeliveryMethod.SpecifiedPickupDirectory)
                {
                    Client.PickupDirectoryLocation = ConvertDirectoryLocation(PickupDirectoryLocation);
                }

                Client.DeliveryMethod = this.DeliveryMethod;
            }

            public string SmtpClientPickUpDirectory { get { return Client.PickupDirectoryLocation; } }
        }


    }
}

#endif
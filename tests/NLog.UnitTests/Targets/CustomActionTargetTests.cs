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

namespace NLog.UnitTests.Targets
{
    using NLog.Common;
    using NLog.Layouts;
    using NLog.Targets;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Xunit;

    public class CustomActionTargetTests : IDisposable
    {
        CustomActionTarget target;
        List<Exception> exceptions = new List<Exception>();
        StringWriter internalLogWriter = new StringWriter();
        AsyncContinuation continuation;
        AsyncLogEventInfo logEvent;

        public CustomActionTargetTests()
        {
            InternalLogger.LogWriter = internalLogWriter;
            continuation = e => { if (e != null) exceptions.Add(e); };
            target = new CustomActionTarget()
            {
                Layout = "${logger} ${message}",
                Name = "TestTarget"
            };
            target.Initialize(null);
            logEvent = new LogEventInfo(LogLevel.Info, "Logger1", "message1")
                                            .WithContinuation(continuation);
        }

        [Fact]
        public void SingleActionTest()
        {
            var actionProvider = new StringActionProvider();
            var expetedFormat = "TestTarget_Logger1 message1";
            CustomActionTarget.Register(actionProvider);

            target.WriteAsyncLogEvent(logEvent);

            Assert.Equal(expetedFormat, actionProvider.FormattedEvent);
            Assert.Empty(exceptions);
        }

        [Fact]
        public void ErroredActionTest()
        {
            var actionProvider = new ErroredActionProvider(false);
            var expetedFormat = "Error1_TestTarget";
            CustomActionTarget.Register(actionProvider);

            target.WriteAsyncLogEvent(logEvent);
            string internalLog = internalLogWriter.ToString();

            Assert.Equal(1, exceptions.Count);
            Assert.Equal(expetedFormat, exceptions[0].Message);
            Assert.Contains("Error when performing action on type", internalLog);
            Assert.Contains(expetedFormat, internalLog);
        }

        [Fact]
        public void NothingRegisteredTest()
        {
            var actionProvider = new StringActionProvider();

            target.WriteAsyncLogEvent(logEvent);

            Assert.Null(actionProvider.FormattedEvent);
            Assert.Empty(exceptions);
        }

        [Fact]
        public void UnregisterRegisteredTest()
        {
            var actionProvider = new StringActionProvider();
            CustomActionTarget.Register(actionProvider);
            CustomActionTarget.Unregister(actionProvider);

            target.WriteAsyncLogEvent(logEvent);

            Assert.Null(actionProvider.FormattedEvent);
            Assert.Empty(exceptions);
        }

        [Fact]
        public void SevereErroredActionTest()
        {
            var actionProvider = new ErroredActionProvider(true);
            var expetedFormat = "Error1_TestTarget";
            CustomActionTarget.Register(actionProvider);

            var exception = Assert.Throws<NLogConfigurationException>(() => target.WriteAsyncLogEvent(logEvent));
            Assert.Equal(expetedFormat, exception.Message);
            Assert.Empty(exceptions);
        }

        [Fact]
        public void MultipleProvidersTest()
        {
            var actionProvider1 = new StringActionProvider();
            var actionProvider2 = new ListActionProvider();
            var expetedFormat = "TestTarget_Logger1 message1";
            CustomActionTarget.Register(actionProvider1);
            CustomActionTarget.Register(actionProvider2);

            target.WriteAsyncLogEvent(logEvent);

            Assert.Empty(exceptions);
            Assert.Equal(expetedFormat, actionProvider1.FormattedEvent);
            Assert.Equal(1, actionProvider2.LoggedEvents.Count);
            Assert.Equal(expetedFormat, actionProvider2.LoggedEvents[0]);
        }

        [Fact]
        public void MultipleErroredPrvidersTest()
        {
            var actionProvider1 = new ErroredActionProvider(false, "provider1_");
            var actionProvider2 = new ErroredActionProvider(false, "provider2_");
            CustomActionTarget.Register(actionProvider1);
            CustomActionTarget.Register(actionProvider2);
            var expetedFormat1 = "provider1_Error1_TestTarget";
            var expetedFormat2 = "provider2_Error1_TestTarget";

            target.WriteAsyncLogEvent(logEvent);
            string internalLog = internalLogWriter.ToString();

            Assert.Equal(1, exceptions.Count);
            Assert.Contains("provider", exceptions[0].Message);
            Assert.Contains("Error when performing action on type", internalLog);
            Assert.Contains(expetedFormat1, internalLog);
            Assert.Contains(expetedFormat2, internalLog);
        }

        [Fact]
        public void MultiplePrviderWithErrorTest()
        {
            var actionProvider1 = new StringActionProvider();
            var actionProvider2 = new ErroredActionProvider(false, "provider2_");
            var actionProvider3 = new ListActionProvider();
            var expetedFormat = "TestTarget_Logger1 message1";
            var expetedFormat2 = "provider2_Error1_TestTarget";
            CustomActionTarget.Register(actionProvider1);
            CustomActionTarget.Register(actionProvider2);
            CustomActionTarget.Register(actionProvider3);

            target.WriteAsyncLogEvent(logEvent);
            string internalLog = internalLogWriter.ToString();

            Assert.Equal(expetedFormat, actionProvider1.FormattedEvent);
            
            Assert.Equal(1, exceptions.Count);
            Assert.Contains("provider", exceptions[0].Message);
            Assert.Contains("Error when performing action on type", internalLog);
            Assert.Contains(expetedFormat2, internalLog);

            Assert.Equal(1, actionProvider3.LoggedEvents.Count);
            Assert.Equal(expetedFormat, actionProvider3.LoggedEvents[0]);
        }

        public void Dispose()
        {
            CustomActionTarget.ClearRegistrations();
            internalLogWriter.Dispose();
            exceptions.Clear();
        }

        public class StringActionProvider : IActionProvider
        {
            public string FormattedEvent { get; private set; }

            public void Action(TargetWithLayout target, LogEventInfo logEvent)
            {
                FormattedEvent = target.Name + "_" + target.Layout.Render(logEvent);
            }
        }

        public class ListActionProvider : IActionProvider
        {
            public List<string> LoggedEvents { get; private set; }

            public ListActionProvider()
            {
                LoggedEvents = new List<string>();
            }

            public void Action(TargetWithLayout target, LogEventInfo logEvent)
            {
                LoggedEvents.Add(target.Name + "_" + target.Layout.Render(logEvent));
            }
        }

        public class ErroredActionProvider : IActionProvider
        {
            private bool mustBeRethrown;
            private string name;

            public ErroredActionProvider(bool severe)
                : this(severe, string.Empty)
            { }

            public ErroredActionProvider(bool severe, string proviederName)
            {
                this.mustBeRethrown = severe;
                this.name = proviederName;
            }

            public void Action(TargetWithLayout target, LogEventInfo logEvent)
            {
                string message = name + "Error1_" + target.Name;

                if (this.mustBeRethrown)
                    throw new NLogConfigurationException(message);

                throw new Exception(message);
            }
        }
    }

    
}

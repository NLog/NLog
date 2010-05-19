// 
// Copyright (c) 2004-2010 Jaroslaw Kowalski <jaak@jkowalski.net>
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

namespace NLog.UnitTests.Layouts
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NLog.LayoutRenderers;
    using NLog.LayoutRenderers.Wrappers;
    using NLog.Layouts;
    using NLog.Config;
    using System;
    using System.Text;
    using NLog.Common;
    using System.IO;

    [TestClass]
    public class SimpleLayoutOutputTests : NLogTestBase
    {
        [TestMethod]
        public void VeryLongRendererOutput()
        {
            int stringLength = 100000;

            SimpleLayout l = new string('x', stringLength) + "${message}";
            string output = l.GetFormattedMessage(LogEventInfo.CreateNullEvent());
            Assert.AreEqual(new string('x', stringLength), output);
            string output2 = l.GetFormattedMessage(LogEventInfo.CreateNullEvent());
            Assert.AreEqual(new string('x', stringLength), output);
            Assert.AreNotSame(output, output2);
        }

        [TestMethod]
        public void LayoutRendererThrows()
        {
            NLogFactories nlogFactories = new NLogFactories();
            nlogFactories.LayoutRendererFactory.RegisterDefinition("throwsException", typeof(ThrowsExceptionRenderer));
            
            SimpleLayout l = new SimpleLayout("xx${throwsException}yy", nlogFactories);
            string output = l.GetFormattedMessage(LogEventInfo.CreateNullEvent());
            Assert.AreEqual("xxyy", output);
        }

        [TestMethod]
        public void LayoutRendererThrows2()
        {
            var stringWriter = new StringWriter();
            var oldWriter = InternalLogger.LogWriter;
            var oldLevel = InternalLogger.LogLevel;
            try
            {
                InternalLogger.LogWriter = stringWriter;
                InternalLogger.LogLevel = LogLevel.Warn;
                NLogFactories nlogFactories = new NLogFactories();
                nlogFactories.LayoutRendererFactory.RegisterDefinition("throwsException", typeof(ThrowsExceptionRenderer));

                SimpleLayout l = new SimpleLayout("xx${throwsException:msg1}yy${throwsException:msg2}zz", nlogFactories);
                string output = l.GetFormattedMessage(LogEventInfo.CreateNullEvent());
                Assert.AreEqual("xxyyzz", output);
                var internalLogOutput = stringWriter.ToString();
                Assert.IsTrue(internalLogOutput.IndexOf("msg1") >= 0, internalLogOutput);
                Assert.IsTrue(internalLogOutput.IndexOf("msg2") >= 0, internalLogOutput);
            }
            finally
            {
                InternalLogger.LogWriter = oldWriter;
                InternalLogger.LogLevel = oldLevel;
            }
        }

        public class ThrowsExceptionRenderer : LayoutRenderer
        {
            public ThrowsExceptionRenderer()
            {
                this.Message = "Some message.";
            }

            [RequiredParameter]
            [DefaultParameter]
            public string Message { get; set; }

            protected override void Append(StringBuilder builder, LogEventInfo logEvent)
            {
                throw new InvalidOperationException(this.Message);
            }
        }
    }
}

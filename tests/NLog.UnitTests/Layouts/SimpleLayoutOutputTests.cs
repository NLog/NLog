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

namespace NLog.UnitTests.Layouts
{
    using System;
    using System.Text;
    using NLog.Config;
    using NLog.LayoutRenderers;
    using NLog.Layouts;
    using Xunit;

    public class SimpleLayoutOutputTests : NLogTestBase
    {
        [Fact]
        public void VeryLongRendererOutput()
        {
            int stringLength = 100000;

            SimpleLayout l = new string('x', stringLength) + "${message}";
            string output = l.Render(LogEventInfo.CreateNullEvent());
            Assert.Equal(new string('x', stringLength), output);
            string output2 = l.Render(LogEventInfo.CreateNullEvent());
            Assert.Equal(new string('x', stringLength), output);
            Assert.NotSame(output, output2);
        }

        [Fact]
        public void LayoutRendererThrows()
        {
            using (new NoThrowNLogExceptions())
            {
                ConfigurationItemFactory configurationItemFactory = new ConfigurationItemFactory();
                configurationItemFactory.LayoutRenderers.RegisterDefinition("throwsException", typeof(ThrowsExceptionRenderer));

                SimpleLayout l = new SimpleLayout("xx${throwsException}yy", configurationItemFactory);
                string output = l.Render(LogEventInfo.CreateNullEvent());
                Assert.Equal("xxyy", output);
            }
        }

        [Fact]
        public void SimpleLayoutCachingTest()
        {
            var l = new SimpleLayout("xx${threadid}yy");
            var ev = LogEventInfo.CreateNullEvent();
            string output1 = l.Render(ev);
            string output2 = l.Render(ev);
            Assert.Same(output1, output2);
        }

        [Fact]
        public void SimpleLayoutToStringTest()
        {
            var l = new SimpleLayout("xx${level}yy");
            Assert.Equal("xx${level}yy", l.ToString());

            var l2 = new SimpleLayout(new LayoutRenderer[0], "someFakeText", ConfigurationItemFactory.Default);
            Assert.Equal("someFakeText", l2.ToString());
        }

        [Fact]
        public void LayoutRendererThrows2()
        {
            string internalLogOutput = RunAndCaptureInternalLog(
                () =>
                    {
                        using (new NoThrowNLogExceptions())
                        {
                            ConfigurationItemFactory configurationItemFactory = new ConfigurationItemFactory();
                            configurationItemFactory.LayoutRenderers.RegisterDefinition("throwsException", typeof(ThrowsExceptionRenderer));

                            SimpleLayout l = new SimpleLayout("xx${throwsException:msg1}yy${throwsException:msg2}zz", configurationItemFactory);
                            string output = l.Render(LogEventInfo.CreateNullEvent());
                            Assert.Equal("xxyyzz", output);
                        }
                    },
                    LogLevel.Warn);

            Assert.True(internalLogOutput.IndexOf("msg1") >= 0, internalLogOutput);
            Assert.True(internalLogOutput.IndexOf("msg2") >= 0, internalLogOutput);
        }

        [Fact]
        public void LayoutInitTest1()
        {
            var lr = new MockLayout();
            Assert.Equal(0, lr.InitCount);
            Assert.Equal(0, lr.CloseCount);

            // make sure render will call Init
            lr.Render(LogEventInfo.CreateNullEvent());
            Assert.Equal(1, lr.InitCount);
            Assert.Equal(0, lr.CloseCount);

            lr.Close();
            Assert.Equal(1, lr.InitCount);
            Assert.Equal(1, lr.CloseCount);

            // second call to Close() will be ignored
            lr.Close();
            Assert.Equal(1, lr.InitCount);
            Assert.Equal(1, lr.CloseCount);
        }

        [Fact]
        public void LayoutInitTest2()
        {
            var lr = new MockLayout();
            Assert.Equal(0, lr.InitCount);
            Assert.Equal(0, lr.CloseCount);

            // calls to Close() will be ignored because 
            lr.Close();
            Assert.Equal(0, lr.InitCount);
            Assert.Equal(0, lr.CloseCount);

            lr.Initialize(null);
            Assert.Equal(1, lr.InitCount);

            // make sure render will not call another Init
            lr.Render(LogEventInfo.CreateNullEvent());
            Assert.Equal(1, lr.InitCount);
            Assert.Equal(0, lr.CloseCount);

            lr.Close();
            Assert.Equal(1, lr.InitCount);
            Assert.Equal(1, lr.CloseCount);
        }

        [Fact]
        public void TryGetRawValue_SingleLayoutRender_ShouldGiveRawValue()
        {
            // Arrange
            SimpleLayout l = "${sequenceid}";
            var logEventInfo = LogEventInfo.CreateNullEvent();

            // Act
            var success = l.TryGetRawValue(logEventInfo, out var value);

            // Assert
            Assert.True(success, "success");
            Assert.IsType<int>(value);
            Assert.True((int)value >= 0, "(int)value >= 0");
        }

        [Fact]
        public void TryGetRawValue_MultipleLayoutRender_ShouldGiveNullRawValue()
        {
            // Arrange
            SimpleLayout l = "${sequenceid} ";
            var logEventInfo = LogEventInfo.CreateNullEvent();

            // Act
            var success = l.TryGetRawValue(logEventInfo, out var value);

            // Assert
            Assert.False(success);
            Assert.Null(value);
        }

        [Fact]
        public void TryGetRawValue_MutableLayoutRender_ShouldGiveNullRawValue()
        {
            // Arrange
            SimpleLayout l = "${event-properties:builder}";
            var logEventInfo = LogEventInfo.CreateNullEvent();
            logEventInfo.Properties["builder"] = new StringBuilder("mybuilder");
            l.Precalculate(logEventInfo);

            // Act
            var success = l.TryGetRawValue(logEventInfo, out var value);

            // Assert
            Assert.False(success);
            Assert.Null(value);
        }

        [Fact]
        public void TryGetRawValue_ImmutableLayoutRender_ShouldGiveRawValue()
        {
            // Arrange
            SimpleLayout l = "${event-properties:correlationid}";
            var logEventInfo = LogEventInfo.CreateNullEvent();
            var correlationId = Guid.NewGuid();
            logEventInfo.Properties["correlationid"] = correlationId;
            l.Precalculate(logEventInfo);

            // Act
            var success = l.TryGetRawValue(logEventInfo, out var value);

            // Assert
            Assert.True(success, "success");
            Assert.IsType<Guid>(value);
            Assert.Equal(correlationId, value);
        }

        [Fact]
        public void TryGetRawValue_WhenEmpty_ShouldNotFailWithNullException()
        {
            // Arrange
            SimpleLayout l = "${event-properties:eventId:whenEmpty=0}";
            var logEventInfo = LogEventInfo.CreateNullEvent();
            l.Precalculate(logEventInfo);

            // Act
            var success = l.TryGetRawValue(logEventInfo, out var value);

            // Assert
            Assert.False(success, "Missing EventId");
        }

        public class ThrowsExceptionRenderer : LayoutRenderer
        {
            public ThrowsExceptionRenderer()
            {
                Message = "Some message.";
            }

            [RequiredParameter]
            [DefaultParameter]
            public string Message { get; set; }

            protected override void Append(StringBuilder builder, LogEventInfo logEvent)
            {
                throw new ApplicationException(Message);
            }
        }

        public class MockLayout : Layout
        {
            public int InitCount { get; set; }

            public int CloseCount { get; set; }

            protected override void InitializeLayout()
            {
                base.InitializeLayout();
                InitCount++;
            }

            protected override void CloseLayout()
            {
                base.CloseLayout();
                CloseCount++;
            }

            protected override string GetFormattedMessage(LogEventInfo logEvent)
            {
                return "foo";
            }
        }
    }
}

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

namespace NLog.UnitTests.Layouts
{
    using System;
    using NUnit.Framework;

#if !NUNIT
    using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
    using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
    using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
    using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
    using ExpectedException = Microsoft.VisualStudio.TestTools.UnitTesting.ExpectedExceptionAttribute;
#endif
    using NLog.LayoutRenderers;
    using NLog.LayoutRenderers.Wrappers;
    using NLog.Layouts;

    [TestFixture]
    public class SimpleLayoutParserTests : NLogTestBase
    {
        [Test]
        public void SimpleTest()
        {
            SimpleLayout l = "${message}";
            Assert.AreEqual(1, l.Renderers.Count);
            Assert.IsInstanceOfType(typeof(MessageLayoutRenderer), l.Renderers[0]);
        }

        [Test]
        public void UnclosedTest()
        {
            new SimpleLayout("${message");
        }

        [Test]
        public void SingleParamTest()
        {
            SimpleLayout l = "${mdc:item=AAA}";
            Assert.AreEqual(1, l.Renderers.Count);
            MdcLayoutRenderer mdc = l.Renderers[0] as MdcLayoutRenderer;
            Assert.IsNotNull(mdc);
            Assert.AreEqual("AAA", mdc.Item);
        }

        [Test]
        public void ValueWithColonTest()
        {
            SimpleLayout l = "${mdc:item=AAA\\:}";
            Assert.AreEqual(1, l.Renderers.Count);
            MdcLayoutRenderer mdc = l.Renderers[0] as MdcLayoutRenderer;
            Assert.IsNotNull(mdc);
            Assert.AreEqual("AAA:", mdc.Item);
        }

        [Test]
        public void ValueWithBracketTest()
        {
            SimpleLayout l = "${mdc:item=AAA\\}\\:}";
            Assert.AreEqual("${mdc:item=AAA\\}\\:}", l.Text);
            Assert.AreEqual(1, l.Renderers.Count);
            MdcLayoutRenderer mdc = l.Renderers[0] as MdcLayoutRenderer;
            Assert.IsNotNull(mdc);
            Assert.AreEqual("AAA}:", mdc.Item);
        }

        [Test]
        public void DefaultValueTest()
        {
            SimpleLayout l = "${mdc:BBB}";
            Assert.AreEqual(1, l.Renderers.Count);
            MdcLayoutRenderer mdc = l.Renderers[0] as MdcLayoutRenderer;
            Assert.IsNotNull(mdc);
            Assert.AreEqual("BBB", mdc.Item);
        }

        [Test]
        public void DefaultValueWithBracketTest()
        {
            SimpleLayout l = "${mdc:AAA\\}\\:}";
            Assert.AreEqual(l.Text, "${mdc:AAA\\}\\:}");
            Assert.AreEqual(1, l.Renderers.Count);
            MdcLayoutRenderer mdc = l.Renderers[0] as MdcLayoutRenderer;
            Assert.IsNotNull(mdc);
            Assert.AreEqual("AAA}:", mdc.Item);
        }

        [Test]
        public void DefaultValueWithOtherParametersTest()
        {
            SimpleLayout l = "${exception:message,type:separator=x}";
            Assert.AreEqual(1, l.Renderers.Count);
            ExceptionLayoutRenderer elr = l.Renderers[0] as ExceptionLayoutRenderer;
            Assert.IsNotNull(elr);
            Assert.AreEqual("message,type", elr.Format);
            Assert.AreEqual("x", elr.Separator);
        }

        [Test]
        public void EmptyValueTest()
        {
            SimpleLayout l = "${mdc:item=}";
            Assert.AreEqual(1, l.Renderers.Count);
            MdcLayoutRenderer mdc = l.Renderers[0] as MdcLayoutRenderer;
            Assert.IsNotNull(mdc);
            Assert.AreEqual("", mdc.Item);
        }

        [Test]
        public void NestedLayoutTest()
        {
            SimpleLayout l = "${rot13:inner=${ndc:topFrames=3:separator=x}}";
            Assert.AreEqual(1, l.Renderers.Count);
            var lr = l.Renderers[0] as Rot13LayoutRendererWrapper;
            Assert.IsNotNull(lr);
            var nestedLayout = lr.Inner as SimpleLayout;
            Assert.IsNotNull(nestedLayout);
            Assert.AreEqual("${ndc:topFrames=3:separator=x}", nestedLayout.Text);
            Assert.AreEqual(1, nestedLayout.Renderers.Count);
            var ndcLayoutRenderer = nestedLayout.Renderers[0] as NdcLayoutRenderer;
            Assert.IsNotNull(ndcLayoutRenderer);
            Assert.AreEqual(3, ndcLayoutRenderer.TopFrames);
            Assert.AreEqual("x", ndcLayoutRenderer.Separator);
        }

        [Test]
        public void DoubleNestedLayoutTest()
        {
            SimpleLayout l = "${rot13:inner=${rot13:inner=${ndc:topFrames=3:separator=x}}}";
            Assert.AreEqual(1, l.Renderers.Count);
            var lr = l.Renderers[0] as Rot13LayoutRendererWrapper;
            Assert.IsNotNull(lr);
            var nestedLayout0 = lr.Inner as SimpleLayout;
            Assert.IsNotNull(nestedLayout0);
            Assert.AreEqual("${rot13:inner=${ndc:topFrames=3:separator=x}}", nestedLayout0.Text);
            var innerRot13 = nestedLayout0.Renderers[0] as Rot13LayoutRendererWrapper;
            var nestedLayout = innerRot13.Inner as SimpleLayout;
            Assert.IsNotNull(nestedLayout);
            Assert.AreEqual("${ndc:topFrames=3:separator=x}", nestedLayout.Text);
            Assert.AreEqual(1, nestedLayout.Renderers.Count);
            var ndcLayoutRenderer = nestedLayout.Renderers[0] as NdcLayoutRenderer;
            Assert.IsNotNull(ndcLayoutRenderer);
            Assert.AreEqual(3, ndcLayoutRenderer.TopFrames);
            Assert.AreEqual("x", ndcLayoutRenderer.Separator);
        }

        [Test]
        public void DoubleNestedLayoutWithDefaultLayoutParametersTest()
        {
            SimpleLayout l = "${rot13:${rot13:${ndc:topFrames=3:separator=x}}}";
            Assert.AreEqual(1, l.Renderers.Count);
            var lr = l.Renderers[0] as Rot13LayoutRendererWrapper;
            Assert.IsNotNull(lr);
            var nestedLayout0 = lr.Inner as SimpleLayout;
            Assert.IsNotNull(nestedLayout0);
            Assert.AreEqual("${rot13:${ndc:topFrames=3:separator=x}}", nestedLayout0.Text);
            var innerRot13 = nestedLayout0.Renderers[0] as Rot13LayoutRendererWrapper;
            var nestedLayout = innerRot13.Inner as SimpleLayout;
            Assert.IsNotNull(nestedLayout);
            Assert.AreEqual("${ndc:topFrames=3:separator=x}", nestedLayout.Text);
            Assert.AreEqual(1, nestedLayout.Renderers.Count);
            var ndcLayoutRenderer = nestedLayout.Renderers[0] as NdcLayoutRenderer;
            Assert.IsNotNull(ndcLayoutRenderer);
            Assert.AreEqual(3, ndcLayoutRenderer.TopFrames);
            Assert.AreEqual("x", ndcLayoutRenderer.Separator);
        }

        [Test]
        public void AmbientPropertyTest()
        {
            SimpleLayout l = "${message:padding=10}";
            Assert.AreEqual(1, l.Renderers.Count);
            var pad = l.Renderers[0] as PaddingLayoutRendererWrapper;
            Assert.IsNotNull(pad);
            var message = ((SimpleLayout)pad.Inner).Renderers[0] as MessageLayoutRenderer;
            Assert.IsNotNull(message);
        }

        [Test]
        [ExpectedException(typeof(NLogConfigurationException))]
        public void MissingLayoutRendererTest()
        {
            SimpleLayout l = "${rot13:${foobar}}";
            Assert.IsNull(l);
        }

        [Test]
        public void DoubleAmbientPropertyTest()
        {
            SimpleLayout l = "${message:uppercase=true:padding=10}";
            Assert.AreEqual(1, l.Renderers.Count);
            var upperCase = l.Renderers[0] as UppercaseLayoutRendererWrapper;
            Assert.IsNotNull(upperCase);
            var pad = ((SimpleLayout)upperCase.Inner).Renderers[0] as PaddingLayoutRendererWrapper;
            Assert.IsNotNull(pad);
            var message = ((SimpleLayout)pad.Inner).Renderers[0] as MessageLayoutRenderer;
            Assert.IsNotNull(message);
        }

        [Test]
        public void ReverseDoubleAmbientPropertyTest()
        {
            SimpleLayout l = "${message:padding=10:uppercase=true}";
            Assert.AreEqual(1, l.Renderers.Count);
            var pad = ((SimpleLayout)l).Renderers[0] as PaddingLayoutRendererWrapper;
            Assert.IsNotNull(pad);
            var upperCase = ((SimpleLayout)pad.Inner).Renderers[0] as UppercaseLayoutRendererWrapper;
            Assert.IsNotNull(upperCase);
            var message = ((SimpleLayout)upperCase.Inner).Renderers[0] as MessageLayoutRenderer;
            Assert.IsNotNull(message);
        }

        [Test]
        public void EscapeTest()
        {
            AssertEscapeRoundTrips(string.Empty);
            AssertEscapeRoundTrips("hello ${${}} world!");
            AssertEscapeRoundTrips("hello $");
            AssertEscapeRoundTrips("hello ${");
            AssertEscapeRoundTrips("hello $${{");
            AssertEscapeRoundTrips("hello ${message}");
            AssertEscapeRoundTrips("hello ${${level}}");
            AssertEscapeRoundTrips("hello ${${level}${message}}");
        }

        [Test]
        public void EvaluateTest()
        {
            var logEventInfo = LogEventInfo.CreateNullEvent();
            logEventInfo.Level = LogLevel.Warn;
            Assert.AreEqual("Warn", SimpleLayout.Evaluate("${level}", logEventInfo));
        }

        [Test]
        public void EvaluateTest2()
        {
            Assert.AreEqual("Off", SimpleLayout.Evaluate("${level}"));
            Assert.AreEqual(string.Empty, SimpleLayout.Evaluate("${message}"));
            Assert.AreEqual(string.Empty, SimpleLayout.Evaluate("${logger}"));
        }

        private static void AssertEscapeRoundTrips(string originalString)
        {
            string escapedString = SimpleLayout.Escape(originalString);
            SimpleLayout l = escapedString;
            string renderedString = l.Render(LogEventInfo.CreateNullEvent());
            Assert.AreEqual(originalString, renderedString);
        }
    }
}

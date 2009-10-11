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

using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog.LayoutRenderers;
using NLog.Layouts;

namespace NLog.UnitTests
{
    [TestClass]
    public class LayoutConfigurationTests : NLogTestBase
    {
        [TestMethod]
        public void SimpleTest()
        {
            SimpleLayout l = "${message}";
            Assert.AreEqual(1, l.Renderers.Count);
            Assert.IsInstanceOfType(l.Renderers[0], typeof(MessageLayoutRenderer));
        }

        [TestMethod]
        public void UnclosedTest()
        {
            Layout l = "${message";
        }

        [TestMethod]
        public void SingleParamTest()
        {
            SimpleLayout l = "${mdc:item=AAA}";
            Assert.AreEqual(1, l.Renderers.Count);
            MdcLayoutRenderer mdc = l.Renderers[0] as MdcLayoutRenderer;
            Assert.IsNotNull(mdc);
            Assert.AreEqual("AAA", mdc.Item);
        }

        [TestMethod]
        public void ValueWithColonTest()
        {
            SimpleLayout l = "${mdc:item=AAA\\:}";
            Assert.AreEqual(1, l.Renderers.Count);
            MdcLayoutRenderer mdc = l.Renderers[0] as MdcLayoutRenderer;
            Assert.IsNotNull(mdc);
            Assert.AreEqual("AAA:", mdc.Item);
        }

        [TestMethod]
        public void ValueWithBracketTest()
        {
            SimpleLayout l = "${mdc:item=AAA\\}\\:}";
            Assert.AreEqual("${mdc:item=AAA\\}\\:}", l.Text);
            Assert.AreEqual(1, l.Renderers.Count);
            MdcLayoutRenderer mdc = l.Renderers[0] as MdcLayoutRenderer;
            Assert.IsNotNull(mdc);
            Assert.AreEqual("AAA}:", mdc.Item);
        }

        [TestMethod]
        public void DefaultValueTest()
        {
            SimpleLayout l = "${mdc:BBB}";
            Assert.AreEqual(1, l.Renderers.Count);
            MdcLayoutRenderer mdc = l.Renderers[0] as MdcLayoutRenderer;
            Assert.IsNotNull(mdc);
            Assert.AreEqual("BBB", mdc.Item);
        }

        [TestMethod]
        public void DefaultValueWithBracketTest()
        {
            SimpleLayout l = "${mdc:AAA\\}\\:}";
            Assert.AreEqual(l.Text, "${mdc:AAA\\}\\:}");
            Assert.AreEqual(1, l.Renderers.Count);
            MdcLayoutRenderer mdc = l.Renderers[0] as MdcLayoutRenderer;
            Assert.IsNotNull(mdc);
            Assert.AreEqual("AAA}:", mdc.Item);
        }

        [TestMethod]
        public void DefaultValueWithOtherParametersTest()
        {
            SimpleLayout l = "${exception:message,type:separator=x}";
            Assert.AreEqual(1, l.Renderers.Count);
            ExceptionLayoutRenderer elr = l.Renderers[0] as ExceptionLayoutRenderer;
            Assert.IsNotNull(elr);
            Assert.AreEqual("message,type", elr.Format);
            Assert.AreEqual("x", elr.Separator);
        }

        [TestMethod]
        public void EmptyValueTest()
        {
            SimpleLayout l = "${mdc:item=}";
            Assert.AreEqual(1, l.Renderers.Count);
            MdcLayoutRenderer mdc = l.Renderers[0] as MdcLayoutRenderer;
            Assert.IsNotNull(mdc);
            Assert.AreEqual("", mdc.Item);
        }

        [TestMethod]
        public void NestedLayoutTest()
        {
            SimpleLayout l = "${file-contents:fileName=${basedir:padding=10}/aaa.txt:encoding=iso-8859-1}";
            Assert.AreEqual(1, l.Renderers.Count);
            FileContentsLayoutRenderer lr = l.Renderers[0] as FileContentsLayoutRenderer;
            Assert.IsNotNull(lr);
            Assert.IsInstanceOfType(lr.FileName, typeof(SimpleLayout));
            Assert.AreEqual("${basedir:padding=10}/aaa.txt", ((SimpleLayout)lr.FileName).Text);
            Assert.AreEqual(1, ((SimpleLayout)lr.FileName).Renderers.Count);
            Assert.AreEqual(Encoding.GetEncoding("iso-8859-1"), lr.Encoding);
        }

        [TestMethod]
        public void DoubleNestedLayoutTest()
        {
            SimpleLayout l = "${file-contents:fileName=${basedir}/${file-contents:fileName=${basedir}/aaa.txt}/aaa.txt}";
            Assert.AreEqual(1, l.Renderers.Count);
            FileContentsLayoutRenderer lr = l.Renderers[0] as FileContentsLayoutRenderer;
            Assert.IsNotNull(lr);
            Assert.IsInstanceOfType(lr.FileName, typeof(Layout));
            Assert.AreEqual("${basedir}/${file-contents:fileName=${basedir}/aaa.txt}/aaa.txt", ((SimpleLayout)lr.FileName).Text);
            Assert.AreEqual(3, ((SimpleLayout)lr.FileName).Renderers.Count);
            Assert.IsInstanceOfType(((SimpleLayout)lr.FileName).Renderers[0], typeof(LiteralLayoutRenderer));
            Assert.IsInstanceOfType(((SimpleLayout)lr.FileName).Renderers[1], typeof(FileContentsLayoutRenderer));
            Assert.IsInstanceOfType(((SimpleLayout)lr.FileName).Renderers[2], typeof(LiteralLayoutRenderer));

            LiteralLayoutRenderer lr1 = (LiteralLayoutRenderer)((SimpleLayout)lr.FileName).Renderers[0];
            FileContentsLayoutRenderer fc = (FileContentsLayoutRenderer)((SimpleLayout)lr.FileName).Renderers[1];
            LiteralLayoutRenderer lr2 = (LiteralLayoutRenderer)((SimpleLayout)lr.FileName).Renderers[2];

            Assert.AreEqual("${basedir}/aaa.txt", ((SimpleLayout)fc.FileName).Text);

        }

        [TestMethod]
        public void DoubleNestedLayoutWithDefaultLayoutParametersTest()
        {
            SimpleLayout l = "${file-contents:${basedir}/${file-contents:${basedir}/aaa.txt}/aaa.txt}";
            Assert.AreEqual(1, l.Renderers.Count);
            FileContentsLayoutRenderer lr = l.Renderers[0] as FileContentsLayoutRenderer;
            Assert.IsNotNull(lr);
            Assert.IsInstanceOfType(lr.FileName, typeof(Layout));
            Assert.AreEqual("${basedir}/${file-contents:${basedir}/aaa.txt}/aaa.txt", ((SimpleLayout)lr.FileName).Text);
            Assert.AreEqual(3, ((SimpleLayout)lr.FileName).Renderers.Count);
            Assert.IsInstanceOfType(((SimpleLayout)lr.FileName).Renderers[0], typeof(LiteralLayoutRenderer));
            Assert.IsInstanceOfType(((SimpleLayout)lr.FileName).Renderers[1], typeof(FileContentsLayoutRenderer));
            Assert.IsInstanceOfType(((SimpleLayout)lr.FileName).Renderers[2], typeof(LiteralLayoutRenderer));

            LiteralLayoutRenderer lr1 = (LiteralLayoutRenderer)((SimpleLayout)lr.FileName).Renderers[0];
            FileContentsLayoutRenderer fc = (FileContentsLayoutRenderer)((SimpleLayout)lr.FileName).Renderers[1];
            LiteralLayoutRenderer lr2 = (LiteralLayoutRenderer)((SimpleLayout)lr.FileName).Renderers[2];

            Assert.AreEqual("${basedir}/aaa.txt", ((SimpleLayout)fc.FileName).Text);

        }
    }
}

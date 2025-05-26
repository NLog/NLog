//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.UnitTests.Internal
{
    using System.Linq;
    using NLog.Config;
    using NLog.Internal;
    using Xunit;

    public class XmlParserTests
    {
        [Fact]
        public void XmlConvertIsXmlCharTest()
        {
            for (char ch = '\0'; ch < char.MaxValue; ++ch)
            {
                var expected = System.Xml.XmlConvert.IsXmlChar(ch);
                var actual = XmlHelper.XmlConvertIsXmlChar(ch);
                if (expected != actual)
                    Assert.True(expected == actual, $"{ch} ({(int)ch})");
            }
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\n")]
        [InlineData("\t")]
        [InlineData(">")]
        [InlineData("<")]
        [InlineData(" >")]
        [InlineData(" <")]
        [InlineData(" > ")]
        [InlineData(" < ")]
        [InlineData(" <> ")]
        [InlineData(" < > ")]
        [InlineData(" </> ")]
        [InlineData(" < /> ")]
        [InlineData(" <\n> ")]
        [InlineData(" <? ")]
        [InlineData(" ?> ")]
        [InlineData(" <?> ")]
        [InlineData(" <?/> ")]
        [InlineData(" <? ?> ")]
        [InlineData(" <? ?/> ")]
        [InlineData(" <?xml ><nlog/> ")]
        [InlineData(" <??xml ?><nlog/> ")]
        [InlineData(" <?xml >\n<nlog/> ")]
        [InlineData(" <?xml ?><nlog> ")]
        [InlineData(" <?xml ?>\n<nlog> ")]
        [InlineData(" <?xml ?></xml> ")]
        [InlineData(" <?xml ?>\n</xml> ")]
        [InlineData("n")]
        [InlineData(" n")]
        [InlineData(" n ")]
        [InlineData("nlog")]
        [InlineData("<nlog>")]
        [InlineData("<nlog>\n")]
        [InlineData("</nlog>")]
        [InlineData("\n</nlog>")]
        [InlineData("<nlog><nlog>")]
        [InlineData("<nlog>\n<nlog>")]
        [InlineData("<nlog/><nlog/>")]
        [InlineData("<nlog/>\n<nlog/>")]
        [InlineData("<nlog<nlog/>")]
        [InlineData("<nlog>\nnl<g\n<nlog/>")]
        [InlineData("<nlog>\nnl>g\n<nlog/>")]
        [InlineData("<nlog></nlo")]
        [InlineData("<nlog></nlog")]
        [InlineData("<nlog></NLOG>")]
        [InlineData("<nlog>\n</NLOG>")]
        [InlineData("<NLOG></nlog>")]
        [InlineData("<NLOG>\n</nlog>")]
        [InlineData("<NLOG><NLOG></nlog>")]
        [InlineData("<NLOG>\n<NLOG>\n</nlog>")]
        [InlineData("<NLOG></nlog></nlog>")]
        [InlineData("<NLOG>\n</nlog>\n</nlog>")]
        [InlineData("<NLOG></NLOG></nlog>")]
        [InlineData("<NLOG>\n</NLOG>\n</nlog>")]
        [InlineData("<nlog></nlog><nlog>")]
        [InlineData("<nlog>\n</nlog>\n<nlog>")]
        [InlineData("<nlog></nlog> nlog")]
        [InlineData("<nlog>\n</nlog>\nnlog")]
        [InlineData("nlog<nlog></nlog>")]
        [InlineData("<nlog<nlog></nlog>")]
        [InlineData("<nlog<nlog>></nlog>")]
        [InlineData("<nlog ='true' />")]
        [InlineData("<nlog throwExceptions />")]
        [InlineData("<nlog throwExceptions= />")]
        [InlineData("<nlog throwExceptions=' />")]
        [InlineData("<nlog throwExceptions=\" />")]
        [InlineData("<nlog throw Exceptions='true' />")]
        [InlineData("<nlog throw<Exceptions='true' />")]
        [InlineData("<nlog throw>Exceptions='true' />")]
        [InlineData("<nlog throwExceptions='true\" />")]
        [InlineData("<nlog throwExceptions=\"true' />")]
        [InlineData("<nlog throwExceptions=true />")]
        [InlineData("<nlog throwExceptions='true />")]
        [InlineData("<nlog throwExceptions=\"true />")]
        [InlineData("<nlog throwExceptions='&gt' />")]
        [InlineData("<nlog xsi:internalLogLevel='Debug' throwExceptions='true\" />")]
        [InlineData("<nlog xsi:internalLogLevel='Debug' throwExceptions=\"true' />")]
        [InlineData("<nlog xsi:internalLogLevel='Debug' throwExceptions=true />")]
        [InlineData("<nlog xsi:internalLogLevel='Debug' throwExceptions='true />")]
        [InlineData("<nlog xsi:internalLogLevel='Debug' throwExceptions=\"true />")]
        [InlineData("<nlog xsi:internalLogLevel='Debug' throwExceptions='&gt' />")]
        [InlineData("<nlog><![CDATA[]]]></nlog>")]
        [InlineData("<nlog><![CDATA[]></nlog>")]
        [InlineData("<nlog><![CDATA]></nlog>")]
        [InlineData("<nlog><![CDATA></nlog>")]
        [InlineData("<nlog><!!----></nlog>")]
        [InlineData("<nlog><!----!></nlog>")]
        [InlineData("<nlog><!---></nlog>")]
        [InlineData("<nlog><!--!></nlog>")]
        [InlineData("<nlog><!--></nlog>")]
        [InlineData("<nlog><!-></nlog>")]
        [InlineData("<nlog><!></nlog>")]
        [InlineData("<nlog>&#12Z;</nlog>")]
        [InlineData("<nlog>&#1234567;</nlog>")]
        [InlineData("<nlog>&#x12Z;</nlog>")]
        [InlineData("<nlog>&#xffffff;</nlog>")]
        [InlineData("<nlog>&quop;</nlog>")]
        [InlineData("<nlog>&quot</nlog>")]
        public void XmlParse_InvalidDocument(string xmlSource)
        {
            Assert.Throws<XmlParserException>(() => new XmlParser(xmlSource).LoadDocument(out var _));
        }

        [Theory]
        [InlineData("<nlog/>")]
        [InlineData("<NLOG/>")]
        [InlineData("<nlog />")]
        [InlineData("<NLOG />")]
        [InlineData("  <nlog />  ")]
        [InlineData("  <NLOG />  ")]
        [InlineData("\n\n<nlog />\n\n")]
        [InlineData("\n\n<NLOG />\n\n")]
        [InlineData("<nlog></nlog>")]
        [InlineData("<NLOG></NLOG>")]
        [InlineData("  <nlog></nlog>  ")]
        [InlineData("  <NLOG></NLOG>  ")]
        [InlineData("\n\n<nlog></nlog>\n\n")]
        [InlineData("\n\n<NLOG></NLOG>\n\n")]
        [InlineData("<nlog>  </nlog>")]
        [InlineData("<NLOG>  </NLOG>")]
        [InlineData("<nlog>\n\n</nlog>")]
        [InlineData("<NLOG>\n\n</NLOG>")]
        [InlineData("<!-- Hello --><nlog />")]
        [InlineData("<?xml ?><nlog />")]
        [InlineData("<?xml ?>\n<nlog />")]
        [InlineData("<?xml ?><!-- Hello --><nlog />")]
        [InlineData("<!-- Hello --><?xml ?><!-- World --><nlog />")]
        public void XmlParse_EmptyDocument(string xmlSource)
        {
            var xmlDocument = new XmlParser(xmlSource).LoadDocument(out var _);
            Assert.NotNull(xmlDocument);
            Assert.Equal("nlog", xmlDocument.Name.ToLower());
            Assert.Empty(xmlDocument.Children);
            Assert.Empty(xmlDocument.Attributes);
        }

        [Theory]
        [InlineData(@"<nlog throwExceptions=""false"" />")]
        [InlineData(@"<nlog throwExceptions='false' />")]
        [InlineData("<nlog\nthrowExceptions='false'\n/>")]
        [InlineData(@"<nlog throwExceptions = ""false"" />")]
        [InlineData(@"<nlog throwExceptions = 'false' />")]
        [InlineData("<nlog\nthrowExceptions = 'false'\n/>")]
        [InlineData(@"<nlog throwExceptions=""false""></nlog>")]
        [InlineData(@"<nlog throwExceptions='false'></nlog>")]
        [InlineData(@"<nlog throwExceptions = ""false""></nlog>")]
        [InlineData("<nlog throwExceptions = 'false'></nlog>")]
        [InlineData(@"<nlog throwExceptions = ""false"" ></nlog>")]
        [InlineData("<nlog throwExceptions = 'false' ></nlog>")]
        [InlineData("<nlog\nthrowExceptions = 'false'\n></nlog>")]
        public void XmlParse_Attributes(string xmlSource)
        {
            var xmlDocument = new XmlParser(xmlSource).LoadDocument(out var _);
            Assert.NotNull(xmlDocument);
            Assert.Equal("nlog", xmlDocument.Name);
            Assert.Empty(xmlDocument.Children);
            Assert.Single(xmlDocument.Attributes);
            Assert.Equal("throwExceptions", xmlDocument.Attributes[0].Key);
            Assert.Equal("false", xmlDocument.Attributes[0].Value);
        }

        [Theory]
        [InlineData(@"<nlog throwExceptions=""false"" internalLogLevel=""Debug"" />")]
        [InlineData(@"<nlog throwExceptions='false' internalLogLevel='Debug' />")]
        [InlineData(@"<nlog throwExceptions=""false"" internalLogLevel='Debug' />")]
        [InlineData(@"<nlog throwExceptions='false' internalLogLevel=""Debug"" />")]
        public void XmlParse_Attributes_Multiple(string xmlSource)
        {
            var xmlDocument = new XmlParser(xmlSource).LoadDocument(out var _);
            Assert.NotNull(xmlDocument);
            Assert.Equal("nlog", xmlDocument.Name);
            Assert.Empty(xmlDocument.Children);
            Assert.Equal(2, xmlDocument.Attributes.Count);
            Assert.Equal("throwExceptions", xmlDocument.Attributes[0].Key);
            Assert.Equal("false", xmlDocument.Attributes[0].Value);
            Assert.Equal("internalLogLevel", xmlDocument.Attributes[1].Key);
            Assert.Equal("Debug", xmlDocument.Attributes[1].Value);
        }

        [Theory]
        [InlineData("<nlog internalLogFile='' />", "")]
        [InlineData("<nlog internalLogFile='.\\logfile.txt' />", ".\\logfile.txt")]
        [InlineData("<nlog internalLogFile='C:\\logfile.txt' />", "C:\\logfile.txt")]
        [InlineData("<nlog internalLogFile='./logfile.txt' />", "./logfile.txt")]
        [InlineData("<nlog internalLogFile='./%HOSTNAME%.txt' />", "./%HOSTNAME%.txt")]
        [InlineData("<nlog internalLogFile='http://example.com' />", "http://example.com")]
        [InlineData("<nlog internalLogFile=' < > ? \" & ' />", " < > ? \" & ")]
        [InlineData("<nlog internalLogFile=' < > ? \" &' />", " < > ? \" &")]
        [InlineData("<nlog internalLogFile=' &lt; &gt; ? &quot; &amp; ' />", " < > ? \" & ")]
        [InlineData("<nlog internalLogFile=' &lt; &gt; ? &quot; &amp;' />", " < > ? \" &")]
        [InlineData("<nlog internalLogFile=' &#60; &#62; ? &#34; &#38; ' />", " < > ? \" & ")]
        [InlineData("<nlog internalLogFile=' &#60; &#62; ? &#34; &#38;' />", " < > ? \" &")]
        [InlineData("<nlog internalLogFile=' &#x3c; &#x3e; ? &#x22; &#x26; ' />", " < > ? \" & ")]
        [InlineData("<nlog internalLogFile=' &#x3c; &#x3e; ? &#x22; &#x26;' />", " < > ? \" &")]
        public void XmlParse_Attributes_Tokens(string xmlSource, string value)
        {
            var xmlDocument = new XmlParser(xmlSource).LoadDocument(out var _);
            Assert.NotNull(xmlDocument);
            Assert.Equal("nlog", xmlDocument.Name);
            Assert.Empty(xmlDocument.Children);
            Assert.Single(xmlDocument.Attributes);
            Assert.Equal("internalLogFile", xmlDocument.Attributes[0].Key);
            Assert.Equal(value, xmlDocument.Attributes[0].Value);
        }

        [Theory]
        [InlineData("<nlog></nlog>", null)]
        [InlineData("<nlog> </nlog>", null)]
        [InlineData("<nlog>\n</nlog>", null)]
        [InlineData("<nlog>\n\n</nlog>", null)]
        [InlineData("<nlog>&lt; &gt; ? &quot; &amp;</nlog>", "< > ? \" &")]
        [InlineData("<nlog> &lt; &gt; ? &quot; &amp; </nlog>", "< > ? \" &")]
        [InlineData("<nlog>\n&lt; &gt; ? &quot; &amp;\n</nlog>", "< > ? \" &")]
        [InlineData("<nlog>&#60; &#62; ? &#34; &#38;</nlog>", "< > ? \" &")]
        [InlineData("<nlog> &#60; &#62; ? &#34; &#38; </nlog>", "< > ? \" &")]
        [InlineData("<nlog>\n&#60; &#62; ? &#34; &#38;\n</nlog>", "< > ? \" &")]
        [InlineData("<nlog>&#x3c; &#x3e; ? &#x22; &#x26;</nlog>", "< > ? \" &")]
        [InlineData("<nlog> &#x3c; &#x3e; ? &#x22; &#x26; </nlog>", "< > ? \" &")]
        [InlineData("<nlog>\n&#x3c; &#x3e; ? &#x22; &#x26;\n</nlog>", "< > ? \" &")]
        [InlineData("<nlog><![CDATA[\n\n]]></nlog>", "\n\n")]
        [InlineData("<nlog> <![CDATA[\n\n]]> </nlog>", "\n\n")]
        [InlineData("<nlog>\n<![CDATA[\n\n]]>\n</nlog>", "\n\n")]
        [InlineData("<nlog>\n<![CDATA[<CDATA>]]>\n</nlog>", "<CDATA>")]
        [InlineData("<nlog>\n<!--CDATA-->\n<![CDATA[<CDATA>]]>\n</nlog>", "<CDATA>")]
        [InlineData("<nlog><!--CDATA--><!--CDATA--><![CDATA[<CD]]>A<!--CDATA--><!--CDATA--><![CDATA[TA>]]></nlog>", "<CDATA>")]
        [InlineData("<nlog><![CDATA[<![CDATA[]]>]]<![CDATA[>]]></nlog>", "<![CDATA[]]>")]
        public void XmlParse_InnerText_Tokens(string xmlSource, string value)
        {
            var xmlDocument = new XmlParser(xmlSource).LoadDocument(out var _);
            Assert.NotNull(xmlDocument);
            Assert.Equal("nlog", xmlDocument.Name);
            Assert.Empty(xmlDocument.Children);
            Assert.Empty(xmlDocument.Attributes);
            Assert.Equal(value, xmlDocument.InnerText);
        }

        [Theory]
        [InlineData("<nlog><variable name='abc' layout='${message}'/></nlog>")]
        [InlineData("<nlog>\n<variable\nname='abc'\nlayout='${message}'/>\n</nlog>")]
        [InlineData("<nlog><variable><name>abc</name><layout>${message}</layout></variable></nlog>")]
        [InlineData("<nlog>\n<variable>\n<name>abc</name>\n<layout>${message}</layout>\n</variable>\n</nlog>")]
        public void XmlParse_Children(string xmlSource)
        {
            var xmlDocument = new XmlParser(xmlSource).LoadDocument(out var _);
            Assert.NotNull(xmlDocument);
            Assert.Equal("nlog", xmlDocument.Name);
            Assert.Single(xmlDocument.Children);
            Assert.Equal("variable", xmlDocument.Children[0].Name);
        }

        [Theory]
        [InlineData("<nlog><variable name='abc' layout='${message}'/><variable name='123' layout='${message}'/></nlog>")]
        [InlineData("<nlog>\n<variable\nname='abc'\nlayout='${message}'/>\n<variable\nname='123'\nlayout='${message}'/>\n</nlog>")]
        [InlineData("<nlog><variable name='abc' layout='${message}'/><variable name='123'><layout>${message}</layout></variable></nlog>")]
        [InlineData("<nlog>\n<variable\nname='abc'\nlayout='${message}'/>\n<variable name='123'>\n<layout>${message}</layout>\n</variable>\n</nlog>")]
        [InlineData("<nlog><variable><name>abc</name><layout>${message}</layout></variable><variable><name>123</name><layout>${message}</layout></variable></nlog>")]
        [InlineData("<nlog>\n<variable>\n<name>abc</name>\n<layout>${message}</layout>\n</variable>\n<variable>\n<name>123</name>\n<layout>${message}</layout>\n</variable>\n</nlog>")]
        public void XmlParse_Children_Multiple(string xmlSource)
        {
            var xmlDocument = new XmlParser(xmlSource).LoadDocument(out var _);
            Assert.NotNull(xmlDocument);
            Assert.Equal("nlog", xmlDocument.Name);
            Assert.Equal(2, xmlDocument.Children.Count);
            Assert.Equal("variable", xmlDocument.Children[0].Name);
            Assert.Equal("variable", xmlDocument.Children[1].Name);
        }

        [Fact]
        public void XmlParse_DeeplyNestedXml_DoesNotThrowStackOverflowException()
        {
            string deeplyNestedXml = "<root>" + string.Join("", Enumerable.Repeat("<a>", 10000).ToArray()) + string.Join("", Enumerable.Repeat("</a>", 10000).ToArray()) + "</root>";
            var xmlDocument = new XmlParser(deeplyNestedXml).LoadDocument(out var _);
            Assert.NotNull(xmlDocument);
            Assert.Equal("root", xmlDocument.Name);
            Assert.Single(xmlDocument.Children);
        }
    }
}

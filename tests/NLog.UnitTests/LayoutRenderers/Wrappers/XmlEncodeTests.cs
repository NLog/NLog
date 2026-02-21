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

namespace NLog.UnitTests.LayoutRenderers.Wrappers
{
    using System;
    using System.Collections.Generic;
    using NLog;
    using NLog.Layouts;
    using Xunit;

    public class XmlEncodeTests : NLogTestBase
    {
        [Fact]
        public void XmlEncodeTest1()
        {
            var propertyValue = " abc<>&'\"def \u0001";
            using (ScopeContext.PushProperty("foo", propertyValue))
            {
                SimpleLayout l = "${xml-encode:${scopeproperty:foo}}";

                Assert.Equal(" abc&lt;&gt;&amp;&apos;&quot;def ", l.Render(LogEventInfo.CreateNullEvent()));
            }

            SimpleLayout l2 = "${xml-encode:${event-properties:foo}}";
            Assert.Equal(" abc&lt;&gt;&amp;&apos;&quot;def ", l2.Render(new LogEventInfo(LogLevel.Off, null, string.Empty, new[] { new NLog.MessageTemplates.MessageTemplateParameter("foo", propertyValue, null) })));
        }

        [Fact]
        public void XmlEncodeBadStringTest1()
        {
            var sb = new System.Text.StringBuilder();

            var forbidden = new HashSet<int>();
            int start = 64976; int end = 65007;

            for (int i = start; i <= end; i++)
            {
                forbidden.Add(i);
            }

            forbidden.Add(0xFFFE);
            forbidden.Add(0xFFFF);

            for (int i = char.MinValue; i <= char.MaxValue; i++)
            {
                char c = Convert.ToChar(i);
                if (char.IsSurrogate(c))
                {
                    continue; // skip surrogates
                }

                if (forbidden.Contains(c))
                {
                    continue;
                }

                sb.Append(c);
            }

            var badString = sb.ToString();

            using (ScopeContext.PushProperty("foo", badString))
            {
                SimpleLayout l = "${xml-encode:${scopeproperty:foo}}";

                var goodString = l.Render(LogEventInfo.CreateNullEvent());
                Assert.NotEmpty(goodString);
                foreach (char c in goodString)
                {
                    Assert.True(System.Xml.XmlConvert.IsXmlChar(c), $"Invalid char {Convert.ToInt32(c)} was not removed");
                }   
            }

            using (ScopeContext.PushProperty("foo", badString))
            {
                SimpleLayout l = "${xml-encode:${scopeproperty:foo}:CDataEncode=true}";

                var goodString = l.Render(LogEventInfo.CreateNullEvent());
                Assert.NotEmpty(goodString);
                foreach (char c in goodString)
                {
                    Assert.True(System.Xml.XmlConvert.IsXmlChar(c), $"Invalid char {Convert.ToInt32(c)} was not removed");
                }
            }
        }

        [Fact]
        public void XmlEncodeCDataTest1()
        {
            var propertyValue = " abc<>&'\"def \u0001";
            using (ScopeContext.PushProperty("foo", propertyValue))
            {
                SimpleLayout l = "${xml-encode:${scopeproperty:foo}:CDataEncode=true}";

                Assert.Equal("<![CDATA[ abc<>&'\"def ]]>", l.Render(LogEventInfo.CreateNullEvent()));
            }

            SimpleLayout l2 = "${xml-encode:${event-properties:foo}:CDataEncode=true}";
            Assert.Equal("<![CDATA[ abc<>&'\"def ]]>", l2.Render(new LogEventInfo(LogLevel.Off, null, string.Empty, new[] { new NLog.MessageTemplates.MessageTemplateParameter("foo", propertyValue, null) })));
        }

        [Fact]
        public void XmlEncodeCDataTest2()
        {
            var propertyValue = " abc<]]>>&'\"def \u0001";
            using (ScopeContext.PushProperty("foo", propertyValue))
            {
                SimpleLayout l = "${xml-encode:${scopeproperty:foo}:CDataEncode=true}";

                Assert.Equal("<![CDATA[ abc<]]]]><![CDATA[>>&'\"def ]]>", l.Render(LogEventInfo.CreateNullEvent()));
            }

            SimpleLayout l2 = "${xml-encode:${event-properties:foo}:CDataEncode=true}";
            Assert.Equal("<![CDATA[ abc<]]]]><![CDATA[>>&'\"def ]]>", l2.Render(new LogEventInfo(LogLevel.Off, null, string.Empty, new[] { new NLog.MessageTemplates.MessageTemplateParameter("foo", propertyValue, null) })));
        }
    }
}

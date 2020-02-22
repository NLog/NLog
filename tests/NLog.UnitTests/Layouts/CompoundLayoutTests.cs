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

using NLog.Config;
using NLog.Targets;

namespace NLog.UnitTests.Layouts
{
    using NLog.Layouts;
    using System;
    using Xunit;

    public class CompoundLayoutTests : NLogTestBase
    {
        [Fact]
        public void CodeCompoundLayoutIsRenderedCorrectly()
        {
            var compoundLayout = new CompoundLayout
            {
                Layouts =
                {
                    new SimpleLayout("Long date - ${longdate}"),
                    new SimpleLayout("|Before| "),
                    new JsonLayout
                    {
                        Attributes =
                        {
                            new JsonAttribute("short date", "${shortdate}"),
                            new JsonAttribute("message", "${message}"),
                        }
                    },
                    new SimpleLayout(" |After|"),
                    new SimpleLayout("Last - ${level}")
                }
            };

            var logEventInfo = new LogEventInfo
            {
                TimeStamp = new DateTime(2010, 01, 20, 12, 34, 56),
                Level = LogLevel.Info,
                Message = "hello, world"
            };

            const string expected = "Long date - 2010-01-20 12:34:56.0000|Before| { \"short date\": \"2010-01-20\", \"message\": \"hello, world\" } |After|Last - Info";
            var actual = compoundLayout.Render(logEventInfo);
            Assert.Equal(expected, actual);
        }


        [Fact]
        public void XmlCompoundLayoutIsRenderedCorrectly()
        {
            const string configXml = @"
<nlog>
  <targets>
    <target name='compoundFile' type='File' fileName='log.txt'>
      <layout type='CompoundLayout'>
        <layout type='SimpleLayout' text='Long date - ${longdate}' />
        <layout type='SimpleLayout' text='|Before| ' />
        <layout type='JsonLayout'>
          <attribute name='short date' layout='${shortdate}' />
          <attribute name='message' layout='${message}' />
        </layout>
        <layout type='SimpleLayout' text=' |After|' />
        <layout type='SimpleLayout' text='Last - ${level}' />
      </layout>
    </target>
  </targets>
  <rules>
  </rules>
</nlog>
";

            var config = XmlLoggingConfiguration.CreateFromXmlString(configXml);

            Assert.NotNull(config);
            var target = config.FindTargetByName<FileTarget>("compoundFile");
            Assert.NotNull(target);
            var compoundLayout = target.Layout as CompoundLayout;
            Assert.NotNull(compoundLayout);
            var layouts = compoundLayout.Layouts;
            Assert.NotNull(layouts);
            Assert.Equal(5, layouts.Count);
            Assert.Equal(typeof(SimpleLayout), layouts[0].GetType());
            Assert.Equal(typeof(SimpleLayout), layouts[1].GetType());
            var innerJsonLayout = (JsonLayout)layouts[2];
            Assert.Equal(typeof(JsonLayout), innerJsonLayout.GetType());
            Assert.Equal(2, innerJsonLayout.Attributes.Count);
            Assert.Equal("${shortdate}", innerJsonLayout.Attributes[0].Layout.ToString());
            Assert.Equal("${message}", innerJsonLayout.Attributes[1].Layout.ToString());
            Assert.Equal(typeof(SimpleLayout), layouts[3].GetType());
            Assert.Equal(typeof(SimpleLayout), layouts[4].GetType());

            var logEventInfo = new LogEventInfo
            {
                TimeStamp = new DateTime(2010, 01, 20, 12, 34, 56),
                Level = LogLevel.Info,
                Message = "hello, world"
            };

            const string expected = "Long date - 2010-01-20 12:34:56.0000|Before| { \"short date\": \"2010-01-20\", \"message\": \"hello, world\" } |After|Last - Info";
            var actual = compoundLayout.Render(logEventInfo);
            Assert.Equal(expected, actual);
        }
    }
}

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

namespace NLog.UnitTests.MessageTemplates
{
    using System;
    using NLog.MessageTemplates;
    using Xunit;
    using Xunit.Extensions;

    public class ParserTests
    {
        private static object[] ManyParameters = new object[100];

        [Theory]
        [InlineData("")]
        [InlineData("Hello {0}")]
        [InlineData("I like my {car}")]
        [InlineData("But when Im drunk I need a {cool} bike")]
        [InlineData("I have {0} {1} {2} parameters")]
        [InlineData("{0} on front")]
        [InlineData(" {0} on front")]
        [InlineData("end {1}")]
        [InlineData("end {1} ")]
        [InlineData("{name} is my name")]
        [InlineData(" {name} is my name")]
        [InlineData("{multiple}{parameters}")]
        [InlineData("I have {{text}} and {{0}}")]
        [InlineData("{{text}}{{0}}")]
        [InlineData(" {{text}}{{0}} ")]
        [InlineData(" {0} ")]
        [InlineData(" {1} ")]
        [InlineData(" {2} ")]
        [InlineData(" {3} {4} {9} {8} {5} {6} {7}")]
        [InlineData(" {{ ")]
        [InlineData("{{ ")]
        [InlineData(" {{")]
        [InlineData(" }} ")]
        [InlineData("}} ")]
        [InlineData(" }}")]
        [InlineData("{0:000}")]
        [InlineData("{aaa:000}")]
        [InlineData(" {@serialize} ")]
        [InlineData(" {$stringify} ")]
        [InlineData(" {alignment,-10} ")]
        [InlineData(" {alignment,10} ")]
        [InlineData(" {0,10} ")]
        [InlineData(" {0,-10} ")]
        [InlineData(" {0,-10:test} ")]
        [InlineData("{{{0:d}}}")]
        [InlineData("{{{0:0{{}")]
        public void ParseAndPrint(string input)
        {
            var logEventInfo = new LogEventInfo(LogLevel.Info, "Logger", null, input, ManyParameters);
            logEventInfo.SetMessageFormatter(new NLog.Internal.LogMessageTemplateFormatter(LogManager.LogFactory.ServiceRepository, true, false).MessageFormatter, null);
            var templateAuto = logEventInfo.MessageTemplateParameters;
        }

        [Theory]
        [InlineData("{0}", 0, null)]
        [InlineData("{001}", 1, null)]
        [InlineData("{9}", 9, null)]
        [InlineData("{1 }", 1, null)]
        [InlineData("{1} {2}", 1, null)]
        [InlineData("{@3} {$4}", 3, null)]
        [InlineData("{3,6}", 3, null)]
        [InlineData("{5:R}", 5, "R")]        
        [InlineData("{0:0}", 0, "0")]        
        public void ParsePositional(string input, int index, string format)
        {
            var logEventInfo = new LogEventInfo(LogLevel.Info, "Logger", null, input, ManyParameters);
            logEventInfo.SetMessageFormatter(new NLog.Internal.LogMessageTemplateFormatter(LogManager.LogFactory.ServiceRepository, true, false).MessageFormatter, null);
            var template = logEventInfo.MessageTemplateParameters;

            Assert.True(template.IsPositional);
            Assert.True(template.Count >= 1);
            Assert.Equal(format, template[0].Format);
            Assert.Equal(index, template[0].PositionalIndex);
        }

        [Theory]
        [InlineData("{ 0}")]
        [InlineData("{-1}")]
        [InlineData("{1.2}")]
        [InlineData("{42r}")]
        [InlineData("{6} {x}")]
        [InlineData("{a} {x}")]
        public void ParseNominal(string input)
        {
            var logEventInfo = new LogEventInfo(LogLevel.Info, "Logger", null, input, ManyParameters);
            logEventInfo.SetMessageFormatter(new NLog.Internal.LogMessageTemplateFormatter(LogManager.LogFactory.ServiceRepository, true, false).MessageFormatter, null);
            var template = logEventInfo.MessageTemplateParameters;

            Assert.False(template.IsPositional);
        }

        [Theory]
        [InlineData("{hello}", "hello")]
        [InlineData("{@hello}", "hello")]
        [InlineData("{$hello}", "hello")]
        [InlineData("{#hello}", "#hello")]
        [InlineData("{  spaces  ,-3}", "  spaces  ")]
        [InlineData("{special!:G})", "special!")]
        [InlineData("{noescape_in_name}}}", "noescape_in_name")]
        [InlineData("{noescape_in_name{{}", "noescape_in_name{{")]
        [InlineData("{0}", "0")]
        [InlineData("{18 }", "18 ")]
        public void ParseName(string input, string name)
        {
            var logEventInfo = new LogEventInfo(LogLevel.Info, "Logger", null, input, ManyParameters);
            logEventInfo.SetMessageFormatter(new NLog.Internal.LogMessageTemplateFormatter(LogManager.LogFactory.ServiceRepository, true, false).MessageFormatter, null);
            var template = logEventInfo.MessageTemplateParameters;

            Assert.Equal(1, template.Count);
            Assert.Equal(name, template[0].Name);
        }

        [Theory]
        [InlineData("{aaa}", "")]
        [InlineData("{@a}", "@")]
        [InlineData("{@A}", "@")]
        [InlineData("{@8}", "@")]
        [InlineData("{@aaa}", "@")]
        [InlineData("{$a}", "$")]
        [InlineData("{$A}", "$")]
        [InlineData("{$9}", "$")]
        [InlineData("{$aaa}", "$")]
        public void ParseHoleType(string input, string holeType)
        {
            var logEventInfo = new LogEventInfo(LogLevel.Info, "Logger", null, input, ManyParameters);
            logEventInfo.SetMessageFormatter(new NLog.Internal.LogMessageTemplateFormatter(LogManager.LogFactory.ServiceRepository, true, false).MessageFormatter, null);
            var template = logEventInfo.MessageTemplateParameters;
            
            Assert.Equal(1, template.Count);
            CaptureType captureType = CaptureType.Normal;
            if (holeType == "@")
                captureType = CaptureType.Serialize;
            else if (holeType == "$")
                captureType = CaptureType.Stringify;
            Assert.Equal(captureType, template[0].CaptureType);
        }

        [Theory]
        [InlineData(" {0,-10:nl-nl} ", -10, "nl-nl")]
        [InlineData(" {0,-10} ", -10, null)]
        [InlineData("{0,  36  }", 36, null)]
        [InlineData("{0,-36  :x}", -36, "x")]
        [InlineData(" {0:nl-nl} ", 0, "nl-nl")]
        [InlineData(" {0} ", 0, null)]
        public void ParseFormatAndAlignment_numeric(string input, int? alignment, string format)
        {
            var logEventInfo = new LogEventInfo(LogLevel.Info, "Logger", null, input, ManyParameters);
            logEventInfo.SetMessageFormatter(new NLog.Internal.LogMessageTemplateFormatter(LogManager.LogFactory.ServiceRepository, true, false).MessageFormatter, null);
            var template = logEventInfo.MessageTemplateParameters;

            Assert.Equal(1, template.Count);
            var hole = template[0];
            Assert.Equal("0", hole.Name);
            Assert.Equal(0, hole.PositionalIndex);
            Assert.Equal(format, hole.Format);
            Assert.NotNull(alignment);
        }

        [Theory]
        [InlineData(" {car,-10:nl-nl} ", -10, "nl-nl")]
        [InlineData(" {car,-10} ", -10, null)]
        [InlineData(" {car:nl-nl} ", 0, "nl-nl")]
        [InlineData(" {car} ", 0, null)]
        public void ParseFormatAndAlignment_text(string input, int? alignment, string format)
        {
            var logEventInfo = new LogEventInfo(LogLevel.Info, "Logger", null, input, ManyParameters);
            logEventInfo.SetMessageFormatter(new NLog.Internal.LogMessageTemplateFormatter(LogManager.LogFactory.ServiceRepository, true, false).MessageFormatter, null);
            var template = logEventInfo.MessageTemplateParameters;

            Assert.Equal(1, template.Count);
            var hole = template[0];
            Assert.False(template.IsPositional);
            Assert.Equal("car", hole.Name);
            Assert.Equal(format, hole.Format);
            Assert.NotNull(alignment);
        }

        [Theory]
        [InlineData("Hello {0")]
        [InlineData("Hello 0}")]
        [InlineData("Hello {a:")]
        [InlineData("Hello {a")]
        [InlineData("Hello {a,")]
        [InlineData("Hello {a,1")]
        [InlineData("{")]
        [InlineData("}")]
        [InlineData("}}}")]
        [InlineData("}}}{")]
        [InlineData("{}}{")]
        [InlineData("{a,-3.5}")]
        [InlineData("{a,2x}")]
        [InlineData("{a,--2}")]
        [InlineData("{a,-2")]
        [InlineData("{a,-2 :N0")]
        [InlineData("{a,-2.0")]
        [InlineData("{a,:N0}")]
        [InlineData("{a,}")]        
        [InlineData("{a,{}")]        
        [InlineData("{a:{}")]        
        [InlineData("{a,d{e}")]        
        [InlineData("{a:d{e}")]        
        public void ThrowsTemplateParserException(string input)
        {
            Assert.Throws<TemplateParserException>(() =>
            {
                var logEventInfo = new LogEventInfo(LogLevel.Info, "Logger", null, input, ManyParameters);
                logEventInfo.SetMessageFormatter(new NLog.Internal.LogMessageTemplateFormatter(LogManager.LogFactory.ServiceRepository, true, false).MessageFormatter, null);
                var template = logEventInfo.MessageTemplateParameters;
            });
        }
    }
}
